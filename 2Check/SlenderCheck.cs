using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._5data;

namespace PortalFrame._2Check
{
    public static class SlenderCheck
    {
        public static void Calculate(MemberData member)
        {
            // 从result里读参数
            var sl = member.CheckResult.Slenderness;
            var gen = member.CheckResult.General;

            double H = member.Frame.Length;    // 柱高
            var secBig = member.StationSections[member.EndStations[1]];
            var secSmall = member.StationSections[member.EndStations[0]];
            double fy = member.Material.GetDesignValues(secBig.Tw).fy;
            double E = MaterialData.E;
            double ly = sl.Ly.Value;
            double lambdaLimit = gen.LambdaLimit.Value;

            // 取回转半径
            double R33, R22;
            if (member.Section is TaperedSectionData)
            {
                R33 = secBig.R33;
                R22 = secBig.R22;
            }
            else if (member.Section is HSectionData sec)
            {
                R33 = sec.R33;
                R22 = sec.R22;
            }
            else
            {
                return;
            }

            if (member.Kz.HasValue)
            {
                sl.Kz.SetCal(member.Kz.Value);
                double Kz = sl.Kz.Value;
                double I1 = secBig.I33;
                double I0 = secSmall.I33;

                double icl = E * I1 / H;
                double K = (Kz / (6.0 * icl)) * Math.Pow(I1 / I0, 0.29);
                double muX = 2.0 * Math.Pow(I1 / I0, 0.145) * Math.Sqrt(1 + 0.38 / K);

                sl.Icl.SetCal(icl);
                sl.K.SetCal(K);
                sl.MuX.SetCal(muX);
            }

            // 长细比
            double lambdaX = sl.MuX.Value * H / R33;
            double lambdaY = sl.Ly.Value / R22;
            sl.LambdaX.SetCal(lambdaX);
            sl.LambdaY.SetCal(lambdaY);

            // 通用长细比
            double lambdaBarX = lambdaX / Math.PI * Math.Sqrt(fy / E);
            double lambdaBarY = lambdaY / Math.PI * Math.Sqrt(fy / E);
            sl.LambdaBarX.SetCal(lambdaBarX);
            sl.LambdaBarY.SetCal(lambdaBarY);

            // 轴压稳定系数（b类截面）
            sl.PhiX.SetCal(Phi(lambdaBarX));
            sl.PhiY.SetCal(Phi(lambdaBarY));

            // 欧拉临界力
            double A = secBig.Area;
            sl.NcrX.SetCal(Math.PI * Math.PI * E * A / (lambdaX * lambdaX));
            sl.NcrY.SetCal(Math.PI * Math.PI * E * A / (lambdaY * lambdaY));

            // 利用率
            sl.UtilX.SetCal((lambdaX) / lambdaLimit);
            sl.UtilY.SetCal((lambdaY) / lambdaLimit);
        }

        // b类截面稳定系数
        private static double Phi(double lambdaN)
        {
            if (lambdaN <= 0.215)
                return 1 - 0.65 * lambdaN * lambdaN;

            double inner = 0.965 + 0.3 * lambdaN + lambdaN * lambdaN;
            return (inner - Math.Sqrt(inner * inner - 4 * lambdaN * lambdaN))
                 / (2 * lambdaN * lambdaN);
        }

    }
}

