using CSiAPIv1;
using PortalFrame._1Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._0Sap
{
    public class PreReader
    {
        private cSapModel _sapModel;

        // 构造时把 SAP 模型交进来
        public PreReader(ref cSapModel sapModel)
        {
            _sapModel = sapModel;
        }


       // =================Geometry=======================
        // 读所有节点：名字 + 坐标
        public Dictionary<string, JointData> GetAllJoints()
        {
            int count = 0;
            string[] names = null!;
            _sapModel.PointObj.GetNameList(ref count, ref names);

            var result = new Dictionary<string, JointData>();
            foreach (string name in names)
            {
                double x = 0, y = 0, z = 0;
                // 问 SAP：这个节点的坐标是多少
                _sapModel.PointObj.GetCoordCartesian(name, ref x, ref y, ref z);
                result[name] = new JointData { Name = name, X = x, Y = y, Z = z };
            }
            return result;
        }

        // 读所有杆件：名字 + 两端节点
        public Dictionary<string,FrameData> GetAllFrames()
        {
            int count = 0;
            string[] names = null!;
            _sapModel.FrameObj.GetNameList(ref count, ref names);

            var result = new Dictionary<string, FrameData>();
            foreach (string name in names)
            {
                string p1 = "", p2 = "";
                // 问 SAP：这根杆件的两端是哪两个节点
                _sapModel.FrameObj.GetPoints(name, ref p1, ref p2);
                result[name]=new FrameData { Name = name, StartJoint = p1, EndJoint = p2 };
            }
            return result;
        }




        //===================Property=====================

        // 读所有钢材材料（自动过滤混凝土、铝等非钢材）
        public Dictionary<string, MaterialData> GetAllMaterials()
        {
            var result = new Dictionary<string, MaterialData>();
            int n = 0;
            string[] names = null!;
            _sapModel.PropMaterial.GetNameList(ref n, ref names);

            for (int i = 0; i < n; i++)
            {
                // 先判断材料类型
                eMatType matType = 0;
                int color = 0;
                string notes = "", guid = "";
                int ret = _sapModel.PropMaterial.GetMaterial(
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

        //读取Frame截面定义表，并自动分类（变截面/等截面）
        public Dictionary<string, SectionData> GetAllSections()
        {
            var result = new Dictionary<string, SectionData>();
            int n = 0;
            string[] names = null!;
            _sapModel.PropFrame.GetNameList(ref n, ref names);

            for (int i = 0; i < n; i++)
            {
                try
                {
                    var sec = GetSectionData(names[i]);
                    result[names[i]] = sec;
                }
                catch
                {
                    // 读不了的截面跳过（混凝土、索等非钢截面）
                }
            }
            return result;
        }

        // 读所有杆件的截面名映射，字典 {杆件名 → 截面名}
        public Dictionary<string, string> GetFrameSectionMap()
        {
            var result = new Dictionary<string, string>();
            int n = 0;
            string[] names = null!;
            _sapModel.FrameObj.GetNameList(ref n, ref names);

            for (int i = 0; i < n; i++)
            {
                string section = "";
                string sAuto = "";
                _sapModel.FrameObj.GetSection(names[i], ref section,ref sAuto);
                result[names[i]] = section;
            }
            return result;
        }



        //==================Private======================
        // 读截面的完整数据
        private SectionData GetSectionData(string sectionName)
        {
            var data = new SectionData { Name = sectionName };

            int n = 0;
            string[] startSec = null!, endSec = null!;
            double[] myLength = null!;
            int[] myType = null!, ei33 = null!, ei22 = null!;
            int color = 0;
            string notes = "", guid = "";

            int ret = _sapModel.PropFrame.GetNonPrismatic(
                sectionName, ref n, ref startSec, ref endSec,
                ref myLength, ref myType, ref ei33, ref ei22,
                ref color, ref notes, ref guid);

            if (ret == 0)
            {
                // ===== 变截面 =====
                data.IsTapered = true;
                data.EI33Type = ei33[0];
                data.EI22Type = ei22[0];

                GetISection(startSec[0],
                    out data.H1, out data.TopB1, out data.TopTf1, out data.BotB1, out data.BotTf1,
                    out data.Tw1, out data.Fillet1, out data.MatProp,
                    out data.A1, out data.As2_1, out data.As3_1, out data.J1,
                    out data.I22_1, out data.I33_1, out data.S22_1, out data.S33_1,
                    out data.Z22_1, out data.Z33_1, out data.R22_1, out data.R33_1);

                GetISection(endSec[0],
                    out data.H2, out data.TopB2, out data.TopTf2, out data.BotB2, out data.BotTf2,
                    out data.Tw2, out data.Fillet2, out _,
                    out data.A2, out data.As2_2, out data.As3_2, out data.J2,
                    out data.I22_2, out data.I33_2, out data.S22_2, out data.S33_2,
                    out data.Z22_2, out data.Z33_2, out data.R22_2, out data.R33_2);
            }
            else
            {
                // ===== 等截面 =====
                data.IsTapered = false;
                GetISection(sectionName,
                    out data.H1, out data.TopB1, out data.TopTf1, out data.BotB1, out data.BotTf1,
                    out data.Tw1, out data.Fillet1, out data.MatProp,
                    out data.A1, out data.As2_1, out data.As3_1, out data.J1,
                    out data.I22_1, out data.I33_1, out data.S22_1, out data.S33_1,
                    out data.Z22_1, out data.Z33_1, out data.R22_1, out data.R33_1);

                // 大端=小端
                data.H2 = data.H1; data.TopB2 = data.TopB1; data.TopTf2 = data.TopTf1;
                data.BotB2 = data.BotB1; data.BotTf2 = data.BotTf1;
                data.Tw2 = data.Tw1; data.Fillet2 = data.Fillet1;
                data.A2 = data.A1; data.As2_2 = data.As2_1; data.As3_2 = data.As3_1;
                data.J2 = data.J1; data.I22_2 = data.I22_1; data.I33_2 = data.I33_1;
                data.S22_2 = data.S22_1; data.S33_2 = data.S33_1;
                data.Z22_2 = data.Z22_1; data.Z33_2 = data.Z33_1;
                data.R22_2 = data.R22_1; data.R33_2 = data.R33_1;
            }

            return data;
        }
        // 读钢材材料属性
        private MaterialData GetMaterialSteel(string materialName)
        {
            var mat = new MaterialData { Name = materialName };

            double fy = 0, fu = 0, eFy = 0, eFu = 0;
            int ssType = 0, ssHysType = 0;
            double strainHardening = 0, strainMax = 0, strainRupture = 0, finalSlope = 0;

            _sapModel.PropMaterial.GetOSteel_1(
                materialName,
                ref fy, ref fu, ref eFy, ref eFu,
                ref ssType, ref ssHysType,
                ref strainHardening, ref strainMax, ref strainRupture, ref finalSlope);

            mat.Fy = fy;
            mat.Fu = fu;
            return mat;
        }
        // 读单个 H 形截面完整数据
        private void GetISection(string name,
            out double h, out double topB, out double topTf,
            out double botB, out double botTf,
            out double tw, out double fillet, out string matProp,
            out double A, out double as2, out double as3, out double J,
            out double i22, out double i33,
            out double s22, out double s33,
            out double z22, out double z33,
            out double r22, out double r33)
        {
            string fileName = "";
            h = topB = topTf = botB = botTf = tw = fillet = 0;
            matProp = "";
            int color = 0;
            string notes = "", guid = "";

            _sapModel.PropFrame.GetISection_1(
                name,
                ref fileName, ref matProp,
                ref h, ref topB, ref topTf, ref tw,
                ref botB, ref botTf, ref fillet,
                ref color, ref notes, ref guid);   // color/notes/guid 忽略

            A = as2 = as3 = J = i22 = i33 = s22 = s33 = z22 = z33 = r22 = r33 = 0;

            _sapModel.PropFrame.GetSectProps(
                name,
                ref A, ref as2, ref as3, ref J,
                ref i22, ref i33, ref s22, ref s33,
                ref z22, ref z33, ref r22, ref r33);
        }




    }
}
