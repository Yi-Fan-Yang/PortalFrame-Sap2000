using CSiAPIv1;

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
        public WorstResult Worst = new();

        // 与组合无关的结果
        public SlendernessResult Slenderness = new();      // 长细比
        public LocalStabilityResult LocalStability = new(); // 局部稳定（宽厚比）

        // 与组合有关的验算项
        public FlexureResult FlexureStrength = new();      // 压弯/拉弯
        public ShearResult ShearStrength = new();         // 抗剪
        public StabilityResult InPlaneStability = new();    // 平面内稳定
        public StabilityResult OutPlaneStability = new();    // 平面外稳定

        
        public void GetWorstResult()
        {
            Worst.MaxUtil = 0;
            Worst.BestCombo = "";
            Worst.CheckType = "";
            Worst.BestStation = null;

            // 1. 抗弯
            foreach (var (combo, list) in FlexureStrength.Combos)
            {
                foreach (var s in list)
                {
                    if (s.Util > Worst.MaxUtil)
                    {
                        Worst.MaxUtil = s.Util;
                        Worst.BestCombo = combo;
                        Worst.CheckType = "压弯/拉弯";
                        Worst.BestStation = s.Station;
                    }
                }
            }

            // 2. 抗剪
            foreach (var (combo, list) in ShearStrength.Combos)
            {
                foreach (var s in list)
                {
                    if (s.Util > Worst.MaxUtil)
                    {
                        Worst.MaxUtil = s.Util;
                        Worst.BestCombo = combo;
                        Worst.CheckType = "抗剪";
                        Worst.BestStation = s.Station;
                    }
                }
            }

            // 3. 平面内稳定
            foreach (var (combo, util) in InPlaneStability.Combos)
            {
                if (util > Worst.MaxUtil)
                {
                    Worst.MaxUtil = util;
                    Worst.BestCombo = combo;
                    Worst.CheckType = "平面内稳定";
                    Worst.BestStation = null;
                }
            }

            // 4. 平面外稳定
            foreach (var (combo, util) in OutPlaneStability.Combos)
            {
                if (util > Worst.MaxUtil)
                {
                    Worst.MaxUtil = util;
                    Worst.BestCombo = combo;
                    Worst.CheckType = "平面外稳定";
                    Worst.BestStation = null;
                }
            }

            // 5. 长细比（没有组合）
            double slendMax = Math.Max(Slenderness.Util3, Slenderness.Util2);
            if (slendMax > Worst.MaxUtil)
            {
                Worst.MaxUtil = slendMax;
                Worst.BestCombo = "";
                Worst.CheckType = "长细比";
                Worst.BestStation = null;
            }

            // 6. 宽厚比（没有组合）
            if (LocalStability.MaxUtil > Worst.MaxUtil)
            {
                Worst.MaxUtil = LocalStability.MaxUtil;
                Worst.BestCombo = "";
                Worst.CheckType = "局部稳定";
                Worst.BestStation = null;
            }

        }

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
        public WorstResult Worst = new();



        // 与组合无关的结果
        public SlendernessResult Slenderness = new();      // 长细比
        public LocalStabilityResult LocalStability = new(); // 局部稳定（宽厚比）

        // 与组合有关的验算项
        public FlexureResult FlexureStrength = new();      // 压弯/拉弯
        public ShearResult ShearStrength = new();         // 抗剪
        public StabilityResult LateralStability = new();    // 整体稳定
        public void GetWorstResult()
        {
            Worst.MaxUtil = 0;
            Worst.BestCombo = "";
            Worst.CheckType = "";
            Worst.BestStation = null;

            // 1. 抗弯
            foreach (var (combo, list) in FlexureStrength.Combos)
            {
                foreach (var s in list)
                {
                    if (s.Util > Worst.MaxUtil)
                    {
                        Worst.MaxUtil = s.Util;
                        Worst.BestCombo = combo;
                        Worst.CheckType = "压弯/拉弯";
                        Worst.BestStation = s.Station;
                    }
                }
            }

            // 2. 抗剪
            foreach (var (combo, list) in ShearStrength.Combos)
            {
                foreach (var s in list)
                {
                    if (s.Util > Worst.MaxUtil)
                    {
                        Worst.MaxUtil = s.Util;
                        Worst.BestCombo = combo;
                        Worst.CheckType = "抗剪";
                        Worst.BestStation = s.Station;
                    }
                }
            }

            // 3. 平面内稳定
            foreach (var (combo, util) in LateralStability.Combos)
            {
                if (util > Worst.MaxUtil)
                {
                    Worst.MaxUtil = util;
                    Worst.BestCombo = combo;
                    Worst.CheckType = "平面内稳定";
                    Worst.BestStation = null;
                }
            }

            // 5. 长细比（没有组合）
            double slendMax = Math.Max(Slenderness.Util3, Slenderness.Util2);
            if (slendMax > Worst.MaxUtil)
            {
                Worst.MaxUtil = slendMax;
                Worst.BestCombo = "";
                Worst.CheckType = "长细比";
                Worst.BestStation = null;
            }

            // 6. 宽厚比（没有组合）
            if (LocalStability.MaxUtil > Worst.MaxUtil)
            {
                Worst.MaxUtil = LocalStability.MaxUtil;
                Worst.BestCombo = "";
                Worst.CheckType = "局部稳定";
                Worst.BestStation = null;
            }

        }

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
        public double MaxUtil => Math.Max(Util3, Util2);
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

        public double MaxUtil
        {
            get
            {
                double flangeMax = Math.Max(FlangeUtil_Top, FlangeUtil_Bot);
                return Math.Max(flangeMax, WebUtil);
            }
        }
    }

    // =====================最不利结果========================
    public class WorstResult
    {
        public double MaxUtil;           // 最大利用率
        public string BestCombo = "";    // 最不利组合
        public string CheckType = "";    // 最不利验算类型
        public double? BestStation;      // 最不利测站（强度项才有）


    }








}
