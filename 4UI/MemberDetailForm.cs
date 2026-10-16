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
        private string _memberName;
        private ColumnCheckResult? _colResult;
        private BeamCheckResult? _beamResult;


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
            this.Font = UiConfig.DefaultFont;
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
            _memberName = member.Name;

            if (member.CheckResult is ColumnCheckResult col)
                _colResult = col;
            else if (member.CheckResult is BeamCheckResult beam)
                _beamResult = beam;

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

            // 左边两个按钮上下排
            _btnDetail.Size = new Size(140, 35);
            _btnDetail.Location = new Point(900, 640);

            _btnReport.Size = new Size(140, 35);
            _btnReport.Location = new Point(900, 685);

            // 右边返回按钮垂直居中
            _btnBack.Size = new Size(120, 40);
            _btnBack.Location = new Point(1100, 662);


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
            double? station = _lstStation.SelectedItems.Count > 0 ? double.Parse(_lstStation.SelectedItems[0].Text) : null;

            string text = Report.BuildDetailText(_member, checkType, comboName, station);
            // 弹一个简单窗口显示
            var form = new Form
            {
                Text = "详细参数",
                Width = 600,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent
            };
            var txt = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10),
                WordWrap = false,
                Text = text
            };
            form.Controls.Add(txt);
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

            if (_colResult != null)
            {
                AddItem("压弯/拉弯", _colResult.FlexureStrength.MaxUtil > 1.0);
                AddItem("抗剪", _colResult.ShearStrength.MaxUtil > 1.0);
                AddItem("平面内稳定", _colResult.InPlaneStability.MaxUtil > 1.0);
                AddItem("平面外稳定", _colResult.OutPlaneStability.MaxUtil > 1.0);
                AddItem("长细比", _colResult.Slenderness.MaxUtil > 1.0);
                AddItem("局部稳定", _colResult.LocalStability.MaxUtil > 1.0);
            }
            else if (_beamResult != null)
            {
                AddItem("压弯/拉弯", _beamResult.FlexureStrength.MaxUtil > 1.0);
                AddItem("抗剪", _beamResult.ShearStrength.MaxUtil > 1.0);
                AddItem("整体稳定", _beamResult.LateralStability.MaxUtil > 1.0);
                AddItem("长细比", _beamResult.Slenderness.MaxUtil > 1.0);
                AddItem("局部稳定", _beamResult.LocalStability.MaxUtil > 1.0);
            }
        }

        private void AddItem(string text, bool overLimit)
        {
            var item = _lstCheckType.Items.Add(text);
            item.UseItemStyleForSubItems = false;
            if (overLimit) item.ForeColor = Color.Red;
        }
        private void FillStationList(string checkType, string comboName)
        {
            _lstStation.Items.Clear();

            // 只有强度验算（压弯/拉弯、抗剪）才有测站
            if (checkType == "压弯/拉弯" && _colResult != null)
            {
                if (_colResult.FlexureStrength.Combos.TryGetValue(comboName, out var list))
                {
                    foreach (var s in list)
                    {
                        var item = _lstStation.Items.Add(s.Station.ToString("F0"));
                        item.UseItemStyleForSubItems = false;
                        item.SubItems.Add(s.Util.Value.ToString("F2"));
                        // 超限的涂红色
                        if (s.Util.Value > 1.0)item.SubItems[1].ForeColor = Color.Red;
                    }
                }
            }
            else if (checkType == "抗剪" && _colResult != null)
            {
                if (_colResult.ShearStrength.Combos.TryGetValue(comboName, out var list))
                {
                    foreach (var s in list)
                    {
                        var item = _lstStation.Items.Add(s.Station.ToString("F0"));
                        item.UseItemStyleForSubItems = false;
                        item.SubItems.Add(s.Util.Value.ToString("F2"));
                        // 超限的涂红色
                        if(s.Util.Value > 1.0)item.SubItems[1].ForeColor = Color.Red;
                    }
                }
            }
            // 稳定、长细比、宽厚比没有测站，测站列表空着

        }
        private void FillComboList(string checkType)
        {
            _lstCombo.Items.Clear();

            // 根据验算类型，拿对应的组合结果
            Dictionary<string, double>? comboResults = null;

            if (_colResult != null)
            {
                switch (checkType)
                {

                    case "压弯/拉弯":
                        // 把每个组合的抗弯最大利用率转成字典
                        comboResults = new Dictionary<string, double>();
                        foreach (var (combo, list) in _colResult.FlexureStrength.Combos)
                        {
                            comboResults[combo] = list.Max(s => s.Util.Value);
                        }
                        break;
                    case "抗剪":
                        comboResults = new Dictionary<string, double>();
                        foreach (var (combo, list) in _colResult.ShearStrength.Combos)
                        {
                            comboResults[combo] = list.Max(s => s.Util.Value);
                        }
                        break;
                    case "平面内稳定":
                        comboResults = _colResult.InPlaneStability.Combos;
                        break;
                    case "平面外稳定":
                        comboResults = _colResult.OutPlaneStability.Combos;
                        break;
                }
            }
            // 梁的后面再加

            // 填到 ListView 里
            if (comboResults != null)
            {
                foreach (var (combo, util) in comboResults)
                {
                    var item = _lstCombo.Items.Add(combo);
                    item.UseItemStyleForSubItems = false;
                    item.SubItems.Add(util.ToString("F2"));

                    // 超限的涂红色
                    if (util > 1.0)item.SubItems[1].ForeColor = Color.Red;
                }
            }
        }
        private void FillStaticData()
        {
            // ===== 截面信息 =====
            _lstSec.Items.Clear();

            var secSmall = _member.SecSmall;
            var secBig = _member.SecBig;
            bool isTapered = _member.Section.IsTapered;

            if (isTapered)
            {
                AddSecRow("截面高度 H", secSmall?.H, "截面高度 H", secBig?.H, false);
                AddSecRow("翼缘宽度 B", secSmall?.TopB, "翼缘宽度 B", secBig?.TopB, false);
                AddSecRow("翼缘宽度 B", secSmall?.BotB, "翼缘宽度 B", secBig?.BotB, false);
                AddSecRow("腹板厚度 tw", secSmall?.Tw, "腹板厚度 tw", secBig?.Tw, false);
                AddSecRow("翼缘厚度 tf", secSmall?.TopTf, "翼缘厚度 tf", secBig?.TopTf, false);
                AddSecRow("翼缘厚度 tf", secSmall?.BotTf, "翼缘厚度 tf", secBig?.BotTf, false);
                AddSecRow("面积 A", secSmall?.Area, "面积 A", secBig?.Area, false);
                AddSecRow("I33", secSmall?.I33, "I33", secBig?.I33, false);
                AddSecRow("S33_上", secSmall?.S33_Top, "S33_上", secBig?.S33_Top, false);
                AddSecRow("S33_下", secSmall?.S33_Bot, "S33_下", secBig?.S33_Bot, false);
            }
            else
            {
                AddSecRow("截面高度 H", secBig?.H, "", null, false);
                AddSecRow("翼缘宽度 B", secBig?.TopB, "", null, false);
                AddSecRow("翼缘宽度 B", secBig?.BotTf, "", null, false);
                AddSecRow("腹板厚度 tw", secBig?.Tw, "", null, false);
                AddSecRow("翼缘厚度 tf", secBig?.TopTf, "", null, false);
                AddSecRow("翼缘宽度 B", secBig?.BotTf, "", null, false);
                AddSecRow("面积 A", secBig?.Area, "", null, false);
                AddSecRow("I33", secBig?.I33, "", null, false);
                AddSecRow("S33_上", secBig?.S33_Top, "", null, false);
                AddSecRow("S33_下", secBig?.S33_Bot, "", null, false);
            }

            // ===== 长细比 =====
            var slend = _colResult?.Slenderness ?? _beamResult?.Slenderness;
            if (slend != null)
            {
                _lstSlend.Items.Clear();

                // 平面内
                var item = _lstSlend.Items.Add("平面内长细比");
                item.SubItems.Add(slend.LambdaX.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(slend.UtilX.Value.ToString("F2"));
                item.UseItemStyleForSubItems = false;
                if (slend.UtilX.Value > 1.0)
                {
                    item.SubItems[1].ForeColor = Color.Red;
                    item.SubItems[3].ForeColor = Color.Red;
                }

                // 平面外
                item = _lstSlend.Items.Add("平面外长细比");
                item.SubItems.Add(slend.LambdaY.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(slend.UtilY.Value.ToString("F2"));
                item.UseItemStyleForSubItems = false;
                if (slend.UtilY.Value > 1.0)
                {
                    item.SubItems[1].ForeColor = Color.Red;
                    item.SubItems[3].ForeColor = Color.Red;
                }
            }

            // ===== 宽厚比 =====
            var local = _colResult?.LocalStability ?? _beamResult?.LocalStability;
            if (local != null)
            {
                _lstLocal.Items.Clear();

                // 上翼缘
                var item = _lstLocal.Items.Add("上翼缘宽厚比");
                item.SubItems.Add(local.FlangeWidthThicknessRatio_Top.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.FlangeUtil_Top.Value.ToString("F2"));
                if (local.FlangeUtil_Top.Value > 1.0)
                {
                    item.SubItems[1].ForeColor = Color.Red;
                    item.SubItems[3].ForeColor = Color.Red;
                }

                // 下翼缘
                item = _lstLocal.Items.Add("下翼缘宽厚比");
                item.SubItems.Add(local.FlangeWidthThicknessRatio_Bot.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.FlangeUtil_Bot.Value.ToString("F2"));
                if (local.FlangeUtil_Bot.Value > 1.0)
                {
                    item.SubItems[1].ForeColor = Color.Red;
                    item.SubItems[3].ForeColor = Color.Red;
                }

                // 腹板
                item = _lstLocal.Items.Add("腹板高厚比");
                item.SubItems.Add(local.WebHightThicknessRatio.Value.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.WebUtil.Value.ToString("F2"));
                if (local.WebUtil.Value > 1.0)
                {
                    item.SubItems[1].ForeColor = Color.Red;
                    item.SubItems[3].ForeColor = Color.Red;
                }
            }
        }

        // 加一行截面参数
        private void AddSecRow(string name1, double? val1, string name2, double? val2, bool overLimit)
        {
            var item = _lstSec.Items.Add(name1);
            item.SubItems.Add(val1?.ToString("F1") ?? "");
            item.SubItems.Add(name2);
            item.SubItems.Add(val2?.ToString("F1") ?? "");

            if (overLimit)
            {
                item.SubItems[1].ForeColor = Color.Red;
                item.SubItems[3].ForeColor = Color.Red;
            }
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
            WorstResult? worst = _colResult?.Worst ?? _beamResult?.Worst;
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
            if (worst.BestStation != null)
            {
                foreach (ListViewItem item in _lstStation.Items)
                {
                    if (item.Text == worst.BestStation.Value.ToString("F0"))
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
            if (checkType == "长细比" || checkType == "局部稳定")
            {
                _btnDetail.Enabled = true;
                return;
            }

            // 稳定验算（平面内/外、整体稳定）：需要选验算类型 + 组合
            if (checkType == "平面内稳定" || checkType == "平面外稳定" || checkType == "整体稳定")
            {
                _btnDetail.Enabled = _lstCombo.SelectedItems.Count > 0;
                return;
            }

            // 强度验算（压弯/拉弯、抗剪）：需要选验算类型 + 组合 + 测站
            if (checkType == "压弯/拉弯" || checkType == "抗剪")
            {
                _btnDetail.Enabled = _lstCombo.SelectedItems.Count > 0
                                  && _lstStation.SelectedItems.Count > 0;
                return;
            }

            _btnDetail.Enabled = false;
        }


    }
}
