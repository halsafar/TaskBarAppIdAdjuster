using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace TaskBarAppIdAdjuster
{
    [DataContract]
    public class Settings
    {
        [DataMember]
        public List<String> ApplicationsToRandomize = default(List<String>);

        public Settings()
        {

        }

        public void Save()
        {
            FileStream stream = new FileStream(Settings.ConfigPath(), FileMode.Create, FileAccess.Write);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Settings));
            ser.WriteObject(stream, this);
        }

        public static Settings Load()
        {
            Settings retVal = null;
            if (!File.Exists(Settings.ConfigPath()))
            {
                retVal = new Settings();
                retVal.ApplicationsToRandomize = new List<String>();
                retVal.ApplicationsToRandomize.Add("notepad");
                retVal.Save();
            }
            else
            {
                FileStream stream = new FileStream(Settings.ConfigPath(), FileMode.Open, FileAccess.Read);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Settings));
                retVal = ser.ReadObject(stream) as Settings;
            }

            return retVal;
        }

        public static String ConfigPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "config.json");
        }
    }
}
