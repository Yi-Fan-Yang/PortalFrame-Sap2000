using CSiAPIv1;
using PortalFrame._1Model;
using PortalFrame._5data;
using PortalFrame._2CheckResults;

namespace PortalFrame._2Check
{
    public static class StrengthCheck
    {
        //抗弯/抗剪验算（每个测站都做）
        public static void Calculate(MemberData member, string comboName)
        {
            // 1. 读当前组合的测站内内力
            var designDict = member.DesignForces[comboName];

            // 2. 读当前组合的结果列表
            var flexList = member.CheckResult.FlexureStrength.Collection[comboName];
            var shearList = member.CheckResult.ShearStrength.Collection[comboName];

            // 3. 遍历每个测站
            foreach (var (station, force) in designDict)
            {
                var flexResult = flexList[station];
                var shearResult = shearList[station];

                var sec = member.StationSections[station];

                // ===== ① 先算抗剪（Vd）=====
                var secBig = member.StationSections[member.EndStations[1]];
                var secSmall = member.StationSections[member.EndStations[0]];
                double hw1 = secBig.H - secBig.TopTf - secBig.BotTf;
                double hw0 = secSmall.H - secSmall.TopTf - secSmall.BotTf;
                double tw = sec.Tw;
                double fy = member.Material.GetDesignValues(tw).fy;

                // 楔率折减
                double gammaP = hw1 / hw0 - 1.0;

                // λs, φps
                double kTau, alpha, omega1, etaS;
                if (member.CheckResult.ShearStrength.HasStiffener.Value==0)
                {
                    alpha = member.Frame.Length / hw1;
                    omega1 = 0.41 - 0.897 * alpha + 0.363 * alpha * alpha - 0.041 * alpha * alpha * alpha;
                    etaS = 1.0 - omega1 * Math.Sqrt(gammaP);
                    kTau = 5.34* etaS;
                }
                else
                {
                    double a = member.CheckResult.ShearStrength.a.Value;
                    alpha = a / hw1;
                    omega1 = 0.41 - 0.897 * alpha + 0.363 * alpha * alpha - 0.041 * alpha * alpha * alpha;
                    etaS = 1.0 - omega1 * Math.Sqrt(gammaP);

                    if (alpha < 1.0)
                    {
                        kTau = 4.0 + 5.34 / (alpha * alpha);
                    }
                    else
                    {
                        kTau = etaS * (5.34 + 4.0 / (alpha * alpha));
                    }
                }

                double chiTap = 1.0 - 0.35 * Math.Pow(alpha, 0.2) * Math.Pow(gammaP, 2.0 / 3.0);
                if (chiTap < 0) chiTap = 0;
                double lambdaS = (hw1 / tw) / (37.0 * Math.Sqrt(kTau) * Math.Sqrt(235.0 / fy));
                double phiPs = Math.Pow(0.51 + Math.Pow(lambdaS, 0.8), 1.0 / 2.6);
                if (phiPs > 1.0) phiPs = 1.0;

                double V2 = Math.Abs(force.V2);
                double fv = member.Material.GetDesignValues(tw).fv;
                double Vd1 = chiTap * phiPs * hw1 * tw * fv;
                double Vd0 = hw0 * tw * fv;
                double Vd= Math.Min(Vd1, Vd0);
                double Util = V2/Vd;

                // 写回抗剪结果
                shearResult.V2.SetCal(V2);
                shearResult.Hw1.SetCal(hw1);
                shearResult.Hw0.SetCal(hw0);
                shearResult.Tw.SetCal(tw);
                shearResult.fv.SetCal(fv);

                shearResult.LambdaS.SetCal(lambdaS);
                shearResult.KTau.SetCal(kTau);
                shearResult.Etas.SetCal(etaS);
                shearResult.Omega1.SetCal(omega1);

                shearResult.PhiPs.SetCal(phiPs);
                shearResult.ChiTap.SetCal(chiTap);
                shearResult.GammaP.SetCal(gammaP);
                shearResult.Alpha.SetCal(alpha);
                shearResult.Vd1.SetCal(Vd1);
                shearResult.Vd0.SetCal(Vd0);
                shearResult.Util.SetCal(Util);

                // ===== ② 再算抗弯（用 Vd）=====
                double N = Math.Abs(force.N);
                double M2 = Math.Abs(force.M2);
                double M3 = Math.Abs(force.M3);
                double f = member.Material.GetDesignValues(sec.TopTf).f;

                double Ae = flexResult.Ae.Value;
                double Wex = Math.Min(flexResult.WeTop.Value, flexResult.WeBottom.Value);
                double Wy = member.StationSections[station].S22;

                double util2=0;
                double NUtil = Math.Abs(N / Ae / f);
                double M2Util = Math.Abs(M2 / Wy / f);
                double M3Util = Math.Abs(M3 / Wex / f);
                double util1 = NUtil+M2Util+M3Util;
                 if (V2 >0.5 * Vd)
                {
                    double Me_N = Wex * (f - N / Ae);
                    double AfTop = flexResult.AfTop.Value;
                    double AfBot = flexResult.AfBot.Value;
                    double d = sec.H - sec.TopTf / 2.0 - sec.BotTf / 2.0;
                    double A_total = AfTop + AfBot;
                    double sigmaN = Math.Abs(N) / A_total;
                    double Mf_N = d * Math.Min(AfTop, AfBot) * (f - sigmaN);
                    double MAllow = Mf_N + (Me_N - Mf_N) * (1 - Math.Pow(V2 / (0.5 * Vd) - 1, 2));
                    util2 = M3 / MAllow;
                    flexResult.Me_N.SetCal(Me_N);
                    flexResult.Mf_N.SetCal(Mf_N);
                    flexResult.MAllow.SetCal(MAllow);

                }
                double util=Math.Max(util1, util2);

                // 写回抗弯结果
                flexResult.N.SetCal(N);
                flexResult.M2.SetCal(M2);
                flexResult.M3.SetCal(M3);
                flexResult.f.SetCal(f);
                flexResult.NUtil.SetCal(NUtil);
                flexResult.M2Util.SetCal(M2Util);
                flexResult.M3Util.SetCal(M3Util);
                flexResult.Util.SetCal(util);
            }
        }

    }
}

