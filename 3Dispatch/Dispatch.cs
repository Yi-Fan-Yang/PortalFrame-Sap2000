using PortalFrame._0Sap;
using PortalFrame._1Model;
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
            // ===== 前处理 =====
            _store.Joints = _pre.GetAllJoints();
            _store.Frames = _pre.GetAllFrames();
            _store.Sections = _pre.GetAllSections();
            _store.Materials = _pre.GetAllMaterials();
            _store.FrameSectionMap = _pre.GetFrameSectionMap();

            // ===== 后处理：组合列表 =====
            _store.Combos = _post.GetAllComboNames();

            // ===== 后处理：所有组合的内力 =====
            foreach (var combo in _store.Combos)
            {
                _store.Forces[combo] = _post.GetAllFrameForces(combo);
            }
            // ===== 把杆件-截面-材料串起来 =====
            _store.Members.Clear();
            foreach (var kv in _store.Frames)
            {
                string frameName = kv.Key;
                string secName = _store.FrameSectionMap[frameName];
                var sec = _store.Sections[secName];
                var mat = _store.Materials[sec.MatProp];

                _store.Members[frameName] = new MemberData
                {
                    Name = frameName,
                    SectionName = secName,
                    Frame = kv.Value,
                    Section = sec,
                    Material = mat
                };
            }

        }
    }
}
