using System;
using System.Diagnostics;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using System.Text.RegularExpressions;
using ActiveUp.Net.Mail;
using TaskBeat.Bootstrapper;
using TaskBeat.Logic;
using TaskBeat.Logic.MailImporter;
using TaskBeat.Logic.Routines.Interfaces;
using TaskBeat.Logic.Settings;
using StructureMap;
using TaskBeat.Logic.Tasks;
using TaskBeat.Logic.Loggers;
using TaskBeat.Logic.Cache;
using TaskBeat.Logic.Helpers;

namespace MailImporter
{
    public partial class MailService : ServiceBase
    {
        // Schedule timer
        private Timer checkerTimer { get; set; }
        
        private ISettings settings { get; set;}

        // Comments routines
        public ICommentsRoutines _commentsRoutines { get; set; }
        
        // Task routines
        public ITaskRoutines _taskRoutines { get; set; }

        // Profile routines
        public IProfileRoutines _profileRoutines { get; set; }

        // Logger
        public ILogger Logger { get; set; }

        // Use SSL
        public bool UseSsl;

        // Mail client instance
        public Imap4Client mailClient;

        // Internal logger
        private void WriteToEventLog(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            if (!EventLog.SourceExists("MailImporter"))
                EventLog.CreateEventSource("MailImporter", "Application");

            EventLog.WriteEntry("MailImporter", message, type);
        }

        // Constructor
        public MailService()
        {
            try
            {
                InitializeComponent();

                settings = new Settings()
                {
                    CurrentCulture = "pl-PL",
                    BinFolder = AppDomain.CurrentDomain.BaseDirectory,
                    ConnectionString = DatabaseAccessor.Instance.GetConnectionString(),
                    MailImporterMessageSeparator = GetConfigurationStringValue("MailImporterMessageSeparator", "--"),
                    MailImporterTitleNewTag = GetConfigurationStringValue("MailTitleNewTag", "#new"),
                    MailImporterProcessInterval = GetConfigurationIntegerValue("MailServerCheckerInterval")
                };

                if (!bool.TryParse(GetConfigurationStringValue("UseSsl", "false"), out UseSsl))
                    UseSsl = false;

                Bootstrapper.Register(settings);
                ObjectFactory.BuildUp(this);

                WriteToEventLog("Initialization succeeded!");
            }
            catch (Exception ex)
            {
                WriteToEventLog("Initialization failed: " + ex.Message, EventLogEntryType.Error);
                throw;
            }
        }

        public void Start()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            checkerTimer = new Timer { Interval = settings.MailImporterProcessInterval };
            checkerTimer.Elapsed += SheduleTimerTick;
            checkerTimer.Enabled = true;
        }

        private void SheduleTimerTick(object sender, EventArgs e)
        {            
            try
            {
                checkerTimer.Enabled = false;
                //Logger.LogInfo("MailImporter is Working...", LogType.Message);

                ProcessMailMessages();
            }
            finally
            {
                checkerTimer.Enabled = true;
            }                    
        }

