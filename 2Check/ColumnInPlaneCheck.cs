using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    /// <summary>
    /// 7.1.3 变截面柱平面内稳定验算
    /// </summary>
    public static class ColumnInPlaneCheck
    {
        public static (double util, string detail) Check(
            MemberData member, ForceData force)
        {
            // 1. 取这个测站的截面和内力
            var sec = member.StationSections[force.Station];
            double N = force.P;
            double M = Math.Abs(force.M3);

            // 2. 取材料强度
            double f = member.Material.GetDesignValues(sec.TopTf).f;
            double fy = member.Material.GetDesignValues(sec.Tw).fy;
            double E = MaterialData.E;

            // 3. 算有效截面
            double Ae = sec.Area;
            double WeTop = sec.S33_Top;
            double WeBottom = sec.S33_Bot;
            double We = Math.Min(WeTop, WeBottom);

            // 4. 算长细比 λ1
            double mu = member.Mu;              // 计算长度系数（后面填）
            double H = member.Frame.Length;      // 柱高
            double ix = sec.R33;                // 绕强轴回转半径
            double lambda1 = mu * H / ix;

            // 5. 算通用长细比 λn
            double lambdaN = lambda1 / Math.PI * Math.Sqrt(fy / E);

            // 6. 算 φx
            double phiX = Phi(lambdaN);

            // 7. 算 ηt
            double etaT = CalcEtaT(member, lambdaN);

            // 8. 算 Ncr
            double Ncr = Math.PI * Math.PI * E * Ae / (lambda1 * lambda1);

            // 9. 算利用率
            double betaMx = 1.0;  // 有侧移刚架
            double util = N / (etaT * phiX * Ae)
                        + betaMx * M / ((1 - N / Ncr) * We);
            util = Math.Abs(util) / f;

            string detail = $"λ1={lambda1:F1}, λn={lambdaN:F2}, φ={phiX:F3}, ηt={etaT:F3}";

            return (util, detail);
        }

        // ===== φx 计算（b类截面）=====
        private static double Phi(double lambdaN)
        {
            if (lambdaN <= 0.215)
                return 1 - 0.65 * lambdaN * lambdaN;

            double inner = 0.965 + 0.3 * lambdaN + lambdaN * lambdaN;
            return (inner - Math.Sqrt(inner * inner - 4 * lambdaN * lambdaN))
                 / (2 * lambdaN * lambdaN);
        }

        // ===== ηt 折减系数 =====
        private static double CalcEtaT(MemberData member, double lambdaN)
        {
            if (lambdaN >= 1.2) return 1.0;

            // A0/A1 = 小端/大端毛截面面积比
            if (member.Section is TaperedSectionData tapered)
            {
                double A0 = tapered.Start.Area;
                double A1 = tapered.End.Area;
                double ratio = A0 / A1;
                return ratio + (1 - ratio) * lambdaN * lambdaN / 1.44;
            }
            else
            {
                // 等截面：A0=A1，ηt=1
                return 1.0;
            }
        }
    }
}
