using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2Check;
using PortalFrame._5data;
using PortalFrame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._3Dispatch
{
    public class Dispatch
    {
        private PreReader _pre;
        private PostReader _post;
        private DataStore _store;

        public Dispatch(PreReader pre, PostReader post, DataStore store)
        {
            _pre = pre;
            _post = post;
            _store = store;
        }

        // 读取模型：一次性把所有数据拉进 DataStore
        public void ReadAllData()
        {
            // ===== 1. 读原始数据 =====
            _store.Joints = _pre.GetAllJoints();
            _store.Frames = _pre.GetAllFrames();
            _store.Sections = _pre.GetAllSections();
            _store.Materials = _pre.GetAllMaterials();
            _store.FrameSectionMap = _pre.GetFrameSectionMap();
            _store.Combos = _post.GetAllComboNames();

            foreach (var combo in _store.Combos)
            {
                _store.Forces[combo] = _post.GetAllFrameForces(combo);
            }

            // ===== 2. 数据预处理 =====
            var builder = new MemberBuilder(_store);
            builder.BuildMembers();
            builder.InterpolateStations();
        }

        // 运行验算：遍历所有杆件所有测站，调抗弯强度
        public void RunCheck(string comboName)
        {
            var flexure = new FlexureCheck();
            _store.CheckResults.Clear();

            foreach (var member in _store.Members.Values)
            {
                var stations = _store.Forces[comboName][member.Name];
                var results = new List<CheckResult>();

                foreach (var st in stations)
                {
                    // 拿该测站的截面属性
                    HSectionData sec;
                    if (member.Section.IsTapered)
                        sec = member.StationSections[st.Station];
                    else
                        sec = (HSectionData)member.Section;

                    // 验算抗弯强度
                    var check = flexure.Check(st.M3, sec.S33, member.Material.Fy);
                    check.Station = st.Station;
                    results.Add(check);
                }

                _store.CheckResults[member.Name] = results;
            }
        }

    }
}
