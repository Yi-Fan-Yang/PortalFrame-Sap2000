using CSiAPIv1;
using PortalFrame._1Model;

namespace PortalFrame._0Sap
{
    // 后处理读取：读计算结果（内力、位移、组合结果等）
    // 后面读测站内力、读组合结果都放这里
    public static class PostReader
    {
        // 读所有荷载组合名
        public static List<string> GetAllComboNames()
        {
            List<string> names = new List<string>();
            int n = 0;
            string[] arr = null!;
            Global.SapModel.RespCombo.GetNameList(ref n, ref arr);
            for (int i = 0; i < n; i++)
                names.Add(arr[i]);
            return names;
        }

        // 读某组合下所有杆件的测站内力
        public static Dictionary<string, List<ForceData>> GetAllFrameForces(string comboName)
        {
            var result = new Dictionary<string, List<ForceData>>();

            // 先拿所有杆件名
            int nFrame = 0;
            string[] frameNames = null!;
            Global.SapModel.FrameObj.GetNameList(ref nFrame, ref frameNames);

            foreach (var frameName in frameNames)
            {
                var forces = GetFrameForces(frameName, comboName);
                if (forces.Count > 0)
                    result[frameName] = forces;
            }
            return result;
        }

        // 读某杆件在某组合下的测站内力
        private static List<ForceData> GetFrameForces(string frameName, string comboName)
        {
            var results = new List<ForceData>();

            int n = 0;
            string[] obj = null!, elm = null!, loadCase = null!, stepType = null!;
            double[] objSta = null!, elmSta = null!, stepNum = null!;
            double[] P = null!, V2 = null!, V3 = null!, T = null!, M2 = null!, M3 = null!;

            Global.SapModel.Results.FrameForce(
                frameName, eItemTypeElm.ObjectElm,
                ref n,
                ref obj, ref objSta,
                ref elm, ref elmSta,
                ref loadCase, ref stepType, ref stepNum,
                ref P, ref V2, ref V3, ref T, ref M2, ref M3);

            // 过滤出我们要的组合
            for (int i = 0; i < n; i++)
            {
                if (loadCase[i] == comboName)
                {
                    results.Add(new ForceData
                    {
                        Station = objSta[i],
                        P = P[i],
                        V2 = V2[i],
                        V3 = V3[i],
                        T = T[i],
                        M2 = M2[i],
                        M3 = M3[i]
                    });
                }
            }
            return results;
        }


        // - 读某杆件的位移/挠度
    }
}
