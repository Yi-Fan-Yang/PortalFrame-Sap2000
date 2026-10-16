using CSiAPIv1;
using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._5data;

namespace PortalFrame._3Dispatch
{
    // 数据预处理：把原始数据串成构件、插值变截面
    public static class MemberBuilder
    {
        // 串杆件-截面-材料
        public static void BuildMembers()
        {
            Global.ModelData.Members.Clear();

            foreach (var kv in Global.ModelData.Frames)
            {
                string frameName = kv.Key;
                string secName = Global.ModelData.FrameSectionMap[frameName];

                if (!Global.ModelData.Sections.TryGetValue(secName, out var sec)) continue;
                if (!Global.ModelData.Materials.TryGetValue(sec.MatProp, out var mat)) continue;

                // 1. 串杆件
                var member = new MemberData
                {
                    Name = frameName,
                    SectionName = secName,
                    Frame = kv.Value,
                    Section = sec,
                    Material = mat
                };
                SectionCal.GetMemberType(member);


                // 2. 判断大小端
                SectionCal.SetBigSmallEnd(member);

                // 3. 算截面补充参数（大端、小端都算）
                SectionCal.CalcSectionParams(member.SecBig);
                SectionCal.CalcSectionParams(member.SecSmall);

                // 4. 变截面插值
                if (member.Section.IsTapered)
                {
                    SectionCal.InterpolateSection(member);
                }

                Global.ModelData.Members[frameName] = member;
            }
        }
        // 变截面杆件：插值各测站的截面属性
        // 变截面杆件：插值各测站的截面属性（插值）

        // 算Kz + 写回 member.Kz
        public static void GetKz(cSapModel sap, SelectedGroups groups)
        {
            // ① 算Kz（填 _preData.Cols）
            var kzCalc = new KzCalculator(sap);
            kzCalc.Calc(groups.Names, Global.ModelData.Cols);

            // ② 把 Kz 写回 member.Kz
            foreach (var info in Global.ModelData.Cols)
            {
                if (Global.ModelData.Members.TryGetValue(info.ColName, out var member))
                {
                    member.Kz = info.Kz;
                }
            }
        }
    }

    public static class SectionCal
    {
        // 判断大小端
        public static void SetBigSmallEnd(MemberData member)
        {
            if (member.Section is TaperedSectionData tapered)
            {
                tapered.Start.Station = 0;
                tapered.End.Station = member.Frame.Length;

                if (tapered.End.H >= tapered.Start.H)
                {
                    member.SecBig = tapered.End;
                    member.SecSmall = tapered.Start;
                }
                else
                {
                    member.SecBig = tapered.Start;
                    member.SecSmall = tapered.End;
                }
            }
            else
            {
                var hsec = (HSectionData)member.Section;
                hsec.Station = 0;
                member.SecBig = hsec;
                member.SecSmall = hsec;
            }
        }

        public static void CalcSectionParams(HSectionData sec)
        {
            // ===== 1. 上下翼缘中面距离 H0 =====
            sec.H0 = sec.H - sec.TopTf / 2.0 - sec.BotTf / 2.0;

            // ===== 2. 形心位置 Yc（从上边缘到形心的距离）=====
            double AfTop = sec.TopB * sec.TopTf;
            double AfBot = sec.BotB * sec.BotTf;
            double Aw = sec.H0 * sec.Tw;
            double A_total = AfTop + AfBot + Aw;

            sec.Yt = (AfTop * sec.TopTf / 2.0
                    + Aw * (sec.TopTf + sec.H0 / 2.0)
                    + AfBot * (sec.H - sec.BotTf / 2.0)) / A_total;
            sec.Yb = sec.H - sec.Yt;

            // ===== 3. 上下翼缘对弱轴的惯性矩 =====
            double I_top = sec.TopTf * Math.Pow(sec.TopB, 3) / 12.0;
            double I_bot = sec.BotTf * Math.Pow(sec.BotB, 3) / 12.0;

            // ===== 4. 翘曲惯性矩 Iw =====
            if (Math.Abs(I_top + I_bot) > 1e-6)
            {
                sec.Iw = (I_top * I_bot / (I_top + I_bot)) * Math.Pow(sec.H0, 2);
            }
            else
            {
                sec.Iw = 0;
            }

            // ===== 5. 剪切中心到上下翼缘外缘距离 =====
            if (Math.Abs(I_top + I_bot) > 1e-6)
            {
                sec.HToSc = (I_bot / (I_top + I_bot)) * sec.H0;
                sec.HBoSc = (I_top / (I_top + I_bot)) * sec.H0;
            }
            else
            {
                sec.HToSc = sec.H0 / 2.0;
                sec.HBoSc = sec.H0 / 2.0;
            }
        }

        public static void InterpolateSection(MemberData member)
        {
            if (!member.Section.IsTapered) return;
            if (member.AnalysisForces.Count == 0) return;

            var tapered = (TaperedSectionData)member.Section;
            double L = member.Frame.Length;
            if (L == 0) return;

            // 用第一个组合的测站
            string firstCombo = member.AnalysisForces.Keys.First();
            var stations = member.AnalysisForces[firstCombo];

            foreach (var st in stations)
            {
                double ratio = st.Station / L;
                member.StationSections[st.Station] = InterpolateOneSection(tapered, ratio);
            }
        }
        // 插值变截面
        private static HSectionData InterpolateOneSection(TaperedSectionData sec, double ratio)
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

        public static double Lerp(double a, double b, double t) => a + (b - a) * t;

        // 判断构件是柱还是梁 与XY平面夹角大于45度是柱，小于45度是梁
        public static void GetMemberType(MemberData member)
        {
            var j1 = member.Frame.StartJoint;
            var j2 = member.Frame.EndJoint;

            double dx = j2.X - j1.X;
            double dy = j2.Y - j1.Y;
            double dz = j2.Z - j1.Z;
            double length = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (length < 0.001)
            {
                member.Type = MemberType.Beam;
                return;
            }

            double sinAngle = System.Math.Abs(dz) / length;
            member.Type = sinAngle > 0.707 ? MemberType.Column : MemberType.Beam;
        }


    }
}


 


