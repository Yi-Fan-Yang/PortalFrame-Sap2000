using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    public static class CalcLength
    {
        public static void SlendernessCheck(MemberData member, SlendernessResult result)
        {
            double L = member.Frame.Length;

            // 取回转半径（变截面用小端）
            double R33, R22;
            if (member.Section is TaperedSectionData tapered)
            {
                R33 = tapered.Start.R33;
                R22 = tapered.Start.R22;
            }
            else
            {
                var sec = member.Section as HSectionData;
                R33 = sec.R33;
                R22 = sec.R22;
            }

            // 计算长度
            double L0x = member.Mu * L;   // 平面内（3轴）
            double L0y = member.L0y;      // 平面外（2轴）

            // 长细比
            result.Lambda3 = L0x / R33;
            result.Lambda2 = L0y / R22;

            // 限值（门式刚架柱 150）
            double lambdaLimit = 150.0;
            result.Util3 = result.Lambda3 / lambdaLimit;
            result.Util2 = result.Lambda2 / lambdaLimit;
        }
    }
}

