using PortalFrame._0Sap;
using PortalFrame._1Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._3Dispatch
{
    public class Dispatch
    {
        private SapModelReader _reader;
        private Dictionary<string, JointData> _joints = new();
        private Dictionary<string, FrameData> _frames = new();
        public Dictionary<string, JointData> GetJoints() => _joints;

        public Dictionary<string, FrameData> GetFrames() => _frames;


        public Dispatch(SapModelReader reader)
        {
            _reader = reader;
        }

        // 读模型，把数据存到字段里
        public void ReadModel()
        {
            _joints = _reader.GetAllJoints();
            _frames = _reader.GetAllFrames();
        }

        // 从字段里取数据，拼一段摘要文字
        public string BuildSummaryText()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"节点数：{_joints.Count}，杆件数：{_frames.Count}");
            return sb.ToString();
        }

        // 读模型，拼好要显示的文本，返回给 MainForm
        public string ReadModelAndBuildText()
        {
            var joints = _reader.GetAllJoints();
            var frames = _reader.GetAllFrames();

            var sb = new StringBuilder();
            sb.AppendLine($"节点数：{joints.Count}，杆件数：{frames.Count}");
            sb.AppendLine("");

            sb.AppendLine("节点坐标：");
            foreach (var j in joints.Values)
            {
                sb.AppendLine($"  {j.Name}: ({j.X:F2}, {j.Y:F2}, {j.Z:F2})");
            }

            sb.AppendLine("");
            sb.AppendLine("杆件连接：");
            foreach (var f in frames.Values)
            {
                sb.AppendLine($"  {f.Name}: {f.StartJoint} → {f.EndJoint}");
            }

            return sb.ToString();
        }
    }
}
