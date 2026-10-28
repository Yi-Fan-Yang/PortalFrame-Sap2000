using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._5data;

namespace PortalFrame._2Check
{
    /// <summary>
    /// 7.1.4 变截面梁整体稳定
    /// 只验算大端，中间参数需要小端数据
    /// </summary>
    public static class StabilityCheck_Beam
    {
        public static void OverallStabilityCheck(MemberData member, string comboName)
        {
            var result = member.CheckResult;

            // 1. 读当前组合的内力
            var forces = member.DesignForces[comboName];

            // 2. 先算 φb（结果存 PhiBDetails）
            var detail = result.PhiBResult.Collection[comboName];
            PhiBCalculator.CalcPhiB(member, comboName);

            // 3. 从 PhiBDetails 读
            double phiB = detail.PhiB.Value;
            double gammaX = detail.GammaX.Value;
            double M1 = detail.M1.Value;
            double Wx1 = detail.Wx1.Value;

            // 4. 算利用率（梁整体稳定只有 term2，没有轴力项）
            var secBig = member.StationSections[member.EndStations[1]];
            double f = member.Material.GetDesignValues(secBig.TopTf).f;
            double util = M1 / (gammaX * phiB * Wx1 * f);

            // 5. 写回结果
            result.LateralStability.Collection[comboName].M1.SetCal(M1);
            result.LateralStability.Collection[comboName].GammaX.SetCal(gammaX);
            result.LateralStability.Collection[comboName].PhiB.SetCal(phiB);
            result.LateralStability.Collection[comboName].W1.SetCal(Wx1);
            result.LateralStability.Collection[comboName].f.SetCal(f);
            result.LateralStability.Collection[comboName].Util.SetCal(util);
        }


    }
}
