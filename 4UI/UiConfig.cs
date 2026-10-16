using System.Drawing;

namespace PortalFrame._4UI
{
    public static class UiConfig
    {
        // 全局字体，以后改这里就行
        public static readonly Font DefaultFont = new Font("Microsoft YaHei", 12);

        // 全局颜色
        public static readonly Color BackColor = Color.White;
        public static readonly Color SelectColor = Color.Blue;
        public static readonly Color OverLimitColor = Color.Red;
        public static readonly Color OkColor = Color.Green;
        public static readonly Color WarnColor = Color.Yellow;
    }
}
