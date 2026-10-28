using PortalFrame._0Sap;

namespace PortalFrame._2CheckResults
{
    /// <summary>
    /// 验算类型名字（全局统一管理，避免到处写对不上）
    /// </summary>
    public static class CheckTypes
    {
        // 强度
        public const string Flexure = "压弯/拉弯";
        public const string Shear = "抗剪";

        // 稳定
        public const string ColumnInPlane = "柱面内稳定";
        public const string ColumnOutPlane = "柱面外稳定";
        public const string BeamLateral = "梁整体稳定";
        public const string PhiB = "整体稳定系数";


        // 静态
        public const string Slenderness = "长细比";
        public const string LocalStability = "局部稳定";
    }
    // =====================通用参数=====================
    public class GeneralPara
    {
        public ParaItem Gamma0 = new("结构重要性系数 γ0");
        public ParaItem MMR = new("弯矩放大系数 MMR");

        public ParaItem SeismicType = new("抗震类型");
        public ParaItem LambdaLimit = new("长细比限值 [λ]");
        public ParaItem FlangeLimit = new("翼缘宽厚比限值 [b/t]");
        public ParaItem WebLimit = new("腹板高厚比限值 [h0/tw]");
        public ParaItem UtilLimit = new("应力比限值");
    }

    // =====================长细比=====================
    public class SlendernessResult
    {
        //======================计算长度======================
        [Display(Group = Groups.Slenderness.CalcLength, Order = 1)]
        public ParaItem Kz = new("梁对柱转动约束刚度 Kz", "N·mm");

        [Display(Group = Groups.Slenderness.CalcLength, Order = 2)]
        public ParaItem Icl = new("柱线刚度 icl", "N·mm²");

        [Display(Group = Groups.Slenderness.CalcLength, Order = 3)]
        public ParaItem K = new("线刚度比 K");

        [Display(Group = Groups.Slenderness.CalcLength, Order = 4)]
        public ParaItem MuX = new("平面内计算长度系数 μx");

        [Display(Group = Groups.Slenderness.CalcLength, Order = 5)]
        public ParaItem Ly = new("平面外计算长度 Ly", "mm");

        //======================长细比与通用长细比======================
        [Display(Group = Groups.Slenderness.Lambda, Order = 1)]
        public ParaItem LambdaX = new("强轴长细比 λx");

        [Display(Group = Groups.Slenderness.Lambda, Order = 2)]
        public ParaItem LambdaY = new("弱轴长细比 λy");

        [Display(Group = Groups.Slenderness.Lambda, Order = 3)]
        public ParaItem LambdaBarX = new("强轴通用长细比 λ̄x");

        [Display(Group = Groups.Slenderness.Lambda, Order = 4)]
        public ParaItem LambdaBarY = new("弱轴通用长细比 λ̄y");

        //======================轴压稳定系数与欧拉临界力======================
        [Display(Group = Groups.Slenderness.Stability, Order = 3)]  
        public ParaItem PhiX = new("强轴稳定系数 φx");

        [Display(Group = Groups.Slenderness.Stability, Order = 3)]
        public ParaItem PhiY = new("弱轴稳定系数 φy");

        [Display(Group = Groups.Slenderness.Stability, Order = 3)]
        public ParaItem NcrX = new("强轴欧拉临界力 Ncrx", "N");

        [Display(Group = Groups.Slenderness.Stability, Order = 3)]
        public ParaItem NcrY = new("弱轴欧拉临界力 Ncry", "N");

        //======================长细比利用率======================
        [Display(Group = Groups.Slenderness.Util, Order = 1)]
        public ParaItem UtilX = new("强轴长细比利用率");
        [Display(Group = Groups.Slenderness.Util, Order = 2)]
        public ParaItem UtilY = new("弱轴长细比利用率");

        public double MaxUtil => Math.Max(UtilX.Value, UtilY.Value);
    }

    // =====================局部稳定（宽厚比）结果=====================
    public class LocalStabilityResult
    {
        //======================数值======================
        [Display(Group = Groups.LocalStability.Value, Order = 1)]
        public ParaItem FlangeWidthThicknessRatio_Top = new("上翼缘宽厚比 b/t");

        [Display(Group = Groups.LocalStability.Value, Order = 2)]
        public ParaItem FlangeWidthThicknessRatio_Bot = new("下翼缘宽厚比 b/t");

        [Display(Group = Groups.LocalStability.Value, Order = 3)]
        public ParaItem WebHightThicknessRatio = new("腹板高厚比 h0/tw");

        //======================利用率======================
        [Display(Group = Groups.LocalStability.Util, Order = 1)]
        public ParaItem FlangeUtil_Top = new("上翼缘宽厚比利用率");

        [Display(Group = Groups.LocalStability.Util, Order = 2)]
        public ParaItem FlangeUtil_Bot = new("下翼缘宽厚比利用率");

        [Display(Group = Groups.LocalStability.Util, Order = 3)]
        public ParaItem WebUtil = new("腹板高厚比利用率");

        public double MaxUtil
        {
            get
            {
                double flangeMax = Math.Max(FlangeUtil_Top.Value, FlangeUtil_Bot.Value);
                return Math.Max(flangeMax, WebUtil.Value);
            }
        }
    }

    // =====================设计内力=====================
    public class DesignForce
    {
        public double N;
        public double V2;
        public double M3;
        public double V3;
        public double M2;
    }

    // =====================最不利结果=====================
    public class WorstResult
    {
        public double MaxUtil;
        public string BestCombo = "";
        public string CheckType = "";
        public double? Station;
    }


}
