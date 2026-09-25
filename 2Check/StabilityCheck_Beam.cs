using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    /// <summary>
    /// 7.1.4 变截面梁整体稳定
    /// 只验算大端，中间参数需要小端数据
    /// </summary>
    public static class StabilityCheck_Beam
    {
        public static void OverallStabilityCheck(MemberData member,List<ForceData> forces,StabilityResult result, bool hasBrace)
        {
            // ===== 第1步：取两个端点的截面和内力 =====
            double tol = 1.0;
            var fI = forces.First(f => f.Station < tol);
            var fJ = forces.Last(f => Math.Abs(f.Station - member.Frame.Length) < tol);

            var secI = member.StationSections[fI.Station];
            var secJ = member.StationSections[fJ.Station];

            // 判断大端小端
            bool iIsBig = secI.H >= secJ.H;
            var secBig = iIsBig ? secI : secJ;
            var secSmall = iIsBig ? secJ : secI;
            var fBig = iIsBig ? fI : fJ;
            var fSmall = iIsBig ? fJ : fI;

            // ===== 第2步：取大小端弯矩和截面模量 =====
            double M1 = Math.Abs(fBig.M3);
            double M0 = Math.Abs(fSmall.M3);
            double W1 = Math.Min(secBig.S33_Top,secBig.S33_Bot);
            double W0 = Math.Min(secSmall.S33_Top, secSmall.S33_Bot);

            // ===== 第3步：kσ（应力比）=====
            double sigma0 = M0 / W0;
            double sigma1 = M1 / W1;
            double kSigma = sigma0 / sigma1;

            // ===== 第4步：kM（弯矩比）=====
            double kM = M0 / M1;

            // ===== 第5步：楔率γ和指数n =====
            double h1 = secBig.H - secBig.TopTf / 2.0 - secBig.BotTf / 2.0;
            double h0 = secSmall.H - secSmall.TopTf / 2.0 - secSmall.BotTf / 2.0;
            double gamma = (h1 - h0) / h0;
            double n = 1.51 * Math.Pow(h1 / h0, 1.0 / 3.0);

            // ===== 第6步：Mcr =====
            double fy = member.Material.GetDesignValues(secBig.Tw).fy;
            double f = member.Material.GetDesignValues(secBig.TopTf).f;

            double Mcr = hasBrace
                ? CalcMcrWithBrace(member, secBig, secSmall, gamma, kM)
                : CalcMcrNoBrace(member, secBig, secSmall, gamma, kM);

            // ===== 第7步：λb =====
            double gammaX = 1.05;
            double lambdaB = Math.Sqrt(gammaX * W1 * fy / Mcr);

            // ===== 第8步：φb =====
            double lambdaB0 = (0.55 - 0.25 * kSigma) / Math.Pow(1 + gamma, n);
            double phiB = 1.0 / Math.Pow(
                1 - Math.Pow(lambdaB0, 2 * n) + Math.Pow(lambdaB, 2 * n),
                1.0 / n);

            // ===== 第9步：利用率 =====
            double util = M1 / (gammaX * phiB * W1) / f;

        }

        // 下面三个方法后面填
        private static double CalcMcrNoBrace(MemberData m, HSectionData big, HSectionData small, double gamma, double kM) { return 0; }
        private static double CalcMcrWithBrace(MemberData m, HSectionData big, HSectionData small, double gamma, double kM) { return 0; }
    }
}
