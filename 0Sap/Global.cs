using CSiAPIv1;
using PortalFrame._3Dispatch;
using PortalFrame._5data;

namespace PortalFrame._0Sap
{
    public static class Global
    {
        // ===== SAP 模型 =====
        public static cSapModel SapModel = null!;

        // ===== 有状态的数据（需要实例）=====
        public static ModelData ModelData = new();
        public static DesignCombos Combos = new();
        public static SelectedGroups Groups = new();
        public static ModelRenderer Renderer = new();
    }
}
