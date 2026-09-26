using PortalFrame._1Model;
using PortalFrame._5data;

namespace PortalFrame._2Check
{
    public static class CheckRunner
    {
        /// <summary>
        /// 柱的全部验算
        /// </summary>
        public static ColumnCheckResult CheckColumn(MemberData member,Dictionary<string, List<ForceData>> forcesByCombo
                                                   ,Preferences prefs, OverWrites overwrites)
        {
            var result = new ColumnCheckResult();

            // 基本信息
            result.MemberName = member.Name;
            result.SectionName = member.SectionName;
            result.Length = member.Frame.Length;
            result.IsTapered = member.Section.IsTapered;

            // 截面信息
            if (member.Section is TaperedSectionData tapered)
            {
                result.IsTapered = true;
                FillSectionInfo(tapered.End, result.SecBig);
                FillSectionInfo(tapered.Start, result.SecSmall);
            }
            else if (member.Section is HSectionData sec)
            {
                result.IsTapered = false;
                FillSectionInfo(sec, result.SecBig);
                // 等截面只填一个
            }

            //================强度验算==================
            foreach (var (comboName, forces) in forcesByCombo)
            {
                var flexList = new List<FlexureStationResult>();
                var shearList = new List<ShearStationResult>();

                foreach (var force in forces)
                {
                    var flexResult = new FlexureStationResult();
                    var shearResult = new ShearStationResult();

                    // 1. 有效截面（填 flexResult.Ae/WeTop/WeBottom）
                    SectionUtils.EffectiveSection(member, force, flexResult, prefs, overwrites);

                    // 2. 抗剪（填 shearResult.Vd/Util）
                    StrengthCheck.ShearCheck(member, force, shearResult, prefs, overwrites);

                    // 3. 抗弯（填 flexResult.Util）
                    StrengthCheck.FlexureCheck(member, force, shearResult, flexResult, prefs, overwrites);

                    flexList.Add(flexResult);
                    shearList.Add(shearResult);
                }
                result.FlexureStrength.Combos[comboName] = flexList;
                result.ShearStrength.Combos[comboName] = shearList;
            }

            //====================长细比=======================
            CalcLength.SlendernessCheck(member, result.Slenderness);

            //====================宽厚比=======================
            SectionUtils.LocalStabilityCheck(member, result.LocalStability);

            // ===== 平面内稳定 =====
            foreach (var (comboName, forces) in forcesByCombo)
            {
                var stability = new StabilityResult();
                stability.Combos[comboName] = 0;
                StabilityCheck_Column.ColumnInPlaneCheck(member, forces, result.Slenderness, stability, prefs, overwrites);
                result.InPlaneStability.Combos[comboName] = stability.Combos[comboName];
            }

            // ===== 平面外稳定=====
            foreach (var (comboName, forces) in forcesByCombo)
            {
                var stability = new StabilityResult();
                stability.Combos[comboName] = 0;
                StabilityCheck_Column.ColumnOutPlaneCheck(member, forces, result.Slenderness, stability, prefs, overwrites);
                result.InPlaneStability.Combos[comboName] = stability.Combos[comboName];
            }
            result.GetWorstResult();
            return result;
        }

        /// <summary>
        /// 梁的全部验算
        /// </summary>
        public static BeamCheckResult CheckBeam(MemberData member,Dictionary<string, List<ForceData>> forcesByCombo
                                               , Preferences prefs, OverWrites overwrites)
        {
            var result = new BeamCheckResult();

            // 填基本信息
            result.MemberName = member.Name;
            result.SectionName = member.SectionName;
            result.Length = member.Frame.Length;
            result.IsTapered = member.Section.IsTapered;

            // 后面填：强度、长细比、宽厚比、稳定

            return result;
        }

        private static void FillSectionInfo(HSectionData sec, SectionInfo info)
        {
            info.H = sec.H;
            info.B = sec.TopB;
            info.Tw = sec.Tw;
            info.Tf = sec.TopTf;
            info.Area = sec.Area;
            info.I33 = sec.I33;
            info.S33_Top = sec.S33_Top;
            info.S33_Bot = sec.S33_Bot;
            info.R33 = sec.R33;
            info.I22 = sec.I22;
            info.S22 = sec.S22;
            info.R22 = sec.R22;
        }

    }
}
