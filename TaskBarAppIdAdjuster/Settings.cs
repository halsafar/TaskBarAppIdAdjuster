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
    /// <summary>
    /// Label for possible actions to take when processing the task bar.
    /// </summary>
    public enum TaskAction
    {
        Ungroup,
        Group,
    }

    /// <summary>
    /// Represent an individual task settings.
    /// </summary>
    [DataContract]
    public class TaskSetting
    {
        [DataMember]
        public String Name;

        [DataMember]
        public TaskAction Action = default(TaskAction);
    }

    /// <summary>
    /// Serialize / Unserialize itself into JSON.
    /// Writes out a default on first run.
    /// </summary>
    [DataContract]
    public class Settings
    {
        [DataMember]
        public List<TaskSetting> ApplicationsToRandomize = default(List<TaskSetting>);

        /// <summary>
        /// Constructor.
        /// </summary>
        public Settings()
        {

        }

        /// <summary>
        /// Serialize file to JSON.
        /// </summary>
        public void Save()
        {
            FileStream stream = new FileStream(Settings.ConfigPath(), FileMode.Create, FileAccess.Write);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Settings));
            ser.WriteObject(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Load file from JSON
        /// </summary>
        /// <returns></returns>
        public static Settings Load()
        {
            Settings retVal = null;
            if (!File.Exists(Settings.ConfigPath()))
            {
                TaskSetting notePadDefault = new TaskSetting();
                notePadDefault.Name = "notepad";
                notePadDefault.Action = TaskAction.Ungroup;

                retVal = new Settings();
                retVal.ApplicationsToRandomize = new List<TaskSetting>();
                retVal.ApplicationsToRandomize.Add(notePadDefault);
                retVal.Save();
            }
            else
            {
                FileStream stream = new FileStream(Settings.ConfigPath(), FileMode.Open, FileAccess.Read);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Settings));
                retVal = ser.ReadObject(stream) as Settings;
                stream.Close();
            }

            return retVal;
        }

        /// <summary>
        /// Retrieve ConfigPath at runtime
        /// </summary>
        /// <returns></returns>
        public static String ConfigPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "config.json");
        }
    }
}
