using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._5data;
using System.Text;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace PortalFrame._3Dispatch
{
    public static class Report
    {
        //=====================显示详细信息========================
        public static string BuildDetailText(MemberData member, string checkType, string? comboName, double? station)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"===== 构件 {member.Name}：{checkType} 详细参数 =====");
            sb.AppendLine();

            var result = member.CheckResult;
            object? target = null;  // 要遍历的目标对象

            // 根据验算类型，找到要显示的结果对象
            if (checkType == "长细比")
            {
                target = result.Slenderness;
            }
            else if (checkType == "局部稳定")
            {
                target = result.LocalStability;
            }
            else if (checkType == "平面内稳定" && result is ColumnCheckResult colIn && comboName != null)
            {
                target = colIn.InPlaneStability;
            }
            else if (checkType == "平面外稳定" && result is ColumnCheckResult colOut && comboName != null)
            {
                target = colOut.OutPlaneStability;
                // 还要把对应组合的 PhiBDetails 也加进去
                AppendObject(sb, colOut.OutPlaneStability.PhiBDetails[comboName], "φb 计算过程");
                return sb.ToString();  // PhiBDetails 已经单独输出了，直接返回
            }
            else if (checkType == "整体稳定" && result is BeamCheckResult beam && comboName != null)
            {
                target = beam.LateralStability;
                AppendObject(sb, beam.LateralStability.PhiBDetails[comboName], "φb 计算过程");
                return sb.ToString();
            }
            else if (checkType == "压弯/拉弯" && comboName != null && station.HasValue)
            {
                var flexList = result.FlexureStrength.Combos[comboName];
                target = flexList.First(s => Math.Abs(s.Station - station.Value) < 1.0);
            }
            else if (checkType == "抗剪" && comboName != null && station.HasValue)
            {
                var shearList = result.ShearStrength.Combos[comboName];
                target = shearList.First(s => Math.Abs(s.Station - station.Value) < 1.0);
            }

            // 遍历目标对象，输出所有 ParaItem
            if (target != null)
            {
                AppendObject(sb, target);
            }

            return sb.ToString();
        }

        // 递归遍历对象，把所有 ParaItem 输出到 sb
        private static void AppendObject(StringBuilder sb, object obj, string? title = null)
        {
            if (title != null)
            {
                sb.AppendLine();
                sb.AppendLine($"--- {title} ---");
            }

            foreach (var field in obj.GetType().GetFields())
            {
                var value = field.GetValue(obj);
                if (value == null) continue;

                // 如果是 ParaItem，直接输出
                if (value is ParaItem item)
                {
                    sb.AppendLine(item.ToString());
                }
                // 其他类（嵌套的），递归进去
                else if (!value.GetType().IsPrimitive && !(value is string))
                {
                    AppendObject(sb, value);
                }
            }
        }

        //=====================生成计算书========================
        /// <summary>
        /// 生成完整计算书（Word文档）
        /// </summary>
        public static void GenerateReport(MemberData member, string savePath)
        {
            using (var doc = DocX.Create(savePath))
            {
                // 标题
                doc.InsertParagraph("门式刚架构件计算书")
                   .FontSize(16).Bold().Alignment = Alignment.center;
                doc.InsertParagraph();

                // 基本信息
                doc.InsertParagraph("基本信息").Heading(HeadingType.Heading1);
                var table = doc.AddTable(4, 2);
                table.Rows[0].Cells[0].Paragraphs[0].Append("构件名");
                table.Rows[0].Cells[1].Paragraphs[0].Append(member.Name);
                table.Rows[1].Cells[0].Paragraphs[0].Append("构件类型");
                table.Rows[1].Cells[1].Paragraphs[0].Append(member.Type == MemberType.Column ? "柱" : "梁");
                table.Rows[2].Cells[0].Paragraphs[0].Append("截面名");
                table.Rows[2].Cells[1].Paragraphs[0].Append(member.SectionName);
                table.Rows[3].Cells[0].Paragraphs[0].Append("材料");
                table.Rows[3].Cells[1].Paragraphs[0].Append(member.Material.Grade.ToString());
                doc.InsertTable(table);

                // 长细比（静态）
                doc.InsertParagraph();
                doc.InsertParagraph("长细比验算").Heading(HeadingType.Heading1);
                AppendObjectToDoc(doc, member.CheckResult.Slenderness);

                // 局部稳定（静态）
                doc.InsertParagraph();
                doc.InsertParagraph("板件宽厚比验算").Heading(HeadingType.Heading1);
                AppendObjectToDoc(doc, member.CheckResult.LocalStability);

                // 抗弯强度
                doc.InsertParagraph();
                doc.InsertParagraph("抗弯强度验算").Heading(HeadingType.Heading1);
                var flex = member.CheckResult.FlexureStrength;
                var flexStation = flex.WorstStation!;
                doc.InsertParagraph($"最不利组合：{flex.WorstCombo}，最大测站：{flexStation.Station:F0} mm");
                AppendObjectToDoc(doc, flexStation);

                // 抗剪强度
                doc.InsertParagraph();
                doc.InsertParagraph("抗剪强度验算").Heading(HeadingType.Heading1);
                var shear = member.CheckResult.ShearStrength;
                var shearStation = shear.WorstStation!;
                doc.InsertParagraph($"最不利组合：{shear.WorstCombo}，最大测站：{shearStation.Station:F0} mm");
                AppendObjectToDoc(doc, shearStation);

                // 稳定验算（柱/梁分开）
                if (member.Type == MemberType.Column && member.CheckResult is ColumnCheckResult col)
                {
                    doc.InsertParagraph();
                    doc.InsertParagraph("柱平面内稳定验算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"最不利组合：{col.InPlaneStability.WorstCombo}，利用率：{col.InPlaneStability.MaxUtil:F2}");
                    AppendObjectToDoc(doc, col.InPlaneStability);

                    doc.InsertParagraph();
                    doc.InsertParagraph("柱平面外稳定验算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"最不利组合：{col.OutPlaneStability.WorstCombo}，利用率：{col.OutPlaneStability.MaxUtil:F2}");
                    AppendObjectToDoc(doc, col.OutPlaneStability);

                    // φb 详细参数
                    doc.InsertParagraph("φb 计算过程").Heading(HeadingType.Heading2);
                    AppendObjectToDoc(doc, col.OutPlaneStability.PhiBDetails[col.OutPlaneStability.WorstCombo]);
                }
                else if (member.Type == MemberType.Beam && member.CheckResult is BeamCheckResult beam)
                {
                    doc.InsertParagraph();
                    doc.InsertParagraph("梁整体稳定验算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"最不利组合：{beam.LateralStability.WorstCombo}，利用率：{beam.LateralStability.MaxUtil:F2}");
                    AppendObjectToDoc(doc, beam.LateralStability);

                    doc.InsertParagraph("φb 计算过程").Heading(HeadingType.Heading2);
                    AppendObjectToDoc(doc, beam.LateralStability.PhiBDetails[beam.LateralStability.WorstCombo]);
                }

                doc.Save();
            }
        }

        // 把对象里的所有 ParaItem 输出到 Word 文档
        private static void AppendObjectToDoc(DocX doc, object obj)
        {
            foreach (var field in obj.GetType().GetFields())
            {
                var value = field.GetValue(obj);
                if (value == null) continue;

                if (value is ParaItem item)
                {
                    doc.InsertParagraph(item.ToString());
                }
                else if (!value.GetType().IsPrimitive && !(value is string))
                {
                    AppendObjectToDoc(doc, value);
                }
            }
        }

    }
}
