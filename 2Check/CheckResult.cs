namespace PortalFrame._2Check
{

    // 截面主要几何参数
    public class SectionInfo
    {
        public double H;        // 截面高度
        public double B;        // 翼缘宽度
        public double Tw;       // 腹板厚度
        public double Tf;       // 翼缘厚度
        public double Area;     // 面积
        public double I33;      // 强轴惯性矩
        public double S33_Top;      // 强轴截面模量1
        public double S33_Bot;      // 强轴截面模量2
        public double R33;      // 强轴回转半径
        public double I22;      // 弱轴惯性矩
        public double S22;      // 弱轴截面模量
        public double R22;      // 弱轴回转半径
    }


    // ===== 柱的验算结果 =====
    public class ColumnCheckResult
    {
        // 基本信息
        public string MemberName = "";
        public string SectionName = "";
        public string MaterialName = "";
        public double Length;
        public bool IsTapered;
        public SectionInfo SecBig = new();
        public SectionInfo SecSmall = new();

        // 与组合无关的结果
        public SlendernessResult Slenderness = new();      // 长细比
        public LocalStabilityResult LocalStability = new(); // 局部稳定（宽厚比）

        // 与组合有关的验算项
        public FlexureResult FlexureStrength = new();      // 压弯/拉弯
        public ShearResult ShearStrength = new();         // 抗剪
        public StabilityResult InPlaneStability = new();    // 平面内稳定
        public StabilityResult OutPlaneStability = new();    // 平面外稳定
    }

    // ===== 梁的验算结果 =====
    public class BeamCheckResult
    {
        // 基本信息
        public string MemberName = "";
        public string SectionName = "";
        public string MaterialName = "";
        public double Length;
        public bool IsTapered;
        public SectionInfo SecBig = new();
        public SectionInfo SecSmall = new();

        // 与组合无关的结果
        public SlendernessResult Slenderness = new();      // 长细比
        public LocalStabilityResult LocalStability = new(); // 局部稳定（宽厚比）

        // 与组合有关的验算项
        public FlexureResult FlexureStrength = new();      // 压弯/拉弯
        public ShearResult ShearStrength = new();         // 抗剪
        public StabilityResult LateralStability = new();    // 整体稳定
    }


    //=====================强度验算========================
    // 单个测站的抗弯结果
    public class FlexureStationResult
    {
        public double Station;
        public double Util;

        // 内力
        public double N, V2, M3, V3, M2;

        // 有效截面（EffectiveSection 填）
        public double Ae;
        public double WeTop;      // 新增
        public double WeBottom;   // 新增

        // 抗弯验算结果（FlexureCheck 填）
        public bool VLess05Vd;
        public double Stress;
        public double Me_N;
        public double Mf_N;
        public double MAllow;
    }

    // 单个测站的抗剪结果
    public class ShearStationResult
    {
        public double Station;
        public double Util;

        // 内力
        public double V2;

        // 抗剪
        public double Vd;
        public double ShearStress;
    }

    // 抗弯验算结果
    public class FlexureResult
    {
        public Dictionary<string, List<FlexureStationResult>> Combos = new();

        public double MaxUtil
        {
            get
            {
                if (Combos.Count == 0) return 0;
                return Combos.Values.Max(l => l.Max(s => s.Util));
            }
        }
    }

    // 抗剪验算结果
    public class ShearResult
    {
        public Dictionary<string, List<ShearStationResult>> Combos = new();

        public double MaxUtil
        {
            get
            {
                if (Combos.Count == 0) return 0;
                return Combos.Values.Max(l => l.Max(s => s.Util));
            }
        }
    }




    //=====================稳定验算========================
    // 稳定验算结果（每个组合只有一个值）
    public class StabilityResult
    {
        // key=组合名, value=利用率
        public Dictionary<string, double> Combos = new();
        public double MaxUtil
        {
            get
            {
                if (Combos.Count == 0) return 0;
                return Combos.Values.Max();
            }
        }
    }



    // =====================长细比结果========================
    public class SlendernessResult
    {
        public double Lambda3;       // 3轴长细比
        public double Lambda2;       // 2轴长细比
        public double Util3;          // 3轴利用率
        public double Util2;          // 2轴利用率
    }

    // =====================局部稳定（宽厚比）结果========================
    public class LocalStabilityResult
    {
        public double FlangeWidthThicknessRatio_Top;      // 上翼缘宽厚比
        public double FlangeWidthThicknessRatio_Bot;      // 下翼缘宽厚比
        public double WebHightThicknessRatio; // 腹板高厚比
        public double FlangeUtil_Top;     // 上翼缘宽厚比利用率
        public double FlangeUtil_Bot;     // 下翼缘宽厚比利用率
        public double WebUtil;        // 腹板高厚比利用率
    }


}
