using CSiAPIv1;

namespace PortalFrame._0Sap
{
    /// <summary>
    /// 封装 SAP ExtendedEntityData API，底层读写自定义数据
    /// </summary>
    public static class SapExtendedData
    {
        /// <summary>
        /// 存字符串数组
        /// </summary>
        public static void SetStrings(cSapModel model, string appName, string key, string[] values)
        {
            int numValues = values.Length;
            model.SetStringsExtendedEntityData(appName, key, numValues, ref values);
        }

        /// <summary>
        /// 拿某个 Key 的所有字符串
        /// </summary>
        public static string[] GetStrings(cSapModel model, string appName, string key)
        {
            int numValues = 0;
            string[] values = null!;
            model.GetKeyStringsExtendedEntityData(appName, key, ref numValues, ref values);
            return values;
        }

        /// <summary>
        /// 拿某个 AppName 下所有的 Key
        /// </summary>
        public static string[] GetAllKeys(cSapModel model, string appName)
        {
            int numKeys = 0;
            string[] keys = null!;
            model.GetKeysWithStringsExtendedEntityData(appName, ref numKeys, ref keys);
            return keys;
        }

        /// <summary>
        /// 删除某个 Key 的数据（NumberValues = 0）
        /// </summary>
        public static void DeleteKey(cSapModel model, string appName, string key)
        {
            int numValues = 0;
            string[] values = null!;
            model.SetStringsExtendedEntityData(appName, key, numValues, ref values);
        }
    }
}
