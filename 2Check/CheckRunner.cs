using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._5data;

namespace PortalFrame._2Check
{
    public static class CheckRunner
    {
        public static CheckResultBase CheckMember(MemberData member)
        {

            // 1. 先读覆盖项的构件类型（如果用户改了，就先改 member.Type）
            double? owType = member.OverWrites.ElementType.GetOw();
            if (owType.HasValue)
            {
                member.Type = (MemberType)(int)owType.Value;
            }
            // 没改的话，就用 MemberData 里原来的 member.Type

            // 建result（用新的type）
            CheckResultBase result;
            if (member.Type == MemberType.Column)
                result = new ColumnCheckResult();
            else
                result = new BeamCheckResult();
            result.InitCombos(Global.Combos.SelectedCombos);
            member.CheckResult = result;

            ApplyAllDefaults(member);       // 设默认值
            ApplyAllPreference(result, Global.ModelData.Preferences);   // 设首选项
            ApplyOverWrites(result, member.OverWrites);    // 设覆盖项

            // 生成设计内力（分析内力×γ0×MMR）
            GenerateDesignForces(member);

            // 验算静态项
            SlenderCheck.Calculate(member);
            SectionUtils.LocalStabilityCheck(member);

            // 4. 遍历用户选定的组合
            foreach (var comboName in Global.Combos.SelectedCombos)
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

            result.GetWorstResult();
            return result;
        }


        // 生成设计内力）
        private static void GenerateDesignForces(MemberData member)
        {
            double gamma0 = member.CheckResult.General.Gamma0.Value;
            double mmr = member.CheckResult.General.MMR.Value;
            member.DesignForces = new();

            foreach (var (comboName, forces) in member.AnalysisForces)
            {
                var designForces = new List<DesignForce>();

                foreach (var f in forces)
                {
                    designForces.Add(new DesignForce
                    {
                        Station = f.Station,
                        N = f.P * gamma0,
                        V2 = f.V2 * gamma0,
                        M3 = f.M3 * gamma0 * mmr,
                        V3 = f.V3 * gamma0,
                        M2 = f.M2 * gamma0 * mmr
                    });
                }

                member.DesignForces[comboName] = designForces;
            }
        }

        //分配默认值给 ParaItem 类型的字段
        private static void ApplyAllDefaults(MemberData member)
        {
            // 遍历 member 里所有字段，找 ParaItem 类型的，批量设默认值
            ApplyDefaultsRecursive(member);
        }
        private static void ApplyDefaultsRecursive(object obj)
        {
            if (obj == null) return;

            foreach (var field in obj.GetType().GetFields())
            {
                var value = field.GetValue(obj);
                if (value == null) continue;

                // 如果是 ParaItem，从 DefaultValues 找同名默认值
                if (value is ParaItem item)
                {
                    var defaultField = typeof(DefaultValues).GetField(field.Name);
                    if (defaultField != null)
                    {
                        double defaultValue = (double)defaultField.GetValue(null)!;
                        item.SetDefault(defaultValue);
                    }
                }
                // 如果是其他类（嵌套的），递归进去
                else if (!value.GetType().IsPrimitive)
                {
                    ApplyDefaultsRecursive(value);
                }
            }
        }

