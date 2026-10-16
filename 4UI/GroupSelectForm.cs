using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class GroupSelectForm : Form
    {
        private SelectedGroups _Groups = null!;
        private Label _lblInfo = null!;
        private CheckedListBox _lstGroups = null!;
        private Button _btnOk = null!;
        private Button _btnCancel = null!;

        public GroupSelectForm(SelectedGroups groups, int nJoints, int nMembers, List<string> allGroups)
        {
            this.Font = UiConfig.DefaultFont;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _Groups = groups;
            Text = "选择计算转动约束的柱组";
            Width = 450;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;

            LayoutControls(nJoints, nMembers, allGroups);
            SetupEvents();
        }

        private void LayoutControls(int nJoints, int nMembers, List<string> allGroups)
        {
            _lblInfo = new Label();
            _lblInfo.Text = $"节点数：{nJoints}    构件数：{nMembers}";
            _lblInfo.Location = new Point(20, 15);
            _lblInfo.Size = new Size(400, 25);
            Controls.Add(_lblInfo);

            Label lblTip = new Label();
            lblTip.Text = "勾选需要计算梁对柱转动约束的柱组：";
            lblTip.Location = new Point(20, 50);
            lblTip.Size = new Size(400, 20);
            Controls.Add(lblTip);

            _lstGroups = new CheckedListBox();
            _lstGroups.Location = new Point(20, 80);
            _lstGroups.Size = new Size(400, 300);
            foreach (var g in allGroups)
                _lstGroups.Items.Add(g, _Groups.Names.Contains(g));
            Controls.Add(_lstGroups);

            _btnOk = new Button();
            _btnOk.Text = "确定";
            _btnOk.Location = new Point(120, 400);
            _btnOk.Size = new Size(100, 35);
            Controls.Add(_btnOk);

            _btnCancel = new Button();
            _btnCancel.Text = "取消";
            _btnCancel.Location = new Point(240, 400);
            _btnCancel.Size = new Size(100, 35);
            Controls.Add(_btnCancel);
        }

        private void SetupEvents()
        {
            _btnOk.Click += BtnOk_Click;
            _btnCancel.Click += BtnCancel_Click;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            _Groups.Names.Clear();
            foreach (var item in _lstGroups.CheckedItems)
                _Groups.Names.Add(item.ToString()!);
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
