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
                // 读两端坐标算长度
                double x1 = 0, y1 = 0, z1 = 0;
                double x2 = 0, y2 = 0, z2 = 0;
                _sapModel.PointObj.GetCoordCartesian(p1, ref x1, ref y1, ref z1);
                _sapModel.PointObj.GetCoordCartesian(p2, ref x2, ref y2, ref z2);
                double len = Math.Sqrt(
                    (x2 - x1) * (x2 - x1) +
                    (y2 - y1) * (y2 - y1) +
                    (z2 - z1) * (z2 - z1));

                result[name] = new FrameData { Name = name, StartJoint = p1, EndJoint = p2, Length = len };
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
        public Dictionary<string, SectionBase> GetAllSections()
        {
            var result = new Dictionary<string, SectionBase>();
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
        // 按比例 ratio 插值截面属性（门刚变截面用）



        //==================Private======================
        // 读截面的完整数据,返回基类（等截面→HSectionData，变截面→TaperedSectionData）
        private SectionBase GetSectionData(string sectionName)
        {
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
                var Start = ReadHSection(startSec[0]);
                var End = ReadHSection(endSec[0]);
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
                sec.Name = sectionName;
                sec.IsTapered = false;
                return sec;
            }
        }
        // 读单个 H 形截面
        private HSectionData ReadHSection(string name)
        {
            string fileName = "", matProp = "";
            double h = 0, topB = 0, topTf = 0, tw = 0, botB = 0, botTf = 0, fillet = 0;
            int color = 0;
            string notes = "", guid = "";

            _sapModel.PropFrame.GetISection_1(
                name,
                ref fileName, ref matProp,
                ref h, ref topB, ref topTf, ref tw,
                ref botB, ref botTf, ref fillet,
                ref color, ref notes, ref guid);

            double A = 0, as2 = 0, as3 = 0, J = 0, i22 = 0, i33 = 0;
            double s22 = 0, s33 = 0, z22 = 0, z33 = 0, r22 = 0, r33 = 0;
            _sapModel.PropFrame.GetSectProps(
                name,
                ref A, ref as2, ref as3, ref J,
                ref i22, ref i33, ref s22, ref s33,
                ref z22, ref z33, ref r22, ref r33);

            return new HSectionData
            {
                MatProp = matProp,
                H = h,
                TopB = topB,
                TopTf = topTf,
                BotB = botB,
                BotTf = botTf,
                Tw = tw,
                Fillet = fillet,
                A = A,
                As2 = as2,
                As3 = as3,
                J = J,
                I22 = i22,
                I33 = i33,
                S22 = s22,
                S33 = s33,
                Z22 = z22,
                Z33 = z33,
                R22 = r22,
                R33 = r33
            };
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




    }
}
