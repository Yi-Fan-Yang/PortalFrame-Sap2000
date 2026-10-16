using CSiAPIv1;

namespace PortalFrame._0Sap
{
    // 单根柱的Kz计算属性（读一次，多处用）
    public class ColumnInfo
    {
        public string ColName = "";   // 柱名
        public string BaseJoint = "";   // 柱底节点
        public string TopJoint = "";     // 柱顶节点
        public bool IIsTop;              // I端是不是上端
        public double Angle;             // 局部轴转角（度）
        public double Kz;                //梁对柱的转动约束
    }

    public class KzCalculator
    {
        private readonly cSapModel _sap;
        private readonly string loadName = "KZ_CASE";
        private readonly double unitLoad = 1e8;
        public KzCalculator(cSapModel sap)
        {
            _sap = sap;
        }

        // 主入口：返回 柱名 → Kz 字典
        public void Calc(List<string> selectedGroups,List<ColumnInfo> cols)
        {
            if (selectedGroups.Count == 0) return;

            // ① 取出这些 group 里的柱名
            GetColumnsInGroups(selectedGroups,cols);
            if (cols.Count == 0) return;

            // ② 先遍历一遍，把每根柱的属性算好存字典
            BuildColumnInfo(cols);

            // ③ 记下原模型路径
            string filepath = _sap.GetModelFilename(true);

            // ④ 临时文件
            string tmpPath = Path.Combine(Path.GetTempPath(), "pf_kz_tmp.sdb");

            try
            {
                // ⑤ 存副本 → 打开副本
                _sap.File.Save(tmpPath);
                _sap.File.OpenFile(tmpPath);
                // 统一单位：N, mm（防止 M=1e8 单位错位）
                _sap.SetPresentUnits(eUnits.N_mm_C);

                // ⑥ 建荷载模式（自动建工况）
                _sap.LoadPatterns.Add(loadName, eLoadPatternType.Other, 0, true);

                // ⑦ 改模型、加力矩
                ModifyModel(cols);

                // ⑧ 跑分析
                RunAnalysis();

                // ⑨ 读转角、算Kz
                 FillKz(cols);
            }
            finally
            {
                // ⑩ 恢复原模型 + 删临时文件
                _sap.File.OpenFile(filepath);
                // 删所有以临时文件名开头的文件（.sdb、.sdb~、.sdb.log、.smdb 等）
                string tmpDir = Path.GetDirectoryName(tmpPath)!;
                string tmpPrefix = Path.GetFileNameWithoutExtension(tmpPath);   // "pf_kz_tmp"
                foreach (var f in Directory.GetFiles(tmpDir, tmpPrefix + "*"))
                {
                    try { File.Delete(f); } catch { }
                }
            }
        }




        //================================ 各个步骤的实现 ==================================
        // 从选中 group 里找出所有柱，创建 ColumnInfo 对象（只填 ColName，其他字段后续填）
        private void GetColumnsInGroups(List<string> groupNames, List<ColumnInfo> cols)
        {
            foreach (var g in groupNames)
            {
                int count = 0;
                int[] objType = null!;
                string[] objName = null!;

                _sap.GroupDef.GetAssignments(g, ref count, ref objType, ref objName);

                for (int i = 0; i < count; i++)
                {
                    if (objType[i] != 2) continue;   // 只要frame对象

                    // 去重：同一根柱可能在多个 group
                    if (cols.Any(c => c.ColName == objName[i])) continue;

                    cols.Add(new ColumnInfo { ColName = objName[i] });
                }
            }
        }


