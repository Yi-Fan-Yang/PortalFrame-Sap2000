using CSiAPIv1;
using PortalFrame._1Model;

namespace PortalFrame._0Sap
{
    public static class PreReader
    {

       // =================Geometry=======================
        // 读所有节点：名字 + 坐标
        public static Dictionary<string, JointData> GetAllJoints()
        {
            int count = 0;
            string[] names = null!;
            Global.SapModel.PointObj.GetNameList(ref count, ref names);

            var result = new Dictionary<string, JointData>();
            foreach (string name in names)
            {
                double x = 0, y = 0, z = 0;
                // 问 SAP：这个节点的坐标是多少
                Global.SapModel.PointObj.GetCoordCartesian(name, ref x, ref y, ref z);
                result[name] = new JointData { Name = name, X = x, Y = y, Z = z };
            }
            return result;
        }

        // 读所有杆件：名字 + 两端节点
        public static Dictionary<string,FrameData> GetAllFrames(Dictionary<string, JointData> joints)
        {
            int count = 0;
            string[] names = null!;
            Global.SapModel.FrameObj.GetNameList(ref count, ref names);

            var result = new Dictionary<string, FrameData>();
            foreach (string name in names)
            {
                string p1 = "", p2 = "";
                // 问 SAP：这根杆件的两端是哪两个节点
                Global.SapModel.FrameObj.GetPoints(name, ref p1, ref p2);
                // 读两端坐标算长度
                double x1 = joints[p1].X, y1 = joints[p1].Y, z1 = joints[p1].Z;
                double x2 = joints[p2].X, y2 = joints[p2].Y, z2 = joints[p2].Z;
                double len = Math.Sqrt(
                    (x2 - x1) * (x2 - x1) +
                    (y2 - y1) * (y2 - y1) +
                    (z2 - z1) * (z2 - z1));

                result[name] = new FrameData { Name = name, StartJoint = joints[p1], EndJoint = joints[p2], Length = len };
            }
            return result;
        }


        

        //===================Property=====================

        // 读所有钢材材料（自动过滤混凝土、铝等非钢材）
        public static Dictionary<string, MaterialData> GetAllMaterials()
        {
            var result = new Dictionary<string, MaterialData>();
            int n = 0;
            string[] names = null!;
            Global.SapModel.PropMaterial.GetNameList(ref n, ref names);

            for (int i = 0; i < n; i++)
            {
                // 先判断材料类型
                eMatType matType = 0;
                int color = 0;
                string notes = "", guid = "";
                int ret = Global.SapModel.PropMaterial.GetMaterial(
                    names[i], ref matType, ref color, ref notes, ref guid);

                // 只留钢材（eMatType_Steel = 1）
                if (ret == 0 && matType == eMatType.Steel)
                {
                    var mat = GetMaterialSteel(names[i]);
                    result[names[i]] = mat;
                }
            }
            return result;
        }

        //读取Frame截面定义表，
        public static Dictionary<string, SectionBase> GetAllSections()
        {
            var result = new Dictionary<string, SectionBase>();
            int n = 0;
            string[] names = null!;
            Global.SapModel.PropFrame.GetNameList(ref n, ref names);

            for (int i = 0; i < n; i++)
            {
                try
                {
                    var sec = GetSectionData(names[i]);
                    if (sec != null) result[names[i]] = sec;
                }
                catch
                {
                    // 读不了的截面跳过（混凝土、索等非钢截面）
                }
            }
            return result;
        }

        // 读所有杆件的截面名映射，字典 {杆件名 → 截面名}
        public static Dictionary<string, string> GetFrameSectionMap()
        {
            var result = new Dictionary<string, string>();
            int n = 0;
            string[] names = null!;
            Global.SapModel.FrameObj.GetNameList(ref n, ref names);
            for (int i = 0; i < n; i++)
            {
                string section = "";
                string sAuto = "";
                Global.SapModel.FrameObj.GetSection(names[i], ref section,ref sAuto);
                result[names[i]] = section;
            }
            return result;
        }
        // 按比例 ratio 插值截面属性（门刚变截面用）

        // 建group-frame映射
        public static void BuildGroupMappings()
        {
            Global.Data.GroupToFrames.Clear();
            Global.Data.FrameToGroups.Clear();

            foreach (var g in Global.Data.AllGroups)
            {
                int count = 0;
                int[] objType = null!;
                string[] objName = null!;
                Global.SapModel.GroupDef.GetAssignments(g, ref count, ref objType, ref objName);

                var frames = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    if (objType[i] != 2) continue;   // 只要frame对象
                    string frame = objName[i];
                    frames.Add(frame);

                    if (!Global.Data.FrameToGroups.ContainsKey(frame))
                        Global.Data.FrameToGroups[frame] = new List<string>();
                    Global.Data.FrameToGroups[frame].Add(g);
                }
                Global.Data.GroupToFrames[g] = frames;
            }
        }


