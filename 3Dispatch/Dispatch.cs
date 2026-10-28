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
                // ===== 清空旧数据 =====
                Global.Data.Joints.Clear();
                Global.Data.Frames.Clear();
                Global.Data.Sections.Clear();
                Global.Data.Materials.Clear();
                Global.Data.FrameSectionMap.Clear();
                Global.Data.Members.Clear();
                Global.Data.AllCombos.Clear();
                Global.Data.Forces.Clear();

                // ===== 读原始数据 =====
                Global.Data.Joints = PreReader.GetAllJoints();
                Global.Data.Frames = PreReader.GetAllFrames(Global.Data.Joints);
                Global.Data.Sections = PreReader.GetAllSections();
                Global.Data.Materials = PreReader.GetAllMaterials();
                Global.Data.FrameSectionMap = PreReader.GetFrameSectionMap();
                Global.Data.AllCombos = PostReader.GetAllComboNames();
                PreReader.BuildGroupMappings();
                foreach (var combo in Global.Data.AllCombos)
                {
                    Global.Data.Forces[combo] = PostReader.GetAllFrameForces(combo);
                }

                // ===== 数据预处理 =====
                MemberBuilder.BuildMembers();
                // 读持久化的首选项和覆盖项，分发到所有构件的结果类
                Persistence.LoadAll();

            }
            catch (Exception ex)
            {
                MessageBox.Show("读取失败：" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        public static void RunCheck(HashSet<string>? selectedMembers = null)
        {
            try
            {
                // 如果没选，就算全部
                IEnumerable<string> memberNames;
                if (selectedMembers == null || selectedMembers.Count == 0)
                {
                    memberNames = Global.Data.Members.Keys;
                }
                else
                {
                    memberNames = selectedMembers;
                }

                if (Global.Data.AllCombos.Count == 0)
                {
                    MessageBox.Show("请先选择组合", "提示");
                    return;
                }

                foreach (var memberName in memberNames)
                {
                    if (!Global.Data.Members.TryGetValue(memberName, out var member)) continue;
                    if (!member.IsCheckable) continue;   // 不参与验算的跳过
                    CheckRunner.CheckMember(member);
                }
            }
            catch (Exception ex)
            {
                // 把所有错误都弹出来，就不会直接崩了
                MessageBox.Show($"验算出错：{ex.Message}\n\n{ex.StackTrace}", "错误");
            }
        }
        
        //获取对应组合的构件内力
        public static Dictionary<string, Dictionary<double, ForceData>> GetMemberForces(string memberName)
        {
            var result = new Dictionary<string, Dictionary<double, ForceData>>();

            foreach (var comboName in Global.Data.AllCombos)
            {
                if (Global.Data.Forces.ContainsKey(comboName) && Global.Data.Forces[comboName].ContainsKey(memberName))
                {
                    result[comboName] = Global.Data.Forces[comboName][memberName];
                }
            }
            return result;
        }

        // 根据验算类型，返回所有构件的利用率字典（用来画布着色）
        public static Dictionary<string, MemberDisplayInfo> GetMemberDisplayInfo(string checkType)
        {
            var result = new Dictionary<string, MemberDisplayInfo>();

            foreach (var (name, member) in Global.Data.Members)
            {
                if (!member.IsCheckable) continue;
                var cr = member.CheckResult;
                double util = checkType switch
                {
                    "汇总" => cr.Worst.MaxUtil,
                    CheckTypes.Flexure => cr.FlexureStrength.GetWorst().MaxUtil,
                    CheckTypes.Shear => cr.ShearStrength.GetWorst().MaxUtil,
                    CheckTypes.Slenderness => Math.Max(cr.Slenderness.UtilX.Value, cr.Slenderness.UtilY.Value),
                    CheckTypes.LocalStability => cr.LocalStability.MaxUtil,
                    CheckTypes.PhiB => cr.PhiBResult.MinPhiB,
                    // 柱专属
                    CheckTypes.ColumnInPlane when member.Type == MemberType.Column => cr.InPlaneStability.MaxUtil,
                    CheckTypes.ColumnOutPlane when member.Type == MemberType.Column => cr.OutPlaneStability.MaxUtil,
                    // 梁专属
                    CheckTypes.BeamLateral when member.Type == MemberType.Beam => cr.LateralStability.MaxUtil,
                    _ => 0
                };

                string text = checkType switch
                {
                    CheckTypes.Slenderness => $"{cr.Slenderness.UtilX.Value:F2}/{cr.Slenderness.UtilY.Value:F2}",
                    CheckTypes.LocalStability => $"{cr.LocalStability.FlangeUtil_Top.Value:F2}/{cr.LocalStability.FlangeUtil_Bot.Value:F2}/{cr.LocalStability.WebUtil.Value:F2}",
                    _ => util.ToString("F2")
                };
                double utilLimit = checkType switch
                {
                    CheckTypes.Slenderness or CheckTypes.LocalStability => 1.0,   // 长细比/板件宽厚比固定1.0
                    _ => cr.General.UtilLimit.Value,                               // 强度/稳定/汇总用UtilLimit
                };
                result[name] = new MemberDisplayInfo { Util = util, UtilLimit = utilLimit , Text = text, };
            }

            return result;
        }

    }
}
