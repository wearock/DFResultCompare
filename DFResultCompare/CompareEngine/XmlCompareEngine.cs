using DFResultCompare.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DFResultCompare.CompareEngine
{
    class XmlCompareEngine : ICompareEngine
    {
        private Dictionary<string, List<ResultEntity>> GoldenStandard { get; set; }
        
        private Dictionary<string, int> Statics { get; set; }

        public XmlCompareEngine()
        {
            GoldenStandard = new Dictionary<string, List<ResultEntity>>();
            Statics = new Dictionary<string, int>();
        }

        public bool IsResultCorrect(string resultFile, string goldenStandard)
        {
            bool isCorrect = true;
            
            // Load the golden standard
            ReadXml(goldenStandard, new Action<ResultEntity>(delegate(ResultEntity entity) {
                if(!GoldenStandard.ContainsKey(entity.Name))
                {
                    GoldenStandard.Add(entity.Name, new List<ResultEntity>());
                }
                GoldenStandard[entity.Name].Add(entity);
            }));

            // Load the result file, and compare the entities with golden standard
            ReadXml(resultFile, new Action<ResultEntity>(delegate(ResultEntity entity)
            {
                ResultEntity standard = null;
                if (Config.CompareExclusions.Contains(new KeyValuePair<string, string>(entity.Name, String.Empty)))
                {
                    return;
                }

                if(!GoldenStandard.ContainsKey(entity.Name) || (standard = GoldenStandard[entity.Name].Find(e => e.Id.Equals(entity.Id))) == null)
                {
                    Config.Logger.LogExtra("DIFF: Found extra entity '{0}' with id '{1}'!", entity.Name, entity.Id);
                    isCorrect = false;
                }
                else
                {
                    if(0 != entity.CompareTo(standard))
                    {
                        isCorrect = false;
                    }

                    GoldenStandard[entity.Name].Remove(standard);
                    if(GoldenStandard[entity.Name].Count == 0)
                    {
                        GoldenStandard.Remove(entity.Name);
                    }
                }

                LogStatics(entity.Name);
            }));


            if(GoldenStandard.Count > 0)
            {
                foreach (string entityName in GoldenStandard.Keys)
                {
                    foreach (ResultEntity entity in GoldenStandard[entityName])
                    {
                        Config.Logger.LogMissing("DIFF: Found missing entity '{0}' with id '{1}'!", entity.Name, entity.Id);

                        LogStatics(entity.Name);
                    }
                }
                isCorrect = false;
            }

            return isCorrect;
        }

        public Dictionary<string, int> GetCompareStatics()
        {
            return Statics;
        }

        private void ReadXml(string sourceFile, Action<ResultEntity> handler)
        {
            Stack<ResultEntity> stkEntities = new Stack<ResultEntity>();
            ResultEntity parentOfCurrent = null;
            bool skipping = false;

            using(XmlReader xReader = XmlReader.Create(sourceFile))
            {
                while (xReader.Read())
                {
                    if (xReader.NodeType == XmlNodeType.Whitespace ||
                        xReader.NodeType == XmlNodeType.XmlDeclaration ||
                        xReader.LocalName.Equals("DemandForce"))
                    {
                        continue;
                    }

                    if (xReader.NodeType == XmlNodeType.Element)
                    {
                        if (skipping)
                        {
                            continue;
                        }

                        if (xReader.LocalName.Equals("Extract") && parentOfCurrent.Name.Equals("Business"))
                        {
                            skipping = true;
                            continue;
                        }

                        ResultEntity node = new ResultEntity(xReader.LocalName);
                        if (xReader.HasAttributes && xReader.MoveToFirstAttribute())
                        {
                            do
                            {
                                if (xReader.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    node.Id = xReader.Value;
                                }
                                else
                                {
                                    node.Properties.Add(xReader.Name, xReader.Value);
                                }
                            } while (xReader.MoveToNextAttribute());

                            xReader.MoveToElement();
                        }

                        if(String.IsNullOrEmpty(node.Id) && parentOfCurrent != null)
                        {
                            node.Id = parentOfCurrent.Id;
                        }

                        handler(node);

                        if(parentOfCurrent != null)
                        {
                            stkEntities.Push(parentOfCurrent);
                        }
                        parentOfCurrent = node;
                    }
                    else if (xReader.NodeType == XmlNodeType.EndElement)
                    {
                        if (xReader.LocalName.Equals("Extract") && parentOfCurrent.Name.Equals("Business"))
                        {
                            skipping = false;
                            continue;
                        }

                        if (skipping)
                        {
                            continue;
                        }

                        if (stkEntities.Count > 0)
                        {
                            parentOfCurrent = stkEntities.Pop();
                        }
                    }
                }
            }
        }

        private void LogStatics(string entityName)
        {
            if (!Statics.ContainsKey(entityName))
            {
                Statics.Add(entityName, 0);
            }
            Statics[entityName]++;
        }
    }
}
