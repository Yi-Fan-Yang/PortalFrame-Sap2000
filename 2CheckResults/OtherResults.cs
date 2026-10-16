namespace PortalFrame._2CheckResults
{
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

    // =====================长细比结果=====================
    public class SlendernessResult
    {
        public ParaItem LambdaX = new("强轴长细比 λx");
        public ParaItem LambdaY = new("弱轴长细比 λy");
        public ParaItem LambdaBarX = new("强轴通用长细比 λ̄x");
        public ParaItem LambdaBarY = new("弱轴通用长细比 λ̄y");

        public ParaItem PhiX = new("强轴稳定系数 φx");
        public ParaItem PhiY = new("弱轴稳定系数 φy");

        public ParaItem NcrX = new("强轴欧拉临界力 Ncrx", "N");
        public ParaItem NcrY = new("弱轴欧拉临界力 Ncry", "N");

        public ParaItem MuX = new("平面内计算长度系数 μx");
        public ParaItem Kz = new("梁对柱转动约束刚度 Kz", "N·mm");
        public ParaItem Icl = new("柱线刚度 icl", "N·mm²");
        public ParaItem K = new("线刚度比 K");

        public ParaItem Ly = new("平面外计算长度 Ly", "mm");

        public ParaItem UtilX = new("强轴长细比利用率");
        public ParaItem UtilY = new("弱轴长细比利用率");

        public double MaxUtil => Math.Max(UtilX.Value, UtilY.Value);
    }

    // =====================局部稳定（宽厚比）结果=====================
    public class LocalStabilityResult
    {
        public ParaItem FlangeWidthThicknessRatio_Top = new("上翼缘宽厚比 b/t");
        public ParaItem FlangeWidthThicknessRatio_Bot = new("下翼缘宽厚比 b/t");
        public ParaItem WebHightThicknessRatio = new("腹板高厚比 h0/tw");
        public ParaItem FlangeUtil_Top = new("上翼缘宽厚比利用率");
        public ParaItem FlangeUtil_Bot = new("下翼缘宽厚比利用率");
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
        public double Station;
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
        public double? BestStation;
    }

    // ============ 最不利结果扫描器 ============
    public static class WorstResultScanner
    {
        public static void ScanFlexure(WorstResult w, string checkType, Dictionary<string, List<FlexureStationResult>> combos)
        {
            foreach (var (combo, list) in combos)
                foreach (var s in list)
                    if ((s.Util.Value) > w.MaxUtil)
                    {
                        w.MaxUtil = s.Util.Value;
                        w.BestCombo = combo;
                        w.CheckType = checkType;
                        w.BestStation = s.Station;
                    }
        }

        public static void ScanShear(WorstResult w, string checkType, Dictionary<string, List<ShearStationResult>> combos)
        {
            foreach (var (combo, list) in combos)
                foreach (var s in list)
                    if ((s.Util.Value) > w.MaxUtil)
                    {
                        w.MaxUtil = s.Util.Value;
                        w.BestCombo = combo;
                        w.CheckType = checkType;
                        w.BestStation = s.Station;
                    }
        }

        public static void ScanCombo(WorstResult w, string checkType, Dictionary<string, double> combos)
        {
            foreach (var (combo, util) in combos)
                if (util > w.MaxUtil)
                {
                    w.MaxUtil = util;
                    w.BestCombo = combo;
                    w.CheckType = checkType;
                    w.BestStation = null;
                }
        }

        public static void ScanStatic(WorstResult w, string checkType, double util)
        {
            if (util > w.MaxUtil)
            {
                w.MaxUtil = util;
                w.BestCombo = "";
                w.CheckType = checkType;
                w.BestStation = null;
            }
        }
    }
}
