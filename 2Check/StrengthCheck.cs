using CSiAPIv1;
using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    public static class StrengthCheck
    {
        /// <summary>
        /// 7.1.2 条 抗弯验算（每个测站都做）
        /// </summary>
        public static void FlexureCheck(MemberData member, ForceData force,ShearStationResult VResult, FlexureStationResult flexResult)
        {
            var sec = member.StationSections[force.Station];

            double N = force.P;
            double V2 = Math.Abs(force.V2);
            double M3 = Math.Abs(force.M3);

            double f = member.Material.GetDesignValues(sec.TopTf).f;

            double Vd = VResult.Vd;
            double Ae = flexResult.Ae;
            double We = Math.Min(flexResult.WeTop, flexResult.WeBottom);

            bool vLess05Vd = V2 <= 0.5 * Vd;
            double util, stress = 0, Me_N = 0, Mf_N = 0, MAllow = 0;

            if (vLess05Vd)
            {
                stress = N / Ae + M3 / We;
                util = Math.Abs(stress) / f;
            }
            else if (V2 < Vd)
            {
                Me_N = We * (f - N / Ae);

                double Af1 = sec.TopB * sec.TopTf;
                double Af2 = sec.BotB * sec.BotTf;
                double d = sec.H - sec.TopTf / 2.0 - sec.BotTf / 2.0;
                double A_total = Af1 + Af2;
                double sigmaN = N < 0 ? Math.Abs(N) / A_total : 0;
                Mf_N = d * Math.Min(Af1, Af2) * (f - sigmaN);

                MAllow = Mf_N + (Me_N - Mf_N) * (1 - Math.Pow(V2 / (0.5 * Vd) - 1, 2));
                util = M3 / MAllow;
            }
            else
            {
                util = double.PositiveInfinity;
            }

            flexResult.Util = util;
            flexResult.N = force.P;
            flexResult.V2 = force.V2;
            flexResult.M3 = force.M3;
            flexResult.V3 = force.V3;
            flexResult.M2 = force.M2;
            flexResult.VLess05Vd = vLess05Vd;
            flexResult.Stress = stress;
            flexResult.Me_N = Me_N;
            flexResult.Mf_N = Mf_N;
            flexResult.MAllow = MAllow;
        }

        /// <summary>
        /// 7.1.1 条 抗剪验算（每个测站都做）
        /// </summary>
        public static void ShearCheck(MemberData member, ForceData force, ShearStationResult result)
        {
            var sec = member.StationSections[force.Station];
            double hw1 = sec.H - sec.TopTf - sec.BotTf;
            double tw = sec.Tw;
            double fy = member.Material.GetDesignValues(tw).fy;

            // 小端腹板高
            double hw0;
            if (member.Section is TaperedSectionData tapered)
            {
                double hStart = tapered.Start.H - tapered.Start.TopTf - tapered.Start.BotTf;
                double hEnd = tapered.End.H - tapered.End.TopTf - tapered.End.BotTf;
                hw0 = Math.Min(hStart, hEnd);
            }
            else
            {
                hw0 = hw1;
            }

            // 楔率折减
            double gammaP = hw1 / hw0 - 1.0;
            double chiAp = 1.0 - 0.35 * Math.Pow(1.0, 0.2) * Math.Pow(gammaP, 2.0 / 3.0);
            if (chiAp < 0) chiAp = 0;

            // λs, φps
            double lambdaS = (hw1 / tw) / (37.0 * Math.Sqrt(5.34 * Math.Sqrt(235.0 / fy)));
            double phiPs = Math.Pow(0.51 + Math.Pow(lambdaS, 0.8), 1.0 / 1.26);
            if (phiPs > 1.0) phiPs = 1.0;

            double fv = member.Material.GetDesignValues(tw).fv;
            double Vd = chiAp * phiPs * hw1 * tw * fv;

            // 填结果
            double V = Math.Abs(force.V2);

            result.Station = force.Station;
            result.V2 = force.V2;
            result.Vd = Vd;
            result.ShearStress = V / (hw1 * tw);
            result.Util = V / Vd;
        }

    }
}

