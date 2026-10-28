using PortalFrame._1Model;
using PortalFrame._2CheckResults;

namespace PortalFrame._5data
{
    public class ConfigItem
    {
        public string Name;    // 中文名称
        public double? Value;   // 数值（用户输入的值，没有就是 null）
        public string Unit;    // 量纲

        public ConfigItem(string name, string unit = "")
        {
            Name = name;
            Unit = unit;
            Value = null;  // 默认没值
        }
    }
    // 全局首选项
    public static class Preferences
    {
        // General
        public static ConfigItem Gamma0 = new("结构重要性系数Gamma0");
        public static ConfigItem MMR = new("弯矩放大系数MMR");
        public static ConfigItem SeismicType = new("抗震类型");
        public static ConfigItem LambdaLimit = new("长细比限值");
        public static ConfigItem FlangeLimit = new("翼缘宽厚比限值");
        public static ConfigItem WebLimit = new("腹板高厚比限值");
        public static ConfigItem UtilLimit = new("应力比限值");

        // Slenderness
        public static ConfigItem Ly = new("平面外计算长度Ly", "mm");

        // PhiBDetails
        public static ConfigItem HasBrace = new("是否隅撑");
        public static ConfigItem Ap = new("檩条截面积Ap", "mm²");
        public static ConfigItem Beta = new("连接点距主梁距离比β");
        public static ConfigItem Lp = new("檩条跨度Lp", "mm");
        public static ConfigItem Ip = new("檩条I33", "mm⁴");
        public static ConfigItem A = new("檩条形心距离A", "mm");
        public static ConfigItem Ak = new("隅撑截面积Ak", "mm²");
        public static ConfigItem Lkk = new("隅撑间距Lkk", "mm");
        public static ConfigItem Alpha = new("隅撑夹角α", "°");

        // ShearStrength
        public static ConfigItem HasStiffener = new("是否加劲肋");
        public static ConfigItem a = new("加劲肋间距a", "mm");
    }
    public static class DefaultValues
    {
        // ===== 通用 =====
        public const double Gamma0 = 1.0;                // 结构重要性系数 γ0
        public const double MMR = 1.0;                    // 弯矩放大系数 MMR
        public const double Ly = 6000;                     // 平面外计算长度 Ly（mm）

        // ===== 加劲肋 =====
        public const double HasStiffener = 0;             // 是否设置加劲肋（0=否, 1=是）
        public const double a = 3000;      // 加劲肋间距 a（mm）

        // ===== 隅撑 =====
        public const double HasBrace = 1;                  // 是否设置隅撑（0=否, 1=是）
        public const double Ap = 720;             // 檩条截面积 Ap（mm²）
        public const double Beta = 0.1;   // 连接点距主梁距离比β
        public const double Lp = 9000;            // 檩条跨度 lp（mm）
        public const double Ip = 448600;           // 檩条惯性矩 Ip33（mm⁴）
        public const double A = 200;      // 檩条形心到梁上翼缘距离 a（mm）
        public const double Ak = 840;               // 隅撑截面积 Ak（mm²）
        public const double Lkk = 3000;           // 隅撑间距 lk（mm）
        public const double Alpha = 45;              // 隅撑与檩条夹角 α（°）

        // ===== 构造限值 =====
        public const double SeismicType = 0;               // 抗震类型（0=自定义, 1=多遇地震, 2=罕遇地震）
        public const double LambdaLimit = 180;            // 长细比限值 [λ]
        public const double FlangeLimit = 15;              // 翼缘宽厚比限值 [b/t]
        public const double WebLimit = 250;              // 腹板高厚比限值 [h0/tw]
         // ===== 抗震类型对应的限值=====
        public static double[] GetSeismicLimits(int seismicType) => seismicType switch
        {
            1 => new[] { 180.0, 15.0, 250.0 },   // 非抗震
            2 => new[] { 150.0, 13.0, 160.0 },   // 抗震
            _ => new[] { 180.0, 15.0, 250.0 }    // 自定义
        };

        public const double UtilLimit = 1.0;               // 应力比限值

        // ===== 通用验算系数（null=程序自己算）=====
        public const double GammaX = 1.05;                 // 塑性发展系数 γx

        // ===== 柱 =====
        public const double MuX = 2.0;                     // 平面内计算长度系数 μx
        public const double BetaMx = 1.0;                  // 等效弯矩系数 βmx

        public static double GetDefault(string paramName)
        {
            var field = typeof(DefaultValues).GetField(paramName);
            if (field != null && field.FieldType == typeof(double))
            {
                return (double)field.GetValue(null)!;
            }
            return 0.0;  // 找不到就返回 0
        }
    }
    public class DesignCombos
    {
        public List<string> SelectedCombos = new();
    }

    // 读取模型时用户选中的 group（用于计算梁对柱转动约束 Kz）
    public class SelectedGroups
    {
        public List<string> CheckGroups = new();    // A: 参与验算的group
        public List<string> KzGroups = new();      // B: 算Kz的group（B⊆A）
    }
    public class MemberDisplayInfo
    {
        public double Util;
        public double UtilLimit;   // 这根构件的应力比限值（已处理覆盖项>首选项>默认值）
        public string Text = "";
    }
}
