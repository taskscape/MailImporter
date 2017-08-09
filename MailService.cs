using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TaskBeat.Logic.Routines.Interfaces;

namespace MailImporter
{
    public partial class MailService : ServiceBase
    {
        // Schedule timer
        private System.Timers.Timer checkerTimer { get; set; }

        // Comments routines
        public static ICommentsRoutines _commentsRoutines { get; set; }

        // Task routines
        public static ITaskRoutines _taskRoutines { get; set; }

        // Profile routines
        public static IProfileRoutines _profileRoutines { get; set; }

        // Task managing variables
        private bool processing = false;
        private readonly object processingLock = new object();

        // List of users
        private readonly List<User> users = new List<User>();

        // SHA1 hash of the Users.xml file
        private byte[] usersHash;

        // SHA1 instance for computing hashes
        private readonly SHA1 sha1 = SHA1.Create();

        // Constructor
        public MailService(ICommentsRoutines commentsRoutines, ITaskRoutines taskRoutines, IProfileRoutines profileRoutines)
        {
            try
            {
                InitializeComponent();

                _commentsRoutines = commentsRoutines;
                _taskRoutines = taskRoutines;
                _profileRoutines = profileRoutines;

                Logger.Log("Initialization succeeded!");
            }
            catch(Exception ex)
            {
                Logger.Log($"Initialization failed: {ex.Message}", LogType.ERROR);
                throw;
            }
        }

        public void Start()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            checkerTimer = new System.Timers.Timer(Config.GetValue("MailServerCheckerInterval", Default.MailImporterProcessInterval));
            checkerTimer.Elapsed += ScheduleTimerTick;
            checkerTimer.Enabled = true;
        }

        private void ScheduleTimerTick(object sender, EventArgs e)
        {
            bool log = false;
            lock(processingLock)
            {
                if (!processing)
                {
                    processing = true;
                    Task.Run(() =>
                    {
                        LoadUsers();
                        ProcessMailMessages();
                        lock(processingLock)
                        {
                            processing = false;
                        }
                    }).ContinueWith(task =>
                    {
                        if(task.IsFaulted)
                        {
                            Logger.Log($"There was a problem with the task! {task?.Exception?.GetBaseException()?.Message ?? string.Empty}", LogType.ERROR);
                            lock(processingLock)
                            {
                                processing = false; // If there was a problem with the task, it might not have set the processing to false
                            }
                        }
                    });
                }
                else
                {
                    log = true;
                }
            }
            if (log) Logger.Log("Previous mail processing is not done yet. Skipping", LogType.WARNING);
        }

        private void ProcessMailMessages()
        {
            ManualResetEvent[] doneEvents = new ManualResetEvent[users.Count];
            CancellationTokenSource[] cancelTokens = new CancellationTokenSource[users.Count];
            for(int i = 0; i < users.Count; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                cancelTokens[i] = new CancellationTokenSource();
                MailProcessJob job = new MailProcessJob(users[i], doneEvents[i], cancelTokens[i].Token);
                ThreadPool.QueueUserWorkItem(job.ProcessMail, i);
            }

            if (doneEvents.Length > 0)
            {
                bool completed = WaitHandle.WaitAll(doneEvents, Default.Timeout);
                if (!completed) // Cancel the jobs
                {
                    foreach(CancellationTokenSource token in cancelTokens)
                    {
                        token.Cancel();
                    }

                    // Wait again for the cancellation to end
                    WaitHandle.WaitAll(doneEvents);
                }
            }
        }

        private void LoadUsers()
        {
            FileStream usersFile = null;
            try
            {
                usersFile = new FileStream("Users.xml", FileMode.Open, FileAccess.Read);
                if (usersFile.CanSeek) usersFile.Position = 0;

                byte[] hash = sha1.ComputeHash(usersFile);
                if (usersHash != hash)
                {
                    usersHash = hash;
                }
                else
                {
                    // The file has not changed since last check, no need to reload
                    return;
                }

                foreach (User user in users)
                {
                    if (user.MailClient != null && user.MailClient.IsConnected)
                    {
                        user.MailClient.Disconnect();
                    }
                }
                users.Clear();

                if (usersFile.CanSeek) usersFile.Position = 0;
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(usersFile);
                    XmlElement root = doc.DocumentElement;
                    XmlNodeList userNodes = root.GetElementsByTagName("user");
                    foreach (XmlNode node in userNodes)
                    {
                        string host = node.Attributes["host"]?.Value;
                        string username = node.Attributes["username"]?.Value;
                        string password = node.Attributes["password"]?.Value;
                        if (host == null || username == null || password == null)
                        {
                            Logger.Log("A host, username or password field is missing somewhere in the Users.xml file", LogType.WARNING);
                            continue;
                        }

                        int port = Default.MailServerPort;
                        try
                        {
                            port = Convert.ToInt32(node.Attributes["port"]?.Value);
                        }
                        catch(Exception)
                        {
                            Logger.Log($"Invalid value in port field for user {username}. Using default: {Default.MailServerPort}", LogType.WARNING);
                            port = Default.MailServerPort;
                        }

                        bool useSSL = Default.UseSSL;
                        try
                        {
                            useSSL = Convert.ToBoolean(node.Attributes["ssl"]?.Value);
                        }
                        catch (FormatException)
                        {
                            Logger.Log($"Invalid value in ssl field for user {username}. Using default: {Default.UseSSL}", LogType.WARNING);
                            useSSL = Default.UseSSL;
                        }

                        User user = new User(host, port, username, password, useSSL);
                        if (user.Good) users.Add(user);
                    }
                }
                catch (XmlException ex)
                {
                    Logger.Log($"Error parsing Xml: {ex.Message}", LogType.ERROR);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred while opening the Users.xml file: {ex.Message}", LogType.ERROR);
                return;
            }
            finally
            {
                usersFile?.Dispose();
            }
        }

        protected override void OnStop()
        {
            foreach(User user in users)
            {
                if(user.MailClient != null && user.MailClient.IsConnected)
                {
                    user.MailClient.Disconnect();
                }

                if(checkerTimer != null)
                {
                    checkerTimer.Enabled = false;
                }
            }
        }
    }
}
