using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._1Model
{
    public class SectionData
    {
        // 基本信息
        public string Name = "";
        public bool IsTapered;
        public string MatProp = "";

        // 变截面插值方式（1=Linear, 2=Parabolic, 3=Cubic）
        public int EI33Type;
        public int EI22Type;

        // ===== 小端 =====
        // 几何（上下翼缘分开）
        public double H1;
        public double TopB1;      // 上翼缘宽
        public double TopTf1;     // 上翼缘厚
        public double BotB1;      // 下翼缘宽
        public double BotTf1;     // 下翼缘厚
        public double Tw1;         // 腹板厚
        public double Fillet1;     // 圆角半径
        // 属性（SAP 全量）
        public double A1;
        public double As2_1, As3_1, J1;
        public double I22_1, I33_1;
        public double S22_1, S33_1;
        public double Z22_1, Z33_1;
        public double R22_1, R33_1;

        // ===== 大端（等截面时与小端相同）=====
        public double H2;
        public double TopB2;
        public double TopTf2;
        public double BotB2;
        public double BotTf2;
        public double Tw2;
        public double Fillet2;
        public double A2;
        public double As2_2, As3_2, J2;
        public double I22_2, I33_2;
        public double S22_2, S33_2;
        public double Z22_2, Z33_2;
        public double R22_2, R33_2;
    }
}
