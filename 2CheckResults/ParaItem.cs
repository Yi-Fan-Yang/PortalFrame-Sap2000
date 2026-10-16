
namespace PortalFrame._2CheckResults
{
    public enum ValueSource
    {
        Preference,   // 首选项
        OverWrite,    // 覆盖项
        Cal,             // 程序算的
        Default          // 默认值
    }

    public class ParaItem
    {
        // ===== 基本信息 =====
        public string Name = "";       // 参数名（带符号，如 "γ0"）
        public string Unit = "";       // 量纲，如 "N/mm²"

        // =====值来源=====
        private double? _preValue;      // 首选项
        private double? _owValue;    // 覆盖项
        private double? _calValue;     // 程序算的
        private double? _defaultValue; // 默认值

        // ===== 最终值来源（只读）=====
        public ValueSource? Source;

        // ===== 构造函数 =====
        public ParaItem() { }

        public ParaItem(string name, string unit = "")
        {
            Name = name;
            Unit = unit;
        }

        // ===== 设值方法 =====
        public void SetOw(double? value)
        {
            _owValue = value;
            Source = value.HasValue ? ValueSource.OverWrite : ValueSource.Default;
        }
        public void SetPre(double? value) => _preValue = value;
        public void SetCal(double value) => _calValue = value;
        public void SetDefault(double? value) => _defaultValue = value;

        // ===== 取值（按优先级：用户>程序>默认）=====
        // Value getter 优先级：Ow > Pre > Cal > Default
        public double Value => _owValue ?? _preValue ?? _calValue ?? _defaultValue ??
            throw new InvalidOperationException("没有值");

        // ===== 快速判断有没有值（UI用）=====
        public bool HasValue => _owValue.HasValue || _preValue.HasValue|| _calValue.HasValue || _defaultValue.HasValue;

        // ===== 重置用户值=====
        public void ResetOw() => _owValue = null;
        public void ResetPre() => _preValue = null;
        // ===== 读取当前值=====
        public double? GetPre() => _preValue;
        public double? GetOw() => _owValue;
        public double? GetDefault() => _defaultValue;

        // ===== 调试/计算书输出 =====
        public override string ToString()
        {
            string val = Value.ToString("F3");
            string unit = string.IsNullOrEmpty(Unit) ? "" : $" {Unit}";
            string src = Source switch
            {
                ValueSource.OverWrite => "【覆盖项】",
                ValueSource.Preference => "【首选项】",
                ValueSource.Cal => "【程序计算】",
                ValueSource.Default => "【默认值】",
                _ => ""
            };
            return $"{Name,-20} = {val,10}{unit,-10} {src}";
        }

    }

}
