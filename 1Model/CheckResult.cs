using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._1Model
{
    public class CheckResult
    {
        public string ItemName = "";      // 验算项名称（如"抗弯强度"）
        public double Utilization;         // 利用率（应力比，≤1 合格）
        public double Station;             // 控制测站位置
        public double M3;                  // 控制弯矩
        public double Stress;              // 计算应力
        public double Allowable;           // 限值
        public string Conclusion = "";     // 结论（"满足"/"不满足"）
    }
}