        //==================Private======================
        // 读截面的完整数据,返回基类（等截面→HSectionData，变截面→TaperedSectionData）
        private static SectionBase? GetSectionData(string sectionName)
        {
            int n = 0;
            string[] startSec = null!, endSec = null!;
            double[] myLength = null!;
            int[] myType = null!, ei33 = null!, ei22 = null!;
            int color = 0;
            string notes = "", guid = "";

            int ret = Global.SapModel.PropFrame.GetNonPrismatic(
                sectionName, ref n, ref startSec, ref endSec,
                ref myLength, ref myType, ref ei33, ref ei22,
                ref color, ref notes, ref guid);

            if (ret == 0)
            {
                // ===== 变截面 =====
                var Start = ReadHSection(startSec[0]);
                var End = ReadHSection(endSec[0]);
                if (Start == null || End == null) return null;
                return new TaperedSectionData
                {
                    Name = sectionName,
                    IsTapered = true,
                    MatProp = Start.MatProp,
                    Start = Start,
                    End = End,
                    EI33Type = ei33[0],
                    EI22Type = ei22[0]
                };
            }
            else
            {
                // ===== 等截面 =====
                var sec = ReadHSection(sectionName);
                if (sec == null) return null;
                sec.Name = sectionName;
                sec.IsTapered = false;
                return sec;
            }
        }
        // 读单个 H 形截面
        private static HSectionData? ReadHSection(string name)
        {
            string fileName = "", matProp = "";
            double h = 0, topB = 0, topTf = 0, tw = 0, botB = 0, botTf = 0, fillet = 0;
            int color = 0;
            string notes = "", guid = "";

            int ret1 = Global.SapModel.PropFrame.GetISection_1(
                name,
                ref fileName, ref matProp,
                ref h, ref topB, ref topTf, ref tw,
                ref botB, ref botTf, ref fillet,
                ref color, ref notes, ref guid);
            if (ret1 != 0) return null;

            double A = 0, as2 = 0, as3 = 0, J = 0, i22 = 0, i33 = 0;
            double s22 = 0, s33 = 0, z22 = 0, z33 = 0, r22 = 0, r33 = 0;
            Global.SapModel.PropFrame.GetSectProps(
                name,
                ref A, ref as2, ref as3, ref J,
                ref i22, ref i33, ref s22, ref s33,
                ref z22, ref z33, ref r22, ref r33);

            // 算形心位置（到顶部距离 yc）
            double A1 = topB * topTf;           // 上翼缘面积
            double hw = h - topTf - botTf;  // 腹板高度
            double Aweb = tw * hw;                   // 腹板面积
            double A2 = botB * botTf;           // 下翼缘面积

            double yc = (A1 * topTf / 2
                       + Aweb * (topTf + hw / 2)
                       + A2 * (topTf + hw + botTf / 2))
                       / A;

            var sec = new HSectionData
            {
                MatProp = matProp,
                H = h,
                TopB = topB,
                TopTf = topTf,
                BotB = botB,
                BotTf = botTf,
                Tw = tw,
                Fillet = fillet,
                Area = A,
                As2 = as2,
                As3 = as3,
                J = J,
                I22 = i22,
                I33 = i33,
                S22 = s22,
                S33_Top = i33 / yc,
                S33_Bot = i33 / (h - yc),
                Z22 = z22,
                Z33 = z33,
                R22 = r22,
                R33 = r33
            };
            return sec;
        }
        // 读钢材材料属性
        private static MaterialData GetMaterialSteel(string materialName)
        {
            var mat = new MaterialData { Name = materialName };

            // 根据名称判断牌号（SAP里就是"Q235"或"Q355"）
            string lower = materialName.Trim().ToUpper();
            if (lower.Contains("Q355"))
                mat.Grade = SteelGrade.Q355;
            else if (lower.Contains("Q235"))
                mat.Grade = SteelGrade.Q235;
            else
                mat.Grade = SteelGrade.Q235;   // 默认Q235

            return mat;
        }

        // 模型是否锁定（=已分析）
        public static bool IsModelLocked()
        {
            bool locked;
            return locked= Global.SapModel.GetModelIsLocked();
        }

        // 获取模型里所有 group 名
        public static List<string> GetGroupList()
        {
            int count = 0;
            string[] names = null!;
            Global.SapModel.GroupDef.GetNameList(ref count, ref names);
            return names?.Where(n => n != "ALL").ToList() ?? new List<string>();
        }



    }
}
