using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._5data;
using System.Security.AccessControl;

namespace PortalFrame._2Check
{
    /// <summary>
    /// 整体稳定系数 φb 计算（梁柱共用）
    /// GB51022-2015 第7.1.4条（无隅撑）+ 第7.1.6条（有隅撑，预留接口）
    /// </summary>
    public static class PhiBCalculator
    {
        /// <summary>
        /// 算φb全过程：取大小端 → 算Mcr → 算λb → 得φb
        /// </summary>
        public static void CalcPhiB(MemberData member, string comboName)
        {
            // 从 member.CheckResult 里找对应的 PhiBDetails
            PhiBDetails d = GetPhiBDetails(member, comboName);
            // 1. 读当前组合的内力
            var forces = member.DesignForces[comboName];

            // 2. 算Mcr
            if (d.HasBrace.Value == 1) CalcMcrWithBrace(member, comboName);// 7.1.6条有隅撑
            else CalcMcrNoBrace(member, comboName);// 7.1.4条无隅撑

            // 3. 材料强度
            double fy = member.Material.GetDesignValues(member.SecBig.Tw).fy;
            double gammaX = d.GammaX.Value;   // 从覆盖项读

            // 4. 指数n
            double n = 1.51 * Math.Pow((1 + d.Gamma.Value), 1.0 / 3.0);

            // 5. 通用长细比λb
            double lambdaB = Math.Sqrt(gammaX * d.Wx1.Value * fy / d.Mcr.Value);

            // 6. λb0
            double lambdaB0 = (0.55 - 0.25 * d.KSigma.Value) / Math.Pow(1 + d.Gamma.Value, n);

            // 7. φb
            double phiB = 1.0 / Math.Pow(
                1 - Math.Pow(lambdaB0, 2 * n) + Math.Pow(lambdaB, 2 * n),
                1.0 / n);

            // 8. 填结果（SetCal：程序算的）
            d.n.SetCal(n);
            d.LambdaB.SetCal(lambdaB);
            d.LambdaB0.SetCal(lambdaB0);
            d.PhiB.SetCal(phiB);
        }

        /// <summary>
        /// 无隅撑时弹性屈曲临界弯矩 Mcr（7.1.4条）
        /// </summary>
        private static void CalcMcrNoBrace(MemberData member, string comboName)
        {
            // 内部自己找 PhiBDetails
            PhiBDetails d = GetPhiBDetails(member, comboName);
            var forces = member.DesignForces[comboName];
            double E = MaterialData.E;
            double G = MaterialData.G;

            // 1. 取两端截面和内力（直接从 member 读大小端）
            var secBig = member.SecBig;
            var secSmall = member.SecSmall;
            var fBig = forces.First(f => Math.Abs(f.Station - secBig.Station) < 1.0);
            var fSmall = forces.First(f => Math.Abs(f.Station - secSmall.Station) < 1.0);

            // 2. 两端弯矩
            double M1 = Math.Abs(fBig.M3);
            double M0 = Math.Abs(fSmall.M3);

            // 3. 受压边缘模量
            bool topCompressed = fBig.M3 >= 0;
            double Wx1 = topCompressed ? secBig.S33_Top : secBig.S33_Bot;
            double Wx0 = topCompressed ? secSmall.S33_Top : secSmall.S33_Bot;

            // 4. kM、kSigma
            double kM = M0 / M1;
            double kSigma = (M0 / Wx0) / (M1 / Wx1);

            // 5. 楔率
            double h1 = secBig.H0;
            double h0 = secSmall.H0;
            double gamma = (h1 - h0) / h0;

            // 6. 两个翼缘绕弱轴惯性矩
            double IyTop = secBig.TopB * secBig.TopB * secBig.TopB * secBig.TopTf / 12.0;
            double IyBot = secBig.BotB * secBig.BotB * secBig.BotB * secBig.BotTf / 12.0;
            double IyT = topCompressed ? IyTop : IyBot;
            double IyB = topCompressed ? IyBot : IyTop;
            double Iy = secBig.I22;

            // 7. etaI、eta
            double etaI = IyB / IyT;
            double eta = 0.55 + 0.04 * Math.Cbrt(IyT / IyB) * (1 - kSigma);

            // 8. C1
            double C1 = 0.46 * kM * kM * Math.Pow(etaI, 0.346)
                      - 1.32 * kM * Math.Pow(etaI, 0.132)
                      + 1.86 * Math.Pow(etaI, 0.023);
            if (C1 > 2.75) C1 = 2.75;

            // 9. 小端翘曲和扭转
            double Iw0 = secSmall.Iw;
            double J0 = secSmall.It;
            double L = member.Frame.Length;

            // 10. 等效参数
            double IwEta = Iw0 * Math.Pow(1 + gamma * eta, 2);
            double Jeta = J0 + (1.0 / 3.0) * gamma * eta * (h0 - secSmall.TopTf) * Math.Pow(secSmall.Tw, 3);
            double betaXEta = 0.45 * (1 + gamma * eta) * h0 * (IyT - IyB) / Iy;

            // 11. 主公式
            double inner = IwEta / Iy * (1 + G * Jeta * L * L / (Math.PI * Math.PI * E * IwEta));
            double sqrtTerm = Math.Sqrt(betaXEta * betaXEta + inner);
            double Mcr = C1 * Math.PI * Math.PI * E * Iy / (L * L) * (betaXEta + sqrtTerm);

            // 12. 填结果（SetCal：程序算的）
            d.M1.SetCal(M1);
            d.M0.SetCal(M0);
            d.Wx1.SetCal(Wx1);
            d.Wx0.SetCal(Wx0);
            d.KM.SetCal(kM);
            d.KSigma.SetCal(kSigma);
            d.Gamma.SetCal(gamma);
            d.IyT.SetCal(IyT);
            d.IyB.SetCal(IyB);
            d.Iy.SetCal(Iy);
            d.EtaI.SetCal(etaI);
            d.Eta.SetCal(eta);
            d.C1.SetCal(C1);
            d.Iw0.SetCal(Iw0);
            d.IwEta.SetCal(IwEta);
            d.JEta.SetCal(Jeta);
            d.BetaXEta.SetCal(betaXEta);
            d.Mcr.SetCal(Mcr);
        }

