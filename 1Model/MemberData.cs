
namespace PortalFrame._1Model
{
    public enum MemberType { Column, Beam }
    // 一根杆件的完整信息（杆件+截面+材料串起来）
    public class MemberData
    {
        public string Name = "";           // 杆件名
        public string SectionName = "";    // 截面名
        public MemberType Type;       // 柱还是梁
        public double Mu;             // 平面内计算长度系数（MuCalculator 填）
        public double L0y;            // 平面外计算长度（用户输入）
        public FrameData Frame=null!;             // 杆件几何（StartJoint/EndJoint）
        public SectionBase Section=null!;         // 截面数据
        public MaterialData Material = null!;       // 材料数据（Fy、Fu）
        public Dictionary<double, HSectionData> StationSections = new();// 各测站的截面属性（key=测站位置 s，value=该位置的截面）
    }
}
