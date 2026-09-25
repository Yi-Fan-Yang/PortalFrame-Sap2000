using CSiAPIv1;
using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    public static class SectionUtils
    {
        /// <summary>
        /// 板件宽厚比验算（翼缘+腹板）,等截面算一次，变截面算大小头
        /// </summary>
        public static void LocalStabilityCheck(MemberData member, LocalStabilityResult result)
        {
            // 收集要验算的截面
            var sectionsToCheck = new List<(string label, HSectionData sec)>();

            if (member.Section is TaperedSectionData tapered)
            {
                sectionsToCheck.Add(("Start", tapered.Start));
                sectionsToCheck.Add(("End", tapered.End));
            }
            else if (member.Section is HSectionData sec)
            {
                sectionsToCheck.Add(("整体", sec));
            }

            double maxBfUtilTop = 0, maxBfUtilBot = 0, maxHwUtil = 0;
            double bfRatioTop = 0, bfRatioBot = 0, hwRatio = 0;

            foreach (var (label, sec) in sectionsToCheck)
            {
                double fy = member.Material.GetDesignValues(sec.Tw).fy;

                // 上翼缘
                double bfTop = (sec.TopB - sec.Tw - 2 * sec.Fillet) / 2.0;
                double bRatioTop = bfTop / sec.TopTf;
                double bfLimit = 15.0 * Math.Sqrt(235.0 / fy);
                double bfUtilTop = bRatioTop / bfLimit;

                // 下翼缘
                double bfBot = (sec.BotB - sec.Tw - 2 * sec.Fillet) / 2.0;
                double bRatioBot = bfBot / sec.BotTf;
                double bfUtilBot = bRatioBot / bfLimit;

                // 腹板
                double hw = sec.H - sec.TopTf - sec.BotTf;
                double hwRatioTmp = hw / sec.Tw;
                double hwLimit = 250.0 * Math.Sqrt(235.0 / fy);
                double hwUtil = hwRatioTmp / hwLimit;

                if (bfUtilTop > maxBfUtilTop)
                {
                    maxBfUtilTop = bfUtilTop;
                    bfRatioTop = bRatioTop;
                }
                if (bfUtilBot > maxBfUtilBot)
                {
                    maxBfUtilBot = bfUtilBot;
                    bfRatioBot = bRatioBot;
                }
                if (hwUtil > maxHwUtil)
                {
                    maxHwUtil = hwUtil;
                    hwRatio = hwRatioTmp;
                }
            }

            result.FlangeWidthThicknessRatio_Top = bfRatioTop;
            result.FlangeWidthThicknessRatio_Bot = bfRatioBot;
            result.WebHightThicknessRatio = hwRatio;
            result.FlangeUtil_Top = maxBfUtilTop;
            result.FlangeUtil_Bot = maxBfUtilBot;
            result.WebUtil = maxHwUtil;
        }


        /// <summary>
        /// 计算腹板有效宽度 he（GB51022-2015 第7.1.1条）
        /// </summary>
        /// <param name="sec">测站截面（毛截面，插值后的）</param>
        /// <param name="N">轴力（压力为正，拉力为负）</param>
        /// <param name="M">弯矩绝对值</param>
        /// <param name="fy">钢材屈服强度</param>
        /// <returns>Ae有效面积, WeTop上边缘有效模量, WeBottom下边缘有效模量</returns>
        public static void EffectiveSection(MemberData member, ForceData force, FlexureStationResult result)
        {
            var sec = member.StationSections[force.Station];
            double tw = sec.Tw;
            double fy = member.Material.GetDesignValues(tw).fy;
            double N = force.P;                    // 正拉负压
            double M = force.M3;
            double hw = sec.H - sec.TopTf - sec.BotTf;

            // ===== 第1步：上下边缘应力 =====
            double edgeTop = N / sec.Area + M / sec.S33_Top;   // 上边缘
            double edgeBot = N / sec.Area - M / sec.S33_Bot;   // 下边缘

            // sigma1 = 绝对值大的，sigma2 = 绝对值小的（仅用于kσ、λp计算）
            double sigma1, sigma2;
            if (Math.Abs(edgeTop) >= Math.Abs(edgeBot))
            { sigma1 = edgeTop; sigma2 = edgeBot; }
            else
            { sigma1 = edgeBot; sigma2 = edgeTop; }

            // ===== 第2步：四种情况 → topWebHeight / bottomWebHeight =====
            double topWebHeight, bottomWebHeight;

            if (edgeTop >= 0 && edgeBot >= 0)
            {
                // 全拉：整个腹板全有效
                topWebHeight = hw;
                bottomWebHeight = 0;
            }
            else if (edgeTop <= 0 && edgeBot <= 0)
            {
                // 全压
                double beta = sigma2 / sigma1;   // 都负，beta>0
                double b2 = beta * beta;
                double kSigma = 16.0 / (Math.Sqrt((1 + b2) * (1 + b2) + 0.112 * (1 - b2) * (1 - b2)) + (1 + b2));
                double cs = -sigma1;
                double gammaR = (fy <= 355) ? 1.1 : 1.0;
                double fEff = (cs < fy) ? gammaR * cs : fy;
                double lambdaP = (hw / tw) / (28.1 * Math.Sqrt(kSigma * Math.Sqrt(235.0 / fEff)));
                double rho = 1.0 / Math.Pow(0.243 + Math.Pow(lambdaP, 1.25), 0.9);
                if (rho > 1.0) rho = 1.0;
                double he = rho * hw;

                double he1 = 2 * he / (5 - beta);   // 靠近压应力大的一边
                double he2 = he - he1;

                // 压应力大的在上边还是下边？
                bool sigma1AtTop = Math.Abs(edgeTop) >= Math.Abs(edgeBot);
                if (sigma1AtTop)
                {
                    topWebHeight = he1;
                    bottomWebHeight = he2;
                }
                else
                {
                    topWebHeight = he2;
                    bottomWebHeight = he1;
                }
            }
            else
            {
                // 一边拉一边压
                bool compressionTop = (edgeTop < 0);   // 直接看上边缘

                // 压区高度 hc
                double hc;
                if (compressionTop)
                    hc = hw * (-edgeTop) / (edgeBot - edgeTop);   // 上压下拉
                else
                    hc = hw * (-edgeBot) / (edgeTop - edgeBot);   // 上拉下压

                double beta = sigma2 / sigma1;   // sigma1压(负), sigma2拉(正), beta<0
                double b2 = beta * beta;
                double kSigma = 16.0 / (Math.Sqrt((1 + b2) * (1 + b2) + 0.112 * (1 - b2) * (1 - b2)) + (1 + b2));
                double cs = compressionTop ? -edgeTop : -edgeBot;
                double gammaR = (fy <= 355) ? 1.1 : 1.0;
                double fEff = (cs < fy) ? gammaR * cs : fy;
                double lambdaP = (hw / tw) / (28.1 * Math.Sqrt(kSigma * Math.Sqrt(235.0 / fEff)));
                double rho = 1.0 / Math.Pow(0.243 + Math.Pow(lambdaP, 1.25), 0.9);
                if (rho > 1.0) rho = 1.0;
                double he = rho * hc;

                double he1 = 0.4 * he;   // 靠近压边
                double he2 = 0.6 * he;   // 靠近零点
                double tensionHeight = hw - hc;   // 受拉区全有效

                if (compressionTop)
                {
                    // 上边压：上边压边he1，下边零点he2+整个拉区
                    topWebHeight = he1;
                    bottomWebHeight = he2 + tensionHeight;
                }
                else
                {
                    // 下边压：上边整个拉区+零点he2，下边压边he1
                    topWebHeight = tensionHeight + he2;
                    bottomWebHeight = he1;
                }
            }

            // ===== 第3步：四块面积和形心（从顶部算）=====
            double a1 = sec.TopB * sec.TopTf;
            double y1 = sec.TopTf / 2.0;

            double a2 = tw * topWebHeight;
            double y2 = sec.TopTf + topWebHeight / 2.0;

            double a3 = tw * bottomWebHeight;
            double y3 = sec.TopTf + hw - bottomWebHeight / 2.0;

            double a4 = sec.BotB * sec.BotTf;
            double y4 = sec.TopTf + hw + sec.BotTf / 2.0;

            // ===== 第4步：新形心 =====
            double ae = a1 + a2 + a3 + a4;
            double yc = (a1 * y1 + a2 * y2 + a3 * y3 + a4 * y4) / ae;

            // ===== 第5步：新惯性矩（移轴公式）=====
            double iEff = a1 * (y1 - yc) * (y1 - yc)
                         + a2 * (y2 - yc) * (y2 - yc) + tw * topWebHeight * topWebHeight * topWebHeight / 12.0
                         + a3 * (y3 - yc) * (y3 - yc) + tw * bottomWebHeight * bottomWebHeight * bottomWebHeight / 12.0
                         + a4 * (y4 - yc) * (y4 - yc);

            // ===== 第6步：两个 We =====
            result.Ae = ae;
            result.WeTop = iEff / yc;
            result.WeBottom = iEff / (sec.H - yc);
        }

    }
}
