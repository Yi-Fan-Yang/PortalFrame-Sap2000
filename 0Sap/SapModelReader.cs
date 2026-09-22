using CSiAPIv1;
using PortalFrame._1Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._0Sap
{
    public class SapModelReader
    {
        private cSapModel _sapModel;

        // 构造时把 SAP 模型交进来
        public SapModelReader(cSapModel sapModel)
        {
            _sapModel = sapModel;
        }

        // 读所有节点：名字 + 坐标
        public Dictionary<string, JointData> GetAllJoints()
        {
            int count = 0;
            string[] names = null!;
            _sapModel.PointObj.GetNameList(ref count, ref names);

            var result = new Dictionary<string, JointData>();
            foreach (string name in names)
            {
                double x = 0, y = 0, z = 0;
                // 问 SAP：这个节点的坐标是多少
                _sapModel.PointObj.GetCoordCartesian(name, ref x, ref y, ref z);
                result[name] = new JointData { Name = name, X = x, Y = y, Z = z };
            }
            return result;
        }

        // 读所有杆件：名字 + 两端节点
        public Dictionary<string,FrameData> GetAllFrames()
        {
            int count = 0;
            string[] names = null!;
            _sapModel.FrameObj.GetNameList(ref count, ref names);

            var result = new Dictionary<string, FrameData>();
            foreach (string name in names)
            {
                string p1 = "", p2 = "";
                // 问 SAP：这根杆件的两端是哪两个节点
                _sapModel.FrameObj.GetPoints(name, ref p1, ref p2);
                result[name]=new FrameData { Name = name, StartJoint = p1, EndJoint = p2 };
            }
            return result;
        }
    }
}
