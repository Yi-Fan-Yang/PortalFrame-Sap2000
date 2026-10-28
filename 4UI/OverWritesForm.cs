using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._3Dispatch;
using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class OverWritesForm : Form
    {
        private List<MemberData> _members;
        private List<Func<MemberData, ParaItem>> _rowGetters = new();
        private TextBox _txtValue = new();
        private string _currentCombo = "";
        private double _currentStation = 0;
        private ListViewItem? _activeCmbRow;


        private Button _btnOk = new() { Text = "确定" };
        private Button _btnCancel = new() { Text = "取消" };
        private Button _btnReset = new() { Text = "清空覆盖" };
        private ListView _lst = new();

        private ComboBox _cmbCombo = new();
        private ComboBox _cmbStation = new();
        private ComboBox _cmbSeismic = new();
        private ComboBox _cmbStiffener = new();
        private ComboBox _cmbBrace = new();
        private ComboBox _cmbType = new();

        private enum Row
        {
            Gamma0,             // 结构重要性系数
            MMR,                // 弯矩放大系数
            GammaX,             // γx
            BetaMx,             // βMx
            PhiB,               // φb
            PhiPs,              // φps
            Rho,                // ρ
            Ly,                 // 计算长度
            HasStiffener,       // 是否加劲肋
            a,                  // 加劲肋间距
            HasBrace,           // 是否隅撑
            Ap,                 // 檩条截面积
            Beta,               // 连接点距主梁距离比β
            Lp,                 // 檩条跨度
            Ip,                 // 檩条I33
            A,                  // 檩条形心距离
            Ak,                 // 隅撑截面积
            Lkk,                // 隅撑间距
            Alpha,              // 隅撑夹角
            SeismicType,        // 抗震类型
            LambdaLimit,        // 长细比限值
            FlangeLimit,        // 翼缘宽厚比
            WebLimit,           // 腹板高厚比
            UtilLimit,          // 应力比限值
            MuX,                // μx
            PhiX,               // φx
            PhiY,               // φy
        }



        public OverWritesForm(List<MemberData> members)
        {
            this.Font = Global.Ui.DefaultFont;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            _members = members;

            Text = "构件覆盖项";
            Width = 580;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;

            PreferencesForm.InitListView(_lst);
            LayoutControls();
            SetupEvents();
            LoadData();
        }

        private void LayoutControls()
        {
            _lst.Dock = DockStyle.Top;
            _lst.Height = 600;
            Controls.Add(_lst);

            _btnReset.Location = new Point(120, 615);
            _btnReset.Size = new Size(100, 35);
            Controls.Add(_btnReset);

            _btnOk.Location = new Point(250, 615);
            _btnOk.Size = new Size(100, 35);
            Controls.Add(_btnOk);

            _btnCancel.Location = new Point(380, 615);
            _btnCancel.Size = new Size(100, 35);
            Controls.Add(_btnCancel);

            _cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbType.Items.AddRange(new object[] { "柱", "梁" });
            _cmbType.SelectedIndex = 0;
            _cmbType.Visible = false;
            Controls.Add(_cmbType);

            // 只加用户选定的组合
            _cmbCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (var combo in Global.Data.Combos.SelectedCombos)
            {
                _cmbCombo.Items.Add(combo);
            }
            if (_cmbCombo.Items.Count > 0)
            {
                _cmbCombo.SelectedIndex = 0;
                _currentCombo = Global.Data.Combos.SelectedCombos[0];
            }
            _cmbCombo.Visible = false;
            Controls.Add(_cmbCombo);
            
            // 从第一个构件的结果类里拿测站列表
            _cmbStation.DropDownStyle = ComboBoxStyle.DropDownList;
            var firstMember = _members[0];
            var flexDict = firstMember.CheckResult.FlexureStrength.Collection[_currentCombo];
            foreach (var station in firstMember.Stations.Values)
            {
                _cmbStation.Items.Add(station);
            }
            if (_cmbStation.Items.Count > 0)
            {
                _cmbStation.SelectedIndex = 0;
                _currentStation = firstMember.Stations[0];
            }
            // 多选的时候，测站下拉框不允许修改
            if (_members.Count > 1)
            {
                _cmbStation.Enabled = false;  // 灰色，不允许修改
                _cmbStation.SelectedIndex = 0;
            }
            _cmbStation.Visible = false;
            Controls.Add(_cmbStation);


            _cmbStiffener.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbStiffener.Items.AddRange(new object[] { "否", "是" });
            _cmbStiffener.SelectedIndex = 0;
            _cmbStiffener.Visible = false;
            Controls.Add(_cmbStiffener);

            _cmbBrace.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbBrace.Items.AddRange(new object[] { "否", "是" });
            _cmbBrace.SelectedIndex = 1;
            _cmbBrace.Visible = false;
            Controls.Add(_cmbBrace);

            _cmbSeismic.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbSeismic.Items.AddRange(new object[] { "自定义限值", "非抗震", "抗震" });
            _cmbSeismic.SelectedIndex = 0;
            _cmbSeismic.Visible = false;
            Controls.Add(_cmbSeismic);

            _txtValue.BorderStyle = BorderStyle.FixedSingle;
            _txtValue.Visible = false;
            Controls.Add(_txtValue);
        }

        private void SetupEvents()
        {
            _lst.MouseClick += (s, e) =>PreferencesForm.Lst_MouseClick(_lst, e, _txtValue, _cmbType, _cmbSeismic, 
                                        _cmbStiffener, _cmbBrace, _cmbCombo, _cmbStation);
            _lst.MouseClick += (s, e) => SyncCmbToRow(_lst, e);
            _cmbType.SelectedIndexChanged += (s, e) =>
            {
                if (_activeCmbRow != null)
                    _activeCmbRow.SubItems[1].Text = _cmbType.SelectedItem?.ToString() ?? "";
                RefreshGray();
            };
            _cmbStiffener.SelectedIndexChanged += Cmb_ValueChanged;
            _cmbBrace.SelectedIndexChanged += Cmb_ValueChanged;
            _cmbSeismic.SelectedIndexChanged += Cmb_ValueChanged;
            _cmbCombo.SelectedIndexChanged += (s, e) =>
            {
                _currentCombo = Global.Data.Combos.SelectedCombos[_cmbCombo.SelectedIndex];
                _lst.Items.Clear();
                LoadData();
            };
            _cmbStation.SelectedIndexChanged += (s, e) =>
            {
                var flexDict = _members[0].CheckResult.FlexureStrength.Collection[_currentCombo];
                _currentStation = _members[0].Stations[_cmbStation.SelectedIndex];
                _lst.Items.Clear();
                LoadData();
            };


            _btnOk.Click += BtnOk_Click;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            _btnReset.Click += BtnReset_Click;
        }

        // 数值下拉框（加劲板/隅撑/抗震）：选中索引=数值，写回单元格
        private void Cmb_ValueChanged(object? sender, EventArgs e)
        {
            if (sender is not ComboBox cmb || _activeCmbRow == null) return;
            int idx = cmb.SelectedIndex;
            if (idx >= 0) _activeCmbRow.SubItems[1].Text = idx.ToString();
            RefreshGray();
        }
        
        // 点行时，把当前单元格值同步到下拉框（避免下拉框残留上次的选择）
        private void SyncCmbToRow(ListView lst, MouseEventArgs e)
        {
            var hit = lst.HitTest(e.Location);
            if (hit.Item == null || hit.SubItem != hit.Item.SubItems[1]) return;
            if (hit.Item.Tag is not ComboBox cmb) return;

            _activeCmbRow = hit.Item;

            // 组合和测站是切换查看用的，不写值，跳过
            if (cmb == _cmbCombo || cmb == _cmbStation) return;

            // 构件类别：文本同步
            if (cmb == _cmbType)
            {
                cmb.SelectedItem = hit.Item.SubItems[1].Text;
                return;
            }

            // 数值下拉框：单元格数字 → 选中对应项
            if (double.TryParse(hit.Item.SubItems[1].Text, out double v))
            {
                int idx = (int)v;
                if (idx >= 0 && idx < cmb.Items.Count) cmb.SelectedIndex = idx;
            }
        }
        private void LoadData()
        {
            // 先清空旧的行和旧的 getter
            _lst.Items.Clear();
            _rowGetters.Clear();
            // 构件类别行
            var rowType = new ListViewItem("构件类别");
            string typeDisplay;
            if (_members.Count == 1)
            {
                typeDisplay = _members[0].Type == MemberType.Column ? "柱" : "梁";
            }
            else
            {
                bool allSameType = _members.All(m => m.Type == _members[0].Type);
                typeDisplay = allSameType ? (_members[0].Type == MemberType.Column ? "柱" : "梁") : "varies";
            }

            rowType.SubItems.Add(typeDisplay);
            rowType.SubItems.Add("");
            rowType.Tag = _cmbType;
            _lst.Items.Add(rowType);


            // 设计组合行（特殊处理，不是 ParaItem）
            var rowCombo = new ListViewItem("设计组合");
            rowCombo.SubItems.Add(_currentCombo);
            rowCombo.SubItems.Add("");
            rowCombo.Tag = _cmbCombo;
            _lst.Items.Add(rowCombo);

            // 测站位置行（特殊处理，不是 ParaItem）
            var rowStation = new ListViewItem("测站位置");
            rowStation.SubItems.Add(_currentStation.ToString("F1"));
            rowStation.SubItems.Add("mm");
            rowStation.Tag = _cmbStation;
            _lst.Items.Add(rowStation);

            // ===== 原来的参数行 =====
            AddRow(m => m.CheckResult.General.Gamma0);
            AddRow(m => m.CheckResult.General.MMR);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].GammaX);
            AddRow(m => m.CheckResult.InPlaneStability.Collection[_currentCombo].BetaMx);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].PhiB);

            AddRow(m => m.CheckResult.ShearStrength.Collection[_currentCombo][_currentStation].PhiPs, multiEditable: false);
            AddRow(m => m.CheckResult.FlexureStrength.Collection[_currentCombo][_currentStation].Rho, multiEditable: false);
            AddRow(m => m.CheckResult.Slenderness.Ly);

            AddRow(m => m.CheckResult.ShearStrength.HasStiffener, _cmbStiffener);
            AddRow(m => m.CheckResult.ShearStrength.a);

            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].HasBrace, _cmbBrace);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].Ap);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].Beta);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].Lp);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].Ip);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].A);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].Ak);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].Lkk);
            AddRow(m => m.CheckResult.PhiBResult.Collection[_currentCombo].Alpha);

            AddRow(m => m.CheckResult.General.SeismicType, _cmbSeismic);
            AddRow(m => m.CheckResult.General.LambdaLimit);
            AddRow(m => m.CheckResult.General.FlangeLimit);
            AddRow(m => m.CheckResult.General.WebLimit);
            AddRow(m => m.CheckResult.General.UtilLimit);

            AddRow(m => m.CheckResult.Slenderness.MuX);
            AddRow(m => m.CheckResult.Slenderness.PhiX);
            AddRow(m => m.CheckResult.Slenderness.PhiY);

            RefreshGray();
        }

        private void AddRow(Func<MemberData, ParaItem> getField, ComboBox? cmb = null, bool multiEditable = true)
        {
            // 存起来
            _rowGetters.Add(getField);

            // 显示值：遍历所有构件，比较 Ow 值
            double? commonVal = GetCommonValue(getField);
            string displayVal;
            if (_members.Count == 1)
            {
                displayVal = commonVal.HasValue ? commonVal.Value.ToString() : "";
            }
            else
            {
                // 多选时，如果不允许修改，就显示第一个构件的值，否则显示 varies
                if (multiEditable)
                {
                    displayVal = commonVal.HasValue ? commonVal.Value.ToString() : "varies";
                }
                else
                {
                    displayVal = commonVal.HasValue ? commonVal.Value.ToString() : "";
                }
            }

            var firstItem = getField(_members[0]);
            if (firstItem == null)
            {
                var row = new ListViewItem("");
                row.SubItems.Add(displayVal);
                row.SubItems.Add("");
                _lst.Items.Add(row);
                return;
            }

            var row2 = new ListViewItem(getField(_members[0]).Name);
            row2.SubItems.Add(displayVal);
            row2.SubItems.Add(getField(_members[0]).Unit);

            if (getField(_members[0]).Source == ValueSource.OverWrite)
            {
                row2.ForeColor = Color.Red;
            }

            _lst.Items.Add(row2);
            if (cmb != null) row2.Tag = cmb;
        }

        private double? GetCommonValue(Func<MemberData, ParaItem> getField)
        {
            double? first = getField(_members[0]).GetOw();
            foreach (var m in _members.Skip(1))
            {
                if (getField(m).GetOw() != first)
                    return null;
            }
            return first;
        }

        private void RefreshGray()
        {
            int offset = 3;  // 前3行是特殊行
            bool mixed = _members.Any(m => m.Type == MemberType.Column) && _members.Any(m => m.Type == MemberType.Beam);

            if (mixed)
            {
                PreferencesForm.SetRowGray(_lst, (int)Row.MuX + offset, true);
                PreferencesForm.SetRowGray(_lst, (int)Row.PhiX + offset, true);
                PreferencesForm.SetRowGray(_lst, (int)Row.PhiY + offset, true);
            }

            PreferencesForm.RefreshSeismicRows(_lst, _cmbSeismic,
                (int)Row.LambdaLimit + offset, (int)Row.FlangeLimit + offset, (int)Row.WebLimit + offset);

            bool hasStiff = _cmbStiffener.SelectedIndex == 1;
            PreferencesForm.SetRowGray(_lst, (int)Row.a + offset, !hasStiff);

            bool hasBrace = _cmbBrace.SelectedIndex == 1;
            PreferencesForm.SetRowGray(_lst, (int)Row.Ap + offset, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Beta + offset, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Lp + offset, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Ip + offset, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.A + offset, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Ak + offset, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Lkk + offset, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Alpha + offset, !hasBrace);
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            try
            {
                // 把TextBox里正在编辑的值写回行
                if (_txtValue.Visible && _lst.SelectedItems.Count > 0)
                {
                    _lst.SelectedItems[0].SubItems[1].Text = _txtValue.Text;
                }
                // ===== 先处理特殊行（不是 ParaItem 类型的） =====
                // 第0行：构件类别
                string typeText = _lst.Items[0].SubItems[1].Text;
                if (typeText == "柱" || typeText == "梁")   // 只有明确选了柱/梁才改
                {
                    MemberType newType = typeText == "柱" ? MemberType.Column : MemberType.Beam;
                    foreach (var member in _members)
                    {
                        member.Type = newType;
                    }
                }
                // 第1行：设计组合（只看不写，不用处理）
                // 第2行：测站位置（只看不写，不用处理）

                // ===== 再处理 ParaItem 行 =====
                // 从第3行开始，原来的 _rowGetters 对应第3行及以后
                for (int i = 0; i < _rowGetters.Count; i++)
                {
                    int rowIndex = i + 3;  // 跳过前3行特殊行

                    // 拿用户输入的值
                    string text = _lst.Items[rowIndex].SubItems[1].Text;
                    double? newVal = double.TryParse(text, out var v) ? v : null;

                    // 遍历所有构件，写回
                    foreach (var member in _members)
                    {
                        var item = _rowGetters[i](member);
                        if (newVal.HasValue)
                            item.SetOw(newVal.Value);
                        else
                            item.ResetOw();
                    }
                }
                // 持久化
                foreach (var member in _members)
                {
                    Persistence.SaveMemberOverWrites(member);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"崩溃了：{ex.Message}\n\n位置：{ex.StackTrace}");
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            foreach (var member in _members)
            {
                var result = member.CheckResult;
                SectionCal.GetMemberType(member);

                // General 里的
                result.General.Gamma0.ResetOw();
                result.General.MMR.ResetOw();
                result.General.SeismicType.ResetOw();
                result.General.LambdaLimit.ResetOw();
                result.General.FlangeLimit.ResetOw();
                result.General.WebLimit.ResetOw();
                result.General.UtilLimit.ResetOw();

                // Slenderness 里的
                result.Slenderness.Ly.ResetOw();
                result.Slenderness.MuX.ResetOw();
                result.Slenderness.PhiX.ResetOw();
                result.Slenderness.PhiY.ResetOw();

                // InPlaneStability 里的
                foreach (var inPlane in result.InPlaneStability.Collection.Values)
                {
                    inPlane.BetaMx.ResetOw();
                }
                result.ShearStrength.HasStiffener.ResetOw();
                result.ShearStrength.a.ResetOw();


                // ShearStrength 里的每个测站
                foreach (var shearDict in result.ShearStrength.Collection.Values)
                {
                    foreach (var shear in shearDict.Values)
                    {
                        shear.PhiPs.ResetOw();
                    }
                }

                // FlexureStrength 里的每个测站
                foreach (var flexDict in result.FlexureStrength.Collection.Values)
                {
                    foreach (var flex in flexDict.Values)
                    {
                        flex.Rho.ResetOw();
                    }
                }


                // PhiBDetails 里的每个
                foreach (var details in result.PhiBResult.Collection.Values)
                {
                    details.HasBrace.ResetOw();
                    details.Ap.ResetOw();
                    details.Beta.ResetOw();
                    details.Lp.ResetOw();
                    details.Ip.ResetOw();
                    details.A.ResetOw();
                    details.Ak.ResetOw();
                    details.Lkk.ResetOw();
                    details.Alpha.ResetOw();
                    details.GammaX.ResetOw();
                    details.PhiB.ResetOw();
                }
            }

            _lst.Items.Clear();
            LoadData();
        }



    }
}
