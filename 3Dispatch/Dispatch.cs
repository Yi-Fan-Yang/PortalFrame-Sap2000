using CSiAPIv1;
using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2Check;
using PortalFrame._2CheckResults;
using PortalFrame._4UI;
using PortalFrame._5data;


namespace PortalFrame._3Dispatch
{
    public static class Dispatch
    {

        // 读取模型：一次性把所有数据拉进 PreData
        public static void ReadAllData()
        {
            try 
            {
                Global.SapModel.SetPresentUnits(eUnits.N_mm_C);
                // 未锁定 = 未分析，不读取
                if (!PreReader.IsModelLocked())
                {
                    MessageBox.Show("请先在SAP中完成分析后，再读取数据", "提示");
                    return;
                }
                // ===== 先清空旧数据 =====
                Global.ModelData.Joints.Clear();
                Global.ModelData.Frames.Clear();
                Global.ModelData.Sections.Clear();
                Global.ModelData.Materials.Clear();
                Global.ModelData.FrameSectionMap.Clear();
                Global.ModelData.Members.Clear();
                Global.ModelData.AllCombos.Clear();
                Global.ModelData.Forces.Clear();

                // ===== 1. 读原始数据 =====
                Global.ModelData.Joints = PreReader.GetAllJoints();
                Global.ModelData.Frames = PreReader.GetAllFrames(Global.ModelData.Joints);
                Global.ModelData.Sections = PreReader.GetAllSections();
                Global.ModelData.Materials = PreReader.GetAllMaterials();
                Global.ModelData.FrameSectionMap = PreReader.GetFrameSectionMap();
                Global.ModelData.AllCombos = PostReader.GetAllComboNames();

                foreach (var combo in Global.ModelData.AllCombos)
                {
                    Global.ModelData.Forces[combo] = PostReader.GetAllFrameForces(combo);
                }

                // ===== 2. 数据预处理 =====
                MemberBuilder.BuildMembers();

                // ===== 弹窗选 group（用于计算 Kz） =====
                var groupNames = PreReader.GetGroupList();
                var dlg = new GroupSelectForm(Global.Groups, Global.ModelData.Joints.Count, Global.ModelData.Members.Count, groupNames);
                dlg.ShowDialog();
                MemberBuilder.GetKz(Global.SapModel, Global.Groups);
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取失败：" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }


        public static void RunCheck(HashSet<string>? selectedMembers = null)
        {

            // 如果没选，就算全部
            IEnumerable<string> memberNames;
            if (selectedMembers == null || selectedMembers.Count == 0)
            {
                memberNames = Global.ModelData.Members.Keys;
            }
            else
            {
                memberNames = selectedMembers;
            }

            if (Global.ModelData.AllCombos.Count == 0)
            {
                MessageBox.Show("请先选择组合", "提示");
                return;
            }

            foreach (var memberName in memberNames)
            {
                if (!Global.ModelData.Members.TryGetValue(memberName, out var member)) continue;
                CheckRunner.CheckMember(member);

            }
        }
        //获取对应组合的构件内力
        public static Dictionary<string, List<ForceData>> GetMemberForces(string memberName)
        {
            var result = new Dictionary<string, List<ForceData>>();

            foreach (var comboName in Global.ModelData.AllCombos)
            {
                if (Global.ModelData.Forces.ContainsKey(comboName) && Global.ModelData.Forces[comboName].ContainsKey(memberName))
                {
                    result[comboName] = Global.ModelData.Forces[comboName][memberName];
                }
            }
            return result;
        }

        /// <summary>
        /// 根据验算类型，返回所有构件的利用率字典（用来画布着色）
        /// </summary>
        public static Dictionary<string, MemberDisplayInfo> GetMemberDisplayInfo(string checkType)
        {
            var result = new Dictionary<string, MemberDisplayInfo>();

            foreach (var (name, member) in Global.ModelData.Members)
            {
                var cr = member.CheckResult;
                double util = checkType switch
                {
                    "汇总" => cr.Worst.MaxUtil,
                    "压弯/拉弯" => cr.FlexureStrength.MaxUtil,
                    "抗剪" => cr.ShearStrength.MaxUtil,
                    "长细比" => Math.Max(cr.Slenderness.UtilX.Value, cr.Slenderness.UtilY.Value),
                    "局部稳定" => cr.LocalStability.MaxUtil,
                    // 柱专属
                    "平面内稳定" when member.Type == MemberType.Column => ((ColumnCheckResult)cr).InPlaneStability.MaxUtil,
                    "平面外稳定" when member.Type == MemberType.Column => ((ColumnCheckResult)cr).OutPlaneStability.MaxUtil,
                    // 梁专属
                    "整体稳定" when member.Type == MemberType.Beam => ((BeamCheckResult)cr).LateralStability.MaxUtil,
                    _ => 0
                };

                string text = checkType switch
                {
                    "长细比" => $"{cr.Slenderness.UtilX.Value:F2}/{cr.Slenderness.UtilY.Value:F2}",
                    "局部稳定" => $"{cr.LocalStability.FlangeUtil_Top.Value:F2}/{cr.LocalStability.WebUtil.Value:F2}",
                    _ => util.ToString("F2")
                };

                result[name] = new MemberDisplayInfo { Util = util, Text = text };
            }

            return result;
        }

    }
}
