using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class PreferencesForm : Form
    {
        private Preferences _prefer;   
        private Button _btnOk = new() { Text = "Ok" };
        private Button _btnCancel = new() { Text = "Cancel" };
        private GroupBox _grpPerfer = new() { Text = "设计首选项" };
        private ListView _lstPerfer = new();



        public PreferencesForm(Preferences prefer)
        {
            _prefer = prefer;

            Text = "验算首选项";
            Width = 350;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            LayoutControls();
            SetupEvents();
        }

        private void LayoutControls()
        {
            // GroupBox 在上
            _grpPerfer.Text = "设计首选项";
            _grpPerfer.Location = new Point(10, 10);
            _grpPerfer.Size = new Size(310, 350);
            Controls.Add(_grpPerfer);

            // ListView 在 GroupBox 里
            _lstPerfer.Dock = DockStyle.Fill;
            _lstPerfer.View = View.Details;
            _lstPerfer.FullRowSelect = true;
            _lstPerfer.GridLines = true;
            _lstPerfer.Columns.Add("参数名", 150);
            _lstPerfer.Columns.Add("参数值", 130);
            _grpPerfer.Controls.Add(_lstPerfer);

            // 加载数据
            _lstPerfer.Items.Add("结构重要性系数 γ0");
            _lstPerfer.Items[0].SubItems.Add(_prefer.Gamma0.ToString());
            // 按钮在下面
            _btnOk.Text = "确定";
            _btnOk.Location = new Point(80, 380);
            _btnOk.Size = new Size(80, 25);
            Controls.Add(_btnOk);

            _btnCancel.Text = "取消";
            _btnCancel.Location = new Point(180, 380);
            _btnCancel.Size = new Size(80, 25);
            Controls.Add(_btnCancel);
        }
        private void SetupEvents()
        {
            _btnOk.Click += btnOk_Click;
            _btnCancel.Click += btnCancel_Click;
        }

        private void btnOk_Click(object? sender, EventArgs e)
        {
            if (double.TryParse(_lstPerfer.Items[0].SubItems[1].Text, out double g))
            {
                _prefer.Gamma0 = g;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("请输入数字");
            }
        }
        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }



    }
}
