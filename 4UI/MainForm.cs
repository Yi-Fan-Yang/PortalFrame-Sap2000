using CSiAPIv1;
using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._3Dispatch;
using System.Linq;
using System.Text;


namespace PortalFrame._4UI
{
    public class MainForm : Form
    {
        private cPluginCallback _pluginCallback = null!;
        private CanvasInteraction _canvasInteraction = null!;


        private Button _btnReadModel = new Button();
        private Button _btnPreferences = new Button();
        private Button _btnOverwrite = new Button();
        private Button _btnCombo = new Button();
        private Button _btnRunCheck = new Button();
        private Button _btnResult = new Button();
        private DoubleBufferedPanel _canvas = new DoubleBufferedPanel();
        private StatusStrip _statusStrip = new StatusStrip();
        private ToolStripStatusLabel _lblTotal = new ToolStripStatusLabel();
        private ToolStripStatusLabel _lblSelected = new ToolStripStatusLabel();
        private ToolStripStatusLabel _lblStatus = new ToolStripStatusLabel();




        public MainForm()
        {
            this.Font = UiConfig.DefaultFont;
            FormBorderStyle = FormBorderStyle.FixedSingle;   // 固定大小，不能拖拽
            MaximizeBox = false;                              // 禁用最大化
            MinimizeBox = false;                              // 禁用最小化

            Text = "门式刚架验算插件";
            Width = 1600;
            Height = 900;
            StartPosition = FormStartPosition.CenterScreen;

            LayoutControls();   // 第②步：摆位置
            SetupEvents();      // 第③步：绑事件
        }
        private void LayoutControls()
        {
            // 顶部按钮
            _btnReadModel.Text = "读取SAP模型";
            _btnReadModel.Size = new Size(110, 30);
            _btnReadModel.Location = new Point(10, 10);

            _btnPreferences.Text = "验算首选项";
            _btnPreferences.Size = new Size(110, 30);
            _btnPreferences.Location = new Point(130, 10);

            _btnOverwrite.Text = "构件覆盖项";
            _btnOverwrite.Size = new Size(110, 30);
            _btnOverwrite.Location = new Point(250, 10);

            _btnCombo.Text = "设计组合";
            _btnCombo.Size = new Size(110, 30);
            _btnCombo.Location = new Point(370, 10);

            _btnRunCheck.Text = "运行验算";
            _btnRunCheck.Size = new Size(110, 30);
            _btnRunCheck.Location = new Point(490, 10);

            _btnResult.Text = "结果查看";
            _btnResult.Size = new Size(110, 30);
            _btnResult.Location = new Point(610, 10);

            // 画布
            _canvas.BackColor = Color.White;
            _canvas.Location = new Point(10, 50);
            _canvas.Size = new Size(Width - 30, Height - 120);
            _canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // 状态栏
            _lblTotal.Text = "总计：柱 0 根，梁 0 根";
            _lblSelected.Text = "已选择：柱 0 根，梁 0 根";
            _lblStatus.Text = "就绪";
            _lblStatus.Spring = true;
            _lblStatus.TextAlign = ContentAlignment.MiddleRight;

            _statusStrip.Items.Add(_lblTotal);
            _statusStrip.Items.Add(new ToolStripStatusLabel("    "));
            _statusStrip.Items.Add(_lblSelected);
            _statusStrip.Items.Add(_lblStatus);
            _statusStrip.Dock = DockStyle.Bottom;   // 状态栏固定在底部


            // 添加到窗体
            Controls.Add(_btnReadModel);
            Controls.Add(_btnPreferences);
            Controls.Add(_btnOverwrite);
            Controls.Add(_btnCombo);
            Controls.Add(_btnRunCheck);
            Controls.Add(_btnResult);
            Controls.Add(_canvas);
            Controls.Add(_statusStrip);
        }
        private void SetupEvents()
        {
            _canvas.Paint += Canvas_Paint;

            _btnReadModel.Click += BtnReadModel_Click;
            _btnPreferences.Click += btnPreferences_Click;
            _btnOverwrite.Click += btnOverwrite_Click;
            _btnCombo.Click += btnCombo_Click;
            _btnRunCheck.Click += BtnRunCheck_Click;
            _btnResult.Click += btnResult_Click;
            FormClosing += MainForm_FormClosing;
        }

