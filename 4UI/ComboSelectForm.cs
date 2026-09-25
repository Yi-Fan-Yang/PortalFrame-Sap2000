using PortalFrame._5data;
using static PortalFrame._5data.CheckPara;


namespace PortalFrame._4UI
{
    public class ComboSelectForm : Form
    {
        private DesignCombos _Combos;
        private ListBox _lstAll = null!;
        private ListBox _lstSelected = null!;
        private Button _btnRight = null!;
        private Button _btnLeft = null!;
        private Button _btnOk = null!;
        private Button _btnCancel = null!;

        public ComboSelectForm(DesignCombos Combos, List<string> allCombos)
        {
            _Combos = Combos;
            Text = "选择设计组合";
            Width = 450;
            Height = 350;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            LayoutControls(allCombos);
            SetupEvents();
        }

        private void LayoutControls(List<string> allCombos)
        {
            // 左边：全部组合
            Label lblLeft = new Label();
            lblLeft.Text = "全部组合";
            lblLeft.Location = new Point(20, 10);
            lblLeft.Size = new Size(100, 20);
            Controls.Add(lblLeft);

            _lstAll = new ListBox();
            _lstAll.Location = new Point(20, 30);
            _lstAll.Size = new Size(150, 220);
            Controls.Add(_lstAll);

            // 中间按钮
            _btnRight = new Button();
            _btnRight.Text = ">";
            _btnRight.Location = new Point(190, 80);
            _btnRight.Size = new Size(40, 25);
            Controls.Add(_btnRight);

            _btnLeft = new Button();
            _btnLeft.Text = "<";
            _btnLeft.Location = new Point(190, 120);
            _btnLeft.Size = new Size(40, 25);
            Controls.Add(_btnLeft);

            // 右边：已选组合
            Label lblRight = new Label();
            lblRight.Text = "设计组合";
            lblRight.Location = new Point(250, 10);
            lblRight.Size = new Size(100, 20);
            Controls.Add(lblRight);

            _lstSelected = new ListBox();
            _lstSelected.Location = new Point(250, 30);
            _lstSelected.Size = new Size(150, 220);
            Controls.Add(_lstSelected);

            // 加载数据
            foreach (var combo in allCombos)
            {
                if (_Combos.SelectedCombos.Contains(combo))
                    _lstSelected.Items.Add(combo);
                else
                    _lstAll.Items.Add(combo);
            }

            // 底部按钮
            _btnOk = new Button();
            _btnOk.Text = "确定";
            _btnOk.Location = new Point(120, 270);
            _btnOk.Size = new Size(80, 25);
            Controls.Add(_btnOk);

            _btnCancel = new Button();
            _btnCancel.Text = "取消";
            _btnCancel.Location = new Point(220, 270);
            _btnCancel.Size = new Size(80, 25);
            Controls.Add(_btnCancel);
        }

        private void SetupEvents()
        {
            _btnRight.Click += BtnRight_Click;
            _btnLeft.Click += BtnLeft_Click;
            _btnOk.Click += BtnOk_Click;
            _btnCancel.Click += BtnCancel_Click;
        }

        private void BtnRight_Click(object? sender, EventArgs e)
        {
            // 左边选中的移到右边
            while (_lstAll.SelectedItem != null)
            {
                var item = _lstAll.SelectedItem;
                _lstSelected.Items.Add(item);
                _lstAll.Items.Remove(item);
            }
        }

        private void BtnLeft_Click(object? sender, EventArgs e)
        {
            // 右边选中的移回左边
            while (_lstSelected.SelectedItem != null)
            {
                var item = _lstSelected.SelectedItem;
                _lstAll.Items.Add(item);
                _lstSelected.Items.Remove(item);
            }
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            _Combos.SelectedCombos.Clear();
            foreach (var item in _lstSelected.Items)
            {
                _Combos.SelectedCombos.Add(item.ToString()!);
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
