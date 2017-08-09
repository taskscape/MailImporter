using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MailImporter
{
    internal class MailProcessJob
    {
        private readonly User user;
        private readonly ManualResetEvent doneEvent;
        private readonly CancellationToken cancelToken;

        public MailProcessJob(User user, ManualResetEvent doneEvent, CancellationToken cancelToken)
        {
            this.user = user;
            this.doneEvent = doneEvent;
            this.cancelToken = cancelToken;
        }

        public void ProcessMail(object threadContext)
        {
            string serverMailboxName = "INBOX";
            Mailbox serverMailbox = user.MailClient.SelectMailbox(serverMailboxName);
            if(serverMailbox == null)
            {
                Logger.Log($"ProcessMail has failed to select mailbox {serverMailboxName} for user {user.Username}", LogType.ERROR);
                goto Exit;
            }

            if (cancelToken.IsCancellationRequested) goto Exit;

            if (!RetrieveMessages(serverMailbox, out MessageCollection unreadMessages)) goto Exit;

            if (cancelToken.IsCancellationRequested) goto Exit;

            // Retrieve and store message UIDs in order to be able to delete messages
            // Unlike IndexOnServer, UIDs do not change when you delete or add new messages
            int[] messageUID = RetrieveUIDs(serverMailbox, unreadMessages);

            for (int i = 0; i < unreadMessages.Count; i++)
            {
                if (cancelToken.IsCancellationRequested) goto Exit;
                if (ProcessMessage(unreadMessages[i])) serverMailbox.UidDeleteMessage(messageUID[i], true);
            }

            Exit:
            doneEvent.Set();
        }

        private bool RetrieveMessages(Mailbox mailbox, out MessageCollection messages)
        {
            messages = null;
            try
            {
                messages = mailbox.SearchParse("UNSEEN");
            }
            catch
            {
                // No other way to catch this error...
                // No unread e-mails found
                return false;
            }

            return messages != null;
        }
        private int[] RetrieveUIDs(Mailbox mailbox, MessageCollection messages)
        {
            int[] uids = new int[messages.Count];
            for (int i = 0; i < uids.Length; i++)
            {
                uids[i] = mailbox.Fetch.Uid(messages[i].IndexOnServer);
            }
            return uids;
        }

        private bool ProcessMessage(Message message)
        {
            if (message.From.Email.Trim().Length == 0) return false;

            string emailSubject = message.Subject.Trim();
            bool containsNewTaskTag = message.Subject.Contains(Config.GetValue("MailTitleNewTag", Default.MailImporterTitleNewTag));
            int taskReferenceNumberIndex = emailSubject.LastIndexOf('#');

            if (taskReferenceNumberIndex == -1 && !containsNewTaskTag) return false;

            if (!int.TryParse(emailSubject.Substring(taskReferenceNumberIndex + 1), out int taskReferenceNumber) && !containsNewTaskTag) return false;

            // Process message task profile
            var userProfile = MailService._profileRoutines.GetProfileByName(message.From.Email.Trim());
            if (userProfile?.Context == null || userProfile.Context.Value == 0)
            {
                Logger.Log($"Could not retrieve user profile for {message.From.Email.Trim()}, skipping this message", LogType.ERROR);
                return false;
            }

            Logger.Log($"ProcessMail processing message: {message.BodyText.Text}");

            // Process message text
            string taskComment = string.Empty;
            string taskClientName = string.Empty;
            string taskClientEmail = string.Empty;

            List<string> stringSplitted = Regex.Split(message.BodyText.Text, "\r\n").ToList();

            if (stringSplitted.Select(x => x.Trim()).ToList().Contains(Config.GetValue("MailImporterMessageSeparator", Default.MailImporterMessageSeparator)))
            {
                int elemsToRemoveCount = stringSplitted.Count - stringSplitted.Select(x => x.Trim()).ToList().IndexOf(Config.GetValue("MailImporterMessageSeparator", Default.MailImporterMessageSeparator));
                stringSplitted.RemoveRange(stringSplitted.Count - elemsToRemoveCount, elemsToRemoveCount);

                for (int j = 0; j < stringSplitted.Count; j++)
                {
                    taskComment += stringSplitted.ElementAt(j);
                    if (j < stringSplitted.Count - 1)
                    {
                        taskComment += Default.NewLine;
                    }
                }
            }
            else
            {
                taskComment = message.BodyText.Text;
            }

            // Process message attribs for new task
            if (containsNewTaskTag)
            {
                MailService._taskRoutines.CreateTaskWithNotes
                    (
                        Regex.Replace(message.Subject.Replace(Config.GetValue("MailTitleNewTag", Default.MailImporterTitleNewTag), string.Empty), @"\t|\n|\r", "").Trim(),
                        taskComment,
                        taskClientName,
                        taskClientEmail,
                        userProfile
                    );
            } // Created task
            else if (!string.IsNullOrWhiteSpace(taskComment))
            {
                MailService._commentsRoutines.CreateCommentByTaskReference
                    (
                        taskReferenceNumber,
                        message.From.Email.Trim(),
                        taskComment,
                        message.Date.ToLocalTime(),
                        userProfile.Context.Value
                    );
            }

            return true;
        }
    }
}
