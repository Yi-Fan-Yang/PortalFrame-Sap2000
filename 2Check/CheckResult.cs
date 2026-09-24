namespace PortalFrame._2Check
{
    // 单个验算项的结果
    public class CheckItem
    {
        public double Util;
        public Dictionary<string, double> Values = new();
    }

    // 截面主要几何参数
    public class SectionInfo
    {
        public double H;        // 截面高度
        public double B;        // 翼缘宽度
        public double Tw;       // 腹板厚度
        public double Tf;       // 翼缘厚度
        public double Area;     // 面积
        public double I33;      // 强轴惯性矩
        public double S33;      // 强轴截面模量
        public double R33;      // 强轴回转半径
        public double I22;      // 弱轴惯性矩
        public double S22;      // 弱轴截面模量
        public double R22;      // 弱轴回转半径
    }

    // 构件基本信息（柱和梁共用）
    public abstract class MemberCheckResultBase
    {
        public string MemberName = "";
        public string MemberType = "";       // "柱" 或 "梁"
        public string SectionName = "";
        public string MaterialName = "";

        // 几何
        public double Length;                 // 构件长度 mm
        public bool IsTapered;                // 是否变截面

        // 截面参数
        public SectionInfo SecBig = new();    // 大端（等截面=SecSmall）
        public SectionInfo SecSmall = new();  // 小端

        // 所有验算项的汇总：名称 + 利用率
        public List<(string name, double util)> Summary = new();
        public double MaxUtil => Summary.Count > 0 ? Summary.Max(s => s.util) : 0;
        public bool AllPass => MaxUtil <= 1.0;
        //算完所有验算项后，调用这个方法刷新汇总
        public abstract void BuildSummary();
    }

    // ===== 柱的验算结果 =====
    public class ColumnCheckResult : MemberCheckResultBase
    {
        public List<StrengthCheckResult> Strength = new();          // 7.1.2 强度
        public CheckItem InPlaneStability = new();  // 7.1.3 平面内稳定
        public CheckItem OutPlaneStability = new();  // 7.1.5 平面外稳定
        public CheckItem Slenderness = new();       // 长细比

        public override void BuildSummary()
        {
            Summary.Clear();
            double maxStrength = Strength.Count > 0 ? Strength.Max(s => s.Util) : 0;
            Summary.Add(("强度", maxStrength));
            Summary.Add(("平面内稳定", InPlaneStability.Util));
            Summary.Add(("平面外稳定", OutPlaneStability.Util));
            Summary.Add(("长细比", Slenderness.Util));
        }
    }

    // ===== 梁的验算结果 =====
    public class BeamCheckResult : MemberCheckResultBase
    {
        public List<StrengthCheckResult> Strength = new();      // 7.1.2 强度
        public CheckItem LateralStability = new();  // 7.1.4 整体稳定
        public CheckItem Slenderness = new();       // 长细比

        public override void BuildSummary()
        {
            Summary.Clear();
            double maxStrength = Strength.Count > 0 ? Strength.Max(s => s.Util) : 0;
            Summary.Add(("强度", maxStrength));
            Summary.Add(("整体稳定", LateralStability.Util));
            Summary.Add(("长细比", Slenderness.Util));
        }
    }

    public class StrengthCheckResult
    {
        // 利用率放第一行
        public double Util;

        // 基本信息
        public double Station;      // 测站位置

        // 内力
        public string Combo = null!;
        public double N;             // 轴力
        public double V2;             // 剪力
        public double M3;             // 剪力
        public double V3;             // 剪力
        public double M2;             // 弯矩

        // 截面
        public double Ae;            // 有效面积
        public double We;            // 有效模量

        // 抗剪
        public double Vd;            // 抗剪承载力
        public bool VLess05Vd;       // V ≤ 0.5Vd？

        // 情况1（V≤0.5Vd）
        public double Stress;        // 组合应力

        // 情况2（V>0.5Vd）
        public double Me_N;          // Me^N
        public double Mf_N;          // Mf^N
        public double MAllow;        // 允许弯矩
    }
}
