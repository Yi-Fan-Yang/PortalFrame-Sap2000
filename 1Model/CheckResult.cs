using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._1Model
{
    public class CheckResult
    {
        public string MemberName = "";

        public double StrengthRatio;     // 7.1.2 强度
        public double InPlaneRatio;      // 7.1.3 柱平面内稳定
        public double OutPlaneRatio;     // 7.1.5 柱平面外稳定
        public double BeamLateralRatio;  // 7.1.4 梁整体稳定
        public double SlendernessRatio;  // 长细比

        public double MaxRatio;          // 以上取 max
    }
}
