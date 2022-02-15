using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

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
        public List<String> Rules;

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
            Settings.Serialize(this, stream);
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
                // Assume first run, create a default config file to serve as an example
                TaskSetting notePadDefault = new TaskSetting();
                notePadDefault.Name = "notepad";
                notePadDefault.Rules = new List<string>() { "notepad" };
                notePadDefault.Action = TaskAction.Ungroup;

                retVal = new Settings();
                retVal.ApplicationsToRandomize = new List<TaskSetting>();
                retVal.ApplicationsToRandomize.Add(notePadDefault);
                retVal.Save();
            }
            else
            {
                FileStream stream = new FileStream(Settings.ConfigPath(), FileMode.Open, FileAccess.Read);
                retVal = Settings.Deserialize(stream);
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

        /// <summary>
        /// Serialize this class to Json into a Stream.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="s"></param>
        public static void Serialize(object value, Stream s)
        {
            using (StreamWriter writer = new StreamWriter(s))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.IndentChar = ' ';
                jsonWriter.Indentation = 4;

                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(jsonWriter, value);
                jsonWriter.Flush();
            }
        }

        /// <summary>
        /// Unserialize this class from a stream containing JSON
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Settings Deserialize(Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<Settings>(jsonReader);
            }
        }
    }
}
