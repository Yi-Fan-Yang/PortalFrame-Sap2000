using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    public static class StrengthCheck
    {
        /// <summary>
        /// 7.1.2 条 强度验算（每个测站都做）
        /// </summary>
        /// <returns>利用率（越大越不利），详细说明</returns>
        public static void Check(MemberData member, List<ForceData> forces, List<StrengthCheckResult> results)
        {
            foreach (var force in forces)
            {
                var result = new StrengthCheckResult();
                // 取测站截面
                var sec = member.StationSections[force.Station];

                // 取内力
                double N = force.P;      // 轴力（正拉负压）
                double V2 = Math.Abs(force.V2);     // 对应 M3 的剪力
                double M3 = Math.Abs(force.M3);  // 绕强轴弯矩（取绝对值）
                double V3 = Math.Abs(force.V3);     // 对应 M3 的剪力
                double M2 = Math.Abs(force.M2);  // 绕强轴弯矩（取绝对值）

                // 取材料强度设计值
                double f = member.Material.GetDesignValues(sec.TopTf).f;

                // 算有效截面
                SectionUtils.EffectiveSection(member, force);
                // 直接读 force 里的有效截面
                double Ae = force.Ae;
                double We = Math.Min(force.WeTop, force.WeBottom);

                // 算抗剪承载力 Vd
                double Vd = SectionUtils.ShearCapacity(member, force);

                // ===== 分两种情况验算 =====
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
                // ===== 结果输出 =====
                result.Util = util;
                result.Station = force.Station;
                result.N = force.P;
                result.V2 = force.V2;
                result.M3 = force.M3;
                result.V3 = force.V3;
                result.M2 = force.M2;
                result.Ae = Ae;
                result.We = We;
                result.Vd = Vd;
                result.VLess05Vd = vLess05Vd;
                result.Stress = stress;
                result.Me_N = Me_N;
                result.Mf_N = Mf_N;
                result.MAllow = MAllow;

                results.Add(result);
            }
        }
    }
}

