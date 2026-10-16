namespace PortalFrame._2CheckResults
{
    // 单个测站的抗弯结果
    public class FlexureStationResult
    {
        public double Station;

        public ParaItem Util = new("抗弯利用率");

        // 有效截面
        public ParaItem Ae = new("有效截面面积 Ae", "mm²");
        public ParaItem WeTop = new("上边缘有效模量 We,top", "mm³");
        public ParaItem WeBottom = new("下边缘有效模量 We,bot", "mm³");
        public ParaItem AfTop = new("上翼缘面积 Af,top", "mm²");
        public ParaItem AfBot = new("下翼缘面积 Af,bot", "mm²");

        // 有效高度过程
        public ParaItem Sigma1 = new("腹板边缘大应力 σ1", "N/mm²");
        public ParaItem Sigma2 = new("腹板边缘小应力 σ2", "N/mm²");
        public ParaItem GammaR = new("钢材强度调整系数 γR");
        public ParaItem Hc = new("腹板受压区高度 hc", "mm");
        public ParaItem He1 = new("受压区有效高度 he1", "mm");
        public ParaItem He2 = new("受压区有效高度 he2", "mm");
        public ParaItem Rho = new("有效宽度系数 ρ");
        public ParaItem Beta = new("应力梯度 β");
        public ParaItem LambdaP = new("正应力屈曲系数 λp");
        public ParaItem KSigma = new("正应力屈曲系数 kσ");

        // 抗弯验算结果
        public bool VLess05Vd;
        public ParaItem Stress = new("组合应力 σ", "N/mm²");
        public ParaItem Me_N = new("考虑轴力等效弯矩 Me^N", "N·mm");
        public ParaItem Mf_N = new("考虑轴力翼缘弯矩 Mf^N", "N·mm");
        public ParaItem Me = new("允许弯矩 Me", "N·mm");
    }

    // 单个测站的抗剪结果
    public class ShearStationResult
    {
        public double Station;

        public ParaItem Util = new("抗剪利用率");

        public bool HasStiffener;
        public ParaItem a = new("加劲肋间距 a", "mm");

        public ParaItem Vd = new("抗剪承载力 Vd", "N");
        public ParaItem ShearStress = new("剪应力 τ", "N/mm²");
        public ParaItem LambdaS = new("受剪板件参数 λs");
        public ParaItem KTau = new("受剪屈曲系数 kτ");
        public ParaItem Omega1 = new("中间参数 ω1");
        public ParaItem Etas = new("中间参数 ηs");
        public ParaItem ChiTap = new("楔率折减系数 χtap");
        public ParaItem PhiPs = new("抗剪折减系数 φps");
        public ParaItem GammaP = new("腹板区格楔率 γp");
        public ParaItem Alpha = new("区格长宽比 α");
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
                return Combos.Values.Max(l => l.Max(s => s.Util.Value));
            }
        }

        // 最不利组合名
        public string WorstCombo
        {
            get
            {
                if (Combos.Count == 0) return "";
                return Combos.OrderByDescending(kv => kv.Value.Max(s => s.Util.Value)).First().Key;
            }
        }

        // 最不利测站
        public FlexureStationResult? WorstStation
        {
            get
            {
                if (Combos.Count == 0) return null;
                var worstCombo = Combos.OrderByDescending(kv => kv.Value.Max(s => s.Util.Value)).First();
                return worstCombo.Value.OrderByDescending(s => s.Util.Value).First();
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
                return Combos.Values.Max(l => l.Max(s => s.Util.Value));
            }
        }

        // 加这两个
        public string WorstCombo
        {
            get
            {
                if (Combos.Count == 0) return "";
                return Combos.OrderByDescending(kv => kv.Value.Max(s => s.Util.Value)).First().Key;
            }
        }

        public ShearStationResult? WorstStation
        {
            get
            {
                if (Combos.Count == 0) return null;
                var worstCombo = Combos.OrderByDescending(kv => kv.Value.Max(s => s.Util.Value)).First();
                return worstCombo.Value.OrderByDescending(s => s.Util.Value).First();
            }
        }
    }
}
