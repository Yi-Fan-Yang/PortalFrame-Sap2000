using CSiAPIv1;
using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2Check;
using PortalFrame._5data;


namespace PortalFrame._3Dispatch
{
    public class Dispatch
    {
        private PreReader _pre;
        private PostReader _post;
        private PreData _preData;
        private Preferences _prefer ;
        private OverWrites _writes ;
        private DesignCombos _Combos ;
        private PostData _postData ;

        public Dispatch(PreReader pre, PostReader post, PreData preData, Preferences prefer, OverWrites writes, DesignCombos Combos,PostData postData)
        {
            _pre = pre;
            _post = post;
            _preData = preData;
            _prefer = prefer;
            _writes = writes;
            _Combos = Combos;
            _postData = postData;
        }

        // 读取模型：一次性把所有数据拉进 PreData
        public void ReadAllData()
        {
            try 
            {
                // ===== 1. 读原始数据 =====
                _preData.Joints = _pre.GetAllJoints();
                _preData.Frames = _pre.GetAllFrames();
                _preData.Sections = _pre.GetAllSections();
                _preData.Materials = _pre.GetAllMaterials();
                _preData.FrameSectionMap = _pre.GetFrameSectionMap();
                _preData.Combos = _post.GetAllComboNames();

                foreach (var combo in _preData.Combos)
                {
                    _preData.Forces[combo] = _post.GetAllFrameForces(combo);
                }

                // ===== 2. 数据预处理 =====
                var builder = new MemberBuilder(_preData);
                builder.BuildMembers();
                builder.InterpolateStations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取失败：" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }


        public void RunCheck(HashSet<string>? selectedMembers = null)
        {
            _postData.ColumnResults.Clear();
            _postData.BeamResults.Clear();

            // 如果没选，就算全部
            IEnumerable<string> memberNames;
            if (selectedMembers == null || selectedMembers.Count == 0)
            {
                memberNames = _preData.Members.Keys;
            }
            else
            {
                memberNames = selectedMembers;
            }

            foreach (var memberName in memberNames)
            {
                if (!_preData.Members.TryGetValue(memberName, out var member)) continue;

                var forcesByCombo = GetMemberForces(memberName);
                if (forcesByCombo.Count == 0) continue;

                if (member.Type == MemberType.Column)
                {
                    var result = CheckRunner.CheckColumn(member, forcesByCombo, _prefer, _writes);
                    _postData.ColumnResults[memberName] = result;
                }
                else
                {
                    var result = CheckRunner.CheckBeam(member, forcesByCombo, _prefer, _writes);
                    _postData.BeamResults[memberName] = result;
                }
            }
        }
        //获取对应组合的构件内力
        public Dictionary<string, List<ForceData>> GetMemberForces(string memberName)
        {
            var result = new Dictionary<string, List<ForceData>>();

            foreach (var comboName in _Combos.SelectedCombos)
            {
                if (_preData.Forces.ContainsKey(comboName) && _preData.Forces[comboName].ContainsKey(memberName))
                {
                    result[comboName] = _preData.Forces[comboName][memberName];
                }
            }
            return result;
        }
        
        /// <summary>
        /// 根据验算类型，返回所有构件的利用率字典（用来画布着色）
        /// </summary>
        public Dictionary<string, double> GetMemberUtilsByCheckType(string checkType)
        {
            var result = new Dictionary<string, double>();

            // 柱
            foreach (var (name, col) in _postData.ColumnResults)
            {
                double util = checkType switch
                {
                    "汇总" => col.Worst.MaxUtil,
                    "压弯/拉弯" => col.FlexureStrength.MaxUtil,
                    "抗剪" => col.ShearStrength.MaxUtil,
                    "平面内稳定" => col.InPlaneStability.MaxUtil,
                    "平面外稳定" => col.OutPlaneStability.MaxUtil,
                    "长细比" => col.Slenderness.MaxUtil,
                    "局部稳定" => col.LocalStability.MaxUtil,
                    _ => 0
                };
                result[name] = util;
            }

            // 梁
            foreach (var (name, beam) in _postData.BeamResults)
            {
                double util = checkType switch
                {
                    "汇总" => beam.Worst.MaxUtil,
                    "压弯/拉弯" => beam.FlexureStrength.MaxUtil,
                    "抗剪" => beam.ShearStrength.MaxUtil,
                    "整体稳定" => beam.LateralStability.MaxUtil,
                    "长细比" => beam.Slenderness.MaxUtil,
                    "局部稳定" => beam.LocalStability.MaxUtil,
                    _ => 0
                };
                result[name] = util;
            }

            return result;
        }

    }
}
