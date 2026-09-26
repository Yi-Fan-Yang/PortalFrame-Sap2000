using PortalFrame._2Check;
using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class MemberDetailForm : Form
    {
        private string _memberName;
        private ColumnCheckResult? _colResult;
        private BeamCheckResult? _beamResult;


        // 左栏
        private GroupBox _grpCheckType = new() { Text = "验算类型" };
        private ListBox _lstCheckType = new();

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
        private Button _btnBack = new() { Text = "返回" };

        public MemberDetailForm(string memberName, PostData postData)
        {
            Text = "构件验算详情";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            SetupLayout();
            SetupEvents();

            Text = $"构件验算详情 - {memberName}";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            _memberName = memberName;

            // 从 PostData 里拿这根构件的结果
            if (postData.ColumnResults.TryGetValue(memberName, out var col))
            {
                _colResult = col;
            }
            else if (postData.BeamResults.TryGetValue(memberName, out var beam))
            {
                _beamResult = beam;
            }

            SetupLayout();
            SetupEvents();
            FillStaticData();   // 填右栏静态数据
            FillCheckTypeList(); // 填左栏验算类型
            SelectWorstResult();
        }

        private void SetupLayout()
        {
            // 左栏上：验算类型
            _grpCheckType.Location = new Point(10, 10);
            _grpCheckType.Size = new Size(200, 200);
            _lstCheckType.Dock = DockStyle.Fill;
            _grpCheckType.Controls.Add(_lstCheckType);

            // 左栏下：测站
            _grpStation.Location = new Point(10, 220);
            _grpStation.Size = new Size(200, 300);
            _lstStation.Dock = DockStyle.Fill;
            _lstStation.View = View.Details;
            _lstStation.FullRowSelect = true;
            _lstStation.Columns.Add("测站", 80);
            _lstStation.Columns.Add("利用率", 80);
            _grpStation.Controls.Add(_lstStation);

            // 中栏：组合
            _grpCombo.Location = new Point(220, 10);
            _grpCombo.Size = new Size(350, 510);
            _lstCombo.Dock = DockStyle.Fill;
            _lstCombo.View = View.Details;
            _lstCombo.FullRowSelect = true;
            _lstCombo.Columns.Add("组合名", 250);
            _lstCombo.Columns.Add("利用率", 80);
            _grpCombo.Controls.Add(_lstCombo);

            // 右栏：静态结果
            // 右栏：三个 GroupBox
            // 1. 截面属性
            _grpSec.Location = new Point(580, 10);
            _grpSec.Size = new Size(250, 250);
            _lstSec.Dock= DockStyle.Fill;
            _lstSec.View = View.Details;
            _lstSec.FullRowSelect = true;
            _lstSec.Columns.Add("项目", 90);
            _lstSec.Columns.Add("小端", 80);
            _lstSec.Columns.Add("项目", 90);
            _lstSec.Columns.Add("大端", 80);
            _grpSec.Controls.Add(_lstSec);


            // 2. 长细比
            _grpSlend.Location = new Point(580, 270);
            _grpSlend.Size = new Size(250, 100);
            _lstSlend.Dock = DockStyle.Fill;
            _lstSlend.View = View.Details;
            _lstSlend.FullRowSelect = true;
            _lstSlend.Columns.Add("项目", 90);
            _lstSlend.Columns.Add("数值", 60);
            _lstSlend.Columns.Add("项目", 90);
            _lstSlend.Columns.Add("数值", 60);
            _grpSlend.Controls.Add(_lstSlend);

            // 3. 局部稳定（宽厚比）
            _grpLocal.Location = new Point(580, 380);
            _grpLocal.Size = new Size(250, 100);
            _lstLocal.Dock = DockStyle.Fill;
            _lstLocal.View = View.Details;
            _lstLocal.FullRowSelect = true;
            _lstLocal.Columns.Add("项目", 90);
            _lstLocal.Columns.Add("数值", 60);
            _lstLocal.Columns.Add("项目", 90);
            _lstLocal.Columns.Add("数值", 60);
            _grpLocal.Controls.Add(_lstLocal);

            // 按钮
            _btnDetail.Location = new Point(600, 490);
            _btnBack.Location = new Point(720, 490);

            Controls.Add(_grpCheckType);
            Controls.Add(_grpStation);
            Controls.Add(_grpCombo);
            Controls.Add(_grpSec);
            Controls.Add(_grpSlend);
            Controls.Add(_grpLocal);
            Controls.Add(_btnDetail);
            Controls.Add(_btnBack);
        }

        private void SetupEvents()
        {
            // 选验算类型 → 更新中栏组合列表
            _lstCheckType.SelectedIndexChanged += LstCheckType_SelectedIndexChanged;

            // 选中组合 → 更新左栏测站列表
            _lstCombo.SelectedIndexChanged += LstCombo_SelectedIndexChanged;

            // 返回按钮
            _btnBack.Click += (s, e) => Close();
        }
        private void FillCheckTypeList()
        {
            _lstCheckType.Items.Clear();

            // 根据是柱还是梁，加不同的验算类型
            if (_colResult != null)
            {
                _lstCheckType.Items.Add("压弯/拉弯");
                _lstCheckType.Items.Add("抗剪");
                _lstCheckType.Items.Add("平面内稳定");
                _lstCheckType.Items.Add("平面外稳定");
                _lstCheckType.Items.Add("长细比");
                _lstCheckType.Items.Add("局部稳定");
            }
            else if (_beamResult != null)
            {
                _lstCheckType.Items.Add("压弯/拉弯");
                _lstCheckType.Items.Add("抗剪");
                _lstCheckType.Items.Add("整体稳定");
                _lstCheckType.Items.Add("长细比");
                _lstCheckType.Items.Add("局部稳定");
            }
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
                        item.SubItems.Add(s.Util.ToString("F2"));
                        // 超限的涂红色
                        if (s.Util > 1.0)item.SubItems[1].ForeColor = Color.Red;
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
                        item.SubItems.Add(s.Util.ToString("F2"));
                        // 超限的涂红色
                        if(s.Util > 1.0)item.SubItems[1].ForeColor = Color.Red;
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
                            comboResults[combo] = list.Max(s => s.Util);
                        }
                        break;
                    case "抗剪":
                        comboResults = new Dictionary<string, double>();
                        foreach (var (combo, list) in _colResult.ShearStrength.Combos)
                        {
                            comboResults[combo] = list.Max(s => s.Util);
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

            var secSmall = _colResult?.SecSmall ?? _beamResult?.SecSmall;
            var secBig = _colResult?.SecBig ?? _beamResult?.SecBig;

            if (_colResult?.IsTapered == true || _beamResult?.IsTapered == true)
            {
                AddSecRow("截面高度 H", secSmall?.H, "截面高度 H", secBig?.H, false);
                AddSecRow("翼缘宽度 B", secSmall?.B, "翼缘宽度 B", secBig?.B, false);
                AddSecRow("腹板厚度 tw", secSmall?.Tw, "腹板厚度 tw", secBig?.Tw, false);
                AddSecRow("翼缘厚度 tf", secSmall?.Tf, "翼缘厚度 tf", secBig?.Tf, false);
                AddSecRow("面积 A", secSmall?.Area, "面积 A", secBig?.Area, false);
                AddSecRow("I33", secSmall?.I33, "I33", secBig?.I33, false);
                AddSecRow("S33_上", secSmall?.S33_Top, "S33_上", secBig?.S33_Top, false);
                AddSecRow("S33_下", secSmall?.S33_Bot, "S33_下", secBig?.S33_Bot, false);
            }
            else
            {
                AddSecRow("截面高度 H", secBig?.H, "", null, false);
                AddSecRow("翼缘宽度 B", secBig?.B, "", null, false);
                AddSecRow("腹板厚度 tw", secBig?.Tw, "", null, false);
                AddSecRow("翼缘厚度 tf", secBig?.Tf, "", null, false);
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
                item.SubItems.Add(slend.Lambda3.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(slend.Util3.ToString("F2"));
                if (slend.Util3 > 1.0)
                {
                    item.SubItems[1].ForeColor = Color.Red;
                    item.SubItems[3].ForeColor = Color.Red;
                }

                // 平面外
                item = _lstSlend.Items.Add("平面外长细比");
                item.SubItems.Add(slend.Lambda2.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(slend.Util2.ToString("F2"));
                if (slend.Util2 > 1.0)
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
                item.SubItems.Add(local.FlangeWidthThicknessRatio_Top.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.FlangeUtil_Top.ToString("F2"));
                if (local.FlangeUtil_Top > 1.0)
                {
                    item.SubItems[1].ForeColor = Color.Red;
                    item.SubItems[3].ForeColor = Color.Red;
                }

                // 下翼缘
                item = _lstLocal.Items.Add("下翼缘宽厚比");
                item.SubItems.Add(local.FlangeWidthThicknessRatio_Bot.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.FlangeUtil_Bot.ToString("F2"));
                if (local.FlangeUtil_Bot > 1.0)
                {
                    item.SubItems[1].ForeColor = Color.Red;
                    item.SubItems[3].ForeColor = Color.Red;
                }

                // 腹板
                item = _lstLocal.Items.Add("腹板高厚比");
                item.SubItems.Add(local.WebHightThicknessRatio.ToString("F1"));
                item.SubItems.Add("利用率");
                item.SubItems.Add(local.WebUtil.ToString("F2"));
                if (local.WebUtil > 1.0)
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
            string checkType = _lstCheckType.SelectedItem?.ToString() ?? "";
            FillComboList(checkType);
        }
        private void LstCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_lstCombo.SelectedItems.Count == 0) return;

            string comboName = _lstCombo.SelectedItems[0].Text;
            string checkType = _lstCheckType.SelectedItem?.ToString() ?? "";

            FillStationList(checkType, comboName);
        }
        // 默认选中最不利结果
        private void SelectWorstResult()
        {
            WorstResult? worst = _colResult?.Worst ?? _beamResult?.Worst;
            if (worst == null || worst.CheckType == "") return;


            // 1. 选最不利的验算类型（会自动触发填组合列表）
            int typeIndex = _lstCheckType.Items.IndexOf(worst.CheckType);
            if (typeIndex < 0) return;
            _lstCheckType.SelectedIndex = typeIndex;

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

    }
}