        public void ProcessMailMessages()
        {           
            try
            {
                if (mailClient == null || !mailClient.IsConnected)
                {
                    mailClient = new Imap4Client();

                    if (UseSsl)
                    {
                        mailClient.ConnectSsl(GetConfigurationStringValue("MailServerHost", string.Empty), GetConfigurationIntegerValue("MailServerPort"));
                        mailClient.Login(GetConfigurationStringValue("MailServerUsername", string.Empty), GetConfigurationStringValue("MailServerPassword", string.Empty));
                    }
                    else
                    {
                        mailClient.Connect(GetConfigurationStringValue("MailServerHost", string.Empty),
                                           GetConfigurationIntegerValue("MailServerPort"),
                                           GetConfigurationStringValue("MailServerUsername", string.Empty),
                                           GetConfigurationStringValue("MailServerPassword", string.Empty));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToEventLog("ProcessMailMessages Failed to connect to mail server: " + ex.Message, EventLogEntryType.Error);
                return;
            }

            // select mailbox

            var serverMailboxName = @"INBOX";
            var serverMailbox = mailClient.SelectMailbox(serverMailboxName);
            if (serverMailbox == null)
            {
                WriteToEventLog(String.Format("ProcessMailMessages failed to select mailbox by name: {0}", serverMailboxName));
                return;
            }

            int[] unreadMessages = null;

            try
            {
                unreadMessages = serverMailbox.Search("UNSEEN");
            }
            catch
            {
                // No other way to catch this error...
                // No unread e-mails found
                return;
            }

            if (unreadMessages == null)
                return;

            // process messages

            for (int i = 0; i < unreadMessages.Length; i++)
            {
                var emailObject = serverMailbox.Fetch.MessageObject(unreadMessages[i]);

                if (emailObject.From.Email.Trim().Length == 0)
                    continue;

                var emailSubject = emailObject.Subject.Trim();
                var containsNewTaskTag = emailObject.Subject.Contains(settings.MailImporterTitleNewTag);
                var taskReferenceNumberIndex = emailSubject.LastIndexOf('#');

                if (taskReferenceNumberIndex == -1 && !containsNewTaskTag)
                    continue;

                var taskReferenceNumber = 0;

                if (!int.TryParse(emailSubject.Substring(taskReferenceNumberIndex + 1), out taskReferenceNumber) && !containsNewTaskTag)
                    continue;

                // process message task profile
                var userProfile = _profileRoutines.GetProfileByName(emailObject.From.Email.Trim());

                if (userProfile == null || userProfile.Context == null || userProfile.Context.Value == 0)
                    return;

                WriteToEventLog(string.Format("ProcessMailMessages processing message:\n\n{0}", string.IsNullOrWhiteSpace(emailObject.BodyText.Text) ? emailObject.BodyHtml.Text.ParseHTML() : emailObject.BodyText.Text));

                // process message text

                var taskComment = string.Empty;
                var taskClientName = string.Empty;
                var taskClientEmail = string.Empty;

                var stringSplitted = Regex.Split(string.IsNullOrWhiteSpace(emailObject.BodyText.Text) ? emailObject.BodyHtml.Text.ParseHTML() : emailObject.BodyText.Text, "\r\n").ToList();

                if (stringSplitted.Select(x => x.Trim()).ToList().Contains(settings.MailImporterMessageSeparator))
                {
                    var elemsToRemoveCount = stringSplitted.Count - stringSplitted.Select(x => x.Trim()).ToList().IndexOf(settings.MailImporterMessageSeparator);

                    while (elemsToRemoveCount > 0)
                    {
                        elemsToRemoveCount--;
                        stringSplitted.RemoveAt(stringSplitted.Count - 1);
                    }

                    for (int j = 0; j < stringSplitted.Count - 1; j++)
                        taskComment += stringSplitted.ElementAt(j) + "\r\n";

                    taskComment += stringSplitted.ElementAt(stringSplitted.Count - 1);
                }
                else
                    taskComment = string.IsNullOrWhiteSpace(emailObject.BodyText.Text) ? emailObject.BodyHtml.Text.ParseHTML() : emailObject.BodyText.Text;

                // process message attribs for new task
                if (containsNewTaskTag)
                {
                    _taskRoutines.CreateTaskWithNotes
                        (
                            Regex.Replace(emailObject.Subject.Replace(settings.MailImporterTitleNewTag, String.Empty), @"\t|\n|\r", "").Trim(),
                            taskComment,
                            taskClientName,
                            taskClientEmail,
                            userProfile
                        );
                } // created task
                else if (!string.IsNullOrWhiteSpace(taskComment))
                {
                    SystemCacheFactory.Method = "FILESYSTEM";
                    
                    _commentsRoutines.CreateComentByTaskReference
                        (
                            taskReferenceNumber,
                            emailObject.From.Email.Trim(),
                            taskComment,
                            emailObject.Date.ToLocalTime(),
                            userProfile.Context.Value
                        );                    
                }
            } // for
        }

        private string GetConfigurationStringValue(string keyName, string defaultValue)
        {
            var result = ConfigurationManager.AppSettings[keyName];

            if (result == null)
            {
                WriteToEventLog("Configuration key " + keyName + " not found!", EventLogEntryType.Warning);
            return defaultValue;
            }
                else if (result.Length == 0)
            {
                WriteToEventLog("Configuration key " + keyName + " is empty!", EventLogEntryType.Warning);
            return defaultValue;
            }
            return result;
        }

        private int GetConfigurationIntegerValue(string key)
        {
            int returnValue = 0;

            switch (key)
            {
                case "MailServerPort":
                    {
                        if (!int.TryParse(ConfigurationManager.AppSettings[key], out returnValue))
                        {
                            WriteToEventLog("Configuration key " + key + " not found! Default in use.", EventLogEntryType.Warning);
                            return 143;
                        }
                        return returnValue;
                    }

                case "MailServerCheckerInterval":
                    {
                        if (!int.TryParse(ConfigurationManager.AppSettings[key], out returnValue))
                        {
                            WriteToEventLog("Configuration key " + key + " not found! Default in use", EventLogEntryType.Warning);
                            return 25000;
                        }
                        return returnValue;
                    }

                case "MailTitleRegexPatternGroupForReferenceNumber":
                    {
                        if (!int.TryParse(ConfigurationManager.AppSettings[key], out returnValue))
                        {
                            WriteToEventLog("Configuration key " + key + " not found! Default in use", EventLogEntryType.Warning);
                            return 3;
                        }
                        return returnValue;
                    }

                default:
                    WriteToEventLog("Configuration key " + key + " not found! No default to use", EventLogEntryType.Warning);
                    return returnValue;
            }
        }


        protected override void OnStop()
        {
            if (mailClient != null && mailClient.IsConnected)
            {
                mailClient.Disconnect();
            }

            if (checkerTimer != null)
            {
                checkerTimer.Enabled = false;
            }
        }
    }
}
