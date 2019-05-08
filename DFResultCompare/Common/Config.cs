using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFResultCompare.Common
{
    static class Config
    {
        internal static List<KeyValuePair<string, string>> CompareExclusions { get; private set; }

        internal static Logger Logger { get; private set; }

        static Config()
        {
            CompareExclusions = new List<KeyValuePair<string, string>>();
        }

        public static void StartLogging(LogLevelEnum level, string logFile)
        {
            Logger = new Logger(level, logFile);
        }

        public static void EndLogging()
        {
            Logger.Dispose();
        }

        public static void AddCompareExclusion(string node, string property)
        {
            CompareExclusions.Add(new KeyValuePair<string, string>(node, property));
        }
    }
}
