using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2Check;
using PortalFrame._2CheckResults;
using PortalFrame._3Dispatch;
using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class MemberDetailForm : Form
    {
        private MemberData _member = null!;
        private CheckResult _result;
        private double _utilLimit;

        // 左栏
        private GroupBox _grpCheckType = new() { Text = "验算类型" };
        private ListView _lstCheckType = new();

        private GroupBox _grpStation = new() { Text = "测站" };
        private ListView _lstStation = new();

        // 中栏
        private GroupBox _grpCombo = new() { Text = "荷载组合" };
        private ListView _lstCombo = new();

        // 右栏
        private GroupBox _grpSec = new() { Text = "截面信息" };
        private ListView _lstSec = new();
        private GroupBox _grpSlend = new() { Text = "长细比" };
        private ListView _lstSlend = new();
        private GroupBox _grpLocal = new() { Text = "板件宽厚比" };
        private ListView _lstLocal = new();

        // 按钮
        private Button _btnDetail = new() { Text = "显示详细文件" };
        private Button _btnReport = new() { Text = "生成计算书" };
        private Button _btnBack = new() { Text = "返回" };

        public MemberDetailForm(MemberData member)
        {
            this.Font = Global.Ui.DefaultFont;
            FormBorderStyle = FormBorderStyle.FixedSingle;   // 固定大小，不能拖拽
            MaximizeBox = false;                              // 禁用最大化
            MinimizeBox = false;                              // 禁用最小化

            Text = "构件验算详情";
            Width = 1330;
            Height = 800;
            StartPosition = FormStartPosition.CenterScreen;

            SetupLayout();
            SetupEvents();
            
            _member = member;
            _result = member.CheckResult;
            _utilLimit = member.CheckResult.General.UtilLimit.Value;

            FillStaticData();   // 填右栏静态数据
            FillCheckTypeList(); // 填左栏验算类型
            SelectWorstResult();
            UpdateBtnDetailEnabled();
        }
        private void SetupLayout()
        {
            // 左栏上：验算类型
            _grpCheckType.Location = new Point(10, 10);
            _grpCheckType.Size = new Size(250, 300);
            _lstCheckType.Dock = DockStyle.Fill;
            _lstCheckType.View = View.List;
            _lstCheckType.FullRowSelect = true;
            _grpCheckType.Controls.Add(_lstCheckType);

            // 左栏下：测站
            _grpStation.Location = new Point(10, 320);
            _grpStation.Size = new Size(250, 420);
            _lstStation.Dock = DockStyle.Fill;
            _lstStation.View = View.Details;
            _lstStation.FullRowSelect = true;
            _lstStation.Columns.Add("测站", 100);
            _lstStation.Columns.Add("利用率", 100);
            _grpStation.Controls.Add(_lstStation);

            // 中栏：组合
            _grpCombo.Location = new Point(270, 10);
            _grpCombo.Size = new Size(450, 730);
            _lstCombo.Dock = DockStyle.Fill;
            _lstCombo.View = View.Details;
            _lstCombo.FullRowSelect = true;
            _lstCombo.Columns.Add("组合名", 300);
            _lstCombo.Columns.Add("利用率", 100);
            _grpCombo.Controls.Add(_lstCombo);

            // 右栏：静态结果
            // 右栏：三个 GroupBox
            // 1. 截面属性
            _grpSec.Location = new Point(740, 10);
            _grpSec.Size = new Size(560, 350);
            _lstSec.Dock= DockStyle.Fill;
            _lstSec.View = View.Details;
            _lstSec.FullRowSelect = true;
            _lstSec.Columns.Add("项目", 120);
            _lstSec.Columns.Add("小端", 160);   
            _lstSec.Columns.Add("项目", 120);
            _lstSec.Columns.Add("大端", 160);   
            _grpSec.Controls.Add(_lstSec);


            // 2. 长细比
            _grpSlend.Location = new Point(740, 370);
            _grpSlend.Size = new Size(560, 120);
            _lstSlend.Dock = DockStyle.Fill;
            _lstSlend.View = View.Details;
            _lstSlend.FullRowSelect = true;
            _lstSlend.Columns.Add("项目", 150);
            _lstSlend.Columns.Add("数值", 80);
            _lstSlend.Columns.Add("项目", 80);
            _lstSlend.Columns.Add("利用率", 80);
            _grpSlend.Controls.Add(_lstSlend);

            // 3. 局部稳定（宽厚比）
            _grpLocal.Location = new Point(740, 500);
            _grpLocal.Size = new Size(560, 160);
            _lstLocal.Dock = DockStyle.Fill;
            _lstLocal.View = View.Details;
            _lstLocal.FullRowSelect = true;
            _lstLocal.Columns.Add("项目", 150);
            _lstLocal.Columns.Add("数值", 80);
            _lstLocal.Columns.Add("项目", 80);
            _lstLocal.Columns.Add("利用率", 80);
            _grpLocal.Controls.Add(_lstLocal);

            // 三个按钮上下排
            _btnDetail.Size = new Size(150, 35);
            _btnDetail.Location = new Point(770, 660);

            _btnReport.Size = new Size(150, 35);
            _btnReport.Location = new Point(970, 660);

            _btnBack.Size = new Size(100, 35);
            _btnBack.Location = new Point(1170, 660);


            Controls.Add(_grpCheckType);
            Controls.Add(_grpStation);
            Controls.Add(_grpCombo);
            Controls.Add(_grpSec);
            Controls.Add(_grpSlend);
            Controls.Add(_grpLocal);
            Controls.Add(_btnDetail);
            Controls.Add(_btnReport);
            Controls.Add(_btnBack);
        }
        private void SetupEvents()
        {
            _lstCheckType.SelectedIndexChanged += (s, e) =>
            {
                LstCheckType_SelectedIndexChanged(s, e);
                UpdateBtnDetailEnabled();
            };

            _lstCombo.SelectedIndexChanged += (s, e) =>
            {
                LstCombo_SelectedIndexChanged(s, e);
                UpdateBtnDetailEnabled();
            };

            _lstStation.SelectedIndexChanged += (s, e) => UpdateBtnDetailEnabled();

            _btnDetail.Click += BtnDetail_Click;
            _btnReport.Click += BtnReport_Click;
            _btnBack.Click += (s, e) => Close();
        }
        private void BtnDetail_Click(object? sender, EventArgs e)
        {
            string checkType = _lstCheckType.SelectedItems[0].Text;
            string? comboName = _lstCombo.SelectedItems.Count > 0 ? _lstCombo.SelectedItems[0].Text : null;
            double? station = _lstStation.SelectedItems.Count > 0 ? _lstStation.SelectedItems[0].Tag as double?: null;

            // ===== 分发层：根据验算类型，决定显示哪个结果对象 =====
            object? target = null;

            switch (checkType)
            {
                case CheckTypes.Slenderness:
                    target = _result.Slenderness;
                    break;

                case CheckTypes.LocalStability:
                    target = _result.LocalStability;
                    break;

                case CheckTypes.PhiB:
                    if (comboName != null) target = _result.PhiBResult.Collection[comboName];
                    break;

                case CheckTypes.ColumnInPlane:
                    if(comboName != null)target = _result.InPlaneStability.Collection[comboName];
                    break;

                case CheckTypes.ColumnOutPlane:
                    if (comboName != null)target = _result.OutPlaneStability.Collection[comboName];
                    break;

                case CheckTypes.BeamLateral:
                    if (comboName != null)target = _result.LateralStability.Collection[comboName];
                    break;

                case CheckTypes.Flexure:
                    if (comboName != null && station.HasValue
                        && _result.FlexureStrength.Collection.TryGetValue(comboName, out var flexDict)
                        && flexDict.TryGetValue(station.Value, out var flexStation))
                    {
                        target = flexStation;
                    }
                    break;

                case CheckTypes.Shear:
                    if (comboName != null && station.HasValue
                        && _result.ShearStrength.Collection.TryGetValue(comboName, out var shearDict)
                        && shearDict.TryGetValue(station.Value, out var shearStation))
                    {
                        target = shearStation;
                    }
                    break;
            }

            if (target == null)
            {
                MessageBox.Show("没有可显示的数据");
                return;
            }

            // ===== 弹表格窗口 =====
            var form = new Form
            {
                Text = $"详细参数 - {checkType}",
                Width = 650,
                Height = 550,
                StartPosition = FormStartPosition.CenterParent
            };

            var list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                ShowGroups = true,
                Font = new Font("微软雅黑", 11)
            };
            list.Columns.Add("项目", 250);
            list.Columns.Add("数值", 180);
            list.Columns.Add("单位", 120);
            list.OwnerDraw = true;
            list.DrawSubItem += (s, e) =>
            {
                if (e.Item?.Tag as string == "header")
                {
                    if (e.ColumnIndex == 0)
                    {
                        int w = list.Columns.Cast<ColumnHeader>().Sum(c => c.Width);
                        e.Graphics.ResetClip();   // 取消裁剪，允许画到整行宽度
                        e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds.X, e.Bounds.Y, w, e.Bounds.Height);
                        e.Graphics.DrawString(e.Item.Text, new Font("微软雅黑", 13, FontStyle.Bold), Brushes.Black,
                        new RectangleF(e.Bounds.X + 6, e.Bounds.Y, w - 6, e.Bounds.Height),
                        new StringFormat { LineAlignment = StringAlignment.Center });
                    }
                    return;   // 标题行的第二、三列什么都不画，不覆盖浅灰背景
                }
                else
                {
                    e.DrawBackground();
                    e.DrawText();
                }
            };


            // 1. 主对象的分组
            var groups = Report.BuildDetailGroups(target);

            // ===== 填到ListView =====
            foreach (var g in groups)
            {
                var headerItem = list.Items.Add(g.Title);
                headerItem.SubItems.Add("");   // 加空SubItems让行占满三列
                headerItem.SubItems.Add("");
                headerItem.Tag = "header";

                foreach (var (order, name, value, unit) in g.Items)
                {
                    var item = list.Items.Add("    " + name);   // 参数行缩进一点，和标题区分
                    item.SubItems.Add(value);
                    item.SubItems.Add(unit);

                    if (value == "无值")
                    {
                        item.SubItems[0].ForeColor = Color.Gray;
                        item.SubItems[1].ForeColor = Color.Gray;
                        item.SubItems[2].ForeColor = Color.Gray;
                    }
                }

                // 每组后插一个空行，分隔更清楚
                list.Items.Add("");
            }

            form.Controls.Add(list);
            form.ShowDialog();
        }
        private void BtnReport_Click(object? sender, EventArgs e)
        {
            // 拿 SAP 模型文件路径
            string modelPath = Global.SapModel.GetModelFilepath();
            string modelDir = Path.GetDirectoryName(modelPath)!;

            // 建"构件计算书"文件夹
            string reportDir = Path.Combine(modelDir, "构件计算书");
            Directory.CreateDirectory(reportDir);

            // 生成文件
            string path = Path.Combine(reportDir, $"构件{_member.Name}计算书.docx");
            Report.GenerateReport(_member, path);

            // 直接打开
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        private void FillCheckTypeList()
        {
            _lstCheckType.Items.Clear();
            var Fl = _result.FlexureStrength.GetWorst();
            AddItem(CheckTypes.Flexure, Fl.MaxUtil, _utilLimit);
            var Sh = _result.ShearStrength.GetWorst();
            AddItem(CheckTypes.Shear, Sh.MaxUtil, _utilLimit);
            AddItem(CheckTypes.PhiB, _result.PhiBResult.MinPhiB, 100);

            // 柱平面内和平面外稳定
            if (_member.Type == MemberType.Column)
            {
                AddItem(CheckTypes.ColumnInPlane, _result.InPlaneStability.MaxUtil, _utilLimit);
                AddItem(CheckTypes.ColumnOutPlane, _result.OutPlaneStability.MaxUtil, _utilLimit);
            }

            // 梁整体稳定
            if (_member.Type == MemberType.Beam)
            {
                AddItem(CheckTypes.BeamLateral, _result.LateralStability.MaxUtil, _utilLimit);
            }

            AddItem(CheckTypes.Slenderness, _result.Slenderness.MaxUtil, 1.0);
            AddItem(CheckTypes.LocalStability, _result.LocalStability.MaxUtil, 1.0);

        }
        private void AddItem(string text, double util, double limit)
        {
            var item = _lstCheckType.Items.Add(text);
            item.UseItemStyleForSubItems = false;
            item.ForeColor = GetUtilColor(util, limit);
        }
        // 根据利用率返回颜色
        private Color GetUtilColor(double util, double limit)
        {
            if (util > limit) return Global.Ui.OverLimitColor0;
            if (util >= 0.8 * limit) return Global.Ui.OverLimitColor1;
            if (util >= 0.6 * limit) return Global.Ui.OverLimitColor2;
            return Global.Ui.OverLimitColor3;
        }

        private void FillStationList(string checkType, string comboName)
        {
            _lstStation.Items.Clear();

            if (checkType == CheckTypes.Flexure && _result != null)
            {
                if (_result.FlexureStrength.Collection.TryGetValue(comboName, out var dict))
                {
                    // 直接从 _member.Stations 里拿测站列表，按顺序遍历
                    foreach (var (idx, station) in _member.Stations)
                    {
                        // 找不到这个测站就跳过
                        if (!dict.TryGetValue(station, out var s)) continue;
                        // 没有值就跳过
                        //if (!s.Util.HasValue) continue;
                        var item = _lstStation.Items.Add(station.ToString("F0"));
                        item.Tag = station;   // 把真实double存到Tag
                        item.UseItemStyleForSubItems = false;
                        item.SubItems.Add(s.Util.Value.ToString("F2"));
                        item.SubItems[1].ForeColor = GetUtilColor(s.Util.Value, _utilLimit);
                    }
                }
            }
            else if (checkType == CheckTypes.Shear && _result != null)
            {
                if (_result.ShearStrength.Collection.TryGetValue(comboName, out var dict))
                {
                    foreach (var (idx, station) in _member.Stations)
                    {
                        if (!dict.TryGetValue(station, out var s)) continue;
                        //if (!s.Util.HasValue) continue;
                        var item = _lstStation.Items.Add(station.ToString("F0"));
                        item.Tag = station;   // 把真实double存到Tag
                        item.UseItemStyleForSubItems = false;
                        item.SubItems.Add(s.Util.Value.ToString("F2"));
                        item.SubItems[1].ForeColor = GetUtilColor(s.Util.Value, _utilLimit);
                    }
                }
            }
        }
        private void FillComboList(string checkType)
        {
            _lstCombo.Items.Clear();

            // 根据验算类型，拿对应的组合结果
            Dictionary<string, double>? comboResults = null;

            if (_result != null)
            {
                comboResults = new Dictionary<string, double>();
                switch (checkType)
                {

                    case CheckTypes.Flexure:
                        // 把每个组合的最大利用率转成字典
                        comboResults = new Dictionary<string, double>();
                        foreach (var combo in Global.Data.Combos.SelectedCombos)
                        {
                            double util = _result.FlexureStrength.GetComboMaxUtil(combo);
                            if (util > 0) comboResults[combo] = util;
                        }
                        break;
                    case CheckTypes.Shear:
                        comboResults = new Dictionary<string, double>();
                        foreach (var combo in Global.Data.Combos.SelectedCombos)
                        {
                            double util = _result.ShearStrength.GetComboMaxUtil(combo);
                            if (util > 0) comboResults[combo] = util;
                        }
                        break;
                    case CheckTypes.PhiB:
                        comboResults = new Dictionary<string, double>();
                        foreach (var combo in Global.Data.Combos.SelectedCombos)
                        {
                            double util = _result.PhiBResult.Collection[combo].PhiB.Value;
                            if (util > 0) comboResults[combo] = util;
                        }
                        break;
                    case CheckTypes.ColumnInPlane:
                        comboResults = new Dictionary<string, double>();
                        foreach (var combo in Global.Data.Combos.SelectedCombos)
                        {
                            double util = _result.InPlaneStability.Collection[combo].Util.Value;
                            if (util > 0) comboResults[combo] = util;
                        }
                        break;
                    case CheckTypes.ColumnOutPlane:
                        comboResults = new Dictionary<string, double>();
                        foreach (var combo in Global.Data.Combos.SelectedCombos)
                        {
                            double util = _result.OutPlaneStability.Collection[combo].Util.Value;
                            if (util > 0) comboResults[combo] = util;
                        }
                        break;
                    case CheckTypes.BeamLateral:
                        comboResults = new Dictionary<string, double>();
                        foreach (var combo in Global.Data.Combos.SelectedCombos)
                        {
                            double util = _result.LateralStability.Collection[combo].Util.Value;
                            if (util > 0) comboResults[combo] = util;
                        }
                        break;
                }
            }

            // 填到 ListView 里
            if (comboResults != null)
            {
                foreach (var (combo, util) in comboResults)
                {
                    var item = _lstCombo.Items.Add(combo);
                    item.UseItemStyleForSubItems = false;
                    item.SubItems.Add(util.ToString("F2"));
                    // 超限的涂红色
                    item.SubItems[1].ForeColor = GetUtilColor(util, _utilLimit);
                }
            }
        }
        private void FillStaticData()
        {
            // ===== 截面信息 =====
            _lstSec.Items.Clear();
                    
            var secBig = _member.StationSections[_member.EndStations[1]];
            var secSmall = _member.StationSections[_member.EndStations[0]];
            bool isTapered = _member.Section.IsTapered;

            if (isTapered)
            {
                AddSecRow("截面高度 H", secSmall?.H, "截面高度 H", secBig?.H);
                AddSecRow("翼缘宽度 B", secSmall?.TopB, "翼缘宽度 B", secBig?.TopB);
                AddSecRow("翼缘宽度 B", secSmall?.BotB, "翼缘宽度 B", secBig?.BotB);
                AddSecRow("腹板厚度 tw", secSmall?.Tw, "腹板厚度 tw", secBig?.Tw);
                AddSecRow("翼缘厚度 tf", secSmall?.TopTf, "翼缘厚度 tf", secBig?.TopTf);
                AddSecRow("翼缘厚度 tf", secSmall?.BotTf, "翼缘厚度 tf", secBig?.BotTf);
                AddSecRow("面积 A", secSmall?.Area, "面积 A", secBig?.Area);
                AddSecRow("I33", secSmall?.I33, "I33", secBig?.I33);
                AddSecRow("S33_上", secSmall?.S33_Top, "S33_上", secBig?.S33_Top);
                AddSecRow("S33_下", secSmall?.S33_Bot, "S33_下", secBig?.S33_Bot);
            }
            else
            {
                AddSecRow("截面高度 H", secBig?.H, "", null);
                AddSecRow("翼缘宽度 B", secBig?.TopB, "", null);
                AddSecRow("翼缘宽度 B", secBig?.BotTf, "", null);
                AddSecRow("腹板厚度 tw", secBig?.Tw, "", null);
                AddSecRow("翼缘厚度 tf", secBig?.TopTf, "", null);
                AddSecRow("翼缘宽度 B", secBig?.BotTf, "", null);
                AddSecRow("面积 A", secBig?.Area, "", null);
                AddSecRow("I33", secBig?.I33, "", null);
                AddSecRow("S33_上", secBig?.S33_Top, "", null);
                AddSecRow("S33_下", secBig?.S33_Bot, "", null);
            }

            // ===== 长细比 =====
            var slend = _result.Slenderness;
            if (slend != null)
            {
                _lstSlend.Items.Clear();

                // 平面内
                var item = _lstSlend.Items.Add("平面内长细比");
                item.SubItems.Add(slend.LambdaX.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(slend.UtilX.Value.ToString("F2"));
                item.UseItemStyleForSubItems = false;
                item.SubItems[1].ForeColor = GetUtilColor(slend.UtilX.Value, 1.0);
                item.SubItems[3].ForeColor = GetUtilColor(slend.UtilX.Value, 1.0);

                // 平面外
                item = _lstSlend.Items.Add("平面外长细比");
                item.SubItems.Add(slend.LambdaY.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(slend.UtilY.Value.ToString("F2"));
                item.UseItemStyleForSubItems = false;
                item.SubItems[1].ForeColor = GetUtilColor(slend.UtilY.Value, 1.0);
                item.SubItems[3].ForeColor = GetUtilColor(slend.UtilY.Value, 1.0);
            }

            // ===== 宽厚比 =====
            var local = _result.LocalStability;
            if (local != null)
            {
                _lstLocal.Items.Clear();

                // 上翼缘
                var item = _lstLocal.Items.Add("上翼缘宽厚比");
                item.SubItems.Add(local.FlangeWidthThicknessRatio_Top.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.FlangeUtil_Top.Value.ToString("F2"));
                item.UseItemStyleForSubItems = false;
                item.SubItems[1].ForeColor = GetUtilColor(local.FlangeUtil_Top.Value, 1.0);
                item.SubItems[3].ForeColor = GetUtilColor(local.FlangeUtil_Top.Value, 1.0);

                // 下翼缘
                item = _lstLocal.Items.Add("下翼缘宽厚比");
                item.SubItems.Add(local.FlangeWidthThicknessRatio_Bot.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.FlangeUtil_Bot.Value.ToString("F2"));
                item.UseItemStyleForSubItems = false;
                item.SubItems[1].ForeColor = GetUtilColor(local.FlangeUtil_Bot.Value, 1.0);
                item.SubItems[3].ForeColor = GetUtilColor(local.FlangeUtil_Bot.Value, 1.0);

                // 腹板
                item = _lstLocal.Items.Add("腹板高厚比");
                item.SubItems.Add(local.WebHightThicknessRatio.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.WebUtil.Value.ToString("F2"));
                item.UseItemStyleForSubItems = false;
                item.SubItems[1].ForeColor = GetUtilColor(local.WebUtil.Value, 1.0);
                item.SubItems[3].ForeColor = GetUtilColor(local.WebUtil.Value, 1.0);
            }
        }

        // 加一行截面参数
        private void AddSecRow(string name1, double? val1, string name2, double? val2)
        {
            var item = _lstSec.Items.Add(name1);
            item.SubItems.Add(val1?.ToString("F1") ?? "");
            item.SubItems.Add(name2);
            item.SubItems.Add(val2?.ToString("F1") ?? "");
        }

        private void LstCheckType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_lstCheckType.SelectedItems.Count == 0) return;
            string checkType = _lstCheckType.SelectedItems[0].Text;
            FillComboList(checkType);
        }
        private void LstCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_lstCombo.SelectedItems.Count == 0) return;

            string comboName = _lstCombo.SelectedItems[0].Text;
            string checkType = _lstCheckType.SelectedItems[0].Text;

            FillStationList(checkType, comboName);
        }
        // 默认选中最不利结果
        private void SelectWorstResult()
        {
            WorstResult? worst = _result.Worst;
            if (worst == null || worst.CheckType == "") return;

            // 1. 选最不利的验算类型
            foreach (ListViewItem item in _lstCheckType.Items)
            {
                if (item.Text == worst.CheckType)
                {
                    item.Selected = true;
                    break;
                }
            }

            // 2. 选最不利的组合（会自动触发填测站列表）
            if (worst.BestCombo != "")
            {
                foreach (ListViewItem item in _lstCombo.Items)
                {
                    if (item.Text == worst.BestCombo)
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

            // 3. 选最不利的测站
            if (worst.Station != null)
            {
                foreach (ListViewItem item in _lstStation.Items)
                {
                    if (item.Text == worst.Station.Value.ToString("F0"))
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
        }
        // 更新“显示详细文件”按钮的可用状态
        private void UpdateBtnDetailEnabled()
        {
            // 没选验算类型，直接禁用
            if (_lstCheckType.SelectedItems.Count == 0)
            {
                _btnDetail.Enabled = false;
                return;
            }

            string checkType = _lstCheckType.SelectedItems[0].Text;

            // 静态验算（长细比、局部稳定）：只需要选验算类型
            if (checkType == CheckTypes.Slenderness || checkType == CheckTypes.LocalStability)
            {
                _btnDetail.Enabled = true;
                return;
            }

            // 稳定验算（平面内/外、整体稳定）：需要选验算类型 + 组合
            if (checkType == CheckTypes.ColumnInPlane || checkType == CheckTypes.ColumnOutPlane || 
                checkType == CheckTypes.BeamLateral || checkType == CheckTypes.PhiB)
            {
                _btnDetail.Enabled = _lstCombo.SelectedItems.Count > 0;
                return;
            }

            // 强度验算（压弯/拉弯、抗剪）：需要选验算类型 + 组合 + 测站
            if (checkType == CheckTypes.Flexure || checkType == CheckTypes.Shear)
            {
                _btnDetail.Enabled = _lstCombo.SelectedItems.Count > 0
                                  && _lstStation.SelectedItems.Count > 0;
                return;
            }

            _btnDetail.Enabled = false;
        }


    }
}
