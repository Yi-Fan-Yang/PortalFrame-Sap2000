using PortalFrame._1Model;
using PortalFrame._2CheckResults;
using PortalFrame._3Dispatch;
using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class OverWritesForm : Form
    {
        private List<MemberData> _members;
        private Dictionary<Row, string> _originalValues = new();
        private List<RowAction> _actions = new();
        private TextBox _txtValue = new();


        private Button _btnOk = new() { Text = "确定" };
        private Button _btnCancel = new() { Text = "取消" };
        private Button _btnReset = new() { Text = "清空覆盖" };
        private ListView _lst = new();

        private ComboBox _cmbSeismic = new();
        private ComboBox _cmbStiffener = new();
        private ComboBox _cmbBrace = new();
        private ComboBox _cmbType = new();

        private enum Row
        {
            ElementType, Gamma0, MMR, GammaX, BetaMx, PhiB, PhiPs, Rho, Ly,
            HasStiffener, StiffenerSpacing,
            HasBrace, PurlinArea, PurlinSpan, PurlinI33, PurlinDistToFlange, BraceArea, BraceSpacing, BraceAngle,
            SeismicType, LambdaLimit, FlangeLimit, WebLimit, UtilLimit,
            MuX, PhiX, PhiY,
        }

        private class RowAction
        {
            public Row Row;
            public Func<string> GetText;
            public Action<MemberData, string> SetValue;

            public RowAction(Row row, Func<string> getText, Action<MemberData, string> setValue)
            {
                Row = row;
                GetText = getText;
                SetValue = setValue;
            }
        }

        public OverWritesForm(List<MemberData> members)
        {
            this.Font = UiConfig.DefaultFont;
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

            _btnReset.Location = new Point(120, 630);
            _btnReset.Size = new Size(100, 35);
            Controls.Add(_btnReset);

            _btnOk.Location = new Point(250, 630);
            _btnOk.Size = new Size(100, 35);
            Controls.Add(_btnOk);

            _btnCancel.Location = new Point(380, 630);
            _btnCancel.Size = new Size(100, 35);
            Controls.Add(_btnCancel);

            _cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbType.Items.AddRange(new object[] { "柱", "梁" });
            _cmbType.SelectedIndex = 0;
            _cmbType.Visible = false;
            Controls.Add(_cmbType);

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
            _lst.MouseClick += (s, e) =>PreferencesForm.Lst_MouseClick(_lst, e, _txtValue, _cmbType, _cmbSeismic, _cmbStiffener, _cmbBrace);

            _cmbType.SelectedIndexChanged += (s, e) => RefreshGray();
            _cmbStiffener.SelectedIndexChanged += (s, e) => RefreshGray();
            _cmbBrace.SelectedIndexChanged += (s, e) => RefreshGray();
            _cmbSeismic.SelectedIndexChanged += (s, e) => RefreshGray();

            _btnOk.Click += BtnOk_Click;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            _btnReset.Click += BtnReset_Click;
        }

        private void LoadData()
        {
            AddRow(m => m.OverWrites.ElementType, _cmbType);
            AddRow(m => m.OverWrites.Gamma0);
            AddRow(m => m.OverWrites.MMR);
            AddRow(m => m.OverWrites.GammaX);
            AddRow(m => m.OverWrites.BetaMx);
            AddRow(m => m.OverWrites.PhiB);
            AddRow(m => m.OverWrites.PhiPs);
            AddRow(m => m.OverWrites.Rho);
            AddRow(m => m.OverWrites.Ly);

            AddRow(m => m.OverWrites.HasStiffener, _cmbStiffener);
            AddRow(m => m.OverWrites.StiffenerSpacing);

            AddRow(m => m.OverWrites.HasBrace, _cmbBrace);
            AddRow(m => m.OverWrites.PurlinArea);
            AddRow(m => m.OverWrites.PurlinSpan);
            AddRow(m => m.OverWrites.PurlinI33);
            AddRow(m => m.OverWrites.PurlinDistToFlange);
            AddRow(m => m.OverWrites.BraceArea);
            AddRow(m => m.OverWrites.BraceSpacing);
            AddRow(m => m.OverWrites.BraceAngle);

            AddRow(m => m.OverWrites.SeismicType, _cmbSeismic);
            AddRow(m => m.OverWrites.LambdaLimit);
            AddRow(m => m.OverWrites.FlangeLimit);
            AddRow(m => m.OverWrites.WebLimit);
            AddRow(m => m.OverWrites.UtilLimit);

            AddRow(m => m.OverWrites.MuX);
            AddRow(m => m.OverWrites.PhiX);
            AddRow(m => m.OverWrites.PhiY);

            BuildActions();  // 填动作列表

            // 记录原始值
            _originalValues = new();
            foreach (var action in _actions)
            {
                _originalValues[action.Row] = action.GetText();
            }

            RefreshGray();
        }

        private void AddRow(Func<MemberData, ParaItem> getField, ComboBox? cmb = null)
        {
            double? commonVal = GetCommonValue(getField);
            string displayVal;
            if (_members.Count == 1)
            {
                // 只选一根构件：不管有没有覆盖值，都不显示 varies
                displayVal = commonVal.HasValue ? commonVal.Value.ToString() : "";
            }
            else
            {
                // 多根构件：值不一样才显示 varies
                displayVal = commonVal.HasValue ? commonVal.Value.ToString() : "varies";
            }

            var row = new ListViewItem(getField(_members[0]).Name);
            row.SubItems.Add(displayVal);
            row.SubItems.Add(getField(_members[0]).Unit);

            if (getField(_members[0]).Source == ValueSource.OverWrite)
            {
                row.ForeColor = Color.Red;
                row.SubItems[1].ForeColor = Color.Red;
            }

            _lst.Items.Add(row);
            if (cmb != null) row.Tag = cmb;
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

        private ListViewItem GetRow(Row r) => _lst.Items[(int)r];

        private void RefreshGray()
        {
            bool mixed = _members.Any(m => m.Type == MemberType.Column) && _members.Any(m => m.Type == MemberType.Beam);

            if (mixed)
            {
                PreferencesForm.SetRowGray(_lst, (int)Row.MuX, true);
                PreferencesForm.SetRowGray(_lst, (int)Row.PhiX, true);
                PreferencesForm.SetRowGray(_lst, (int)Row.PhiY, true);
            }

            PreferencesForm.RefreshSeismicRows(_lst, _cmbSeismic,
                (int)Row.LambdaLimit, (int)Row.FlangeLimit, (int)Row.WebLimit);

            bool hasStiff = _cmbStiffener.SelectedIndex == 1;
            PreferencesForm.SetRowGray(_lst, (int)Row.StiffenerSpacing, !hasStiff);

            bool hasBrace = _cmbBrace.SelectedIndex == 1;
            PreferencesForm.SetRowGray(_lst, (int)Row.PurlinArea, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.PurlinSpan, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.PurlinI33, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.PurlinDistToFlange, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.BraceArea, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.BraceSpacing, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.BraceAngle, !hasBrace);
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            foreach (var action in _actions)
            {
                string currentText = action.GetText();

                if (currentText == _originalValues[action.Row])
                    continue;

                foreach (var member in _members)
                {
                    action.SetValue(member, currentText);
                }
            }

            // 遍历选中构件，存覆盖项
            foreach (var member in _members)
            {
                Persistence.SaveMemberOverWrites(member);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            foreach (var member in _members)
            {
                member.OverWrites.ElementType.ResetOw();
                member.OverWrites.Gamma0.ResetOw();
                member.OverWrites.MMR.ResetOw();
                member.OverWrites.GammaX.ResetOw();
                member.OverWrites.BetaMx.ResetOw();
                member.OverWrites.PhiB.ResetOw();
                member.OverWrites.PhiPs.ResetOw();
                member.OverWrites.Rho.ResetOw();
                member.OverWrites.Ly.ResetOw();
                member.OverWrites.HasStiffener.ResetOw();
                member.OverWrites.StiffenerSpacing.ResetOw();
                member.OverWrites.HasBrace.ResetOw();
                member.OverWrites.PurlinArea.ResetOw();
                member.OverWrites.PurlinSpan.ResetOw();
                member.OverWrites.PurlinI33.ResetOw();
                member.OverWrites.PurlinDistToFlange.ResetOw();
                member.OverWrites.BraceArea.ResetOw();
                member.OverWrites.BraceSpacing.ResetOw();
                member.OverWrites.BraceAngle.ResetOw();
                member.OverWrites.SeismicType.ResetOw();
                member.OverWrites.LambdaLimit.ResetOw();
                member.OverWrites.FlangeLimit.ResetOw();
                member.OverWrites.WebLimit.ResetOw();
                member.OverWrites.UtilLimit.ResetOw();
                member.OverWrites.MuX.ResetOw();
                member.OverWrites.PhiX.ResetOw();
                member.OverWrites.PhiY.ResetOw();
            }

            _lst.Items.Clear();
            LoadData();
        }

        private void BuildActions()
        {
            _actions = new List<RowAction>
            {
                            // ===== 下拉框 =====
                new RowAction(Row.ElementType,
                    () => _cmbType.SelectedIndex.ToString(),
                    (m, v) => m.OverWrites.ElementType.SetOw(int.Parse(v))),

                new RowAction(Row.HasStiffener,
                    () => _cmbStiffener.SelectedIndex.ToString(),
                    (m, v) =>
                    {
                        m.OverWrites.HasStiffener.SetOw(int.Parse(v) == 1 ? 1.0 : 0.0);
                        if (int.Parse(v) == 1)
                        {
                            string spacing = GetRow(Row.StiffenerSpacing).SubItems[1].Text;
                            m.OverWrites.StiffenerSpacing.SetOw(double.Parse(spacing));
                        }
                    }),

                new RowAction(Row.HasBrace,
                    () => _cmbBrace.SelectedIndex.ToString(),
                    (m, v) =>
                    {
                        m.OverWrites.HasBrace.SetOw(int.Parse(v) == 1 ? 1.0 : 0.0);
                        if (int.Parse(v) == 1)
                        {
                            m.OverWrites.PurlinArea.SetOw(double.Parse(GetRow(Row.PurlinArea).SubItems[1].Text));
                            m.OverWrites.PurlinSpan.SetOw(double.Parse(GetRow(Row.PurlinSpan).SubItems[1].Text));
                            m.OverWrites.PurlinI33.SetOw(double.Parse(GetRow(Row.PurlinI33).SubItems[1].Text));
                            m.OverWrites.PurlinDistToFlange.SetOw(double.Parse(GetRow(Row.PurlinDistToFlange).SubItems[1].Text));
                            m.OverWrites.BraceArea.SetOw(double.Parse(GetRow(Row.BraceArea).SubItems[1].Text));
                            m.OverWrites.BraceSpacing.SetOw(double.Parse(GetRow(Row.BraceSpacing).SubItems[1].Text));
                            m.OverWrites.BraceAngle.SetOw(double.Parse(GetRow(Row.BraceAngle).SubItems[1].Text));
                        }
                    }),

                new RowAction(Row.SeismicType,
                    () => _cmbSeismic.SelectedIndex.ToString(),
                    (m, v) =>
                    {
                        m.OverWrites.SeismicType.SetOw(int.Parse(v));
                        if (int.Parse(v) == 1)
                        {
                            m.OverWrites.LambdaLimit.SetOw(DefaultValues.Seismic1_LambdaLimit);
                            m.OverWrites.FlangeLimit.SetOw(DefaultValues.Seismic1_FlangeLimit);
                            m.OverWrites.WebLimit.SetOw(DefaultValues.Seismic1_WebLimit);
                        }
                        else if (int.Parse(v) == 2)
                        {
                            m.OverWrites.LambdaLimit.SetOw(DefaultValues.Seismic2_LambdaLimit);
                            m.OverWrites.FlangeLimit.SetOw(DefaultValues.Seismic2_FlangeLimit);
                            m.OverWrites.WebLimit.SetOw(DefaultValues.Seismic2_WebLimit);
                        }
                    }),

                // ===== 数值行 =====
                new RowAction(Row.Gamma0,
                    () => GetRow(Row.Gamma0).SubItems[1].Text,
                    (m, v) => m.OverWrites.Gamma0.SetOw(double.Parse(v))),

                new RowAction(Row.MMR,
                    () => GetRow(Row.MMR).SubItems[1].Text,
                    (m, v) => m.OverWrites.MMR.SetOw(double.Parse(v))),

                new RowAction(Row.GammaX,
                    () => GetRow(Row.GammaX).SubItems[1].Text,
                    (m, v) => m.OverWrites.GammaX.SetOw(double.Parse(v))),

                new RowAction(Row.BetaMx,
                    () => GetRow(Row.BetaMx).SubItems[1].Text,
                    (m, v) => m.OverWrites.BetaMx.SetOw(double.Parse(v))),

                new RowAction(Row.PhiB,
                    () => GetRow(Row.PhiB).SubItems[1].Text,
                    (m, v) => m.OverWrites.PhiB.SetOw(double.Parse(v))),

                new RowAction(Row.PhiPs,
                    () => GetRow(Row.PhiPs).SubItems[1].Text,
                    (m, v) => m.OverWrites.PhiPs.SetOw(double.Parse(v))),

                new RowAction(Row.Rho,
                    () => GetRow(Row.Rho).SubItems[1].Text,
                    (m, v) => m.OverWrites.Rho.SetOw(double.Parse(v))),

                new RowAction(Row.Ly,
                    () => GetRow(Row.Ly).SubItems[1].Text,
                    (m, v) => m.OverWrites.Ly.SetOw(double.Parse(v))),

                new RowAction(Row.StiffenerSpacing,
                    () => GetRow(Row.StiffenerSpacing).SubItems[1].Text,
                    (m, v) => m.OverWrites.StiffenerSpacing.SetOw(double.Parse(v))),

                new RowAction(Row.PurlinArea,
                    () => GetRow(Row.PurlinArea).SubItems[1].Text,
                    (m, v) => m.OverWrites.PurlinArea.SetOw(double.Parse(v))),

                new RowAction(Row.PurlinSpan,
                    () => GetRow(Row.PurlinSpan).SubItems[1].Text,
                    (m, v) => m.OverWrites.PurlinSpan.SetOw(double.Parse(v))),

                new RowAction(Row.PurlinI33,
                    () => GetRow(Row.PurlinI33).SubItems[1].Text,
                    (m, v) => m.OverWrites.PurlinI33.SetOw(double.Parse(v))),

                new RowAction(Row.PurlinDistToFlange,
                    () => GetRow(Row.PurlinDistToFlange).SubItems[1].Text,
                    (m, v) => m.OverWrites.PurlinDistToFlange.SetOw(double.Parse(v))),

                new RowAction(Row.BraceArea,
                    () => GetRow(Row.BraceArea).SubItems[1].Text,
                    (m, v) => m.OverWrites.BraceArea.SetOw(double.Parse(v))),

                new RowAction(Row.BraceSpacing,
                    () => GetRow(Row.BraceSpacing).SubItems[1].Text,
                    (m, v) => m.OverWrites.BraceSpacing.SetOw(double.Parse(v))),

                new RowAction(Row.BraceAngle,
                    () => GetRow(Row.BraceAngle).SubItems[1].Text,
                    (m, v) => m.OverWrites.BraceAngle.SetOw(double.Parse(v))),

                new RowAction(Row.LambdaLimit,
                    () => GetRow(Row.LambdaLimit).SubItems[1].Text,
                    (m, v) => m.OverWrites.LambdaLimit.SetOw(double.Parse(v))),

                new RowAction(Row.FlangeLimit,
                    () => GetRow(Row.FlangeLimit).SubItems[1].Text,
                    (m, v) => m.OverWrites.FlangeLimit.SetOw(double.Parse(v))),

                new RowAction(Row.WebLimit,
                    () => GetRow(Row.WebLimit).SubItems[1].Text,
                    (m, v) => m.OverWrites.WebLimit.SetOw(double.Parse(v))),

                new RowAction(Row.UtilLimit,
                    () => GetRow(Row.UtilLimit).SubItems[1].Text,
                    (m, v) => m.OverWrites.UtilLimit.SetOw(double.Parse(v))),

                new RowAction(Row.MuX,
                    () => GetRow(Row.MuX).SubItems[1].Text,
                    (m, v) => m.OverWrites.MuX.SetOw(double.Parse(v))),

                new RowAction(Row.PhiX,
                    () => GetRow(Row.PhiX).SubItems[1].Text,
                    (m, v) => m.OverWrites.PhiX.SetOw(double.Parse(v))),

                new RowAction(Row.PhiY,
                    () => GetRow(Row.PhiY).SubItems[1].Text,
                    (m, v) => m.OverWrites.PhiY.SetOw(double.Parse(v))),
            };
        }

    }
}
