using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2Check;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._5data
{
    public class ModelData
    {
        // 前处理数据
        public Dictionary<string, JointData> Joints = new();
        public Dictionary<string, FrameData> Frames = new();
        public Dictionary<string, SectionBase> Sections = new();
        public Dictionary<string, MaterialData> Materials = new();
        public Dictionary<string, string> FrameSectionMap = new();
        public Dictionary<string, MemberData> Members = new();
        // Key1: 组合名, Key2: 杆件名, Value: 测站内力列表
        public Dictionary<string, Dictionary<string, List<ForceData>>> Forces = new();
        public Preferences Preferences = new();


        // 临时模型数据
        public List<ColumnInfo> Cols = new();//算Kz用的柱信息
        public List<string> AllCombos = new();
        // Key1: 组合名, Key2: 杆件名, Value: 测站内力列表


    }
}
