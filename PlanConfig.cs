using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using ZapClient.Helpers;

namespace ZapReport
{
    public class PlanConfig
    {
        public string Folder;
        public string Culture;
        public bool LineColorChanging = true;
        public Dictionary<string, byte[]> StructureColors;
        public List<string> DoNotPrintPTVsWith;
        public List<string> DoNotPrintVOIsWith;
        public string StructureForVolumes;
        public List<ReportTypeData> ReportTypes;

        private static string _filename;

        public static PlanConfig LoadConfigData(string filename = "")
        {
            // Check first for filename with hostname, then with network adress or, at the end, use this without
            if (string.IsNullOrEmpty(filename))
            {
                filename = "ZapReport." + Network.GetHostName() + ".cfg";

                if (!File.Exists(filename))
                {
                    filename = "ZapReport." + Network.GetIPAdress() + ".cfg";
                }

                if (!File.Exists(filename))
                {
                    filename = "ZapReport.cfg";
                }
            }

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename);
            }

            _filename = filename;

            using (StreamReader file = File.OpenText(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (PlanConfig)serializer.Deserialize(file, typeof(PlanConfig));
            }
        }

        public static void SaveConfigData(PlanConfig planConfig)
        {
            using (StreamWriter file = File.CreateText(_filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented; 
                serializer.Serialize(file, planConfig);
            }
        }

        public byte[] GetColorForStructure(string name)
        {
            if (StructureColors == null || !StructureColors.ContainsKey(name))
                return null;

            return StructureColors[name];
        }
    }

    public class ReportTypeData
    {
        public string Title;
        public string Filename;
        public List<string> Components;
    }
}
