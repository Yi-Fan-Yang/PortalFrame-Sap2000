using PortalFrame._1Model;

namespace PortalFrame._2CheckResults
{
    public class CheckResultBase
    {
        public GeneralPara General = new();
        // 与组合无关的结果
        public SlendernessResult Slenderness = new();
        public LocalStabilityResult LocalStability = new();

        // 与组合有关的强度验算
        public FlexureResult FlexureStrength = new();
        public ShearResult ShearStrength = new();

        // 最不利结果
        public WorstResult Worst = new();

        // 通用部分（柱梁都算这些）
        public virtual void GetWorstResult()
        {
            Worst.MaxUtil = 0;
            Worst.BestCombo = "";
            Worst.CheckType = "";
            Worst.BestStation = null;

            WorstResultScanner.ScanFlexure(Worst, "压弯/拉弯", FlexureStrength.Combos);
            WorstResultScanner.ScanShear(Worst, "抗剪", ShearStrength.Combos);
            WorstResultScanner.ScanStatic(Worst, "长细比", Slenderness.MaxUtil);
            WorstResultScanner.ScanStatic(Worst, "局部稳定", LocalStability.MaxUtil);
        }
        public virtual void InitCombos(List<string> combos)
        {
            foreach (var combo in combos)
            {
                FlexureStrength.Combos[combo] = new List<FlexureStationResult>();
                ShearStrength.Combos[combo] = new List<ShearStationResult>();
            }
        }
    }
}
