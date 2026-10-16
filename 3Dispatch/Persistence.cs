using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._5data;
using System.Reflection;

namespace PortalFrame._3Dispatch
{
    /// <summary>
    /// 持久化：首选项和覆盖项存到 sdb 文件
    /// </summary>
    public static class Persistence
    {
        private const string AppName = "PortalFrame";
        private const string KeyPreferences = "Preferences";
        private const string KeyOverWritesPrefix = "OverWrites:";

        /// <summary>
        /// 存首选项
        /// </summary>
        public static void SavePreferences(Preferences prefs)
        {
            var list = new List<string>();

            foreach (var field in typeof(Preferences).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType != typeof(ParaItem)) continue;

                string paramName = field.Name;
                ParaItem item = (ParaItem)field.GetValue(prefs)!;
                double? preVal = item!.GetPre();

                // 有值存值，没值存空
                list.Add(preVal.HasValue ? $"{paramName}={preVal.Value}" : $"{paramName}=");
            }

            string[] values = list.ToArray();
            SapExtendedData.SetStrings(Global.SapModel, AppName, KeyPreferences, values);
        }

        /// <summary>
        /// 存构件覆盖项（单个构件）
        /// </summary>
        public static void SaveMemberOverWrites(MemberData member)
        {
            var list = new List<string>();
            var ow = member.OverWrites;

            foreach (var field in typeof(OverWrites).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType != typeof(ParaItem)) continue;

                string paramName = field.Name;
                ParaItem item = (ParaItem)field.GetValue(ow)!;
                double? owVal = item!.GetOw();

                list.Add(owVal.HasValue ? $"{paramName}={owVal.Value}" : $"{paramName}=");
            }

            string key = KeyOverWritesPrefix + member.Name;
            string[] values = list.ToArray();
            SapExtendedData.SetStrings(Global.SapModel, AppName, key, values);
        }


        /// <summary>
        /// 启动时读全部（首选项 + 覆盖项）
        /// </summary>
        public static void LoadAll()
        {
            string[] keys = SapExtendedData.GetAllKeys(Global.SapModel, AppName);
            if (keys == null) return;   // 没有存过任何数据，直接返回
            foreach (string key in keys)
            {
                if (key == KeyPreferences)
                {
                    // 读首选项
                    string[] values = SapExtendedData.GetStrings(Global.SapModel, AppName, key);
                    LoadPreferences(values, Global.ModelData.Preferences);
                }
                else if (key.StartsWith(KeyOverWritesPrefix))
                {
                    // 读构件覆盖项
                    string memberName = key.Substring(KeyOverWritesPrefix.Length);
                    if (Global.ModelData.Members.TryGetValue(memberName, out var member))
                    {
                        string[] values = SapExtendedData.GetStrings(Global.SapModel, AppName, key);
                        LoadOverWrites(values, member.OverWrites);
                    }
                    else
                    {
                        // 构件已经删了，删掉存储记录
                        SapExtendedData.DeleteKey(Global.SapModel, AppName, key);
                    }
                }
            }
        }


        private static void LoadPreferences(string[] values, Preferences prefs)
        {
            foreach (string str in values)
            {
                int eqPos = str.IndexOf('=');
                string paramName = str.Substring(0, eqPos);
                string paramValue = str.Substring(eqPos + 1);

                // 用反射找到对应的字段，写值
                var field = typeof(Preferences).GetField(paramName);
                if (field == null || field.FieldType != typeof(ParaItem)) continue;

                ParaItem item = (ParaItem)field.GetValue(prefs)!;
                if (double.TryParse(paramValue, out double val))
                {
                    item.SetPre(val);
                }
            }
        }

        private static void LoadOverWrites(string[] values, OverWrites ow)
        {
            foreach (string str in values)
            {
                int eqPos = str.IndexOf('=');
                string paramName = str.Substring(0, eqPos);
                string paramValue = str.Substring(eqPos + 1);

                var field = typeof(OverWrites).GetField(paramName);
                if (field == null || field.FieldType != typeof(ParaItem)) continue;

                ParaItem item = (ParaItem)field.GetValue(ow)!;
                if (double.TryParse(paramValue, out double val))
                {
                    item.SetOw(val);
                }
            }
        }


    }
}