        /// <summary>
        /// 有隅撑时弹性屈曲临界弯矩 Mcr（7.1.6条）
        /// </summary>
        public static void CalcMcrWithBrace(MemberData member, string comboName)
        {
            var sec = member.SecBig;
            // 内部自己找 PhiBDetails
            PhiBDetails d = GetPhiBDetails(member, comboName);
            double E = MaterialData.E;
            double G = MaterialData.G;

            // ===== 1. 读参数 =====
            double lp = d.Lp.Value;
            double beta = d.Beta.Value;
            double alpha = d.Alpha.Value;
            double a = d.A.Value;
            double Ap = d.Ap.Value;
            double Ip = d.Ip.Value;
            double Ak = d.Ak.Value;
            double lkk = d.Lkk.Value;

            double H = sec.H;
            double h = sec.H0;
            double HToSc = sec.HToSc;
            double J = sec.J;
            double Iy = sec.I22;
            double Iw = sec.Iw;

            double I1 = sec.TopTf * Math.Pow(sec.TopB, 3) / 12.0;
            double I2 = sec.BotTf * Math.Pow(sec.BotB, 3) / 12.0;

            // ===== 2. 算中间值 =====
            double alphaRad = alpha * Math.PI / 180.0;
            double lk = lp * beta / Math.Cos(alphaRad);
            double e = a + H - 50;
            double e1 = HToSc + a;
            double betaX = 0.45 * h * (I1 - I2) / Iy;

            // ===== 3. 算 kb =====
            double term1 = (1 - 2 * beta) * lp / (2 * E * Ap);
            double term2 = (a + h) * (3 - 4 * beta) * beta * lp * lp * Math.Tan(alphaRad) / (6 * E * Ip);
            double term3 = lk * lk / (beta * lp * E * Ak * Math.Cos(alphaRad));

            double kb = (1.0 / lkk) / Math.Pow(term1 + term2 + term3, -1);

            // ===== 4. 算 Mcr =====
            double mcr = (G * J + 2 * e * Math.Sqrt(kb * (E * Iy * e1 * e1 + E * Iw)))
                       / (2 * (e1 - betaX));


            d.Lk.SetCal(lk);
            d.E.SetCal(e);
            d.E1.SetCal(e1);
            d.BetaXBrace.SetCal(betaX);
            d.kb.SetCal(kb);
            d.Mcr.SetCal(mcr);
        }

        private static PhiBDetails GetPhiBDetails(MemberData member, string comboName)
        {
            if (member.Type == MemberType.Column)
            {
                var col = (ColumnCheckResult)member.CheckResult;
                return col.OutPlaneStability.PhiBDetails[comboName];
            }
            else
            {
                var beam = (BeamCheckResult)member.CheckResult;
                return beam.LateralStability.PhiBDetails[comboName];
            }
        }

    }
}