        //分配覆盖项
        private static void ApplyOverWrites(CheckResultBase result, OverWrites ow)
        {
            // ===== 1. 抗震类型特殊处理 =====
            if (ow.SeismicType.Value == 1)
            {
                ow.LambdaLimit.SetOw(180);
                ow.FlangeLimit.SetOw(15);
                ow.WebLimit.SetOw(250);
            }
            else if (ow.SeismicType.Value == 2)
            {
                ow.LambdaLimit.SetOw(150);
                ow.FlangeLimit.SetOw(13);
                ow.WebLimit.SetOw(160);
            }

            // ===== 2. 分发参数 =====
            // General
            result.General.Gamma0.SetOw(ow.Gamma0.Value);
            result.General.MMR.SetOw(ow.MMR.Value);
            result.General.SeismicType.SetOw(ow.SeismicType.Value);
            result.General.LambdaLimit.SetOw(ow.LambdaLimit.Value);
            result.General.FlangeLimit.SetOw(ow.FlangeLimit.Value);
            result.General.WebLimit.SetOw(ow.WebLimit.Value);
            result.General.UtilLimit.SetOw(ow.UtilLimit.Value);

            // 长细比
            result.Slenderness.MuX.SetOw(ow.MuX.Value);
            result.Slenderness.Ly.SetOw(ow.Ly.Value);
            result.Slenderness.PhiX.SetOw(ow.PhiX.Value);
            result.Slenderness.PhiY.SetOw(ow.PhiY.Value);

            // 柱平面内
            if (result is ColumnCheckResult col)
            {
                col.InPlaneStability.BetaMx.SetOw(ow.BetaMx.Value);

                foreach (var details in col.OutPlaneStability.PhiBDetails.Values)
                {
                    details.HasBrace.SetOw(ow.HasBrace.Value);
                    details.GammaX.SetOw(ow.GammaX.Value);
                    details.PhiB.SetOw(ow.PhiB.Value);
                    details.Ap.SetOw(ow.PurlinArea.Value);
                    details.Lp.SetOw(ow.PurlinSpan.Value);
                    details.Ip.SetOw(ow.PurlinI33.Value);
                    details.A.SetOw(ow.PurlinDistToFlange.Value);
                    details.Ak.SetOw(ow.BraceArea.Value);
                    details.Lkk.SetOw(ow.BraceSpacing.Value);
                    details.Alpha.SetOw(ow.BraceAngle.Value);
                }
            }

            // 梁整体稳定
            if (result is BeamCheckResult beam)
            {
                foreach (var details in beam.LateralStability.PhiBDetails.Values)
                {
                    details.HasBrace.SetOw(ow.HasBrace.Value);
                    details.GammaX.SetOw(ow.GammaX.Value);
                    details.PhiB.SetOw(ow.PhiB.Value);
                    details.Ap.SetOw(ow.PurlinArea.Value);
                    details.Lp.SetOw(ow.PurlinSpan.Value);
                    details.Ip.SetOw(ow.PurlinI33.Value);
                    details.A.SetOw(ow.PurlinDistToFlange.Value);
                    details.Ak.SetOw(ow.BraceArea.Value);
                    details.Lkk.SetOw(ow.BraceSpacing.Value);
                    details.Alpha.SetOw(ow.BraceAngle.Value);
                }
            }

            // 抗弯
            foreach (var flexList in result.FlexureStrength.Combos.Values)
            {
                foreach (var flex in flexList)
                {
                    flex.Rho.SetOw(ow.Rho.Value);
                }
            }

            // 抗剪
            foreach (var shearList in result.ShearStrength.Combos.Values)
            {
                foreach (var shear in shearList)
                {
                    shear.PhiPs.SetOw(ow.PhiPs.Value);
                    shear.HasStiffener = ow.HasStiffener.Value == 1;
                    shear.a.SetOw(ow.StiffenerSpacing.Value);
                }
            }
        }

        //分配首选项
        private static void ApplyAllPreference(CheckResultBase result, Preferences prefs)
        {
            // ===== 1. 抗震类型特殊处理 =====
            if (prefs.SeismicType.Value == 1)
            {
                prefs.LambdaLimit.SetPre(180);
                prefs.FlangeLimit.SetPre(15);
                prefs.WebLimit.SetPre(250);
            }
            else if (prefs.SeismicType.Value == 2)
            {
                prefs.LambdaLimit.SetPre(150);
                prefs.FlangeLimit.SetPre(13);
                prefs.WebLimit.SetPre(160);
            }

            // ===== 2. 分发参数 =====
            // General
            result.General.Gamma0.SetPre(prefs.Gamma0.Value);
            result.General.MMR.SetPre(prefs.MMR.Value);
            result.General.SeismicType.SetPre(prefs.SeismicType.Value);
            result.General.LambdaLimit.SetPre(prefs.LambdaLimit.Value);
            result.General.FlangeLimit.SetPre(prefs.FlangeLimit.Value);
            result.General.WebLimit.SetPre(prefs.WebLimit.Value);
            result.General.UtilLimit.SetPre(prefs.UtilLimit.Value);

            // 长细比
            result.Slenderness.Ly.SetPre(prefs.Ly.Value);

            // 柱平面内
            if (result is ColumnCheckResult col)
            {
                foreach (var details in col.OutPlaneStability.PhiBDetails.Values)
                {
                    details.HasBrace.SetPre(prefs.HasBrace.Value);
                    details.Ap.SetPre(prefs.PurlinArea.Value);
                    details.Lp.SetPre(prefs.PurlinSpan.Value);
                    details.Ip.SetPre(prefs.PurlinI33.Value);
                    details.A.SetPre(prefs.PurlinDistToFlange.Value);
                    details.Ak.SetPre(prefs.BraceArea.Value);
                    details.Lkk.SetPre(prefs.BraceSpacing.Value);
                    details.Alpha.SetPre(prefs.BraceAngle.Value);
                }
            }

            // 梁整体稳定
            if (result is BeamCheckResult beam)
            {
                foreach (var details in beam.LateralStability.PhiBDetails.Values)
                {
                    details.HasBrace.SetPre(prefs.HasBrace.Value);
                    details.Ap.SetPre(prefs.PurlinArea.Value);
                    details.Lp.SetPre(prefs.PurlinSpan.Value);
                    details.Ip.SetPre(prefs.PurlinI33.Value);
                    details.A.SetPre(prefs.PurlinDistToFlange.Value);
                    details.Ak.SetPre(prefs.BraceArea.Value);
                    details.Lkk.SetPre(prefs.BraceSpacing.Value);
                    details.Alpha.SetPre(prefs.BraceAngle.Value);
                }
            }

            // 抗剪
            foreach (var shearList in result.ShearStrength.Combos.Values)
            {
                foreach (var shear in shearList)
                {
                    shear.HasStiffener = prefs.HasStiffener.Value == 1;
                    shear.a.SetPre(prefs.StiffenerSpacing.Value);
                }
            }
        }


    }
}
