using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SampleCode
{
    public static class Log
    {
        public enum Category
        {
            Debug = 0x01 << 0,
            Lua = 0x01 << 1,
            Error = 0x01 << 2,
        }

        public class Message
        {
            public Category category;

            public string text;
        }

        public static int Categories = 0;

        private static string filePath = "";

        private static List<Message> categorizedLogs = new List<Message>();

        public static List<Message> CategorizedLogs
        {
            get
            {
                if (categorizedLogs == null)
                {
                    categorizedLogs = new List<Message>();
                }

                return categorizedLogs;
            }
        }

        private static HashSet<string> uniqueLogs = new HashSet<string>();

        public static void Push(string log_, Category category_ = Category.Debug)
        {
#if UNITY_EDITOR
            categorizedLogs.Add(new Message() { text = log_, category = category_ });
#if EH_DEBUG
                                
#endif
#endif
        }

        public static bool UniquePush(string log_, Category category_ = Category.Debug)
        {
#if UNITY_EDITOR
            if (uniqueLogs.Add(log_))
            {
                categorizedLogs.Add(new Message() { text = log_, category = category_ });

                return true;
            }
#if EH_DEBUG
                                
#endif
#endif
            return false;
        }

        public static void Start()
        {
            Clear();
#if DEBUG_OUTPUT
                // create log file

                string directoryName = Application.persistentDataPath + "/BattleLog";

                if (!System.IO.Directory.Exists(directoryName))
                {
                    System.IO.Directory.CreateDirectory(directoryName);
                }

                filePath = directoryName + "/" + System.DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".txt";
#endif
        }

        public static void End()
        {
            Clear();

            filePath = string.Empty;
        }

        public static void Clear()
        {
            categorizedLogs.Clear();

            uniqueLogs.Clear();
        }

        public static void SaveToLocal(string filePath_)
        {
            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath_, false, System.Text.Encoding.UTF8);

                int i, count = categorizedLogs.Count;

                for (i = 0; i < count; ++i)
                {
                    writer.WriteLine(categorizedLogs[i].text);
                }

                writer.Close();
            }

            catch (System.Exception e_)
            {
                UnityEngine.Debug.LogException(e_);
            }
        }

        public static void Push(UnityEngine.Object[] objects_, System.Type type_)
        {
            Clear();

            SampleCode.Bitwise.AddRef(ref Categories, (int)Category.Debug);

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendFormat("[{0}] {1}", type_.FullName, objects_.Length);

            Push(builder.ToString());

            Push(string.Empty);

            //
            //
            //

            for (int i = 0; i < objects_.Length; ++i)
            {
                builder.Clear();

                builder.AppendFormat(" {0}", objects_[i].name);

                Push(builder.ToString());
            }
        }
    }
}


