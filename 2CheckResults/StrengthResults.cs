using PortalFrame._0Sap;

namespace PortalFrame._2CheckResults
{
    // 单个测站的抗弯结果
    public class FlexureSingleResult
    {
        //=====================内力 =====================
        [Display(Group = Groups.Flexure.Force, Order = 1)]
        public ParaItem N = new("轴力 N", "N");

        [Display(Group = Groups.Flexure.Force, Order = 2)]
        public ParaItem M2 = new("弯矩 M2", "N·mm");

        [Display(Group = Groups.Flexure.Force, Order = 3)]
        public ParaItem M3 = new("弯矩 M3", "N·mm");

        [Display(Group = Groups.Flexure.Force, Order = 4)]
        public ParaItem f = new("钢材设计强度 f", "N/mm²");

        //=====================原始截面=====================
        [Display(Group = Groups.Flexure.Section, Order = 1)]
        public ParaItem AfTop = new("上翼缘面积 Af,top", "mm²");

        [Display(Group = Groups.Flexure.Section, Order = 2)]
        public ParaItem AfBot = new("下翼缘面积 Af,bot", "mm²");

        //=====================有效截面=====================
        [Display(Group = Groups.Flexure.EffectiveSection, Order = 1)]
        public ParaItem Rho = new("有效宽度系数 ρ");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 2)]
        public ParaItem LambdaP = new("正应力屈曲系数 λp");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 3)]
        public ParaItem KSigma = new("正应力屈曲系数 kσ");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 4)]
        public ParaItem Beta = new("应力梯度 β");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 5)]
        public ParaItem Sigma1 = new("腹板边缘大应力 σ1", "N/mm²");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 6)]
        public ParaItem Sigma2 = new("腹板边缘小应力 σ2", "N/mm²");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 7)]
        public ParaItem GammaR = new("钢材强度调整系数 γR");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 8)]
        public ParaItem Hc = new("腹板受压区高度 hc", "mm");
        [Display(Group = Groups.Flexure.EffectiveSection, Order = 9)]
        public ParaItem He1 = new("受压区有效高度 he1", "mm");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 10)]
        public ParaItem He2 = new("受压区有效高度 he2", "mm");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 11)]
        public ParaItem Ae = new("有效截面面积 Ae", "mm²");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 12)]
        public ParaItem WeTop = new("上边缘有效模量 We,top", "mm³");

        [Display(Group = Groups.Flexure.EffectiveSection, Order = 13)]
        public ParaItem WeBottom = new("下边缘有效模量 We,bot", "mm³");

        // =====================抗弯验算=====================
        [Display(Group = Groups.Flexure.Main, Order = 1)]
        public ParaItem NUtil = new("轴力利用率");

        [Display(Group = Groups.Flexure.Main, Order = 2)]
        public ParaItem M2Util = new("弯矩 M2 利用率");

        [Display(Group = Groups.Flexure.Main, Order = 3)]
        public ParaItem M3Util = new("弯矩 M3 利用率");

        [Display(Group = Groups.Flexure.Main, Order = 4)]
        public ParaItem Me_N = new("考虑轴力等效弯矩 Me^N", "N·mm");

        [Display(Group = Groups.Flexure.Main, Order = 5)]
        public ParaItem Mf_N = new("考虑轴力翼缘弯矩 Mf^N", "N·mm");

        [Display(Group = Groups.Flexure.Main, Order = 6)]
        public ParaItem MAllow = new("允许弯矩 MAllow", "N·mm");

        [Display(Group = Groups.Flexure.Main, Order = 7)]
        public ParaItem Util = new("抗弯利用率");

    }

    // 单个测站的抗剪结果
    public class ShearSingleResult
    {
        // =====================内力 =====================
        [Display(Group = Groups.Shear.Force, Order = 1)]
        public ParaItem V2 = new("剪力 V2", "N");

        // =====================截面属性=====================
        [Display(Group = Groups.Shear.Section, Order = 1)]
        public ParaItem Hw1 = new("大端腹板高度 Hw1", "mm");

        [Display(Group = Groups.Shear.Section, Order = 2)]
        public ParaItem Hw0 = new("小端腹板高度 Hw0", "mm");

        [Display(Group = Groups.Shear.Section, Order = 3)]
        public ParaItem Tw = new("腹板厚度 Tw", "mm");

        [Display(Group = Groups.Shear.Section, Order = 4)]
        public ParaItem fv = new("腹板抗剪设计强度 fv", "N/mm²");

        //======================参数 λs=========================
        [Display(Group = Groups.Shear.LambdaS, Order = 1)]
        public ParaItem LambdaS = new("受剪板件参数 λs");

        [Display(Group = Groups.Shear.LambdaS, Order = 2)]
        public ParaItem KTau = new("受剪屈曲系数 kτ");

        [Display(Group = Groups.Shear.LambdaS, Order = 3)]
        public ParaItem Etas = new("中间参数 ηs");

        [Display(Group = Groups.Shear.LambdaS, Order = 4)]
        public ParaItem Omega1 = new("中间参数 ω1");

        // =====================抗剪验算=====================
        [Display(Group = Groups.Shear.Main, Order = 1)]
        public ParaItem PhiPs = new("抗剪折减系数 φps");

        [Display(Group = Groups.Shear.Main, Order = 2)]
        public ParaItem ChiTap = new("楔率折减系数 χtap");

        [Display(Group = Groups.Shear.Main, Order = 3)]
        public ParaItem GammaP = new("腹板区格楔率 γp");

        [Display(Group = Groups.Shear.Main, Order = 4)]
        public ParaItem Alpha = new("区格长宽比 α");

        [Display(Group = Groups.Shear.Main, Order = 5)]
        public ParaItem Vd1 = new("大端抗剪承载力 Vd1", "N");

        [Display(Group = Groups.Shear.Main, Order = 6)]
        public ParaItem Vd0 = new("小端抗剪承载力 Vd0", "N");

        [Display(Group = Groups.Shear.Main, Order = 7)]
        public ParaItem Util = new("抗剪利用率");

    }

    // 抗弯验算结果
    public class FlexureResult
    {
        public Dictionary<string, Dictionary<double, FlexureSingleResult>> Collection = new();
        public (double MaxUtil, string Combo, double Station) GetWorst()
        {
            if (Collection.Count == 0) return (0, "", 0);

            double maxUtil = -1;
            string worstCombo = "";
            double worstStation = 0;

            // 只遍历用户选中的组合
            foreach (var combo in Global.Data.Combos.SelectedCombos)
            {
                // 从字典里找这个组合，找不到就跳过
                if (!Collection.TryGetValue(combo, out var dict)) continue;

                foreach (var (station, s) in dict)
                {
                    if (!s.Util.HasValue) continue;   // 没有值就跳过

                    if (s.Util.Value > maxUtil)
                    {
                        maxUtil = s.Util.Value;
                        worstCombo = combo;
                        worstStation = station;
                    }
                }
            }

            return (maxUtil, worstCombo, worstStation);
        }
        // 算某个组合的最大利用率
        public double GetComboMaxUtil(string comboName)
        {
            if (!Collection.TryGetValue(comboName, out var dict)) return 0;

            // 找这个组合里有值的测站，取最大
            var validUtils = dict.Values.Where(s => s.Util.HasValue)
                                        .Select(s => s.Util.Value)
                                        .ToList();
            return validUtils.Count == 0 ? 0 : validUtils.Max();
        }

    }

    // 抗剪验算结果
    public class ShearResult
    {
        public Dictionary<string, Dictionary<double, ShearSingleResult>> Collection = new();
        public ParaItem HasStiffener = new("是否设置加劲肋");
        public ParaItem a = new("加劲肋间距 a", "mm");
        public (double MaxUtil, string Combo, double Station) GetWorst()
        {
            if (Collection.Count == 0) return (0, "", 0);

            double maxUtil = -1;
            string worstCombo = "";
            double worstStation = 0;

            // 只遍历用户选中的组合
            foreach (var combo in Global.Data.Combos.SelectedCombos)
            {
                // 从字典里找这个组合，找不到就跳过
                if (!Collection.TryGetValue(combo, out var dict)) continue;

                foreach (var (station, s) in dict)
                {
                    if (!s.Util.HasValue) continue;   // 没有值就跳过

                    if (s.Util.Value > maxUtil)
                    {
                        maxUtil = s.Util.Value;
                        worstCombo = combo;
                        worstStation = station;
                    }
                }
            }

            return (maxUtil, worstCombo, worstStation);
        }
        // 查某个组合的最大利用率
        public double GetComboMaxUtil(string comboName)
        {
            if (!Collection.TryGetValue(comboName, out var dict)) return 0;

            var validUtils = dict.Values.Where(s => s.Util.HasValue)
                                        .Select(s => s.Util.Value)
                                        .ToList();
            return validUtils.Count == 0 ? 0 : validUtils.Max();
        }

    }
}
