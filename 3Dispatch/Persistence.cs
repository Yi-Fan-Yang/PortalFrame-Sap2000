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
        private const string KeySelectedCombos = "SelectedCombos";
        private const string KeyCheckGroups = "CheckGroups";
        private const string KeyKzGroups = "KzGroups";

        private const string KeyKzAll = "KzAll";


        // 存首选项
        public static void SavePreferences()
        {
            var list = new List<string>();

            // 遍历 Preferences 静态类的所有 ConfigItem 字段
            foreach (var field in typeof(Preferences).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(ConfigItem)) continue;

                string paramName = field.Name;
                ConfigItem item = (ConfigItem)field.GetValue(null)!;

                // 有值存值，没值存空
                list.Add(item.Value.HasValue ? $"{paramName}={item.Value.Value}" : $"{paramName}=");
            }

            string[] values = [.. list];
            SapExtendedData.SetStrings(Global.SapModel, AppName, KeyPreferences, values);
        }

        // 存构件覆盖项（单个构件）
        public static void SaveMemberOverWrites(MemberData member)
        {
            var list = new List<string>();
            var result = member.CheckResult;

            // 遍历结果类的所有字段，找 ParaItem 类型的
            CollectParaItems(result, "", list);

            string key = KeyOverWritesPrefix + member.Name;
            string[] values = [.. list];
            SapExtendedData.SetStrings(Global.SapModel, AppName, key, values);
        }

        // 保存用户选中的组合
        public static void SaveSelectedCombos(List<string> combos)
        {
            SapExtendedData.SetStrings(Global.SapModel, AppName, KeySelectedCombos, combos.ToArray());
        }        
        public static void SaveAllKz()
        {
            var list = new List<string>();
            foreach (var member in Global.Data.Members.Values)
            {
                if (member.Kz.HasValue)
                {
                    list.Add($"{member.Name}={member.Kz.Value}");
                }
            }
            string values = string.Join(",", list);
            SapExtendedData.SetStrings(Global.SapModel, AppName, KeyKzAll, new[] { values });
        }
        public static void SaveCheckGroups()
        {
            SapExtendedData.SetStrings(Global.SapModel, AppName, KeyCheckGroups, Global.Data.Groups.CheckGroups.ToArray());
        }
        public static void SaveKzGroups()
        {
            SapExtendedData.SetStrings(Global.SapModel, AppName, KeyKzGroups, Global.Data.Groups.KzGroups.ToArray());
        }



        //=========================Load=========================
        // 启动时读全部（首选项 + 覆盖项）
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
                    LoadPreferences(values);
                }
                else if (key == KeySelectedCombos)
                {
                    // 读用户选中的组合
                    string[] values = SapExtendedData.GetStrings(Global.SapModel, AppName, key);
                    LoadSelectedCombos(values);
                }
                else if (key == KeyKzAll)
                {
                    // 读所有构件的Kz值
                    string[] values = SapExtendedData.GetStrings(Global.SapModel, AppName, key);
                    LoadAllKz(values);
                }
                else if (key.StartsWith(KeyOverWritesPrefix))
                {
                    // 读构件覆盖项
                    string memberName = key[KeyOverWritesPrefix.Length..];
                    if (Global.Data.Members.TryGetValue(memberName, out var member))
                    {
                        string[] values = SapExtendedData.GetStrings(Global.SapModel, AppName, key);
                        LoadOverWrites(member, values);
                    }
                    else
                    {
                        // 构件已经删了，删掉存储记录
                        SapExtendedData.DeleteKey(Global.SapModel, AppName, key);
                    }
                }
            }
        }

        // 读首选项到 Preferences 静态类
        private static void LoadPreferences(string[] values)
        {
            foreach (string str in values)
            {
                int eqPos = str.IndexOf('=');
                if (eqPos < 0) continue;

                string paramName = str[..eqPos];
                string paramValue = str[(eqPos + 1)..];

                // 用反射找到对应的 ConfigItem 字段
                var field = typeof(Preferences).GetField(paramName);
                if (field == null || field.FieldType != typeof(ConfigItem)) continue;

                ConfigItem item = (ConfigItem)field.GetValue(null)!;
                if (double.TryParse(paramValue, out double val))
                {
                    item.Value = val;
                }
            }

            // 读完首选项后，分发到所有构件的结果类
            // 这里可以调用首选项窗口的分发方法，或者直接在 LoadAll 里分发
            // 简单起见，我们直接在这里分发
            DispatchPreferencesToAllMembers();
        }
        // 读构件覆盖项到结果类
        private static void LoadOverWrites(MemberData member, string[] values)
        {
            var result = member.CheckResult;

            foreach (string str in values)
            {
                int eqPos = str.IndexOf('=');
                if (eqPos < 0) continue;

                string paramPath = str[..eqPos];  // 比如 "General.Gamma0"
                string paramValue = str[(eqPos + 1)..];

                // 用反射从结果类里找到对应的 ParaItem
                ParaItem? item = FindParaItemByPath(result, paramPath);
                if (item != null && double.TryParse(paramValue, out double val))
                {
                    item.SetOw(val);
                }
            }
        }
        // 读取用户选中的组合
        public static void LoadSelectedCombos(string[] values)
        {
            // 从 sdb 文件里读
            if (values == null || values.Length == 0)
                return ;
            // 把字符串转成列表
            var savedCombos = values.ToList();

            // 只保留存在的组合（防止模型改了，有些组合不存在了）
            Global.Data.Combos.SelectedCombos = savedCombos.Where(c => Global.Data.AllCombos.Contains(c)).ToList();
        }
        // 读取所有构件的Kz值
        private static void LoadAllKz(string[] values)
        {
            if (values == null || values.Length == 0)
                return;

            var pairs = values[0].Split(',');
            foreach (var pair in pairs)
            {
                int eqPos = pair.IndexOf('=');
                if (eqPos < 0) continue;

                string memberName = pair[..eqPos];
                if (double.TryParse(pair[(eqPos + 1)..], out double kz))
                {
                    if (Global.Data.Members.TryGetValue(memberName, out var member))
                    {
                        member.Kz = kz;
                    }
                }
            }
        }
        
        // 只读group设置（不依赖Members，点验算设置时调）
        public static void LoadCheckSettings()
        {
            string[] keys = SapExtendedData.GetAllKeys(Global.SapModel, AppName);
            if (keys == null) return;

            foreach (var key in keys)
            {
                if (key == KeyCheckGroups)
                {
                    string[] values = SapExtendedData.GetStrings(Global.SapModel, AppName, key);
                    if (values != null)
                        Global.Data.Groups.CheckGroups = values.ToList();
                }
                else if (key == KeyKzGroups)
                {
                    string[] values = SapExtendedData.GetStrings(Global.SapModel, AppName, key);
                    if (values != null)
                        Global.Data.Groups.KzGroups = values.ToList();
                }
            }
        }



        //=========================辅助方法=========================
        // 根据路径从对象里找 ParaItem（比如 "General.Gamma0"）
        private static ParaItem? FindParaItemByPath(object obj, string path)
        {
            string[] parts = path.Split('.');
            object current = obj;

            for (int i = 0; i < parts.Length; i++)
            {
                var field = current.GetType().GetField(parts[i]);
                if (field == null) return null;

                if (i == parts.Length - 1)
                {
                    // 最后一个字段，应该是 ParaItem 类型
                    if (field.FieldType == typeof(ParaItem))
                    {
                        return (ParaItem)field.GetValue(current)!;
                    }
                    return null;
                }
                else
                {
                    // 中间的字段，继续往下找
                    current = field.GetValue(current)!;
                    if (current == null) return null;
                }
            }

            return null;
        }

        // 把 Preferences 静态类的值分发到所有构件的结果类
        private static void DispatchPreferencesToAllMembers()
        {
            foreach (var member in Global.Data.Members.Values)
            {
                if (!member.IsCheckable) continue;   // 非验算构件不分发首选项
                var result = member.CheckResult;
                // General
                if (Preferences.Gamma0.Value.HasValue)result.General.Gamma0.SetPre(Preferences.Gamma0.Value.Value);
                if (Preferences.MMR.Value.HasValue)result.General.MMR.SetPre(Preferences.MMR.Value.Value);
                if (Preferences.SeismicType.Value.HasValue)result.General.SeismicType.SetPre(Preferences.SeismicType.Value.Value);
                if (Preferences.LambdaLimit.Value.HasValue)result.General.LambdaLimit.SetPre(Preferences.LambdaLimit.Value.Value);
                if (Preferences.FlangeLimit.Value.HasValue)result.General.FlangeLimit.SetPre(Preferences.FlangeLimit.Value.Value);
                if (Preferences.WebLimit.Value.HasValue)result.General.WebLimit.SetPre(Preferences.WebLimit.Value.Value);
                if (Preferences.UtilLimit.Value.HasValue)result.General.UtilLimit.SetPre(Preferences.UtilLimit.Value.Value);

                // Slenderness
                if (Preferences.Ly.Value.HasValue)result.Slenderness.Ly.SetPre(Preferences.Ly.Value.Value);

                // ShearStrength
                if (Preferences.HasStiffener.Value.HasValue) result.ShearStrength.HasStiffener.SetPre(Preferences.HasStiffener.Value.Value);
                if (Preferences.a.Value.HasValue) result.ShearStrength.a.SetPre(Preferences.a.Value.Value);

                // PhiBDetails
                foreach (var details in result.PhiBResult.Collection.Values)
                {
                    if (Preferences.HasBrace.Value.HasValue) details.HasBrace.SetPre(Preferences.HasBrace.Value.Value);
                    if (Preferences.Ap.Value.HasValue) details.Ap.SetPre(Preferences.Ap.Value.Value);
                    if (Preferences.Beta.Value.HasValue) details.Beta.SetPre(Preferences.Beta.Value.Value);
                    if (Preferences.Lp.Value.HasValue)details.Lp.SetPre(Preferences.Lp.Value.Value);
                    if (Preferences.Ip.Value.HasValue) details.Ip.SetPre(Preferences.Ip.Value.Value);
                    if (Preferences.A.Value.HasValue)details.A.SetPre(Preferences.A.Value.Value);
                    if (Preferences.Ak.Value.HasValue) details.Ak.SetPre(Preferences.Ak.Value.Value);
                    if (Preferences.Lkk.Value.HasValue)details.Lkk.SetPre(Preferences.Lkk.Value.Value);
                    if (Preferences.Alpha.Value.HasValue)details.Alpha.SetPre(Preferences.Alpha.Value.Value);
                }


            }
        }
        
        // 递归遍历对象，把所有 ParaItem 的 Ow 值收集起来
        private static void CollectParaItems(object? obj, string prefix, List<string> list)
        {
            if (obj == null) return;

            foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(ParaItem))
                {
                    ParaItem item = (ParaItem)field.GetValue(obj)!;
                    string paramName = prefix + field.Name;

                    // 有 Ow 值就存，没值就跳过
                    double? owVal = item.GetOw();
                    if (owVal.HasValue)
                    {
                        list.Add($"{paramName}={owVal.Value}");
                    }
                }
                else if (field.FieldType.IsClass && field.FieldType != typeof(string))
                {
                    // 递归遍历嵌套的对象
                    object? child = field.GetValue(obj);
                    CollectParaItems(child, prefix + field.Name + ".", list);
                }
            }
        }




    }
}
