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


        public void RunCheck()
        {
            _postData.ColumnResults.Clear();
            _postData.BeamResults.Clear();

            foreach (var member in _preData.Members.Values)
            {
                // 取内力
                var forcesByCombo = GetMemberForces(member.Name);

                if (member.Type == MemberType.Column)
                {
                    var result = CheckRunner.CheckColumn(member, forcesByCombo);
                    _postData.ColumnResults[member.Name] = result;
                }
                else
                {
                    var result = CheckRunner.CheckBeam(member, forcesByCombo);
                    _postData.BeamResults[member.Name] = result;
                }
            }
        }

        public Dictionary<string, List<ForceData>> GetMemberForces(string memberName)
        {
            var result = new Dictionary<string, List<ForceData>>();

            foreach (var comboName in _Combos.SelectedCombos)
            {
                // 调 PostReader 取这个构件在这个组合下的所有测站内力
                // 具体方法后面再细化，先占位
                var forces = new List<ForceData>();
                result[comboName] = forces;
            }

            return result;
        }

    }
}
