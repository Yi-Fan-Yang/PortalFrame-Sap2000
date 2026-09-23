using PortalFrame._1Model;
using PortalFrame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._5data
{
    public class DataStore
    {
        // 前处理数据
        public Dictionary<string, JointData> Joints = new();
        public Dictionary<string, FrameData> Frames = new();
        public Dictionary<string, SectionData> Sections = new();
        public Dictionary<string, MaterialData> Materials = new();
        public Dictionary<string, string> FrameSectionMap = new();
        public Dictionary<string, MemberData> Members = new();


        // 后处理数据
        public List<string> Combos = new();
        // Key1: 组合名, Key2: 杆件名, Value: 测站内力列表
        public Dictionary<string, Dictionary<string, List<ForceData>>> Forces = new();

        // 验算结果
        public Dictionary<string, List<CheckResult>> CheckResults = new();
    }
}
