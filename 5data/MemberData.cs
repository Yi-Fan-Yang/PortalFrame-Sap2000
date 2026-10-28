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
        public MaterialData Material = null!;       // 材料数据
        public Dictionary<int, double> Stations = new();  // key=序号(从0开始), value=测站位置
        public Dictionary<double, HSectionData> StationSections = new();// 各测站的截面属性（key=测站位置 s，value=该位置的截面）
        public double[] EndStations = new double[2];  // [0]=小端测站位置, [1]=大端测站位置
        public Dictionary<string, Dictionary<double, ForceData>> AnalysisForces = new();
        public Dictionary<string, Dictionary<double, DesignForce>> DesignForces = new();//key1=组合名，key2=测站位置，value=该位置的设计内力
        public double? Kz;   // 梁对柱转动约束（N·mm）
        public CheckResult CheckResult = null!;
        public bool IsCheckable = true;              // 是否参与验算（在A里）
        public List<string> Groups = new();          // 所属group列表（备用）
    }
}
