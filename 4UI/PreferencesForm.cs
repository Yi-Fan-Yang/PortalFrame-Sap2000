using PortalFrame._0Sap;
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
            a,                  // 加劲肋间距
            Ly,                 // 平面外计算长度
            HasBrace,           // 是否隅撑
            Ap,         // 檩条截面积
            Beta,        // 连接点距主梁距离比β
            Lp,         // 檩条跨度
            Ip,          // 檩条I33
            A,          // 檩条形心距离
            Ak,          // 隅撑截面积
            Lkk,       // 隅撑间距
            Alpha,         // 隅撑夹角
            SeismicType,        // 抗震类型
            LambdaLimit,        // 长细比限值
            FlangeLimit,        // 翼缘宽厚比
            WebLimit,           // 腹板高厚比
            UtilLimit,          // 应力比限值
        }

        private Button _btnOk = new() { Text = "确定" };
        private Button _btnCancel = new() { Text = "取消" };
        private Button _btnReset = new() { Text = "还原默认" };

        private ListView _lst = new();
        private TextBox _txtValue = new();
        private static ListViewItem? _prevEditingRow;//记录上一个编辑行


        private ComboBox _cmbSeismic = new();
        private ComboBox _cmbStiffener = new();
        private ComboBox _cmbBrace = new();

        public PreferencesForm()
        {
            this.Font = Global.Ui.DefaultFont;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;


            Text = "验算首选项";
            Width = 580;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;

            InitListView(_lst);
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
            _cmbStiffener.Items.AddRange(["否", "是" ]);
            _cmbStiffener.SelectedIndex = Preferences.HasStiffener.Value == 1 ? 1 : 0;
            _cmbStiffener.Visible = false;
            Controls.Add(_cmbStiffener);

            _cmbBrace.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbBrace.Items.AddRange(["否", "是"]);
            _cmbBrace.SelectedIndex = Preferences.HasBrace.Value == 1 ? 1 : 0;
            _cmbBrace.Visible = false;
            Controls.Add(_cmbBrace);

            _cmbSeismic.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbSeismic.Items.AddRange(["自定义限值", "非抗震", "抗震"]);
            _cmbSeismic.SelectedIndex = (int)(Preferences.SeismicType.Value ?? 0);
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
            // General 里的参数
            AddRow(Preferences.Gamma0);
            AddRow(Preferences.MMR);

            // 加劲肋
            AddRow(Preferences.HasStiffener, _cmbStiffener);
            AddRow(Preferences.a);
            AddRow(Preferences.Ly);

            // 隅撑/檩条
            AddRow(Preferences.HasBrace, _cmbBrace);
            AddRow(Preferences.Ap);
            AddRow(Preferences.Beta);
            AddRow(Preferences.Lp);
            AddRow(Preferences.Ip);
            AddRow(Preferences.A);
            AddRow(Preferences.Ak);
            AddRow(Preferences.Lkk);
            AddRow(Preferences.Alpha);

            // 抗震类型和限值
            AddRow(Preferences.SeismicType, _cmbSeismic);
            AddRow(Preferences.LambdaLimit);
            AddRow(Preferences.FlangeLimit);
            AddRow(Preferences.WebLimit);

            AddRow(Preferences.UtilLimit);

            RefreshGray();
        }

        private void AddRow(ConfigItem item, ComboBox? cmb = null)
        {
            // 显示值：有值就显示，没值就显示默认值（从 DefaultValues 拿）
            double? displayVal = item.Value;
            if (!displayVal.HasValue)
            {
                // 从 DefaultValues 拿默认值
                displayVal = DefaultValues.GetDefault(item.Name);
            }

            string displayStr = displayVal.HasValue ? displayVal.Value.ToString() : "";
            var row = new ListViewItem(item.Name);
            row.SubItems.Add(displayStr);
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
            PreferencesForm.SetRowGray(_lst, (int)Row.a, !hasStiff);

            // 隅撑：选"否"时，下面7行都灰掉
            bool hasBrace = _cmbBrace.SelectedIndex == 1;
            PreferencesForm.SetRowGray(_lst, (int)Row.Ap, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Beta, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Lp, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Ip, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.A, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Ak, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Lkk, !hasBrace);
            PreferencesForm.SetRowGray(_lst, (int)Row.Alpha, !hasBrace);
        }

        // 辅助方法：按枚举拿某一行
        private ListViewItem GetRow(Row r) => _lst.Items[(int)r];
        private void BtnOk_Click(object? sender, EventArgs e)
        {
            // 把TextBox里正在编辑的值写回行
            if (_txtValue.Visible && _lst.SelectedItems.Count > 0)
            {
                _lst.SelectedItems[0].SubItems[1].Text = _txtValue.Text;
            }
            // 写回 Preferences 静态类
            Preferences.Gamma0.Value = double.Parse(GetRow(Row.Gamma0).SubItems[1].Text);
            Preferences.MMR.Value = double.Parse(GetRow(Row.MMR).SubItems[1].Text);

            Preferences.HasStiffener.Value = _cmbStiffener.SelectedIndex == 1 ? 1.0 : 0.0;
            // 不管选"是"还是"否"，都保留加劲肋间距的值
            Preferences.a.Value = double.Parse(GetRow(Row.a).SubItems[1].Text);
            Preferences.Ly.Value = double.Parse(GetRow(Row.Ly).SubItems[1].Text);

            Preferences.HasBrace.Value = _cmbBrace.SelectedIndex == 1 ? 1.0 : 0.0;
            // 不管选"是"还是"否"，都保留隅撑和檩条的值
            Preferences.Ap.Value = double.Parse(GetRow(Row.Ap).SubItems[1].Text);
            Preferences.Beta.Value = double.Parse(GetRow(Row.Beta).SubItems[1].Text);
            Preferences.Lp.Value = double.Parse(GetRow(Row.Lp).SubItems[1].Text);
            Preferences.Ip.Value = double.Parse(GetRow(Row.Ip).SubItems[1].Text);
            Preferences.A.Value = double.Parse(GetRow(Row.A).SubItems[1].Text);
            Preferences.Ak.Value = double.Parse(GetRow(Row.Ak).SubItems[1].Text);
            Preferences.Lkk.Value = double.Parse(GetRow(Row.Lkk).SubItems[1].Text);
            Preferences.Alpha.Value = double.Parse(GetRow(Row.Alpha).SubItems[1].Text);

            Preferences.SeismicType.Value = _cmbSeismic.SelectedIndex;
            Preferences.LambdaLimit.Value = double.Parse(GetRow(Row.LambdaLimit).SubItems[1].Text);
            Preferences.FlangeLimit.Value = double.Parse(GetRow(Row.FlangeLimit).SubItems[1].Text);
            Preferences.WebLimit.Value = double.Parse(GetRow(Row.WebLimit).SubItems[1].Text);
            Preferences.UtilLimit.Value = double.Parse(GetRow(Row.UtilLimit).SubItems[1].Text);

            // 2. 再分发到所有构件的结果类
            foreach (var member in Global.Data.Members.Values)
            {
                if (!member.IsCheckable) continue;
                var result = member.CheckResult;
                // General
                if (Preferences.Gamma0.Value.HasValue)result.General.Gamma0.SetPre(Preferences.Gamma0.Value.Value);
                if (Preferences.MMR.Value.HasValue)result.General.MMR.SetPre(Preferences.MMR.Value.Value);
                if (Preferences.SeismicType.Value.HasValue)result.General.SeismicType.SetPre(Preferences.SeismicType.Value.Value);
                if (Preferences.LambdaLimit.Value.HasValue)result.General.LambdaLimit.SetPre(Preferences.LambdaLimit.Value.Value);
                if (Preferences.FlangeLimit.Value.HasValue)result.General.FlangeLimit.SetPre(Preferences.FlangeLimit.Value.Value);
                if (Preferences.WebLimit.Value.HasValue)result.General.WebLimit.SetPre(Preferences.WebLimit.Value.Value);
                if (Preferences.UtilLimit.Value.HasValue)result.General.UtilLimit.SetPre(Preferences.UtilLimit.Value.Value);

                // Slenderness
                if (Preferences.Ly.Value.HasValue)result.Slenderness.Ly.SetPre(Preferences.Ly.Value.Value);

                // ShearStrength
                if (Preferences.HasStiffener.Value.HasValue)result.ShearStrength.HasStiffener.SetPre(Preferences.HasStiffener.Value.Value);
                if (Preferences.a.Value.HasValue)result.ShearStrength.a.SetPre(Preferences.a.Value.Value);

                // PhiBDetails
                foreach (var details in result.PhiBResult.Collection.Values)
                {
                    if (Preferences.HasBrace.Value.HasValue)details.HasBrace.SetPre(Preferences.HasBrace.Value.Value);
                    if (Preferences.Ap.Value.HasValue)details.Ap.SetPre(Preferences.Ap.Value.Value);
                    if (Preferences.Beta.Value.HasValue) details.Beta.SetPre(Preferences.Beta.Value.Value);
                    if (Preferences.Lp.Value.HasValue)details.Lp.SetPre(Preferences.Lp.Value.Value);
                    if (Preferences.Ip.Value.HasValue)details.Ip.SetPre(Preferences.Ip.Value.Value);
                    if (Preferences.A.Value.HasValue)details.A.SetPre(Preferences.A.Value.Value);
                    if (Preferences.Ak.Value.HasValue)details.Ak.SetPre(Preferences.Ak.Value.Value);
                    if (Preferences.Lkk.Value.HasValue)details.Lkk.SetPre(Preferences.Lkk.Value.Value);
                    if (Preferences.Alpha.Value.HasValue)details.Alpha.SetPre(Preferences.Alpha.Value.Value);
                }

            }

            // 3. 持久化
            Persistence.SavePreferences();

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
            GetRow(Row.a).SubItems[1].Text = DefaultValues.a.ToString();
            GetRow(Row.Ly).SubItems[1].Text = DefaultValues.Ly.ToString();
            GetRow(Row.Ap).SubItems[1].Text = DefaultValues.Ap.ToString();
            GetRow(Row.Beta).SubItems[1].Text = DefaultValues.Beta.ToString();
            GetRow(Row.Lp).SubItems[1].Text = DefaultValues.Lp.ToString();
            GetRow(Row.Ip).SubItems[1].Text = DefaultValues.Ip.ToString();
            GetRow(Row.A).SubItems[1].Text = DefaultValues.A.ToString();
            GetRow(Row.Ak).SubItems[1].Text = DefaultValues.Ak.ToString();
            GetRow(Row.Lkk).SubItems[1].Text = DefaultValues.Lkk.ToString();
            GetRow(Row.Alpha).SubItems[1].Text = DefaultValues.Alpha.ToString();
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

            // 用新的方法拿抗震限值
            var limits = DefaultValues.GetSeismicLimits(cmbSeismic.SelectedIndex);
            lst.Items[lambdaRow].SubItems[1].Text = limits[0].ToString();
            lst.Items[flangeRow].SubItems[1].Text = limits[1].ToString();
            lst.Items[webRow].SubItems[1].Text = limits[2].ToString();
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
            // 先把上一个TextBox编辑的值写回
            if (_prevEditingRow != null && txtValue.Visible)
            {
                _prevEditingRow.SubItems[1].Text = txtValue.Text;
            }

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
                _prevEditingRow = row;
            }
        }


    }
}
