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
        public double Area, As2, As3, J, I22, I33, S22,S33_Top, S33_Bot,Z22, Z33, R22, R33;
        // 材料
    }
}
