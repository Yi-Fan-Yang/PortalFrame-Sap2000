using PortalFrame._1Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._2CheckResults
{
    public class BeamCheckResult : CheckResultBase
    {
        // 梁专属的整体稳定
        public BeamLateralStabilityResult LateralStability = new();

        public override void GetWorstResult()
        {
            base.GetWorstResult();
            WorstResultScanner.ScanCombo(Worst, "整体稳定", LateralStability.Combos);
        }

        public override void InitCombos(List<string> combos)
        {
            base.InitCombos(combos);
            foreach (var combo in combos)
            {
                LateralStability.Combos[combo] = 0;
                LateralStability.PhiBDetails[combo] = new PhiBDetails();
            }
        }

    }
}
