using PortalFrame._0Sap;
using System.Linq;

namespace PortalFrame._2CheckResults
{
    //=====================稳定性验算结果========================
    public class BeamLateralStabilityResult
    {
        public Dictionary<string, BeamLateralStabilitySingleResult> Collection = new();
        public double MaxUtil => Collection.Values.Where(s => s.Util.HasValue).Select(s => s.Util.Value).DefaultIfEmpty(0).Max();
        public string WorstCombo => Collection.FirstOrDefault(kv => kv.Value.Util.HasValue && kv.Value.Util.Value == MaxUtil).Key ?? "";
    }
    public class ColumnOutPlaneStabilityResult
    {
        public Dictionary<string, ColumnOutPlaneStabilitySingleResult> Collection = new();
        public double MaxUtil => Collection.Values.Where(s => s.Util.HasValue).Select(s => s.Util.Value).DefaultIfEmpty(0).Max();
        public string WorstCombo => Collection.FirstOrDefault(kv => kv.Value.Util.HasValue && kv.Value.Util.Value == MaxUtil).Key ?? "";
    }
    public class ColumnInPlaneStabilityResult
    {
        public Dictionary<string, ColumnInPlaneStabilitySingleResult> Collection = new();
        public double MaxUtil => Collection.Values.Where(s => s.Util.HasValue).Select(s => s.Util.Value).DefaultIfEmpty(0).Max();
        public string WorstCombo => Collection.FirstOrDefault(kv => kv.Value.Util.HasValue && kv.Value.Util.Value == MaxUtil).Key ?? "";
    }
    public class PhiBResult
    {
        public Dictionary<string, PhiBSingleResult> Collection = new();
        public double MinPhiB => Collection.Values.Where(s => s.PhiB.HasValue).Select(s => s.PhiB.Value).DefaultIfEmpty(0).Min();
        public string WorstCombo => Collection.FirstOrDefault(kv => kv.Value.PhiB.HasValue && kv.Value.PhiB.Value == MinPhiB).Key ?? "";
    }

    //=====================单个组合的稳定性验算结果========================
    public class BeamLateralStabilitySingleResult
    {
        //==========================设计内力==========================
        [Display(Group = Groups.BeamLateral.Force, Order = 1)]
        public ParaItem M1 = new("大端弯矩M1", "N·mm");


        //==========================截面属性==========================
        [Display(Group = Groups.BeamLateral.Section, Order = 1)]
        public ParaItem W1 = new("大端截面模量W1", "mm³");

        [Display(Group = Groups.BeamLateral.Section, Order = 2)]
        public ParaItem f = new("设计强度f", "N/mm²");

        //==========================主要参数==========================
        [Display(Group = Groups.BeamLateral.Main, Order = 1)]
        public ParaItem GammaX = new("塑性发展系数γx");

        [Display(Group = Groups.BeamLateral.Main, Order = 2)]
        public ParaItem PhiB = new("整体稳定系数φb");
        
        [Display(Group = Groups.BeamLateral.Main, Order = 3)]
        public ParaItem Util = new("利用率");

    }
    public class ColumnOutPlaneStabilitySingleResult
    {
        //==========================设计内力==========================
        [Display(Group = Groups.ColumnOutPlane.Force, Order = 1)]
        public ParaItem N1 = new("大端轴力N1", "N");

        [Display(Group = Groups.ColumnOutPlane.Force, Order = 2)]
        public ParaItem M1 = new("大端弯矩M1", "N·mm");


        //==========================截面属性==========================
        [Display(Group = Groups.ColumnOutPlane.Section, Order = 1)]
        public ParaItem A1 = new("大端截面面积A1", "mm²");

        [Display(Group = Groups.ColumnOutPlane.Section, Order = 2)]
        public ParaItem W1 = new("大端截面模量W1", "mm³");

        [Display(Group = Groups.ColumnOutPlane.Section, Order = 3)]
        public ParaItem f = new("设计强度f", "N/mm²");

        //==========================主要参数==========================
        [Display(Group = Groups.ColumnOutPlane.Main, Order = 1)]
        public ParaItem EtaY = new("楔率折减系数ηy");

        [Display(Group = Groups.ColumnOutPlane.Main, Order = 2)]
        public ParaItem PhiY = new("弱轴稳定系数 φy");

        [Display(Group = Groups.ColumnOutPlane.Main, Order = 3)]
        public ParaItem PhiB = new("整体稳定系数φb");

        [Display(Group = Groups.ColumnOutPlane.Main, Order = 4)]
        public ParaItem GammaX = new("塑性发展系数γx");

