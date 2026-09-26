namespace PortalFrame._4UI
{
    public class ResultSelectForm : Form
    {
        // 验算类型选项
        private RadioButton _rbSummary = null!;
        private RadioButton _rbFlexure = null!;
        private RadioButton _rbShear = null!;
        private RadioButton _rbInPlane = null!;
        private RadioButton _rbOutPlane = null!;
        private RadioButton _rbSlenderness = null!;
        private RadioButton _rbLocalStab = null!;

        private Button _btnOk = null!;
        private Button _btnCancel = null!;

        public string SelectedCheckType { get; private set; } = "汇总";

        public ResultSelectForm()
        {
            Text = "选择验算结果";
            Width = 250;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            LayoutControls();
            SetupEvents();
        }

        private void LayoutControls()
        {
            int y = 20;

            _rbSummary.Text = "汇总";
            _rbSummary.Location = new Point(20, y);
            _rbSummary.Checked = true;
            y += 30;

            _rbFlexure.Text = "压弯/拉弯";
            _rbFlexure.Location = new Point(20, y);
            y += 30;

            _rbShear.Text = "抗剪";
            _rbShear.Location = new Point(20, y);
            y += 30;

            _rbInPlane.Text = "平面内稳定";
            _rbInPlane.Location = new Point(20, y);
            y += 30;

            _rbOutPlane.Text = "平面外稳定";
            _rbOutPlane.Location = new Point(20, y);
            y += 30;

            _rbSlenderness.Text = "长细比";
            _rbSlenderness.Location = new Point(20, y);
            y += 30;

            _rbLocalStab.Text = "局部稳定";
            _rbLocalStab.Location = new Point(20, y);
            y += 40;

            _btnOk.Text = "确定";
            _btnOk.Size = new Size(75, 25);
            _btnOk.Location = new Point(30, y);

            _btnCancel.Text = "取消";
            _btnCancel.Size = new Size(75, 25);
            _btnCancel.Location = new Point(130, y);

            Controls.AddRange(new Control[]
            {
                _rbSummary, _rbFlexure, _rbShear, _rbInPlane, _rbOutPlane,
                _rbSlenderness, _rbLocalStab,
                _btnOk, _btnCancel
            });
        }

        private void SetupEvents()
        {
            _btnOk.Click += BtnOk_Click;
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (_rbSummary.Checked) SelectedCheckType = "汇总";
            else if (_rbFlexure.Checked) SelectedCheckType = "压弯/拉弯";
            else if (_rbShear.Checked) SelectedCheckType = "抗剪";
            else if (_rbInPlane.Checked) SelectedCheckType = "平面内稳定";
            else if (_rbOutPlane.Checked) SelectedCheckType = "平面外稳定";
            else if (_rbSlenderness.Checked) SelectedCheckType = "长细比";
            else if (_rbLocalStab.Checked) SelectedCheckType = "局部稳定";

            DialogResult = DialogResult.OK;
        }
    }
}

