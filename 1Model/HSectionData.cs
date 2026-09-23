using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._1Model
{
    public class HSectionData:SectionBase
    {
        // 几何
        public double H, TopB, TopTf, BotB, BotTf, Tw, Fillet;
        // 属性
        public double A, As2, As3, J, I22, I33, S22, S33, Z22, Z33, R22, R33;
        // 材料
    }
}
