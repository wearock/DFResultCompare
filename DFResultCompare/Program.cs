using DFResultCompare.Common;
using DFResultCompare.CompareEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DFResultCompare
{
    class Program
    {
        static string goldenStandard;
        static string resultFile;
        static LogLevelEnum logLevel = LogLevelEnum.ALL;
        static string logFile = null;
        
        static void Main(string[] args)
        {
            ParseArgs(args);

            DateTime compareStart = DateTime.Now;

            Config.StartLogging(logLevel, logFile);
            ICompareEngine engine = new XmlCompareEngine();
            bool compareResult = engine.IsResultCorrect(resultFile, goldenStandard);
            Dictionary<string, int> statics = engine.GetCompareStatics();
            Config.EndLogging();

            DateTime compareCompleted = DateTime.Now;

            Console.WriteLine("========================================================");
            Console.WriteLine("Compare completed. Overall result: " + (compareResult ? "Identical" : "Different"));
            Console.WriteLine("========================================================");
            Console.Write("Total compared: ");
            foreach (string entityName in statics.Keys)
            {
                Console.Write("{0} {1}(s),", statics[entityName], entityName);
            }
            Console.WriteLine();
            Console.WriteLine("Total time used: {0} seconds.", compareCompleted.Subtract(compareStart).Seconds);
        }

        static void ParseArgs(string[] args)
        {
            int index = 0;
            while (index < args.Length)
            {
                switch (args[index].ToLower())
                {
                    case "-s":
                        goldenStandard = args[index + 1];
                        break;
                    case "-t":
                        resultFile = args[index + 1];
                        break;
                    case "-l":
                        logLevel = (LogLevelEnum)Enum.Parse(typeof(LogLevelEnum), args[index + 1].ToUpper());
                        break;
                    case "-f":
                        logFile = args[index + 1];
                        break;
                    case "-i":
                        using (StreamReader reader = new StreamReader(args[index + 1]))
                        {
                            while (!reader.EndOfStream)
                            {
                                string[] ignore = reader.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (ignore.Length == 0)
                                {
                                    continue;
                                }
                                else if (ignore.Length == 1)
                                {
                                    Config.AddCompareExclusion(ignore[0], String.Empty);
                                }
                                else
                                {
                                    Config.AddCompareExclusion(ignore[0], ignore[1]);
                                }
                            }
                        }
                        break;
                }
                index += 2;
            }
        }
    }
}
