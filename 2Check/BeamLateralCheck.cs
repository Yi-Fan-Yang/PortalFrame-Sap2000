using PortalFrame._1Model;

namespace PortalFrame._2Check
{
    public static class BeamLateralCheck
    {
        // 梁整体稳定利用率
        public static double Check(MemberData beam, double N, double M, MaterialData mat)
        { /* TODO */ return 0; }

        // 给柱平面外复用的 φb
        public static double CalcPhiB(MemberData beam, double M0, double M1)
        { /* TODO */ return 1.0; }
    }
}
