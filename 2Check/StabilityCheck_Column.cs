using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    /// <summary>
    /// 7.1.3 变截面柱平面内稳定验算
    /// </summary>
    public static class StabilityCheck_Column
    {
        public static void ColumnInPlaneCheck(MemberData member,List<ForceData> forces,SlendernessResult slenderness,StabilityResult result)
        {
            string combo = forces[0].Combo;

            // 1. 取大小端
            var (secBig, fBig, secSmall, fSmall) = GetEndData(member, forces);

            double N1 = fBig.P;
            double M1 = Math.Abs(fBig.M3);

            // 2. 大端毛截面（稳定验算用毛截面）
            double A1 = secBig.Area;
            double We1 = Math.Min(secBig.S33_Top, secBig.S33_Bot);

            // 3. 长细比（直接用外面算好的）
            double lambda1 = slenderness.Lambda3;   // 平面内长细比
            double fy = member.Material.GetDesignValues(secBig.Tw).fy;
            double E = MaterialData.E;

            // 4. 通用长细比 λn
            double lambdaN = lambda1 / Math.PI * Math.Sqrt(fy / E);

            // 5. φx（b类截面）
            double phiX = Phi(lambdaN);

            // 6. ηt
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

            // 7. Ncr
            double Ncr = Math.PI * Math.PI * E * A1 / (lambda1 * lambda1);

            // 8. 利用率
            double f = member.Material.GetDesignValues(secBig.TopTf).f;
            double betaMx = 1.0;  // 有侧移刚架

            double util = N1 / (etaT * phiX * A1)
                        + betaMx * M1 / ((1 - N1 / Ncr) * We1);
            util = Math.Abs(util) / f;

            result.Combos[combo] = util;
        }

        public static void ColumnOutPlaneCheck(MemberData member,List<ForceData> forces,StabilityResult result)
        {

        }

        // ===== φx 计算（b类截面）====="
        private static double Phi(double lambdaN)
        {
            if (lambdaN <= 0.215)
                return 1 - 0.65 * lambdaN * lambdaN;

            double inner = 0.965 + 0.3 * lambdaN + lambdaN * lambdaN;
            return (inner - Math.Sqrt(inner * inner - 4 * lambdaN * lambdaN))
                 / (2 * lambdaN * lambdaN);
        }


        // 取大小端的截面和内力
        private static (HSectionData secBig, ForceData forceBig,HSectionData secSmall, ForceData forceSmall)
            GetEndData(MemberData member, List<ForceData> forces)
        {
            double tol = 1.0;
            var fI = forces.First(f => f.Station < tol);
            var fJ = forces.Last(f => Math.Abs(f.Station - member.Frame.Length) < tol);

            var secI = member.StationSections[fI.Station];
            var secJ = member.StationSections[fJ.Station];

            bool iIsBig = secI.H >= secJ.H;
            var secBig = iIsBig ? secI : secJ;
            var fBig = iIsBig ? fI : fJ;
            var secSmall = iIsBig ? secJ : secI;
            var fSmall = iIsBig ? fJ : fI;

            return (secBig, fBig, secSmall, fSmall);
        }

    }
}
