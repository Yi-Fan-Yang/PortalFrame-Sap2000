using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._1Model
{
    public enum SteelGrade
    {
        Q235,
        Q355
    }
    public class MaterialData
    {
        public string Name = "";           // SAP里的材料名
        public SteelGrade Grade;           // 牌号

        // 物理常数（跟牌号无关，定值）
        public const double E = 206000;       // 弹性模量 N/mm²
        public const double G = 79000;        // 剪切模量 N/mm²
        public const double Alpha = 1.2e-5;   // 线膨胀系数 /°C
        public const double Rho = 7850;      // 质量密度 kg/m³

        /// <summary>
        /// 根据板厚查强度设计值（GB50017-2017 表4.4.1，2019勘误）
        /// </summary>
        /// <param name="t">板厚（mm），验算f用翼缘厚，验算fv用腹板厚，验算fy用腹板厚</param>
        public (double f, double fv, double fy, double fu) GetDesignValues(double t)
        {
            switch (Grade)
            {
                case SteelGrade.Q235:
                    if (t <= 16) return (215, 125, 235, 370);
                    if (t <= 40) return (205, 120, 225, 370);
                    if (t <= 60) return (200, 115, 215, 370);
                    return (190, 110, 205, 370);

                case SteelGrade.Q355:
                    if (t <= 16) return (305, 175, 355, 470);
                    if (t <= 40) return (295, 170, 345, 470);
                    if (t <= 63) return (290, 165, 335, 470);
                    if (t <= 80) return (280, 160, 325, 470);
                    return (270, 155, 315, 470);

                default:
                    return (305, 175, 355, 470);
            }
        }
    }
}

