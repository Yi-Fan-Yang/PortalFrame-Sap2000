using System;

namespace PortalFrame._2CheckResults
{
    // 字段显示标签：分组、顺序、条件显示
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayAttribute : Attribute
    {
        public string Group = "";    // 分组标题，引用Groups里的常量
        public int Order = 0;        // 组内顺序，越小越靠前
        public string OnlyWhen = ""; // 可选：该字段值==1时才显示，填字段名
    }


    // 所有详细信息分组标题
    public static class Groups
    {
        public static class Slenderness
        {
            public const string CalcLength = "1. 计算长度";
            public const string Lambda = "2. 长细比与通用长细比";
            public const string Stability = "3. 轴压稳定系数与欧拉临界力(GB50017-2017 附录D)";
            public const string Util = "4. 长细比利用率(GB51022-2015 7.1.1.1)";
        }

        public static class LocalStability
        {
            public const string Value = "1. 数值";
            public const string Util = "2. 利用率(GB51022-2015 3.4.2,3.4.3)";

        }
        public static class ColumnInPlane
        {
            public const string Force = "1. 内力";
            public const string Section = "2. 截面属性";
            public const string Main = "3. 主要参数(GB51022-2015 7.1.3)";
        }
        public static class ColumnOutPlane
        {
            public const string Force = "1. 内力";
            public const string Section = "2. 截面属性";
            public const string Main = "3. 主要参数(GB51022-2015 7.1.5)";
        }
        public static class BeamLateral
        {
            public const string Force = "1. 内力";
            public const string Section = "2. 截面属性";
            public const string Main = "3. 主要参数(GB51022-2015 7.1.4)";
        }
        public static class Flexure
        {
            public const string Force = "1. 内力";
            public const string Section = "2. 原始截面属性";
            public const string EffectiveSection = "3. 有效截面属性(GB51022-2015 7.1.1)";
            public const string Main = "4. 主要参数(GB51022-2015 7.1.2)";
        }
        public static class Shear
        {
            public const string Force = "1. 内力";
            public const string Section = "2. 截面属性";
            public const string LambdaS = "3. 参数 λs(GB51022-2015 7.1.1.6)";
            public const string Main = "4. 抗剪验算(GB51022-2015 7.1.1.5)";
        }
        public static class PhiB
        {
            public const string Forces = "1. 设计内力";
            public const string Section = "2. 截面参数";
            public const string Main = "3. 主体参数(GB51022-2015 7.1.4)";
            public const string Bracing = "4. 有隅撑(GB51022-2015 7.1.6.7)";
            public const string NoBracing = "5. 无隅撑(GB51022-2015 7.1.4.2)";

        }
    }
}