        public void Connect(ref cSapModel sapModel, ref cPluginCallback pluginCallback)
        {
            _pluginCallback = pluginCallback;
            Global.SapModel = sapModel;
            // 初始化画布交互
            _canvasInteraction = new CanvasInteraction(Global.Renderer, _canvas);
            _canvasInteraction.MemberRightClicked += OnMemberRightClicked;
            _canvasInteraction.SelectionChanged += UpdateStatusBar;
        }
        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            Global.Renderer.Draw(e.Graphics, _canvas.ClientSize.Width, _canvas.ClientSize.Height);
        }
        private void BtnReadModel_Click(object? sender, EventArgs e)
        {
            Dispatch.ReadAllData();
            Persistence.LoadAll();
            Global.Renderer.SetData(Global.ModelData.Joints, Global.ModelData.Frames);
            _canvas.Invalidate();

            // 加这两行：统计柱和梁的总数
            int totalCol = Global.ModelData.Members.Values.Count(m => m.Type == MemberType.Column);
            int totalBeam = Global.ModelData.Members.Values.Count(m => m.Type == MemberType.Beam);
            _lblTotal.Text = $"总计：柱 {totalCol} 根，梁 {totalBeam} 根";

            MessageBox.Show(
                $"读取完成：\n" +
                $"节点 {Global.ModelData.Joints.Count}，杆件 {Global.ModelData.Frames.Count}\n" +
                $"截面 {Global.ModelData.Sections.Count}，材料 {Global.ModelData.Materials.Count}\n" +
                $"组合 {Global.ModelData.AllCombos.Count}");
        }
        private void btnPreferences_Click(object? sender, EventArgs e)
        {
            var form = new PreferencesForm(Global.ModelData.Preferences);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _lblStatus.Text = "首选项已更新";
            }
        }
        private void btnOverwrite_Click(object? sender, EventArgs e)
        {
            var selected = Global.Renderer.GetSelected();

            if (selected.Count == 0)
            {
                MessageBox.Show("请先在画布上选中构件");
                return;
            }

            // 把选中的构件都拿出来
            var members = new List<MemberData>();
            foreach (var name in selected)
            {
                if (Global.ModelData.Members.TryGetValue(name, out var member))
                {
                    members.Add(member);
                }
            }

            if (members.Count == 0)
            {
                MessageBox.Show("找不到选中的构件");
                return;
            }

            var form = new OverWritesForm(members);
            form.ShowDialog();
        }
        private void btnCombo_Click(object? sender, EventArgs e)
        {
            var form = new ComboSelectForm(Global.Combos, Global.ModelData.AllCombos);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _lblStatus.Text = $"已选 {Global.Combos.SelectedCombos.Count} 个设计组合";
            }
        }
        private void BtnRunCheck_Click(object? sender, EventArgs e)
        {
            try
            {
                _lblStatus.Text = "验算中...";
                Application.DoEvents();
                //只验算选中的构件
                var selected = Global.Renderer.GetSelected();
                Dispatch.RunCheck(selected.Count > 0 ? selected : null);
                // 把结果传给渲染器着色
                var display = Dispatch.GetMemberDisplayInfo("汇总");
                Global.Renderer.SetResults(display);
                _canvas.Invalidate();
                int colCount = Global.ModelData.Members.Values.Count(m => m.Type == MemberType.Column);
                int beamCount = Global.ModelData.Members.Values.Count(m => m.Type == MemberType.Beam);
                _lblStatus.Text = $"验算完成：柱 {colCount} 根，梁 {beamCount} 根";

            }
            catch (Exception ex)
            {
                MessageBox.Show("验算出错：" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }
        private void btnResult_Click(object? sender, EventArgs e)
        {
            try
            {
                var form = new ResultSelectForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var display = Dispatch.GetMemberDisplayInfo(form.SelectedCheckType);
                    Global.Renderer.SetResults(display);
                    _canvas.Invalidate();

                    _lblStatus.Text = $"显示结果：{form.SelectedCheckType}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("验算出错：" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }
        private void UpdateStatusBar()
        {
            var selected = Global.Renderer.GetSelected();
            int columnCount = 0;
            int beamCount = 0;

            foreach (var name in selected)
            {
                if (Global.ModelData.Members.TryGetValue(name, out var member))
                {
                    if (member.Type == MemberType.Column) columnCount++;
                    else beamCount++;
                }
            }

            _lblSelected.Text = $"已选择：柱 {columnCount} 根，梁 {beamCount} 根";
        }
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
        }

        private void OnMemberRightClicked(string memberName)
        {
            if (Global.ModelData.Members.TryGetValue(memberName, out var member))
            {
                var form = new MemberDetailForm(member);
                form.ShowDialog();
            }
        }
    }
}
