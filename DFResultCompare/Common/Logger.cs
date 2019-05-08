using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DFResultCompare.Common
{
    class Logger : IDisposable
    {
        private StreamWriter strWriter;
        private LogLevelEnum logLevel;

        public Logger(LogLevelEnum logLevel)
        {
            this.logLevel = logLevel;
        }

        public Logger(LogLevelEnum logLevel, string filePath)
            : this(logLevel)
        {
            if (!String.IsNullOrEmpty(filePath))
            {
                strWriter = new StreamWriter(filePath, false);
            }
        }

        ~Logger()
        {

        }

        public void LogDifferent(string message, params object[] args)
        {
            if (this.logLevel == LogLevelEnum.ALL || this.logLevel == LogLevelEnum.DIFF)
            {
                LogMessage(message, args);
            }
        }

        public void LogMissing(string message, params object[] args)
        {
            if (this.logLevel == LogLevelEnum.ALL || this.logLevel == LogLevelEnum.MISSING)
            {
                LogMessage(message, args);
            }
        }

        public void LogExtra(string message, params object[] args)
        {
            if (this.logLevel == LogLevelEnum.ALL || this.logLevel == LogLevelEnum.EXTRA)
            {
                LogMessage(message, args);
            }
        }

        private void LogMessage(string message, params object[] args)
        {
            if (strWriter != null)
            {
                strWriter.WriteLine(message, args);
            }
            else
            {
                ConsoleColor currentColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(message, args);
                Console.ForegroundColor = currentColor;
            }
        }

        public void Dispose()
        {
            if (strWriter != null)
            {
                strWriter.Flush();
                strWriter.Close();
            }
        }
    }
}