        [Display(Group = Groups.ColumnOutPlane.Main, Order = 5)]
        public ParaItem Ksigma = new("大小端应力比kσ");

        [Display(Group = Groups.ColumnOutPlane.Main, Order = 6)]
        public ParaItem NUtil = new("轴力利用率");

        [Display(Group = Groups.ColumnOutPlane.Main, Order = 7)]
        public ParaItem MUtil = new("弯曲利用率");

        [Display(Group = Groups.ColumnOutPlane.Main, Order = 8)]
        public ParaItem Util = new("利用率");

    }
    public class ColumnInPlaneStabilitySingleResult
    {
        //==========================设计内力==========================
        [Display(Group = Groups.ColumnInPlane.Force, Order = 1)]
        public ParaItem N1 = new ("大端轴力N1", "N");

        [Display(Group = Groups.ColumnInPlane.Force, Order = 2)]
        public ParaItem M1 = new ("大端弯矩M1", "N·mm");


        //==========================截面属性==========================
        [Display(Group = Groups.ColumnInPlane.Section, Order = 1)]
        public ParaItem A1= new("大端截面面积A1", "mm²");

        [Display(Group = Groups.ColumnInPlane.Section, Order = 2)]
        public ParaItem W1 = new("大端截面模量W1", "mm³");

        [Display(Group = Groups.ColumnInPlane.Section, Order = 3)]
        public ParaItem f = new("设计强度f", "N/mm²");


        //==========================主要参数==========================
        [Display(Group = Groups.ColumnInPlane.Main, Order = 1)]
        public ParaItem PhiX = new("强轴稳定系数 φx");

        [Display(Group = Groups.ColumnInPlane.Main, Order = 2)]
        public ParaItem EtaT = new("楔率折减系数ηt");

        [Display(Group = Groups.ColumnInPlane.Main, Order = 3)]
        public ParaItem NcrX = new("临界轴力NcrX", "N");

        [Display(Group = Groups.ColumnInPlane.Main, Order = 4)]
        public ParaItem BetaMx = new("等效弯矩系数βmx");

        [Display(Group = Groups.ColumnInPlane.Main, Order = 5)]
        public ParaItem NUtil = new("轴力利用率");

        [Display(Group = Groups.ColumnInPlane.Main, Order = 6)]
        public ParaItem MUtil = new("弯曲利用率");

        [Display(Group = Groups.ColumnInPlane.Main, Order = 7)]
        public ParaItem Util = new("利用率");
    }
    public class PhiBSingleResult
    {
        //==========================设计内力==========================
        [Display(Group = Groups.PhiB.Forces, Order = 1)]
        public ParaItem M0 = new("小端弯矩M0", "N·mm");

        [Display(Group = Groups.PhiB.Forces, Order = 2)]
        public ParaItem M1 = new("大端弯矩M1", "N·mm");

        //==========================截面参数==========================
        [Display(Group = Groups.PhiB.Section, Order = 1)]
        public ParaItem Wx0 = new("小端受压模量Wx0", "mm³");

        [Display(Group = Groups.PhiB.Section, Order = 2)]
        public ParaItem Wx1 = new("大端受压模量Wx1", "mm³");

        [Display(Group = Groups.PhiB.Section, Order = 3)]
        public ParaItem IyT = new("受压翼缘弱轴惯性矩", "mm⁴");

        [Display(Group = Groups.PhiB.Section, Order = 4)]
        public ParaItem IyB = new("受拉翼缘弱轴惯性矩", "mm⁴");

        [Display(Group = Groups.PhiB.Section, Order = 5)]
        public ParaItem Iy = new("弱轴总惯性矩", "mm⁴");

        // =========================主要参数==========================
        [Display(Group = Groups.PhiB.Main, Order = 1)]
        public ParaItem PhiB = new("整体稳定系数φb");

        [Display(Group = Groups.PhiB.Main, Order = 2)]
        public ParaItem LambdaB0 = new("换算长细比λb0");

        [Display(Group = Groups.PhiB.Main, Order = 3)]
        public ParaItem n = new("指数n");

        [Display(Group = Groups.PhiB.Main, Order = 4)]
        public ParaItem KSigma = new("小端应力比kσ");

        [Display(Group = Groups.PhiB.Main, Order = 5)]
        public ParaItem LambdaB = new("通用长细比λb");

        [Display(Group = Groups.PhiB.Main, Order = 6)]
        public ParaItem HasBrace = new("是否设隅撑");

