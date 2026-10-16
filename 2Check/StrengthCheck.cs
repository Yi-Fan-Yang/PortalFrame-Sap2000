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
            var forces = member.DesignForces[comboName];

            // 2. 读当前组合的结果列表
            var flexList = member.CheckResult.FlexureStrength.Combos[comboName];
            var shearList = member.CheckResult.ShearStrength.Combos[comboName];

            // 3. 遍历每个测站
            foreach (var force in forces)
            {
                var flexResult = flexList.First(s => s.Station == force.Station);
                var shearResult = shearList.First(s => s.Station == force.Station);

                var sec = member.StationSections[force.Station];

                // ===== ① 先算抗剪（Vd）=====
                double hw1 = member.SecBig.H - member.SecBig.TopTf - member.SecBig.BotTf;
                double hw0 = member.SecSmall.H - member.SecSmall.TopTf - member.SecSmall.BotTf;
                double tw = sec.Tw;
                double fy = member.Material.GetDesignValues(tw).fy;

                // 楔率折减
                double gammaP = hw1 / hw0 - 1.0;

                // λs, φps
                double kTau, alpha, omega1, etaS;
                if (!shearResult.HasStiffener)
                {
                    alpha = member.Frame.Length / hw1;
                    omega1 = 0.41 - 0.897 * alpha + 0.363 * alpha * alpha - 0.041 * alpha * alpha * alpha;
                    etaS = 1.0 - omega1 * Math.Sqrt(gammaP);
                    kTau = 5.34* etaS;
                }
                else
                {
                    double a = shearResult.a.Value;
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

                double fv = member.Material.GetDesignValues(tw).fv;
                double Vd = chiTap * phiPs * hw1 * tw * fv;

                // 写回抗剪结果
                double V = Math.Abs(force.V2);
                shearResult.LambdaS.SetCal(lambdaS);
                shearResult.KTau.SetCal(kTau);
                shearResult.Omega1.SetCal(omega1);
                shearResult.Etas.SetCal(etaS);
                shearResult.Alpha.SetCal(alpha);
                shearResult.Station = force.Station;
                shearResult.Vd.SetCal(Vd);
                shearResult.ChiTap.SetCal(chiTap);
                shearResult.GammaP.SetCal(gammaP);
                shearResult.ShearStress.SetCal(V / (hw1 * tw));
                shearResult.Util.SetCal(V / Vd);

                // ===== ② 再算抗弯（用 Vd）=====
                double N = force.N;
                double M3 = Math.Abs(force.M3);
                double f = member.Material.GetDesignValues(sec.TopTf).f;

                double Ae = flexResult.Ae.Value;
                double We = Math.Min(flexResult.WeTop.Value, flexResult.WeBottom.Value);

                bool vLess05Vd = V <= 0.5 * Vd;
                double util, stress = 0, Me_N = 0, Mf_N = 0, MAllow = 0;

                if (vLess05Vd)
                {
                    stress = N / Ae + M3 / We;
                    util = Math.Abs(stress) / f;
                }
                else if (V < Vd)
                {
                    Me_N = We * (f - N / Ae);

                    double Af1 = sec.TopB * sec.TopTf;
                    double Af2 = sec.BotB * sec.BotTf;
                    double d = sec.H - sec.TopTf / 2.0 - sec.BotTf / 2.0;
                    double A_total = Af1 + Af2;
                    double sigmaN = N < 0 ? Math.Abs(N) / A_total : 0;
                    Mf_N = d * Math.Min(Af1, Af2) * (f - sigmaN);

                    MAllow = Mf_N + (Me_N - Mf_N) * (1 - Math.Pow(V / (0.5 * Vd) - 1, 2));
                    util = M3 / MAllow;
                }
                else
                {
                    util = double.PositiveInfinity;
                }

                // 写回抗弯结果
                flexResult.Util.SetCal(util);
                flexResult.VLess05Vd = vLess05Vd;
                flexResult.Stress.SetCal(stress);
                flexResult.Me_N.SetCal(Me_N);
                flexResult.Mf_N.SetCal(Mf_N);
                flexResult.Me.SetCal(MAllow);
            }
        }

    }
}

