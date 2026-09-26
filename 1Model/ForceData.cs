using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._1Model
{
    public class ForceData
    {
        public double Station;  // 距离 I 端的距离
        public double P;         // 轴力
        public double V2;        // 局部 2 向剪力
        public double V3;        // 局部 3 向剪力
        public double T;         // 扭矩
        public double M2;        // 绕局部 2 轴弯矩
        public double M3;        // 绕局部 3 轴弯矩

    }
}
