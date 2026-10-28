using CSiAPIv1;
using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2Check;
using PortalFrame._2CheckResults;
using PortalFrame._3Dispatch;
using PortalFrame._5data;
using System.Drawing;

namespace PortalFrame
{
    // 全局入口
    public static class Global
    {
        public static cSapModel SapModel = null!;
        public static ModelData Data = new();
        public static UiConfig Ui = new();
        public static ModelRenderer Renderer = new();
    }

    // 模型数据
    public class ModelData
    {
        public Dictionary<string, JointData> Joints = new();
        public Dictionary<string, FrameData> Frames = new();
        public Dictionary<string, SectionBase> Sections = new();
        public Dictionary<string, MaterialData> Materials = new();
        public Dictionary<string, string> FrameSectionMap = new();
        public Dictionary<string, MemberData> Members = new();
        public List<string> AllCombos = new();
        public Dictionary<string, Dictionary<string, Dictionary<double, ForceData>>> Forces = new();
        public List<ColumnInfo> Cols = new();
        public List<string> AllGroups = new();
        public Dictionary<string, List<string>> GroupToFrames = new();
        public Dictionary<string, List<string>> FrameToGroups = new();
        public DesignCombos Combos = new();
        public SelectedGroups Groups = new();
    }

    // UI配置
    public class UiConfig
    {
        public Font DefaultFont = new Font("Microsoft YaHei", 12);
        public Color BackColor = Color.White;
        public Color UncheckColor = Color.LightGray;
        public Color SelectColor = Color.Blue;
        public Color OverLimitColor0 = Color.Red;
        public Color OverLimitColor1 = Color.Orange;
        public Color OverLimitColor2 = Color.DeepSkyBlue;
        public Color OverLimitColor3 = Color.Green;
    }
}