        [Display(Group = Groups.PhiB.Main, Order = 7)]
        public ParaItem Mcr = new("临界弯矩Mcr", "N·mm");

        [Display(Group = Groups.PhiB.Main, Order = 8)]
        public ParaItem KM = new("弯矩比kM");

        [Display(Group = Groups.PhiB.Main, Order = 9)]
        public ParaItem Gamma = new("楔率γ");

        [Display(Group = Groups.PhiB.Main, Order = 10)]
        public ParaItem GammaX = new("塑性发展系数γx");

        //==========================无隅撑参数==========================
        [Display(Group = Groups.PhiB.NoBracing, Order = 1)]
        public ParaItem C1 = new("等效弯矩系数C1");

        [Display(Group = Groups.PhiB.NoBracing, Order = 2)]
        public ParaItem BetaXEta = new("截面不对称系数βxη");

        [Display(Group = Groups.PhiB.NoBracing, Order = 3)]
        public ParaItem Eta = new("楔率相关系数η");

        [Display(Group = Groups.PhiB.NoBracing, Order = 4)]
        public ParaItem IwEta = new("等效翘曲惯性矩Iwη", "mm⁶");

        [Display(Group = Groups.PhiB.NoBracing, Order = 5)]
        public ParaItem Iw0 = new("小端翘曲惯性矩Iw0", "mm⁶");

        [Display(Group = Groups.PhiB.NoBracing, Order = 6)]
        public ParaItem JEta = new("等效扭转常数Jη", "mm⁴");

        [Display(Group = Groups.PhiB.NoBracing, Order = 7)]
        public ParaItem EtaI = new("惯性矩比ηi");

        // =============================有隅撑==========================
        [Display(Group = Groups.PhiB.Bracing, Order = 1)]
        public ParaItem kb = new("檩条刚度kb", "N/mm");

        [Display(Group = Groups.PhiB.Bracing, Order = 2)]
        public ParaItem J = new ("大端截面的自由扭转常数", "mm⁴");

        [Display(Group = Groups.PhiB.Bracing, Order = 3)]
        public ParaItem Iw = new ("大端截面翘曲惯性矩", "mm⁶");

        [Display(Group = Groups.PhiB.Bracing, Order = 4)]
        public ParaItem e1 = new("梁剪切中心到檩条形心距离e1", "mm");

        [Display(Group = Groups.PhiB.Bracing, Order = 5)]
        public ParaItem BetaX = new("过程参数 βx");

        [Display(Group = Groups.PhiB.Bracing, Order = 6)]
        public ParaItem Alpha = new("隅撑与檩条夹角α", "°");

        [Display(Group = Groups.PhiB.Bracing, Order = 7)]
        public ParaItem Beta = new("连接点距主梁距离比β");

        [Display(Group = Groups.PhiB.Bracing, Order = 8)]
        public ParaItem h = new("大端上下翼缘中心距离h", "mm");

        [Display(Group = Groups.PhiB.Bracing, Order = 9)]
        public ParaItem A = new("檩条形心到梁上翼缘距离A", "mm");

        [Display(Group = Groups.PhiB.Bracing, Order = 10)]
        public ParaItem Lp = new("檩条跨度Lp", "mm");

        [Display(Group = Groups.PhiB.Bracing, Order = 11)]
        public ParaItem Ip = new("檩条强轴惯性矩Ip", "mm⁴");

        [Display(Group = Groups.PhiB.Bracing, Order = 12)]
        public ParaItem Ap = new("檩条截面积Ap", "mm²");

        [Display(Group = Groups.PhiB.Bracing, Order = 13)]
        public ParaItem Ak = new("隅撑杆截面积Ak", "mm²");

        [Display(Group = Groups.PhiB.Bracing, Order = 14)]
        public ParaItem Lk = new("隅撑杆长度Lk", "mm");

        [Display(Group = Groups.PhiB.Bracing, Order = 15)]
        public ParaItem Lkk = new("隅撑间距Lkk", "mm");

        [Display(Group = Groups.PhiB.Bracing, Order = 16)]
        public ParaItem e = new("隅撑下支撑点到檩条形心距离e", "mm");

        [Display(Group = Groups.PhiB.Bracing, Order = 17)]
        public ParaItem I1 = new("被支撑翼缘弱轴惯性矩I1", "mm⁴");

        [Display(Group = Groups.PhiB.Bracing, Order = 18)]
        public ParaItem I2 = new("连接檩条翼缘弱轴惯性矩I2", "mm⁴");

    }
}
