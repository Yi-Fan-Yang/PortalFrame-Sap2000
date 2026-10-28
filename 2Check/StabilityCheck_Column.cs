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
            var result = member.CheckResult;
            var sl = member.CheckResult.Slenderness;
            // 读取数据
            var forces = member.DesignForces[comboName];
            var secBig = member.StationSections[member.EndStations[1]];
            var secSmall = member.StationSections[member.EndStations[0]];
            var fBig = forces[member.EndStations[1]];
            var fSmall = forces[member.EndStations[0]];
            double N1 = fBig.N;
            double M1 = Math.Abs(fBig.M3);
            double A1 = secBig.Area;
            double W1 = Math.Min(secBig.S33_Top, secBig.S33_Bot);
            double lambda1 = sl.LambdaX.Value;
            double lambdaN = sl.LambdaBarX.Value;
            double phiX = sl.PhiX.Value;
            double NcrX = sl.NcrX.Value;
            double betaMx = result.InPlaneStability.Collection[comboName].BetaMx.Value;
            double f = member.Material.GetDesignValues(secBig.TopTf).f;

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
            double NUtil = Math.Abs(N1 / (etaT * phiX * A1 * f));
            double MUtil = Math.Abs(betaMx * M1 / ((1-N1/NcrX) *W1* f));
            double util = NUtil + MUtil;

            // 写回结果
            result.InPlaneStability.Collection[comboName].N1.SetCal(N1);
            result.InPlaneStability.Collection[comboName].M1.SetCal(M1);
            result.InPlaneStability.Collection[comboName].A1.SetCal(A1);
            result.InPlaneStability.Collection[comboName].W1.SetCal(W1);
            result.InPlaneStability.Collection[comboName].f.SetCal(f);
            result.InPlaneStability.Collection[comboName].PhiX.SetCal(phiX);
            result.InPlaneStability.Collection[comboName].EtaT.SetCal(etaT);
            result.InPlaneStability.Collection[comboName].NcrX.SetCal(NcrX);
            result.InPlaneStability.Collection[comboName].NUtil.SetCal(NUtil);
            result.InPlaneStability.Collection[comboName].MUtil.SetCal(MUtil);
            result.InPlaneStability.Collection[comboName].Util.SetCal(util);
        }

        public static void ColumnOutPlaneCheck(MemberData member, string comboName)
        {
            var result = member.CheckResult;
            var sl = member.CheckResult.Slenderness;

            // 1. 读数据
            var forces = member.DesignForces[comboName];
            var secBig = member.StationSections[member.EndStations[1]];
            var secSmall = member.StationSections[member.EndStations[0]];
            var fBig = forces[member.EndStations[1]];
            var fSmall = forces[member.EndStations[0]];

            // 2. 先算 φb（结果存 PhiBDetails）
            var detail = result.PhiBResult.Collection[comboName];
            PhiBCalculator.CalcPhiB(member, comboName);

            // 3. 从 SlendernessResult 读
            double lambdaBarY = sl.LambdaBarY.Value;   // λ̄1y
            double phiY = sl.PhiY.Value;

            // 4. 从 PhiBDetails 读
            double phiB = detail.PhiB.Value;
            double kSigma = detail.KSigma.Value;
            double gammaX = detail.GammaX.Value;
            double M1 = detail.M1.Value;
            double W1 = detail.Wx1.Value;

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

            double NUtil = Math.Abs(N1 / (etaY * phiY * A1 * f));
            double MUtil = Math.Abs(Math.Pow(M1 / (phiB * gammaX * W1 * f), 1.3 - 0.3 * kSigma));
            double util = NUtil + MUtil;

            // 7. 写回结果
            result.OutPlaneStability.Collection[comboName].N1.SetCal(N1);
            result.OutPlaneStability.Collection[comboName].M1.SetCal(M1);
            result.OutPlaneStability.Collection[comboName].A1.SetCal(A1);
            result.OutPlaneStability.Collection[comboName].W1.SetCal(W1);
            result.OutPlaneStability.Collection[comboName].f.SetCal(f);
            result.OutPlaneStability.Collection[comboName].EtaY.SetCal(etaY);
            result.OutPlaneStability.Collection[comboName].PhiY.SetCal(phiY);
            result.OutPlaneStability.Collection[comboName].PhiB.SetCal(phiB);
            result.OutPlaneStability.Collection[comboName].GammaX.SetCal(gammaX);
            result.OutPlaneStability.Collection[comboName].Ksigma.SetCal(kSigma);
            result.OutPlaneStability.Collection[comboName].NUtil.SetCal(NUtil);
            result.OutPlaneStability.Collection[comboName].MUtil.SetCal(MUtil);
            result.OutPlaneStability.Collection[comboName].Util.SetCal(util);
        }
    }
}
