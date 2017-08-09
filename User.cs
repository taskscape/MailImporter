using System;
using ActiveUp.Net.Mail;

namespace MailImporter
{
    public sealed class User
    {
        public Imap4Client MailClient { get; set; }
        public string Username { get; }
        public bool Good { get; }

        public User(string host, int port, string username, string password, bool useSSL)
        {
            Username = username;
            try
            {
                if (useSSL)
                {
                    MailClient.ConnectSsl(host, port);
                    MailClient.Login(username, password);
                }
                else
                {
                    MailClient.Connect(host, port, username, password);
                }

                Good = true;
            }
            catch(Exception ex)
            {
                Logger.Log($"Failed to connect to mail server. User: [{username}], SSL: [{useSSL}]. Exception: {ex.Message}", LogType.WARNING);
                Good = false;
            }
        }
    }
}
