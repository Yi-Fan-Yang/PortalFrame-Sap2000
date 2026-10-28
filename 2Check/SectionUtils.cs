using CSiAPIv1;
using PortalFrame._1Model;
using PortalFrame._5data;
using PortalFrame._2CheckResults;

namespace PortalFrame._2Check
{
    public static class SectionUtils
    {
        /// <summary>
        /// 板件宽厚比验算（翼缘+腹板）,等截面算一次，变截面算大小头
        /// </summary>
        public static void LocalStabilityCheck(MemberData member)
        {
            // 1. 读结果类
            var result = member.CheckResult.LocalStability;

            // 收集要验算的截面
            var sectionsToCheck = new List<HSectionData>();

            if (member.Section is TaperedSectionData tapered)
            {
                sectionsToCheck.Add(tapered.Start);
                sectionsToCheck.Add(tapered.End);
            }
            else if (member.Section is HSectionData sec)
            {
                sectionsToCheck.Add(sec);
            }

            double maxBfUtilTop = 0, maxBfUtilBot = 0, maxHwUtil = 0;
            double bfRatioTop = 0, bfRatioBot = 0, hwRatio = 0;

            foreach (var sec in sectionsToCheck)
            {
                double fy = member.Material.GetDesignValues(sec.Tw).fy;

                // 上翼缘
                double bfTop = (sec.TopB - sec.Tw - 2 * sec.Fillet) / 2.0;
                double bRatioTop = bfTop / sec.TopTf;
                double bfLimit =member.CheckResult.General.FlangeLimit.Value * Math.Sqrt(235.0 / fy);
                double bfUtilTop = bRatioTop / bfLimit;

                // 下翼缘
                double bfBot = (sec.BotB - sec.Tw - 2 * sec.Fillet) / 2.0;
                double bRatioBot = bfBot / sec.BotTf;
                double bfUtilBot = bRatioBot / bfLimit;

                // 腹板
                double hw = sec.H - sec.TopTf - sec.BotTf;
                double hwRatioTmp = hw / sec.Tw;
                double hwLimit = member.CheckResult.General.WebLimit.Value * Math.Sqrt(235.0 / fy);
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

            // 写回结果（用 SetCal）
            result.FlangeWidthThicknessRatio_Top.SetCal(bfRatioTop);
            result.FlangeWidthThicknessRatio_Bot.SetCal(bfRatioBot);
            result.WebHightThicknessRatio.SetCal(hwRatio);
            result.FlangeUtil_Top.SetCal(maxBfUtilTop);
            result.FlangeUtil_Bot.SetCal(maxBfUtilBot);
            result.WebUtil.SetCal(maxHwUtil);
        }



        /// <summary>
        // 计算腹板有效宽度 he（GB51022-2015 第7.1.1条）
        /// </summary>
        public static void EffectiveSection(MemberData member, string comboName)
        {
            var designDict = member.DesignForces[comboName];
            var flexDict = member.CheckResult.FlexureStrength.Collection[comboName];

            foreach (var (station, force) in designDict)
            {
                var flexResult = flexDict[station];
                var sec = member.StationSections[station];

                double tw = sec.Tw;
                double fy = member.Material.GetDesignValues(tw).fy;
                double N = force.N;
                double M = force.M3;
                double hw = sec.H - sec.TopTf - sec.BotTf;

                // ===== 第1步：上下边缘应力 =====
                double edgeTop = N / sec.Area + M / sec.S33_Top;
                double edgeBot = N / sec.Area - M / sec.S33_Bot;

                double sigma1, sigma2;
                if (Math.Abs(edgeTop) >= Math.Abs(edgeBot))
                { sigma1 = edgeTop; sigma2 = edgeBot; }
                else
                { sigma1 = edgeBot; sigma2 = edgeTop; }

                // 写回 σ1、σ2
                flexResult.Sigma1.SetCal(sigma1);
                flexResult.Sigma2.SetCal(sigma2);

                // 翼缘面积
                double AfTop = sec.TopB * sec.TopTf;
                double AfBot = sec.BotB * sec.BotTf;
                flexResult.AfTop.SetCal(AfTop);
                flexResult.AfBot.SetCal(AfBot);

                // ===== 第2步：四种情况 =====
                double topWebHeight, bottomWebHeight;
                double beta = 0, kSigma = 0, cs = 0, gammaR = 0, lambdaP = 0, rho = 0, hc = 0, he1 = 0, he2 = 0;

                if (edgeTop >= 0 && edgeBot >= 0)
                {
                    // 全拉
                    topWebHeight = hw;
                    bottomWebHeight = 0;
                    hc = 0;
                    he1 = 0;
                    he2 = 0;
                    rho = 1.0;
                }
                else if (edgeTop <= 0 && edgeBot <= 0)
                {
                    // 全压
                    beta = sigma2 / sigma1;
                    double b2 = beta * beta;
                    kSigma = 16.0 / (Math.Sqrt((1 + b2) * (1 + b2) + 0.112 * (1 - b2) * (1 - b2)) + (1 + b2));
                    cs = -sigma1;
                    gammaR = (fy <= 355) ? 1.1 : 1.0;
                    double fEff = (cs < fy) ? gammaR * cs : fy;
                    lambdaP = (hw / tw) / (28.1 * Math.Sqrt(kSigma * Math.Sqrt(235.0 / fEff)));
                    rho = 1.0 / Math.Pow(0.243 + Math.Pow(lambdaP, 1.25), 0.9);
                    if (rho > 1.0) rho = 1.0;
                    double he = rho * hw;

                    he1 = 2 * he / (5 - beta);
                    he2 = he - he1;
                    hc = hw;

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
                    bool compressionTop = (edgeTop < 0);

                    if (compressionTop)
                        hc = hw * (-edgeTop) / (edgeBot - edgeTop);
                    else
                        hc = hw * (-edgeBot) / (edgeTop - edgeBot);

                    beta = sigma2 / sigma1;
                    double b2 = beta * beta;
                    kSigma = 16.0 / (Math.Sqrt((1 + b2) * (1 + b2) + 0.112 * (1 - b2) * (1 - b2)) + (1 + b2));
                    cs = compressionTop ? -edgeTop : -edgeBot;
                    gammaR = (fy <= 355) ? 1.1 : 1.0;
                    double fEff = (cs < fy) ? gammaR * cs : fy;
                    lambdaP = (hw / tw) / (28.1 * Math.Sqrt(kSigma * Math.Sqrt(235.0 / fEff)));
                    rho = 1.0 / Math.Pow(0.243 + Math.Pow(lambdaP, 1.25), 0.9);
                    if (rho > 1.0) rho = 1.0;
                    double he = rho * hc;

                    he1 = 0.4 * he;
                    he2 = 0.6 * he;
                    double tensionHeight = hw - hc;

                    if (compressionTop)
                    {
                        topWebHeight = he1;
                        bottomWebHeight = he2 + tensionHeight;
                    }
                    else
                    {
                        topWebHeight = tensionHeight + he2;
                        bottomWebHeight = he1;
                    }
                }

                // 写回中间过程值
                flexResult.Beta.SetCal(beta);
                flexResult.KSigma.SetCal(kSigma);
                flexResult.GammaR.SetCal(gammaR);
                flexResult.LambdaP.SetCal(lambdaP);
                flexResult.Rho.SetCal(rho);
                flexResult.Hc.SetCal(hc);
                flexResult.He1.SetCal(he1);
                flexResult.He2.SetCal(he2);

                // ===== 第3步：四块面积和形心 =====
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

                // ===== 第5步：新惯性矩 =====
                double iEff = a1 * (y1 - yc) * (y1 - yc)
                             + a2 * (y2 - yc) * (y2 - yc) + tw * topWebHeight * topWebHeight * topWebHeight / 12.0
                             + a3 * (y3 - yc) * (y3 - yc) + tw * bottomWebHeight * bottomWebHeight * bottomWebHeight / 12.0
                             + a4 * (y4 - yc) * (y4 - yc);

                // ===== 第6步：写回 Ae、We =====
                flexResult.Ae.SetCal(ae);
                flexResult.WeTop.SetCal(iEff / yc);
                flexResult.WeBottom.SetCal(iEff / (sec.H - yc));
            }
        }



    }
}
