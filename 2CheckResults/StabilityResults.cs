namespace PortalFrame._2CheckResults
{
    public class BeamLateralStabilityResult
    {
        public Dictionary<string, double> Combos = new();
        public Dictionary<string, PhiBDetails> PhiBDetails = new();
        public double MaxUtil => Combos.Count == 0 ? 0 : Combos.Values.Max();
        public string WorstCombo => Combos.Count == 0 ? "" : Combos.OrderByDescending(kv => kv.Value).First().Key;

    }

    public class ColumnOutPlaneStabilityResult
    {
        public Dictionary<string, double> Combos = new();
        public Dictionary<string, PhiBDetails> PhiBDetails = new();
        public double MaxUtil => Combos.Count == 0 ? 0 : Combos.Values.Max();
        public string WorstCombo => Combos.Count == 0 ? "" : Combos.OrderByDescending(kv => kv.Value).First().Key;

        public ParaItem EtaY = new("楔率折减系数ηy");
    }

    public class ColumnInPlaneStabilityResult
    {
        public Dictionary<string, double> Combos = new();
        public double MaxUtil => Combos.Count == 0 ? 0 : Combos.Values.Max();
        public string WorstCombo => Combos.Count == 0 ? "" : Combos.OrderByDescending(kv => kv.Value).First().Key;
        public ParaItem EtaT = new("楔率折减系数ηt");
        public ParaItem BetaMx = new("等效弯矩系数βmx");
    }

    public class PhiBDetails
    {
        // 内力
        public ParaItem M0 = new("小端弯矩M0", "N·mm");
        public ParaItem M1 = new("大端弯矩M1", "N·mm");
        public ParaItem KM = new("弯矩比kM");
        public ParaItem Wx0 = new("小端受压模量Wx0", "mm³");
        public ParaItem Wx1 = new("大端受压模量Wx1", "mm³");
        public ParaItem KSigma = new("小端应力比kσ");

        // 翼缘惯性矩
        public ParaItem IyT = new("受压翼缘弱轴惯性矩", "mm⁴");
        public ParaItem IyB = new("受拉翼缘弱轴惯性矩", "mm⁴");
        public ParaItem Iy = new("弱轴总惯性矩", "mm⁴");
        public ParaItem EtaI = new("惯性矩比ηi");

        // 等效参数
        public ParaItem Gamma = new("楔率γ");
        public ParaItem Eta = new("楔率相关系数η");
        public ParaItem C1 = new("等效弯矩系数C1");
        public ParaItem BetaXEta = new("截面不对称系数βxη");
        public ParaItem Iw0 = new("小端翘曲惯性矩", "mm⁶");
        public ParaItem IwEta = new("等效翘曲惯性矩", "mm⁶");
        public ParaItem JEta = new("等效扭转常数", "mm⁴");

        // 临界弯矩→φb
        public ParaItem Mcr = new("临界弯矩Mcr", "N·mm");
        public ParaItem LambdaB0 = new("起始通用长细比λb0");
        public ParaItem n = new("指数n");
        public ParaItem LambdaB = new("通用长细比λb");
        public ParaItem GammaX = new("塑性发展系数γx");
        public ParaItem PhiB = new("整体稳定系数φb");

        // 有隅撑时
        public ParaItem HasBrace = new("是否设隅撑");
        public ParaItem BetaXBrace = new("有隅撑βx");
        public ParaItem Beta = new("连接点距主梁距离比β");
        public ParaItem kb = new("檩条刚度", "N/mm");
        public ParaItem Alpha = new("隅撑与檩条夹角", "°");
        public ParaItem H = new("大端上下翼缘中心距离", "mm");
        public ParaItem A = new("檩条形心到梁上翼缘距离", "mm");
        public ParaItem Lp = new("檩条跨度", "mm");
        public ParaItem Ip = new("檩条强轴惯性矩", "mm⁴");
        public ParaItem Ap = new("檩条截面积", "mm²");
        public ParaItem Ak = new("隅撑杆截面积", "mm²");
        public ParaItem Lk = new("隅撑杆长度", "mm");
        public ParaItem Lkk = new("隅撑间距", "mm");
        public ParaItem E = new("隅撑下支撑点到檩条形心距离", "mm");
        public ParaItem E1 = new("梁剪切中心到檩条形心距离", "mm");
        public ParaItem I1 = new("被支撑翼缘弱轴惯性矩", "mm⁴");
        public ParaItem I2 = new("连接檩条翼缘弱轴惯性矩", "mm⁴");
    }
}
