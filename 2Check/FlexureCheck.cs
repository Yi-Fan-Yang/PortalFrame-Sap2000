using PortalFrame._1Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._2Check
{
    public class FlexureCheck
    {
        // 验算某测站的抗弯强度
        // 输入：该测站的内力、该位置的截面模量、钢材抗弯强度设计值 f
        public CheckResult Check(double m3, double s33, double f)
        {
            double stress = Math.Abs(m3)/ s33;   // 
            double utilization = stress / f;

            return new CheckResult
            {
                ItemName = "抗弯强度",
                Utilization = utilization,
                M3 = m3,
                Stress = stress,
                Allowable = f,
                Conclusion = utilization <= 1.0 ? "满足" : "不满足"
            };
        }
    }
}
