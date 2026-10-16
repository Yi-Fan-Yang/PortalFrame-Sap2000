using PortalFrame._2CheckResults;
using PortalFrame._5data;

namespace PortalFrame._1Model
{
    public enum MemberType { Column, Beam }
    // 一根杆件的完整信息（杆件+截面+材料串起来）
    public class MemberData
    {
        public string Name = "";           // 杆件名
        public string SectionName = "";    // 截面名
        public MemberType Type;       // 柱还是梁
        public FrameData Frame=null!;             // 杆件几何（StartJoint/EndJoint）
        public SectionBase Section=null!;         // 截面数据
        public MaterialData Material = null!;       // 材料数据（Fy、Fu）
        public Dictionary<double, HSectionData> StationSections = new();// 各测站的截面属性（key=测站位置 s，value=该位置的截面）
        public HSectionData SecBig = null!;//大端截面（MemberBuilder里赋值）
        public HSectionData SecSmall = null!;//小端截面（MemberBuilder里赋值）
        public Dictionary<string, List<ForceData>> AnalysisForces = new();  // 分析内力（key=组合名称）
        public Dictionary<string, List<DesignForce>> DesignForces = new();  // 设计内力（key=组合名称）
        public OverWrites OverWrites=new();
        public double? Kz;   // 梁对柱转动约束（N·mm）
        public CheckResultBase CheckResult = null!;
    }
}
