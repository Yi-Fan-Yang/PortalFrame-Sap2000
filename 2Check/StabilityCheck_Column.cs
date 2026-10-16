using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._5data;

namespace PortalFrame._2Check
{
    /// <summary>
    /// 7.1.3 变截面柱平面内稳定验算
    /// </summary>
    public static class StabilityCheck_Column
    {

        public static void ColumnInPlaneCheck(MemberData member, string comboName)
        {
            var colResult = (ColumnCheckResult)member.CheckResult;
            var sl = member.CheckResult.Slenderness;
            // 读取数据
            var forces = member.DesignForces[comboName];
            var secBig = member.SecBig;
            var secSmall = member.SecSmall;
            var fBig = forces.First(f => Math.Abs(f.Station - secBig.Station) < 1.0);
            var fSmall = forces.First(f => Math.Abs(f.Station - secSmall.Station) < 1.0);
            double N1 = fBig.N;
            double M1 = Math.Abs(fBig.M3);
            double A1 = secBig.Area;
            double We1 = Math.Min(secBig.S33_Top, secBig.S33_Bot);
            double lambda1 = sl.LambdaX.Value;
            double lambdaN = sl.LambdaBarX.Value;
            double phiX = sl.PhiX.Value;
            double Ncr = sl.NcrX.Value;
            double betaMx = colResult.InPlaneStability.BetaMx.Value;

            // ηt（楔率折减系数）
            double etaT;
            if (lambdaN >= 1.2)
            {
                etaT = 1.0;
            }
            else
            {
                double A0 = secSmall.Area;
                double ratio = A0 / A1;
                etaT = ratio + (1 - ratio) * lambdaN * lambdaN / 1.44;
            }

            // 利用率
            double f = member.Material.GetDesignValues(secBig.TopTf).f;
            double util = N1 / (etaT * phiX * A1)
                        + betaMx * M1 / ((1 - N1 / Ncr) * We1);
            util = Math.Abs(util) / f;

            // 写回结果
            colResult.InPlaneStability.EtaT.SetCal(etaT);
            colResult.InPlaneStability.Combos[comboName] = util;
        }

        public static void ColumnOutPlaneCheck(MemberData member, string comboName)
        {
            var colResult = (ColumnCheckResult)member.CheckResult;
            var sl = member.CheckResult.Slenderness;

            // 1. 读数据
            var forces = member.DesignForces[comboName];
            var secBig = member.SecBig;
            var secSmall = member.SecSmall;
            var fBig = forces.First(f => Math.Abs(f.Station - secBig.Station) < 1.0);

            // 2. 先算 φb（结果存 PhiBDetails）
            var detail = colResult.OutPlaneStability.PhiBDetails[comboName];
            PhiBCalculator.CalcPhiB(member, comboName);

            // 3. 从 SlendernessResult 读
            double lambdaBarY = sl.LambdaBarY.Value;   // λ̄1y
            double phiY = sl.PhiY.Value;

            // 4. 从 PhiBDetails 读
            double phiB = detail.PhiB.Value;
            double kSigma = detail.KSigma.Value;
            double gammaX = detail.GammaX.Value;
            double M1 = detail.M1.Value;
            double Wx1 = detail.Wx1.Value;

            // 5. 算 ηty
            double A1 = secBig.Area;
            double A0 = secSmall.Area;
            double etaY;
            if (lambdaBarY >= 1.3)
            {
                etaY = 1.0;
            }
            else
            {
                double ratio = A0 / A1;
                etaY = ratio + (1 - ratio) * lambdaBarY * lambdaBarY / 1.69;
            }

            // 6. 算利用率
            double N1 = fBig.N;
            double f = member.Material.GetDesignValues(secBig.TopTf).f;

            double term1 = N1 / (etaY * phiY * A1 * f);
            double term2 = M1 / (phiB * gammaX * Wx1 * f);
            double util = term1 + Math.Pow(term2, 1.3 - 0.3 * kSigma);

            // 7. 写回结果
            colResult.OutPlaneStability.EtaY.SetCal(etaY);
            colResult.OutPlaneStability.Combos[comboName] = util;
        }
    }
}
