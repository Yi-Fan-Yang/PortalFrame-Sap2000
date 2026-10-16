namespace PortalFrame._1Model
{
    public class HSectionData : SectionBase
    {
        // ===== 几何尺寸 =====
        public double H;            // 截面高度（mm）
        public double TopB;         // 上翼缘宽度（mm）
        public double TopTf;        // 上翼缘厚度（mm）
        public double BotB;         // 下翼缘宽度（mm）
        public double BotTf;        // 下翼缘厚度（mm）
        public double Tw;           // 腹板厚度（mm）
        public double Fillet;       // 圆角半径（mm）

        // ===== 截面特性 =====
        public double Area;         // 截面面积（mm²）
        public double As2;          // 绕强轴剪切面积（mm²）
        public double As3;          // 绕弱轴剪切面积（mm²）
        public double J;            // 圣维南扭转常数（mm⁴）
        public double I22;          // 绕弱轴惯性矩（mm⁴）
        public double I33;          // 绕强轴惯性矩（mm⁴）
        public double S22;          // 绕弱轴静矩（mm³）
        public double S33_Top;      // 上边缘绕强轴模量（mm³）
        public double S33_Bot;      // 下边缘绕强轴模量（mm³）
        public double Z22;          // 绕弱轴塑性模量（mm³）
        public double Z33;          // 绕强轴塑性模量（mm³）
        public double R22;          // 绕弱轴回转半径（mm）
        public double R33;          // 绕强轴回转半径（mm）

        // ===== 扭转翘曲参数 =====
        public double It;           // 圣维南扭转常数（mm⁴）
        public double Iw;           // 翘曲惯性矩（mm⁶）
        public double H0;            // 上下翼缘中面距离（mm）
        public double HToSc;        // 剪切中心到上翼缘顶距离（mm）
        public double HBoSc;        // 剪切中心到下翼缘底距离（mm）

        // ===== 形心与剪切中心 =====
        public double Yt;           // 形心位置（从上边缘到形心的距离，mm）
        public double Yb;           // 形心位置（从下边缘到形心的距离，mm）

        // ===== 其他 =====
        public double Station;      // 截面在杆件上的测站编号（变截面用）
    }
}
