using PortalFrame._0Sap;
using PortalFrame._3Dispatch;
using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class CheckSettingsForm : Form
    {
        // ===== A：验算group（双栏）=====
        private ListBox _lstAllGroups = new();      // 左边：全部group
        private ListBox _lstCheckGroups = new();    // 右边：参与验算的group
        private Button _btnRight1 = new();
        private Button _btnLeft1 = new();

        // ===== B：Kz group（双栏）=====
        private ListBox _lstKzSource = new();      // 左边：从A里选
        private ListBox _lstKzGroups = new();       // 右边：算Kz的group
        private Button _btnRight2 = new();
        private Button _btnLeft2 = new();

        // ===== 底部按钮 =====
        private Button _btnReadData = new();
        private Button _btnCancel = new();

        public CheckSettingsForm()
        {
            LayoutControls();
            SetupEvents();
            LoadData();
        }

        private void LoadData()
        {
            Global.Data.AllGroups = PreReader.GetGroupList();

            // A组
            foreach (var g in Global.Data.AllGroups)
            {
                if (Global.Data.Groups.CheckGroups.Contains(g))
                    _lstCheckGroups.Items.Add(g);
                else
                    _lstAllGroups.Items.Add(g);
            }

            // B组
            foreach (var g in Global.Data.AllGroups)
            {
                if (Global.Data.Groups.KzGroups.Contains(g))
                    _lstKzGroups.Items.Add(g);
                else
                    _lstKzSource.Items.Add(g);
            }
        }

        private void LayoutControls()
        {
            this.Font = Global.Ui.DefaultFont;
            Text = "验算设置";
            Width = 800;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // ===== 上半部分：A 验算group =====
            Label lblA = new Label { Text = "参与验算的Group", Location = new Point(20, 10), Size = new Size(200, 25) };
            Controls.Add(lblA);

            _lstAllGroups = new ListBox { Location = new Point(20, 40), Size = new Size(320, 250), SelectionMode = SelectionMode.MultiSimple };
            Controls.Add(_lstAllGroups);

            _btnRight1 = new Button { Text = ">", Location = new Point(370, 140), Size = new Size(60, 40) };
            Controls.Add(_btnRight1);
            _btnLeft1 = new Button { Text = "<", Location = new Point(370, 190), Size = new Size(60, 40) };
            Controls.Add(_btnLeft1);

            _lstCheckGroups = new ListBox { Location = new Point(470, 40), Size = new Size(300, 250), SelectionMode = SelectionMode.MultiSimple };
            Controls.Add(_lstCheckGroups);

            // ===== 下半部分：B Kz group =====
            Label lblB = new Label { Text = "立柱Group（计算Kz）", Location = new Point(20, 310), Size = new Size(300, 25) };
            Controls.Add(lblB);

            _lstKzSource = new ListBox { Location = new Point(20, 340), Size = new Size(320, 200), SelectionMode = SelectionMode.MultiSimple };
            Controls.Add(_lstKzSource);

            _btnRight2 = new Button { Text = ">", Location = new Point(370, 420), Size = new Size(60, 40) };
            Controls.Add(_btnRight2);
            _btnLeft2 = new Button { Text = "<", Location = new Point(370, 470), Size = new Size(60, 40) };
            Controls.Add(_btnLeft2);

            _lstKzGroups = new ListBox { Location = new Point(470, 340), Size = new Size(300, 200), SelectionMode = SelectionMode.MultiSimple };
            Controls.Add(_lstKzGroups);

            // ===== 底部按钮 =====
            _btnReadData = new Button { Text = "读取数据", Location = new Point(220, 580), Size = new Size(120, 40) };
            Controls.Add(_btnReadData);

            _btnCancel = new Button { Text = "取消", Location = new Point(380, 580), Size = new Size(120, 40) };
            Controls.Add(_btnCancel);
        }

        private void SetupEvents()
        {
            // A组：左右移动
            _btnRight1.Click += (s, e) =>
            {
                while (_lstAllGroups.SelectedItem != null)
                {
                    var item = _lstAllGroups.SelectedItem;
                    _lstCheckGroups.Items.Add(item);
                    _lstAllGroups.Items.Remove(item);
                }
            };

            _btnLeft1.Click += (s, e) =>
            {
                while (_lstCheckGroups.SelectedItem != null)
                {
                    var item = _lstCheckGroups.SelectedItem;
                    _lstAllGroups.Items.Add(item);
                    _lstCheckGroups.Items.Remove(item);
                }
            };

            // B组：左右移动
            _btnRight2.Click += (s, e) =>
            {
                while (_lstKzSource.SelectedItem != null)
                {
                    var item = _lstKzSource.SelectedItem;
                    _lstKzGroups.Items.Add(item);
                    _lstKzSource.Items.Remove(item);
                }
            };

            _btnLeft2.Click += (s, e) =>
            {
                while (_lstKzGroups.SelectedItem != null)
                {
                    var item = _lstKzGroups.SelectedItem;
                    _lstKzSource.Items.Add(item);
                    _lstKzGroups.Items.Remove(item);
                }
            };

            // 底部按钮
            _btnReadData.Click += BtnReadData_Click;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        }

        private void BtnReadData_Click(object? sender, EventArgs e)
        {
            // A为空不能读
            if (_lstCheckGroups.Items.Count == 0)
            {
                MessageBox.Show("请至少选择一个参与验算的group");
                return;
            }

            // 保存A
            Global.Data.Groups.CheckGroups.Clear();
            foreach (var item in _lstCheckGroups.Items)
                Global.Data.Groups.CheckGroups.Add(item.ToString()!);

            // 保存B
            Global.Data.Groups.KzGroups.Clear();
            foreach (var item in _lstKzGroups.Items)
                Global.Data.Groups.KzGroups.Add(item.ToString()!);

            // 持久化
            Persistence.SaveCheckGroups();
            Persistence.SaveKzGroups();

            // 读模型
            Dispatch.ReadAllData();
            Global.Renderer.SetData(Global.Data.Joints, Global.Data.Frames);

            // 提示
            MessageBox.Show(
                $"读取完成：\n" +
                $"节点 {Global.Data.Joints.Count}，杆件 {Global.Data.Frames.Count}\n" +
                $"截面 {Global.Data.Sections.Count}，材料 {Global.Data.Materials.Count}\n" +
                $"组合 {Global.Data.AllCombos.Count}");

            DialogResult = DialogResult.OK;
            Close();
        }

    }
}

