namespace PortalFrame._5data
{
    public class Preferences
    {
        // ===== 验算首选项 =====
        public double Gamma0 = 1.0;           // 结构重要性系数
    }
    public class OverWrites
    {
        // ===== 构件覆盖项 =====
        // 后面再加
    }

    public class DesignCombos
    {
        // ===== 设计组合选择 =====
        public List<string> SelectedCombos = new();  // 用户勾选的组合
    }
}
