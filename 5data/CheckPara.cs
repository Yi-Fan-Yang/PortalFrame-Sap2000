using PortalFrame._1Model;
using PortalFrame._2CheckResults;

namespace PortalFrame._5data
{
    public class Preferences
    {
        // ===== 通用 =====
        public ParaItem Gamma0 = new("结构重要性系数 γ0");
        public ParaItem MMR = new("弯矩放大系数 MMR");
        public ParaItem Ly = new("平面外计算长度 Ly", "mm");

        // ===== 加劲肋 =====
        public ParaItem HasStiffener = new("是否设置加劲肋");
        public ParaItem StiffenerSpacing = new("加劲肋间距 a", "mm");

        // ===== 隅撑 =====
        public ParaItem HasBrace = new("是否设置隅撑");
        public ParaItem PurlinArea = new("檩条截面积 Ap", "mm²");
        public ParaItem PurlinSpan = new("檩条跨度 lp", "mm");
        public ParaItem PurlinI33 = new("檩条惯性矩 Ip33", "mm⁴");
        public ParaItem PurlinDistToFlange = new("檩条形心到梁上翼缘距离 a", "mm");
        public ParaItem BraceArea = new("隅撑截面积 Ab", "mm²");
        public ParaItem BraceSpacing = new("隅撑间距 lk", "mm");
        public ParaItem BraceAngle = new("隅撑与檩条夹角 α", "°");

        // ===== 构造 =====
        public ParaItem SeismicType = new("抗震类型");
        public ParaItem LambdaLimit = new("长细比限值 [λ]");
        public ParaItem FlangeLimit = new("翼缘宽厚比限值 [b/t]");
        public ParaItem WebLimit = new("腹板高厚比限值 [h0/tw]");
        public ParaItem UtilLimit = new("应力比限值");
    }

    public class OverWrites : Preferences
    {
        public ParaItem ElementType = new("构件类型");   // 0=柱, 1=梁

        // ===== 通用 =====
        public ParaItem Rho = new("有效宽度系数 ρ");
        public ParaItem PhiPs = new("抗剪折减系数 φps");
        public ParaItem PhiB = new("整体稳定系数 φb");
        public ParaItem GammaX = new("塑性发展系数 γx");

        // ===== 柱 =====
        public ParaItem MuX = new("平面内计算长度系数 μx");
        public ParaItem PhiX = new("平面内稳定系数 φx");
        public ParaItem BetaMx = new("等效弯矩系数 βmx");
        public ParaItem PhiY = new("平面外稳定系数 φy");
    }

    public static class DefaultValues
    {
        // ===== 通用 =====
        public const double Gamma0 = 1.0;                // 结构重要性系数 γ0
        public const double MMR = 1.0;                    // 弯矩放大系数 MMR
        public const double Ly = 6000;                     // 平面外计算长度 Ly（mm）

        // ===== 加劲肋 =====
        public const double HasStiffener = 0;             // 是否设置加劲肋（0=否, 1=是）
        public const double StiffenerSpacing = 3000;      // 加劲肋间距 a（mm）

        // ===== 隅撑 =====
        public const double HasBrace = 1;                  // 是否设置隅撑（0=否, 1=是）
        public const double PurlinArea = 720;             // 檩条截面积 Ap（mm²）
        public const double PurlinSpan = 9000;            // 檩条跨度 lp（mm）
        public const double PurlinI33 = 448600;           // 檩条惯性矩 Ip33（mm⁴）
        public const double PurlinDistToFlange = 200;      // 檩条形心到梁上翼缘距离 a（mm）
        public const double BraceArea = 840;               // 隅撑截面积 Ab（mm²）
        public const double BraceSpacing = 3000;           // 隅撑间距 lk（mm）
        public const double BraceAngle = 45;              // 隅撑与檩条夹角 α（°）

        // ===== 构造限值 =====
        public const double SeismicType = 0;               // 抗震类型（0=自定义, 1=多遇地震, 2=罕遇地震）
        public const double LambdaLimit = 180;            // 长细比限值 [λ]
        public const double FlangeLimit = 15;              // 翼缘宽厚比限值 [b/t]
        public const double WebLimit = 250;              // 腹板高厚比限值 [h0/tw]
        // ===== 抗震类型1（非抗震）限值 =====
        public const double Seismic1_LambdaLimit = 180;
        public const double Seismic1_FlangeLimit = 15;
        public const double Seismic1_WebLimit = 250;

        // ===== 抗震类型2（抗震）限值 =====
        public const double Seismic2_LambdaLimit = 150;
        public const double Seismic2_FlangeLimit = 13;
        public const double Seismic2_WebLimit = 160;

        public const double UtilLimit = 1.0;               // 应力比限值

        // ===== 通用验算系数（null=程序自己算）=====
        public const double GammaX = 1.05;                 // 塑性发展系数 γx

        // ===== 柱 =====
        public const double MuX = 1.0;                     // 平面内计算长度系数 μx
        public const double BetaMx = 1.0;                  // 等效弯矩系数 βmx
    }


    public class DesignCombos
    {
        public List<string> SelectedCombos = new();
    }

    // 读取模型时用户选中的 group（用于计算梁对柱转动约束 Kz）
    public class SelectedGroups
    {
        public List<string> Names = new();
    }


    public class MemberDisplayInfo
    {
        public double Util;
        public string Text = "";
    }
}
