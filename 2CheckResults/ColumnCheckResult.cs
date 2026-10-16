using PortalFrame._1Model;

namespace PortalFrame._2CheckResults
{
    public class ColumnCheckResult : CheckResultBase
    {
        // 柱专属的稳定验算
        public ColumnInPlaneStabilityResult InPlaneStability = new();
        public ColumnOutPlaneStabilityResult OutPlaneStability = new();

        public override void GetWorstResult()
        {
            base.GetWorstResult();   // 先算通用的
            WorstResultScanner.ScanCombo(Worst, "平面内稳定", InPlaneStability.Combos);
            WorstResultScanner.ScanCombo(Worst, "平面外稳定", OutPlaneStability.Combos);
        }

        public override void InitCombos(List<string> combos)
        {
            base.InitCombos(combos);
            foreach (var combo in combos)
            {
                InPlaneStability.Combos[combo] = 0;
                OutPlaneStability.Combos[combo] = 0;
                OutPlaneStability.PhiBDetails[combo] = new PhiBDetails();
            }
        }
    }
}
