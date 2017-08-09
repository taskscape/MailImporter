using System;

namespace MailImporter
{
    public static class Default
    {
        public const bool UseSSL = false;
        public const int MailServerPort = 143;
        public const int MailTitleRegexPatternGroupForReferenceNumber = 3;
        public const int Timeout = 30 * 1000; // Time in milliseconds
        public const string NewLine = "\r\n";

        // Settings stand-in
        public const string CurrentCulture = "pl-PL";
        public static readonly string BinFolder = AppDomain.CurrentDomain.BaseDirectory;
        public const string MailImporterMessageSeparator = "--";
        public const string MailImporterTitleNewTag = "#new";
        public const int MailImporterProcessInterval = 25000;
    }
}
