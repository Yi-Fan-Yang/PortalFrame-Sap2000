using PortalFrame._2CheckResults;
using PortalFrame._3Dispatch;
using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class PreferencesForm : Form
    {
        // 行索引枚举（顺序=窗口里行的顺序，以后插行只改这里）
        private enum Row
        {
            Gamma0,             // 结构重要性系数
            MMR,                // 弯矩放大系数
            HasStiffener,       // 是否加劲肋
            StiffenerSpacing,   // 加劲肋间距
            HasBrace,           // 是否隅撑
            PurlinArea,         // 檩条截面积
            PurlinSpan,         // 檩条跨度
            PurlinI33,          // 檩条I33
            PurlinDistToFlange, // 檩条形心距离
            BraceArea,          // 隅撑截面积
            BraceSpacing,       // 隅撑间距
            BraceAngle,         // 隅撑夹角
            SeismicType,        // 抗震类型
            LambdaLimit,        // 长细比限值
            FlangeLimit,        // 翼缘宽厚比
            WebLimit,           // 腹板高厚比
            UtilLimit,          // 应力比限值
        }

        private Preferences _prefer;
        private Button _btnOk = new() { Text = "确定" };
        private Button _btnCancel = new() { Text = "取消" };
        private Button _btnReset = new() { Text = "还原默认" };

        private ListView _lst = new();
        private TextBox _txtValue = new();

        private ComboBox _cmbSeismic = new();
        private ComboBox _cmbStiffener = new();
        private ComboBox _cmbBrace = new();

        public PreferencesForm(Preferences prefer)
        {
            this.Font = UiConfig.DefaultFont;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            _prefer = prefer;

            Text = "验算首选项";
            Width = 580;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;

            PreferencesForm.InitListView(_lst);
            LayoutControls();
            SetupEvents();
            LoadData();
        }


        private void LayoutControls()
        {
            // ListView：占满上半部分
            _lst.Dock = DockStyle.Top;
            _lst.Height = 500;
            Controls.Add(_lst);

            // 下拉框
            _cmbStiffener.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbStiffener.Items.AddRange(new object[] { "否", "是" });
            _cmbStiffener.SelectedIndex = _prefer.HasStiffener.GetPre() == 1 ? 1 : 0;
            _cmbStiffener.Visible = false;
            Controls.Add(_cmbStiffener);

            _cmbBrace.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbBrace.Items.AddRange(new object[] { "否", "是" });
            _cmbBrace.SelectedIndex = _prefer.HasBrace.GetPre() == 1 ? 1 : 0;
            _cmbBrace.Visible = false;
            Controls.Add(_cmbBrace);

            _cmbSeismic.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbSeismic.Items.AddRange(new object[] { "自定义限值", "非抗震", "抗震" });
            _cmbSeismic.SelectedIndex = (int)(_prefer.SeismicType.GetPre() ?? 0);
            _cmbSeismic.Visible = false;
            Controls.Add(_cmbSeismic);

            // 三个按钮：水平居中，放在底部
            int btnY = 540;
            int btnW = 100;
            int btnH = 35;
            int gap = 30;
            int totalW = btnW * 3 + gap * 2;
            int startX = (ClientSize.Width - totalW) / 2;

            _btnReset.Location = new Point(startX, btnY);
            _btnReset.Size = new Size(btnW, btnH);
            Controls.Add(_btnReset);

            _btnOk.Location = new Point(startX + btnW + gap, btnY);
            _btnOk.Size = new Size(btnW, btnH);
            Controls.Add(_btnOk);

            _btnCancel.Location = new Point(startX + (btnW + gap) * 2, btnY);
            _btnCancel.Size = new Size(btnW, btnH);
            Controls.Add(_btnCancel);

            _txtValue.BorderStyle = BorderStyle.FixedSingle;
            _txtValue.Visible = false;
            Controls.Add(_txtValue);
        }

        private void SetupEvents()
        {
            _btnOk.Click += BtnOk_Click;
            _btnCancel.Click += BtnCancel_Click;
            _btnReset.Click += BtnReset_Click;

            _cmbSeismic.SelectedIndexChanged += (s, e) => RefreshGray();
            _cmbStiffener.SelectedIndexChanged += (s, e) => RefreshGray();
            _cmbBrace.SelectedIndexChanged += (s, e) => RefreshGray();

            _lst.MouseClick += (s, e) => PreferencesForm.Lst_MouseClick(_lst, e, _txtValue, _cmbSeismic, _cmbStiffener, _cmbBrace);
        }

        private void LoadData()
        {
            AddRow(_prefer.Gamma0);
            AddRow(_prefer.MMR);

            AddRow(_prefer.HasStiffener, _cmbStiffener);
            AddRow(_prefer.StiffenerSpacing);

            AddRow(_prefer.HasBrace, _cmbBrace);
            AddRow(_prefer.PurlinArea);
            AddRow(_prefer.PurlinSpan);
            AddRow(_prefer.PurlinI33);
            AddRow(_prefer.PurlinDistToFlange);
            AddRow(_prefer.BraceArea);
            AddRow(_prefer.BraceSpacing);
            AddRow(_prefer.BraceAngle);

            AddRow(_prefer.SeismicType, _cmbSeismic);
            AddRow(_prefer.LambdaLimit);
            AddRow(_prefer.FlangeLimit);
            AddRow(_prefer.WebLimit);

            AddRow(_prefer.UtilLimit);

            RefreshGray();
        }

        private void AddRow(ParaItem item, ComboBox? cmb = null)
        {
            // 先找用户保存的值
            double? preVal = item.GetPre();

            // 没有的话，从 DefaultValues 类里找默认值
            if (!preVal.HasValue)
            {
                // 找到 item 对应的字段名
                foreach (var field in typeof(Preferences).GetFields())
                {
                    if (ReferenceEquals(field.GetValue(_prefer), item))
                    {
                        // 去 DefaultValues 里找同名的字段
                        var defaultField = typeof(DefaultValues).GetField(field.Name);
                        if (defaultField != null)
                        {
                            preVal = (double)defaultField.GetValue(null)!;
                            // 顺便给 item 设上默认值，后面就不用再找了
                            item.SetDefault(preVal);
                        }
                        break;
                    }
                }
            }

            string displayVal = preVal.HasValue ? preVal.Value.ToString() : "";
            var row = new ListViewItem(item.Name);
            row.SubItems.Add(displayVal);
            row.SubItems.Add(item.Unit);
            _lst.Items.Add(row);
            if (cmb != null) row.Tag = cmb;
        }

        private void RefreshGray()
        {
            // 抗震类型：1或2时，三个限值行灰掉
            PreferencesForm.RefreshSeismicRows(_lst, _cmbSeismic,(int)Row.LambdaLimit, (int)Row.FlangeLimit, (int)Row.WebLimit);


            // 加劲肋：选"否"时，间距行灰掉
            bool hasStiff = _cmbStiffener.SelectedIndex == 1;
            PreferencesForm.SetRowGray(_lst, (int)Row.StiffenerSpacing, !hasStiff);

            // 隅撑：选"否"时，下面7行都灰掉
            bool hasBrace = _cmbBrace.SelectedIndex == 1;
            PreferencesForm.SetRowGray(_lst, (int)Row.PurlinArea, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.PurlinSpan, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.PurlinI33, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.PurlinDistToFlange, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.BraceArea, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.BraceSpacing, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.BraceAngle, !hasBrace);
        }

        // 辅助方法：按枚举拿某一行
        private ListViewItem GetRow(Row r) => _lst.Items[(int)r];

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            _prefer.Gamma0.SetPre(double.Parse(GetRow(Row.Gamma0).SubItems[1].Text));
            _prefer.MMR.SetPre(double.Parse(GetRow(Row.MMR).SubItems[1].Text));

            _prefer.HasStiffener.SetPre(_cmbStiffener.SelectedIndex == 1 ? 1.0 : 0.0);
            if (_cmbStiffener.SelectedIndex == 1)
                _prefer.StiffenerSpacing.SetPre(double.Parse(GetRow(Row.StiffenerSpacing).SubItems[1].Text));

            _prefer.HasBrace.SetPre(_cmbBrace.SelectedIndex == 1 ? 1.0 : 0.0);
            if (_cmbBrace.SelectedIndex == 1)
            {
                _prefer.PurlinArea.SetPre(double.Parse(GetRow(Row.PurlinArea).SubItems[1].Text));
                _prefer.PurlinSpan.SetPre(double.Parse(GetRow(Row.PurlinSpan).SubItems[1].Text));
                _prefer.PurlinI33.SetPre(double.Parse(GetRow(Row.PurlinI33).SubItems[1].Text));
                _prefer.PurlinDistToFlange.SetPre(double.Parse(GetRow(Row.PurlinDistToFlange).SubItems[1].Text));
                _prefer.BraceArea.SetPre(double.Parse(GetRow(Row.BraceArea).SubItems[1].Text));
                _prefer.BraceSpacing.SetPre(double.Parse(GetRow(Row.BraceSpacing).SubItems[1].Text));
                _prefer.BraceAngle.SetPre(double.Parse(GetRow(Row.BraceAngle).SubItems[1].Text));
            }

            _prefer.SeismicType.SetPre(_cmbSeismic.SelectedIndex);
            _prefer.LambdaLimit.SetPre(double.Parse(GetRow(Row.LambdaLimit).SubItems[1].Text));
            _prefer.FlangeLimit.SetPre(double.Parse(GetRow(Row.FlangeLimit).SubItems[1].Text));
            _prefer.WebLimit.SetPre(double.Parse(GetRow(Row.WebLimit).SubItems[1].Text));
            _prefer.UtilLimit.SetPre(double.Parse(GetRow(Row.UtilLimit).SubItems[1].Text));

            Persistence.SavePreferences(_prefer);
            DialogResult = DialogResult.OK;
            Close();
        }
        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        private void BtnReset_Click(object? sender, EventArgs e)
        {
            // 把所有行的值重置为 DefaultValues
            GetRow(Row.Gamma0).SubItems[1].Text = DefaultValues.Gamma0.ToString();
            GetRow(Row.MMR).SubItems[1].Text = DefaultValues.MMR.ToString();
            GetRow(Row.StiffenerSpacing).SubItems[1].Text = DefaultValues.StiffenerSpacing.ToString();
            GetRow(Row.PurlinArea).SubItems[1].Text = DefaultValues.PurlinArea.ToString();
            GetRow(Row.PurlinSpan).SubItems[1].Text = DefaultValues.PurlinSpan.ToString();
            GetRow(Row.PurlinI33).SubItems[1].Text = DefaultValues.PurlinI33.ToString();
            GetRow(Row.PurlinDistToFlange).SubItems[1].Text = DefaultValues.PurlinDistToFlange.ToString();
            GetRow(Row.BraceArea).SubItems[1].Text = DefaultValues.BraceArea.ToString();
            GetRow(Row.BraceSpacing).SubItems[1].Text = DefaultValues.BraceSpacing.ToString();
            GetRow(Row.BraceAngle).SubItems[1].Text = DefaultValues.BraceAngle.ToString();
            GetRow(Row.LambdaLimit).SubItems[1].Text = DefaultValues.LambdaLimit.ToString();
            GetRow(Row.FlangeLimit).SubItems[1].Text = DefaultValues.FlangeLimit.ToString();
            GetRow(Row.WebLimit).SubItems[1].Text = DefaultValues.WebLimit.ToString();
            GetRow(Row.UtilLimit).SubItems[1].Text = DefaultValues.UtilLimit.ToString();

            // 下拉框也重置
            _cmbStiffener.SelectedIndex = (int)DefaultValues.HasStiffener;
            _cmbBrace.SelectedIndex = (int)DefaultValues.HasBrace;
            _cmbSeismic.SelectedIndex = (int)DefaultValues.SeismicType;
        }


        //====================公共方法=========================
        public static void InitListView(ListView lst)
        {
            lst.View = View.Details;
            lst.FullRowSelect = true;
            lst.GridLines = true;
            lst.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lst.Columns.Add("参数名", 240);
            lst.Columns.Add("参数值", 180);
            lst.Columns.Add("量纲", 100);
        }
        public static void ShowComboInCell(ListView lst, params ComboBox[] combos)
        {
            foreach (var c in combos)
                c.Visible = false;

            if (lst.SelectedItems.Count == 0) return;
            var item = lst.SelectedItems[0];
            if (item.Tag is ComboBox cmb)
            {
                var r = lst.GetItemRect(lst.SelectedIndices[0]);
                cmb.Location = new Point(r.Right - 180, r.Top);
                cmb.Size = new Size(180, r.Height);
                cmb.Visible = true;
            }
        }
        public static void SetRowGray(ListView lst, int rowIndex, bool gray)
        {
            var item = lst.Items[rowIndex];
            item.BackColor = gray ? Color.LightGray : Color.White;
            item.ForeColor = gray ? Color.Gray : Color.Black;
        }
        public static void RefreshSeismicRows(ListView lst, ComboBox cmbSeismic,int lambdaRow, int flangeRow, int webRow)
        {
            bool fixedLimit = cmbSeismic.SelectedIndex != 0;
            SetRowGray(lst, lambdaRow, fixedLimit);
            SetRowGray(lst, flangeRow, fixedLimit);
            SetRowGray(lst, webRow, fixedLimit);

            if (cmbSeismic.SelectedIndex == 1)
            {
                lst.Items[lambdaRow].SubItems[1].Text = DefaultValues.Seismic1_LambdaLimit.ToString();
                lst.Items[flangeRow].SubItems[1].Text = DefaultValues.Seismic1_FlangeLimit.ToString();
                lst.Items[webRow].SubItems[1].Text = DefaultValues.Seismic1_WebLimit.ToString();
            }
            else if (cmbSeismic.SelectedIndex == 2)
            {
                lst.Items[lambdaRow].SubItems[1].Text = DefaultValues.Seismic2_LambdaLimit.ToString();
                lst.Items[flangeRow].SubItems[1].Text = DefaultValues.Seismic2_FlangeLimit.ToString();
                lst.Items[webRow].SubItems[1].Text = DefaultValues.Seismic2_WebLimit.ToString();
            }
        }
        // 隐藏所有编辑控件
        public static void HideAllEditors(params Control[] editors)
        {
            foreach (var ed in editors)
                ed.Visible = false;
        }

        // 点击ListView第2列，弹出对应的编辑控件
        public static void Lst_MouseClick(ListView lst, MouseEventArgs e, TextBox txtValue, params ComboBox[] combos)
        {
            var hitTest = lst.HitTest(e.Location);
            if (hitTest.Item == null) return;
            // 只点第2列（参数值）才弹出编辑框
            if (hitTest.SubItem != hitTest.Item.SubItems[1]) return;

            var row = hitTest.Item;

            HideAllEditors(new Control[] { txtValue }.Concat(combos).ToArray());

            if (row.Tag is ComboBox cmb)
            {
                var r = row.Bounds;
                cmb.Location = new Point(r.X + lst.Columns[0].Width, r.Y);
                cmb.Size = new Size(lst.Columns[1].Width, r.Height);
                cmb.Visible = true;
                cmb.BringToFront();
            }
            else
            {
                var r = row.Bounds;
                txtValue.Location = new Point(r.X + lst.Columns[0].Width, r.Y);
                txtValue.Size = new Size(lst.Columns[1].Width, r.Height);
                txtValue.Text = row.SubItems[1].Text;
                txtValue.Visible = true;
                txtValue.BringToFront();
            }
        }


    }
}
