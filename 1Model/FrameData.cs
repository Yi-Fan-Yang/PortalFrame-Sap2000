using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._1Model
{
    public class FrameData
    {
        public string Name = "";
        public JointData StartJoint = null!;
        public JointData EndJoint = null!;
        public double Length;   // 杆件长度

    }
}
