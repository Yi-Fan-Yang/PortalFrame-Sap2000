using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    public static class SectionUtils
    {
        // 宽厚比限值检查，返回利用率
        public static (double bfUtil, double hwUtil) CheckWidthThickness(HSectionData sec, MaterialData mat)
        {
            double fy = mat.Fy;
            double k = Math.Sqrt(235.0 / fy);

            // --- 翼缘宽厚比（上下翼缘取大值）---
            double bTop = (sec.TopB - sec.Tw) / 2.0;
            double bBot = (sec.BotB - sec.Tw) / 2.0;
            double bfActual = Math.Max(bTop / sec.TopTf, bBot / sec.BotTf);
            double bfLimit = 15.0 * k;
            double bfUtil = bfActual / bfLimit;

            // --- 腹板高厚比 ---
            double h0 = sec.H - sec.TopTf - sec.BotTf;
            double hwActual = h0 / sec.Tw;
            double hwLimit = 250.0 * k;
            double hwUtil = hwActual / hwLimit;

            return (bfUtil, hwUtil);
        }

        /// <summary>
        /// 计算腹板有效宽度 he（GB51022-2015 第7.1.1条）
        /// </summary>
        /// <param name="sec">测站截面（毛截面，插值后的）</param>
        /// <param name="N">轴力（压力为正，拉力为负）</param>
        /// <param name="M">弯矩绝对值</param>
        /// <param name="fy">钢材屈服强度</param>
        /// <returns>Ae有效面积, WeTop上边缘有效模量, WeBottom下边缘有效模量</returns>
        public static (double Ae, double WeTop, double WeBottom) EffectiveSection(HSectionData sec, double N, double M, double fy)
        {
            {
                double hw = sec.H - sec.TopTf - sec.BotTf;
                double tw = sec.Tw;
                double TopB = sec.TopB;
                double BotB = sec.BotB;

                // ===== 第1步：两个边缘应力（正拉负压）=====
                double edgeA = N / sec.Area + M / sec.S33;
                double edgeB = N / sec.Area - M / sec.S33;

                double sigma1, sigma2;
                if (Math.Abs(edgeA) >= Math.Abs(edgeB))
                { sigma1 = edgeA; sigma2 = edgeB; }
                else
                { sigma1 = edgeB; sigma2 = edgeA; }

                // 第2步：4种情况 → 顶部腹板有效高度 he1_top，底部腹板有效高度 he2_bottom
                // 我们统一用"从顶部算"的有效腹板段高度
                double topWebHeight, bottomWebHeight;

                if (sigma1 >= 0 && sigma2 >= 0)
                {
                    // 全拉：全腹板有效
                    topWebHeight = hw;
                    bottomWebHeight = 0;
                }
                else if (sigma1 <= 0 && sigma2 <= 0)
                {
                    // 全压
                    double hc = hw;
                    double beta = sigma2 / sigma1;
                    double b2 = beta * beta;
                    double kSigma = 16.0 / (Math.Sqrt((1 + b2) * (1 + b2) + 0.112 * (1 - b2) * (1 - b2)) + (1 + b2));
                    double cs = -sigma1;
                    double gammaR = (fy <= 355) ? 1.1 : 1.0;
                    double fEff = (cs < fy) ? gammaR * cs : fy;
                    double lambdaP = (hw / tw) / (28.1 * Math.Sqrt(kSigma * Math.Sqrt(235.0 / fEff)));
                    double rho = 1.0 / Math.Pow(0.243 + Math.Pow(lambdaP, 1.25), 0.9);
                    if (rho > 1.0) rho = 1.0;
                    double he = rho * hc;

                    double he1 = 2 * he / (5 - beta);   // 靠近 σ1
                    double he2 = he - he1;               // 靠近 σ2
                                                         // 全压时 σ1 是压边（绝对值大），假设 σ1 在上边
                    topWebHeight = he1;
                    bottomWebHeight = he2;
                }
                else
                {
                    // 一边拉一边压
                    bool compressionTop = (sigma1 < 0 && sigma2 > 0);  // σ1在上边压
                    double hc = compressionTop
                        ? hw * (-sigma1) / (sigma2 - sigma1)
                        : hw * (-sigma2) / (sigma1 - sigma2);

                    double beta = sigma2 / sigma1;
                    double b2 = beta * beta;
                    double kSigma = 16.0 / (Math.Sqrt((1 + b2) * (1 + b2) + 0.112 * (1 - b2) * (1 - b2)) + (1 + b2));
                    double cs = compressionTop ? -sigma1 : -sigma2;
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
                        // 上边压：顶部 he1 有效，底部 he2+受拉区 有效
                        topWebHeight = he1;
                        bottomWebHeight = he2 + tensionHeight;
                    }
                    else
                    {
                        // 下边压：顶部 受拉区+he2 有效，底部 he1 有效
                        topWebHeight = tensionHeight + he2;
                        bottomWebHeight = he1;
                    }
                }

                // ===== 第3步：四块面积和形心（从顶部算）=====
                double a1 = TopB * sec.TopTf;
                double y1 = sec.TopTf / 2.0;

                double a2 = tw * topWebHeight;
                double y2 = sec.TopTf + topWebHeight / 2.0;

                double a3 = tw * bottomWebHeight;
                double y3 = sec.TopTf + hw - bottomWebHeight / 2.0;

                double a4 = BotB * sec.BotTf;
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
                double weTop = iEff / yc;
                double weBottom = iEff / (sec.H - yc);

                return (ae, weTop, weBottom);
            }
        }


        // 腹板受剪承载力 Vd
        public static double ShearCapacity(HSectionData sec, MaterialData mat, double stiffenerSpacing)
        { /* TODO */ return 0; }
    }
}
