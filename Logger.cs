using log4net;

namespace MailImporter
{
    public enum LogType
    {
        INFO    = 0,
        WARNING = 1,
        ERROR   = 2
    }

    public static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger("RollingFileAppender");

        public static void Log(string message, LogType type = LogType.INFO)
        {
            switch(type)
            {
                case LogType.INFO:
                {
                    log.Info(message);
                    break;
                }
                case LogType.WARNING:
                {
                    log.Warn(message);
                    break;
                }
                case LogType.ERROR:
                {
                    log.Error(message);
                    break;
                }
                default: break;
            }
        }
    }
}
