using PortalFrame._1Model;
using PortalFrame._5data;

namespace PortalFrame._3Dispatch
{
    // 数据预处理：把原始数据串成构件、插值变截面
    public class MemberBuilder
    {
        private PreData _preData;

        public MemberBuilder(PreData store)
        {
            _preData = store;
        }

        // 串杆件-截面-材料
        public void BuildMembers()
        {
            _preData.Members.Clear();
            foreach (var kv in _preData.Frames)
            {
                string frameName = kv.Key;
                string secName = _preData.FrameSectionMap[frameName];
                if (!_preData.Sections.TryGetValue(secName, out var sec))continue;   // 非H型钢，跳过
                if (!_preData.Materials.TryGetValue(sec.MatProp, out var mat))continue;   // 材料不是钢材，跳过

                _preData.Members[frameName] = new MemberData
                {
                    Name = frameName,
                    SectionName = secName,
                    Frame = kv.Value,
                    Section = sec,
                    Material = mat
                };
            }
        }

        // 变截面杆件：插值各测站的截面属性
        public void InterpolateStations()
        {
            string firstCombo = _preData.Combos.First();
            foreach (var member in _preData.Members.Values)
            {
                if (!member.Section.IsTapered) continue;

                var tapered = (TaperedSectionData)member.Section;
                double L = member.Frame.Length;
                if (L == 0) continue;

                var stations = _preData.Forces[firstCombo][member.Name];
                foreach (var st in stations)
                {
                    double ratio = st.Station / L;
                    member.StationSections[st.Station] = InterpolateSection(tapered, ratio);
                }
            }
        }

        // 插值变截面
        public HSectionData InterpolateSection(TaperedSectionData sec, double ratio)
        {
            int i33Order = sec.EI33Type;
            int i22Order = sec.EI22Type;
            int s33Order = Math.Max(1, sec.EI33Type - 1);
            int s22Order = Math.Max(1, sec.EI22Type - 1);

            double tLinear = ratio;
            double tI33 = Math.Pow(ratio, i33Order);
            double tI22 = Math.Pow(ratio, i22Order);
            double tS33 = Math.Pow(ratio, s33Order);
            double tS22 = Math.Pow(ratio, s22Order);

            return new HSectionData
            {
                MatProp = sec.MatProp,
                IsTapered = false,

                H = Lerp(sec.Start.H, sec.End.H, tLinear),
                TopB = Lerp(sec.Start.TopB, sec.End.TopB, tLinear),
                TopTf = Lerp(sec.Start.TopTf, sec.End.TopTf, tLinear),
                BotB = Lerp(sec.Start.BotB, sec.End.BotB, tLinear),
                BotTf = Lerp(sec.Start.BotTf, sec.End.BotTf, tLinear),
                Tw = Lerp(sec.Start.Tw, sec.End.Tw, tLinear),
                Fillet = Lerp(sec.Start.Fillet, sec.End.Fillet, tLinear),
                Area = Lerp(sec.Start.Area, sec.End.Area, tLinear),
                As2 = Lerp(sec.Start.As2, sec.End.As2, tLinear),
                As3 = Lerp(sec.Start.As3, sec.End.As3, tLinear),
                J = Lerp(sec.Start.J, sec.End.J, tLinear),
                Z22 = Lerp(sec.Start.Z22, sec.End.Z22, tLinear),
                Z33 = Lerp(sec.Start.Z33, sec.End.Z33, tLinear),
                R22 = Lerp(sec.Start.R22, sec.End.R22, tLinear),
                R33 = Lerp(sec.Start.R33, sec.End.R33, tLinear),

                I33 = Lerp(sec.Start.I33, sec.End.I33, tI33),
                I22 = Lerp(sec.Start.I22, sec.End.I22, tI22),
                S33_Top = Lerp(sec.Start.S33_Top, sec.End.S33_Top, tS33),
                S33_Bot = Lerp(sec.Start.S33_Bot, sec.End.S33_Bot, tS33),
                S22 = Lerp(sec.Start.S22, sec.End.S22, tS22)
            };
        }

        private double Lerp(double a, double b, double t) => a + (b - a) * t;
    }
}
