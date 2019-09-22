using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DaxConverter {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine($"{Directory.GetCurrentDirectory()}");

            if (args[0] == "fix") {
                AddRuntimeAndSizeAttributesToNodes(args[1], args[2]);
            } else if (args[0] == "updateAll") {
                SetRealRuntimeAndSize(args[1], args[2], args[3], args[4]);
            } else if (args[0] == "updateRuntime") {
                SetRealRuntime(args[1], args[2], args[4]);
            } else if (args[0] == "updateSize") {
                SetRealSize(args[1], args[2], args[4]);
            }

        }


        private static void AddRuntimeAndSizeAttributesToNodes(string daxFile, string outputFile) {
            var xDoc = new XmlDocument();

            try {
                xDoc.Load(daxFile);

                foreach (XmlNode jobNode in xDoc.GetElementsByTagName("job")) {
                    AddRuntimeAttributeToNode(xDoc, jobNode);
                    AddSizeAttributeToUsesNodes(xDoc, jobNode);
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            xDoc.Save(outputFile);
        }


        private static void SetRealRuntimeAndSize(string daxFile, string executionTimeLogFile, string filesPath, string outputFile) {
            var xDoc = new XmlDocument();
            var jobRuntimeDict = new Dictionary<string, string>();
            var fileSizeDict = new Dictionary<string, long>();

            FillJobRuntimeDict(jobRuntimeDict, executionTimeLogFile);
            FillFileSizeDict(fileSizeDict, filesPath);

            try {
                xDoc.Load(daxFile);

                foreach (XmlNode jobNode in xDoc.GetElementsByTagName("job")) {
                    SetRealRuntimeInNode(jobNode, jobRuntimeDict);
                    SetRealFileSizesInNode(jobNode, fileSizeDict);
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            xDoc.Save(outputFile);
        }


        private static void SetRealRuntime(string daxFile, string executionTimeLogFile, string outputFile) {
            var xDoc = new XmlDocument();
            var jobRuntimeDict = new Dictionary<string, string>();

            FillJobRuntimeDict(jobRuntimeDict, executionTimeLogFile);

            try {
                xDoc.Load(daxFile);

                foreach (XmlNode jobNode in xDoc.GetElementsByTagName("job")) {
                    SetRealRuntimeInNode(jobNode, jobRuntimeDict);
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            xDoc.Save(outputFile);
        }


        private static void SetRealSize(string daxFile, string filesPath, string outputFile) {
            var xDoc = new XmlDocument();
            var fileSizeDict = new Dictionary<string, long>();

            FillFileSizeDict(fileSizeDict, filesPath);

            try {
                xDoc.Load(daxFile);

                foreach (XmlNode jobNode in xDoc.GetElementsByTagName("job")) {
                    SetRealFileSizesInNode(jobNode, fileSizeDict);
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            xDoc.Save(outputFile);
        }


        private static void SetRealFileSizesInNode(XmlNode jobNode, Dictionary<string, long> fileSizeDict) {
            foreach (XmlNode jobNodeChildNode in jobNode.ChildNodes) {
                if (jobNodeChildNode.Name == "uses") {

                    var fileName = string.Empty;
                    foreach (XmlAttribute attribute in jobNodeChildNode.Attributes) {
                        if (attribute.Name == "name") {
                            fileName = attribute.Value;
                            break;
                        }
                    }

                    foreach (XmlAttribute attribute in jobNodeChildNode.Attributes) {
                        if (attribute.Name == "size") {
                            attribute.Value = fileSizeDict[fileName].ToString();
                            break;
                        }
                    }
                }
            }
        }


        private static void FillFileSizeDict(Dictionary<string, long> fileSizeDict, string filesPath) {
            var allFiles = Directory.GetFiles(filesPath);

            foreach (var file in allFiles) {
                var fileSize = new FileInfo(file).Length;
                if (fileSizeDict.ContainsKey(file)) {
                    continue;
                }

                fileSizeDict.Add(file, fileSize);
            }
        }


        private static void FillJobRuntimeDict(Dictionary<string, string> jobRuntimeDict, string executionTimeLogFile) {
            foreach (var line in File.ReadAllLines(executionTimeLogFile)) {
                var tokens = line.Substring(line.IndexOf(']') + 1).Trim().Split(',');

                var jobId = string.Empty;
                var executionTime = string.Empty;

                foreach (var token in tokens) {
                    if (token.StartsWith("jobId")) {
                        jobId = token.Split(':')[1].Trim();
                    } else if (token.StartsWith("executionTime")) {
                        executionTime = token.Split(':')[1].Trim();
                    } else {
                        // do nothing
                    }
                }

                jobRuntimeDict.Add(jobId, executionTime);
            }
        }


        private static void SetRealRuntimeInNode(XmlNode jobNode, Dictionary<string, string> jobRuntimeDict) {
            foreach (XmlAttribute jobNodeAttribute in jobNode.Attributes) {
                if (jobNodeAttribute.Name == "runtime") {
                    jobNodeAttribute.Value = jobRuntimeDict[jobNodeAttribute.Name.Remove(0,2).TrimStart('0')];
                }
            }
        }




        private static void AddSizeAttributeToUsesNodes(XmlDocument xDoc, XmlNode jobNode) {
            foreach (XmlNode jobNodeChildNode in jobNode.ChildNodes) {
                if (jobNodeChildNode.Name == "uses") {
                    AddSizeAttributeToNode(xDoc, jobNodeChildNode);
                }
            }
        }


        private static void AddSizeAttributeToNode(XmlDocument xDoc, XmlNode jobNode) {
            var typeAttr = xDoc.CreateAttribute("size");
            typeAttr.Value = "100000";
            jobNode.Attributes.Append(typeAttr);
        }


        private static void AddRuntimeAttributeToNode(XmlDocument xDoc, XmlNode jobNode) {
            var typeAttr = xDoc.CreateAttribute("runtime");
            typeAttr.Value = "10.5";
            jobNode.Attributes.Append(typeAttr);
        }
    }
}

