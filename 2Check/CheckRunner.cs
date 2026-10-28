using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._5data;
using System.Reflection;

namespace PortalFrame._2Check
{
    public static class CheckRunner
    {
        public static CheckResult CheckMember(MemberData member)
        {
            // 直接用已经建好的结果类
            CheckResult result = member.CheckResult;

            // 生成设计内力（分析内力×γ0×MMR）
            GenerateDesignForces(member);

            // 验算静态项
            SlenderCheck.Calculate(member);
            SectionUtils.LocalStabilityCheck(member);

            // 遍历用户选定的组合
            foreach (var comboName in Global.Data.Combos.SelectedCombos)
            {
                // 强度
                SectionUtils.EffectiveSection(member, comboName);
                StrengthCheck.Calculate(member, comboName);

                // 稳定
                if (member.Type == MemberType.Column)
                {
                    StabilityCheck_Column.ColumnInPlaneCheck(member, comboName);
                    StabilityCheck_Column.ColumnOutPlaneCheck(member, comboName);
                }
                else
                {
                    StabilityCheck_Beam.OverallStabilityCheck(member, comboName);
                }
            }

            result.GetWorstResult(member);
            return result;
        }


        // 生成设计内力
        private static void GenerateDesignForces(MemberData member)
        {
            double gamma0 = member.CheckResult.General.Gamma0.Value;
            double mmr = member.CheckResult.General.MMR.Value;
            member.DesignForces = new();

            // 只生成用户选中的组合的设计内力
            foreach (var comboName in Global.Data.Combos.SelectedCombos)
            {
                // 从分析内力里找这个组合
                if (!member.AnalysisForces.TryGetValue(comboName, out var forceDict))
                    continue;  // 找不到就跳过

                var designDict = new Dictionary<double, DesignForce>();
                foreach (var (station, f) in forceDict)
                {
                    designDict[station] = new DesignForce
                    {
                        N = f.P * gamma0,
                        V2 = f.V2 * gamma0,
                        M3 = f.M3 * gamma0 * mmr,
                        V3 = f.V3 * gamma0,
                        M2 = f.M2 * gamma0 * mmr
                    };
                }
                member.DesignForces[comboName] = designDict;
            }
        }


    }
}
