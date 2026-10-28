using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using System.Reflection;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace PortalFrame._3Dispatch
{
    public static class Report
    {
        // 分组结构
        public class DetailGroup
        {
            public string Title = "";
            public List<(int order, string name, string value, string unit)> Items = new();
        }

        // 通用输出：给一个结果对象，返回分组列表
        public static List<DetailGroup> BuildDetailGroups(object obj)
        {
            var groups = new Dictionary<string, DetailGroup>();

            foreach (var field in obj.GetType().GetFields())
            {
                if (field.GetValue(obj) is not ParaItem item) continue;

                var attr = field.GetCustomAttribute<DisplayAttribute>();
                string groupName = attr?.Group ?? "其他";
                int order = attr?.Order ?? 999;

                // OnlyWhen条件不满足就跳过
                if (!string.IsNullOrEmpty(attr?.OnlyWhen))
                {
                    var condField = obj.GetType().GetField(attr.OnlyWhen);
                    if (condField?.GetValue(obj) is ParaItem cond && cond.Value != 1)
                        continue;
                }

                string val = item.HasValue ? item.Value.ToString("F3") : "无值";

                if (!groups.ContainsKey(groupName))
                    groups[groupName] = new DetailGroup { Title = groupName };

                groups[groupName].Items.Add((order, item.Name, val, item.Unit));
            }

            // 组内按Order排序
            foreach (var g in groups.Values)
                g.Items.Sort((a, b) => a.order.CompareTo(b.order));

            return groups.Values.ToList();
        }
        
        //=====================显示详细信息========================
        // 递归遍历对象，把所有ParaItem加到列表里
        private static void AppendObjectItems(List<(string name, string value, string unit)> items, object obj, string? title = null)
        {
            if (title != null)
            {
                items.Add(($"--- {title} ---", "", ""));  // 标题行，跨三列
            }

            foreach (var field in obj.GetType().GetFields())
            {
                var value = field.GetValue(obj);
                if (value == null) continue;

                if (value is ParaItem item)
                {
                    string val = item.HasValue ? item.Value.ToString("F3") : "无值";
                    items.Add((item.Name, val, item.Unit));
                }
                else if (!value.GetType().IsPrimitive && !(value is string))
                {
                    AppendObjectItems(items, value);
                }
            }
        }

        //=====================生成计算书========================
        public static void GenerateReport(MemberData member, string savePath)
        {
            using (var doc = DocX.Create(savePath))
            {
                // 标题
                doc.InsertParagraph("门式刚架构件计算书")
                   .FontSize(16).Bold().Alignment = Alignment.center;
                doc.InsertParagraph();

                // ===== 基本信息 =====
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
                doc.InsertParagraph();

                var CR = member.CheckResult;

                // ===== 1. 长细比验算（静态）=====
                doc.InsertParagraph("长细比验算").Heading(HeadingType.Heading1);
                AppendGroupsToDoc(doc, BuildDetailGroups(CR.Slenderness));

                // ===== 2. 板件宽厚比验算（静态）=====
                doc.InsertParagraph("板件宽厚比验算").Heading(HeadingType.Heading1);
                AppendGroupsToDoc(doc, BuildDetailGroups(CR.LocalStability));

                // ===== 3. 抗弯强度验算 =====
                var (flexUtil, flexCombo, flexStation) = CR.FlexureStrength.GetWorst();
                if (flexUtil > 0)
                {
                    doc.InsertParagraph("抗弯强度验算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"最不利组合：{flexCombo}，最不利测站：{flexStation:F0} mm，利用率：{flexUtil:F3}");
                    AppendGroupsToDoc(doc, BuildDetailGroups(CR.FlexureStrength.Collection[flexCombo][flexStation]));
                }

                // ===== 4. 抗剪强度验算 =====
                var (shearUtil, shearCombo, shearStation) = CR.ShearStrength.GetWorst();
                if (shearUtil > 0)
                {
                    doc.InsertParagraph("抗剪强度验算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"最不利组合：{shearCombo}，最不利测站：{shearStation:F0} mm，利用率：{shearUtil:F3}");
                    AppendGroupsToDoc(doc, BuildDetailGroups(CR.ShearStrength.Collection[shearCombo][shearStation]));
                }

                // ===== 5. 整体稳定系数φb =====
                if (CR.PhiBResult.WorstCombo != "")
                {
                    doc.InsertParagraph("整体稳定系数φb计算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"控制组合：{CR.PhiBResult.WorstCombo}（φb={CR.PhiBResult.MinPhiB:F3}）");
                    AppendGroupsToDoc(doc, BuildDetailGroups(CR.PhiBResult.Collection[CR.PhiBResult.WorstCombo]));
                }

                // ===== 6. 柱面内稳定 =====
                if (member.Type == MemberType.Column && CR.InPlaneStability.WorstCombo != "")
                {
                    doc.InsertParagraph("柱平面内稳定验算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"最不利组合：{CR.InPlaneStability.WorstCombo}，利用率：{CR.InPlaneStability.MaxUtil:F3}");
                    AppendGroupsToDoc(doc, BuildDetailGroups(CR.InPlaneStability.Collection[CR.InPlaneStability.WorstCombo]));
                }

                // ===== 7. 柱面外稳定 =====
                if (member.Type == MemberType.Column && CR.OutPlaneStability.WorstCombo != "")
                {
                    doc.InsertParagraph("柱平面外稳定验算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"最不利组合：{CR.OutPlaneStability.WorstCombo}，利用率：{CR.OutPlaneStability.MaxUtil:F3}");
                    AppendGroupsToDoc(doc, BuildDetailGroups(CR.OutPlaneStability.Collection[CR.OutPlaneStability.WorstCombo]));
                }

                // ===== 8. 梁整体稳定 =====
                if (member.Type == MemberType.Beam && CR.LateralStability.WorstCombo != "")
                {
                    doc.InsertParagraph("梁整体稳定验算").Heading(HeadingType.Heading1);
                    doc.InsertParagraph($"最不利组合：{CR.LateralStability.WorstCombo}，利用率：{CR.LateralStability.MaxUtil:F3}");
                    AppendGroupsToDoc(doc, BuildDetailGroups(CR.LateralStability.Collection[CR.LateralStability.WorstCombo]));
                }

                doc.Save();
            }
        }

        // 把分组列表输出到Word（小标题+两列表格）
        private static void AppendGroupsToDoc(DocX doc, List<DetailGroup> groups)
        {
            foreach (var g in groups)
            {
                // 分组小标题
                doc.InsertParagraph(g.Title).Heading(HeadingType.Heading2);

                // 两列表格
                var table = doc.AddTable(g.Items.Count + 1, 2);
                table.Rows[0].Cells[0].Paragraphs[0].Append("参数").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("数值").Bold();

                for (int i = 0; i < g.Items.Count; i++)
                {
                    var (order, name, value, unit) = g.Items[i];
                    table.Rows[i + 1].Cells[0].Paragraphs[0].Append(name);
                    // 数值带单位
                    string valText = string.IsNullOrEmpty(unit) ? value : $"{value} {unit}";
                    table.Rows[i + 1].Cells[1].Paragraphs[0].Append(valText);
                }
                doc.InsertTable(table);
                doc.InsertParagraph();  // 组后空一行
            }
        }

    }
}
