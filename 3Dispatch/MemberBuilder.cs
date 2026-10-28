using CSiAPIv1;
using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2Check;
using PortalFrame._2CheckResults;
using PortalFrame._5data;
using System.Reflection;

namespace PortalFrame._3Dispatch
{
    // 数据预处理：把原始数据串成构件、插值变截面
    public static class MemberBuilder
    {
        // 串杆件-截面-材料
        public static void BuildMembers()
        {
            Global.Data.Members.Clear();

            foreach (var kv in Global.Data.Frames)
            {
                string frameName = kv.Key;
                string secName = Global.Data.FrameSectionMap[frameName];

                // 1. 串杆件
                var member = new MemberData
                {
                    Name = frameName,
                    SectionName = secName,
                    Frame = kv.Value,
                };

                // 读截面和材料，读不到就IsCheckable=false
                if (!Global.Data.Sections.TryGetValue(secName, out var sec) ||
                    !Global.Data.Materials.TryGetValue(sec.MatProp, out var mat))
                {
                    member.IsCheckable = false;
                    Global.Data.Members[frameName] = member;
                    continue;
                }

                member.Section = sec;
                member.Material = mat;
                SectionCal.GetMemberType(member);

                // 判断是否参与验算
                member.Groups = Global.Data.FrameToGroups.GetValueOrDefault(frameName, new List<string>());
                member.IsCheckable = member.Groups.Any(g => Global.Data.Groups.CheckGroups.Contains(g));

                if (!member.IsCheckable)
                {
                    Global.Data.Members[frameName] = member;
                    continue;
                }

                // 2. 给这个构件赋值分析内力
                member.AnalysisForces = new();
                foreach (var (comboName, forces) in Global.Data.Forces)
                {
                    if (forces.ContainsKey(frameName))
                    {
                        // 把列表转成字典，key=测站位置
                        var forceDict = new Dictionary<double, ForceData>();
                        foreach (var (station, f) in forces[frameName])
                        {
                            forceDict[station] = f;
                        }
                        member.AnalysisForces[comboName] = forceDict;
                    }
                }

                // 3. 截面插值（等截面和变截面都调用）
                SectionCal.InterpolateSection(member);

                // 4. 判断大小端
                SectionCal.SetBigSmallEnd(member);

                // 5. 算截面补充参数（大端、小端都算）
                SectionCal.CalcSectionParams(member.StationSections[member.EndStations[1]]);
                SectionCal.CalcSectionParams(member.StationSections[member.EndStations[0]]);

                // 6. 建结果类
                member.CheckResult = new CheckResult();

                // 7. 初始化所有组合的结果对象
                member.CheckResult.InitCombos(member, Global.Data.AllCombos);

                // 8. 用反射设 Default 值
                SetDefaults(member.CheckResult);

                Global.Data.Members[frameName] = member;
            }
        }

        // 算Kz + 写回 member.Kz
        public static void GetKz(cSapModel sap, List<string> kzGroups)
        {
            // ① 算Kz
            var kzCalc = new KzCalculator(sap);
            kzCalc.Calc(kzGroups);

            // ② 把 Kz 写回 member.Kz
            foreach (var (g, columns) in kzCalc.GroupColumns)
            {
                foreach (var (colName, info) in columns)
                {
                    if (Global.Data.Members.TryGetValue(colName, out var member) && member.IsCheckable)
                    {
                        member.Kz = info.Kz;
                    }
                }
            }
        }

        private static void SetDefaults(object obj)
        {
            if (obj == null) return;

            //如果是字典，遍历所有 value 递归
            if (obj is System.Collections.IDictionary dict)
            {
                foreach (var value in dict.Values)
                {
                    SetDefaults(value);
                }
                return;
            }

            foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.IsStatic) continue;

                var value = field.GetValue(obj);
                if (value == null) continue;

                // 如果是 ParaItem，从 DefaultValues 找同名默认值
                if (value is ParaItem item)
                {
                    var defaultField = typeof(DefaultValues).GetField(field.Name, BindingFlags.Public | BindingFlags.Static);
                    if (defaultField != null)
                    {
                        double defaultValue = (double)defaultField.GetValue(null)!;
                        item.SetDefault(defaultValue);
                    }
                }
                // 如果是其他类（嵌套的），递归进去
                else if (!value.GetType().IsPrimitive && value.GetType() != typeof(string))
                {
                    SetDefaults(value);
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
                // 从 Stations 字典里找最小和最大的测站位置
                double minStation = member.Stations.Values.Min();
                double maxStation = member.Stations.Values.Max();
                // 找这两个位置对应的截面
                var secAtMin = member.StationSections[minStation];
                var secAtMax = member.StationSections[maxStation];

                if (secAtMax.H >= secAtMin.H)
                {
                    // 最大测站位置的截面更高 → 大端
                    member.EndStations[1] = maxStation;
                    member.EndStations[0] = minStation;
                }
                else
                {
                    // 最小测站位置的截面更高 → 大端
                    member.EndStations[1] = minStation;
                    member.EndStations[0] = maxStation;
                }
            }
            else
            {
                // 等截面，大小端都用第一个测站位置
                double firstStation = member.Stations[0];
                member.EndStations[0] = firstStation;
                member.EndStations[1] = firstStation;
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
            if (member.AnalysisForces.Count == 0) return;

            double L = member.Frame.Length;
            if (L == 0) return;

            // 用第一个组合的测站
            string firstCombo = member.AnalysisForces.Keys.First();
            var stations = member.AnalysisForces[firstCombo];

            if (member.Section.IsTapered)
            {
                // 变截面：插值每个测站的截面
                var tapered = (TaperedSectionData)member.Section;
                foreach (var (station, f) in stations)
                {
                    double ratio = station / L;
                    member.StationSections[station] = InterpolateOneSection(tapered, ratio);
                }
            }
            else
            {
                // 等截面：所有测站的截面都是同一个
                var hsec = (HSectionData)member.Section;
                foreach (var (station, f) in stations)
                {
                    member.StationSections[station] = hsec;
                }
            }
            // 把 StationSections 的 key 按顺序存到字典里
            member.Stations = new Dictionary<int, double>();
            int i = 0;
            foreach (var station in member.StationSections.Keys)
            {
                member.Stations[i] = station;
                i++;
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


 