        // 遍历每根柱，算好节点、转角，存字典
        private void BuildColumnInfo(List<ColumnInfo> cols)
        {
            foreach (var info in cols)
            {
                // 两端节点
                string jointI = "", jointJ = "";
                _sap.FrameObj.GetPoints(info.ColName, ref jointI, ref jointJ);

                // 比较Z坐标
                double xi = 0, yi = 0, zi = 0;
                double xj = 0, yj = 0, zj = 0;
                _sap.PointObj.GetCoordCartesian(jointI, ref xi, ref yi, ref zi);
                _sap.PointObj.GetCoordCartesian(jointJ, ref xj, ref yj, ref zj);

                info.BaseJoint = zi < zj ? jointI : jointJ;
                info.TopJoint = zi < zj ? jointJ : jointI;
                info.IIsTop = zi > zj;

                // 局部轴转角
                bool advanced = false;
                _sap.FrameObj.GetLocalAxes(info.ColName, ref info.Angle, ref advanced);
            }
        }

        // 修改模型：柱底固定、柱顶端部释放、节点局部轴对齐、加力矩
        private void ModifyModel(List<ColumnInfo> cols)
        {
            foreach (var info in cols)
            {
                // ① 柱底节点：六自由度全约束
                bool[] baseRestraint = { true, true, true, true, true, true };
                _sap.PointObj.SetRestraint(info.BaseJoint, ref baseRestraint);

                // ② 柱顶端部释放（上端释放 M2、M3）
                bool[] releaseTop = { false, false, false, false, true, true };
                bool[] releaseBottom = { false, false, false, false, false, false };
                double[] spring = { 0, 0, 0, 0, 0, 0 };

                bool[] startRelease = info.IIsTop ? releaseTop : releaseBottom;
                bool[] endRelease = info.IIsTop ? releaseBottom : releaseTop;
                _sap.FrameObj.SetReleases(info.ColName, ref startRelease, ref endRelease,
                                          ref spring, ref spring);

                // ③ 柱顶节点局部轴：绕Z轴转 angle，使局部Y轴跟柱强轴对齐
                _sap.PointObj.SetLocalAxes(info.TopJoint, info.Angle, 0, 0);

                // ④ 柱顶节点加单位力矩，局部坐标系，My
                double[] loadValues = new double[6];
                loadValues[4] = unitLoad;   // My
                _sap.PointObj.SetLoadForce(info.TopJoint, loadName, ref loadValues, true, "Local");
            }
        }


        // 运行分析：只跑 KZ_CASE
        private void RunAnalysis()
        {
            int count = 0;
            string[] caseNames = null!;
            _sap.LoadCases.GetNameList_1(ref count, ref caseNames);

            if (caseNames != null)
            {
                foreach (var caseName in caseNames)
                {
                    if (caseName != loadName)
                        _sap.Analyze.SetRunCaseFlag(caseName, false);
                }
            }

            _sap.Analyze.RunAnalysis();
        }

        // 读转角，Kz=M/θ，填到 info.Kz
        private void FillKz(List<ColumnInfo> cols)
        {
            // 结果输出只看 KZ_CASE
            _sap.Results.Setup.DeselectAllCasesAndCombosForOutput();
            _sap.Results.Setup.SetCaseSelectedForOutput(loadName);

            foreach (var info in cols)
            {
                int n = 0;
                string[] obj = null!;
                string[] elm = null!;
                string[] loadCase = null!;
                string[] stepType = null!;
                double[] stepNum = null!;
                double[] u1 = null!;
                double[] u2 = null!;
                double[] u3 = null!;
                double[] r1 = null!;
                double[] r2 = null!;
                double[] r3 = null!;

                _sap.Results.JointDispl(info.TopJoint, eItemTypeElm.ObjectElm, ref n,
                                         ref obj, ref elm, ref loadCase, ref stepType, ref stepNum,
                                         ref u1, ref u2, ref u3, ref r1, ref r2, ref r3);

                if (n == 0 || r2 == null) continue;

                // R2 = 绕节点Y轴转角
                double theta = r2[0];
                if (Math.Abs(theta) > 1e-12)
                    info.Kz = unitLoad / Math.Abs(theta);
            }
        }

    }
}
