using DFResultCompare.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFResultCompare
{
    class ResultEntity : IComparable<ResultEntity>
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public ResultEntity(string localName)
        {
            this.Name = localName;

            this.Id = String.Empty;
            this.Properties = new Dictionary<string, string>();
        }

        public int CompareTo(ResultEntity other)
        {
            int result = 0;

            // Compare properties
            foreach (string propName in Properties.Keys)
            {
                // Filter out the compare exclusions
                if (Config.CompareExclusions.Contains(new KeyValuePair<string, string>(Name, propName)) ||
                    Config.CompareExclusions.Contains(new KeyValuePair<string, string>(Name, String.Empty)))
                {
                    if (other.Properties.ContainsKey(propName))
                    {
                        other.Properties.Remove(propName);
                    }
                    continue;
                }

                if (other.Properties.ContainsKey(propName))
                {
                    DateTime sourceDateTime;
                    DateTime targetDateTime;
                    bool sourceBoolean;
                    bool targetBoolean;
                    double sourceDouble;
                    double targetDouble;
                    if (DateTime.TryParse(Properties[propName], out sourceDateTime) && DateTime.TryParse(other.Properties[propName], out targetDateTime))
                    {
                        sourceDateTime = DateTime.Parse(Properties[propName].Substring(0, 19));
                        targetDateTime = DateTime.Parse(other.Properties[propName].Substring(0, 19));
                        // Different datetime observed
                        if (!sourceDateTime.Equals(targetDateTime))
                        {
                            Config.Logger.LogDifferent("DIFF: Different value of property '{0}' observed for node '{1}' with id '{2}'! Source value: '{3}'; Other value: '{4}'.",
                                propName, Name, Id, sourceDateTime.ToString("s"), targetDateTime.ToString("s"));
                            result = 1;
                        }
                    }
                    else if (Boolean.TryParse(Properties[propName], out sourceBoolean) || Boolean.TryParse(other.Properties[propName], out targetBoolean))
                    {
                        sourceBoolean = ToBoolean(Properties[propName]);
                        targetBoolean = ToBoolean(other.Properties[propName]);
                        // Different boolean observed
                        if (!sourceBoolean.Equals(targetBoolean))
                        {
                            Config.Logger.LogDifferent("DIFF: Different value of property '{0}' observed for node '{1}' with id '{2}'! Source value: '{3}'; Other value: '{4}'.",
                                propName, Name, Id, sourceBoolean.ToString(), targetBoolean.ToString());
                            result = 1;
                        }
                    }
                    else if (Double.TryParse(Properties[propName], out sourceDouble) && Double.TryParse(other.Properties[propName], out targetDouble))
                    {
                        sourceDouble = Double.Parse(Properties[propName]);
                        targetDouble = Double.Parse(other.Properties[propName]);
                        // Different boolean observed
                        if (!sourceDouble.Equals(targetDouble))
                        {
                            Config.Logger.LogDifferent("DIFF: Different value of property '{0}' observed for node '{1}' with id '{2}'! Source value: '{3}'; Other value: '{4}'.",
                                propName, Name, Id, sourceDouble.ToString(), targetDouble.ToString());
                            result = 1;
                        }
                    }
                    else
                    {
                        string sourceString = Properties[propName].Replace("&apos;", "'").Replace("&amp;", "&").Replace("&quot;", "\"");
                        string targetString = other.Properties[propName].Replace("&apos;", "'").Replace("&amp;", "&").Replace("&quot;", "\"");
                        // Different value observed
                        if (!sourceString.Equals(targetString))
                        {
                            Config.Logger.LogDifferent("DIFF: Different value of property '{0}' observed for node '{1}' with id '{2}'! Source value: '{3}'; Other value: '{4}'.",
                                propName, Name, Id, sourceString, targetString);
                            result = 1;
                        }
                    }

                    other.Properties.Remove(propName);
                }
                else
                {
                    Config.Logger.LogExtra("DIFF: Found extra property '{0}' with value '{1}' for node '{2}' of id '{3}'!",
                        propName, Properties[propName], Name, Id);
                    result = 1;
                }
            }

            // If there is still properties left from other entity, then that will be missing ones from our entity
            if (other.Properties.Count > 0)
            {
                foreach (string propName in other.Properties.Keys)
                {
                    if (Config.CompareExclusions.Contains(new KeyValuePair<string, string>(Name, propName)) ||
                        Config.CompareExclusions.Contains(new KeyValuePair<string, string>(Name, String.Empty)))
                    {
                        continue;
                    }

                    Config.Logger.LogMissing("DIFF: Found missing property '{0}' with value '{1}' for node '{2}' of id '{3}'!",
                        propName, other.Properties[propName], Name, Id);
                }
                result = -1;
            }

            return result;
        }

        private bool ToBoolean(string value)
        {
            bool bValue;
            if (Boolean.TryParse(value, out bValue))
            {
                return bValue;
            }
            else
            {
                int iValue;
                if (Int32.TryParse(value, out iValue))
                {
                    return Convert.ToBoolean(iValue);
                }
                throw new ArgumentException();
            }
        }
    }
}
