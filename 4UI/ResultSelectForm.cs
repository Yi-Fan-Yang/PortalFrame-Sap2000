using PortalFrame._2CheckResults;

namespace PortalFrame._4UI
{
    public class ResultSelectForm : Form
    {
        // 验算类型选项
        private RadioButton _rbSummary = new ();
        private RadioButton _rbFlexure = new ();
        private RadioButton _rbShear = new ();
        private RadioButton _rbPhiB = new();
        private RadioButton _rbColInPlane = new ();
        private RadioButton _rbColOutPlane = new ();
        private RadioButton _rbBeamLateral = new ();
        private RadioButton _rbSlenderness = new ();
        private RadioButton _rbLocalStab = new();

        private Button _btnOk = new ();
        private Button _btnCancel = new ();

        public string SelectedCheckType { get; private set; } = "汇总";

        public ResultSelectForm()
        {
            this.Font = Global.Ui.DefaultFont;
            FormBorderStyle = FormBorderStyle.FixedSingle;   // 固定大小，不能拖拽
            MaximizeBox = false;                              // 禁用最大化
            MinimizeBox = false;                              // 禁用最小化

            Text = "选择验算结果";
            Width = 350;
            Height = 550;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            LayoutControls();
            SetupEvents();
        }

        private void LayoutControls()
        {
            int y = 30;

            _rbSummary.Text = "汇总";
            _rbSummary.Location = new Point(30, y);
            _rbSummary.Checked = true;
            y += 45;

            _rbFlexure.Text = CheckTypes.Flexure;
            _rbFlexure.Location = new Point(30, y);
            y += 45;

            _rbShear.Text = CheckTypes.Shear;
            _rbShear.Location = new Point(30, y);
            y += 45;

            _rbPhiB.Text = CheckTypes.PhiB;
            _rbPhiB.Location = new Point(30, y);
            y += 45;

            _rbColInPlane.Text = CheckTypes.ColumnInPlane;
            _rbColInPlane.Location = new Point(30, y);
            y += 45;

            _rbColOutPlane.Text = CheckTypes.ColumnOutPlane;
            _rbColOutPlane.Location = new Point(30, y);
            y += 45;

            _rbBeamLateral.Text = CheckTypes.BeamLateral;
            _rbBeamLateral.Location = new Point(30, y);
            y += 45;

            _rbSlenderness.Text = CheckTypes.Slenderness;
            _rbSlenderness.Location = new Point(30, y);
            y += 45;

            _rbLocalStab.Text = CheckTypes.LocalStability;
            _rbLocalStab.Location = new Point(30, y);
            y += 50;

            _btnOk.Text = "确定";
            _btnOk.Size = new Size(100, 35);
            _btnOk.Location = new Point(50, y);

            _btnCancel.Text = "取消";
            _btnCancel.Size = new Size(100, 35);
            _btnCancel.Location = new Point(180, y);

            Controls.AddRange(new Control[]
            {
        _rbSummary, _rbFlexure, _rbShear,_rbPhiB, _rbColInPlane, _rbColOutPlane, _rbBeamLateral,
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
            else if (_rbFlexure.Checked) SelectedCheckType = CheckTypes.Flexure;
            else if (_rbShear.Checked) SelectedCheckType = CheckTypes.Shear;
            else if (_rbPhiB.Checked) SelectedCheckType = CheckTypes.PhiB;
            else if (_rbColInPlane.Checked) SelectedCheckType = CheckTypes.ColumnInPlane;
            else if (_rbColOutPlane.Checked) SelectedCheckType = CheckTypes.ColumnOutPlane;
            else if (_rbBeamLateral.Checked) SelectedCheckType = CheckTypes.BeamLateral;
            else if (_rbSlenderness.Checked) SelectedCheckType = CheckTypes.Slenderness;
            else if (_rbLocalStab.Checked) SelectedCheckType = CheckTypes.LocalStability;

            DialogResult = DialogResult.OK;
        }
    }
}

