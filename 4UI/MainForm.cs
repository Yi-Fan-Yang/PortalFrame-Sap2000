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


        private Button _btnCheckSettings = new ();
        private Button _btnPreferences = new ();
        private Button _btnOverwrite = new ();
        private Button _btnCombo = new ();
        private Button _btnRunCheck = new ();
        private Button _btnCalcKz = new();
        private Button _btnResult = new ();
        private DoubleBufferedPanel _canvas = new ();
        private StatusStrip _statusStrip = new ();
        private ToolStripStatusLabel _lblTotal = new ();
        private ToolStripStatusLabel _lblSelected = new ();
        private ToolStripStatusLabel _lblStatus = new ();




        public MainForm()
        {
            this.Font = Global.Ui.DefaultFont;
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
            _btnCheckSettings.Text = "验算设置";
            _btnCheckSettings.Size = new Size(110, 30);
            _btnCheckSettings.Location = new Point(10, 10);

            _btnPreferences.Text = "首选项";
            _btnPreferences.Size = new Size(110, 30);
            _btnPreferences.Location = new Point(130, 10);

            _btnOverwrite.Text = "覆盖项";
            _btnOverwrite.Size = new Size(110, 30);
            _btnOverwrite.Location = new Point(250, 10);

            _btnCombo.Text = "设计组合";
            _btnCombo.Size = new Size(110, 30);
            _btnCombo.Location = new Point(370, 10);

            _btnCalcKz.Text = "计算Kz";
            _btnCalcKz.Size = new Size(110, 30);
            _btnCalcKz.Location = new Point(490, 10);

            _btnRunCheck.Text = "运行验算";
            _btnRunCheck.Size = new Size(110, 30);
            _btnRunCheck.Location = new Point(610, 10);

            _btnResult.Text = "结果查看";
            _btnResult.Size = new Size(110, 30);
            _btnResult.Location = new Point(730, 10);

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
            Controls.Add(_btnCheckSettings);
            Controls.Add(_btnPreferences);
            Controls.Add(_btnOverwrite);
            Controls.Add(_btnCombo);
            Controls.Add(_btnCalcKz);
            Controls.Add(_btnRunCheck);
            Controls.Add(_btnResult);
            Controls.Add(_canvas);
            Controls.Add(_statusStrip);

            // 初始只有验算设置亮，其他灰
            _btnPreferences.Enabled = false;
            _btnOverwrite.Enabled = false;
            _btnCombo.Enabled = false;
            _btnCalcKz.Enabled = false;
            _btnRunCheck.Enabled = false;
            _btnResult.Enabled = false;
        }
        private void SetupEvents()
        {
            _canvas.Paint += Canvas_Paint;

            _btnCheckSettings.Click += BtnCheckSettings_Click;
            _btnPreferences.Click += BtnPreferences_Click;
            _btnOverwrite.Click += BtnOverwrite_Click;
            _btnCombo.Click += BtnCombo_Click;
            _btnCalcKz.Click += BtnCalcKz_Click;
            _btnRunCheck.Click += BtnRunCheck_Click;
            _btnResult.Click += BtnResult_Click;
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
        private void BtnCheckSettings_Click(object? sender, EventArgs e)
        {
            Persistence.LoadCheckSettings();   // 先读上次保存的A/B
            var form = new CheckSettingsForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                // 刷新画布和状态栏
                _canvas.Invalidate();

                var checkable = Global.Data.Members.Values.Where(m => m.IsCheckable).ToList();
                int totalCol = checkable.Count(m => m.Type == MemberType.Column);
                int totalBeam = checkable.Count(m => m.Type == MemberType.Beam);
                _lblTotal.Text = $"总计：柱 {totalCol} 根，梁 {totalBeam} 根";

                // 其他按钮亮
                _btnPreferences.Enabled = true;
                _btnOverwrite.Enabled = true;
                _btnCombo.Enabled = true;
                _btnRunCheck.Enabled = true;
                _btnResult.Enabled = true;
                _btnCalcKz.Enabled = Global.Data.Groups.KzGroups.Count > 0;
            }
        }

        private void BtnPreferences_Click(object? sender, EventArgs e)
        {
            var form = new PreferencesForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _lblStatus.Text = "首选项已更新";
            }
        }
        private void BtnOverwrite_Click(object? sender, EventArgs e)
        {
            var selected = Global.Renderer.GetSelected();

            if (selected.Count == 0)
            {
                MessageBox.Show("请先在画布上选中构件");
                return;
            }

            // 先判断有没有读模型，有没有结果类
            if (Global.Data.Members.Count == 0)
            {
                MessageBox.Show("请先读取模型数据");
                return;
            }

            // 先判断第一个选中的构件有没有结果类
            var firstMember = Global.Data.Members[selected.First()];
            if (firstMember.CheckResult == null)
            {
                MessageBox.Show("请先读取模型数据");
                return;
            }

            // 判断有没有选设计组合
            if (Global.Data.Combos.SelectedCombos.Count == 0)
            {
                MessageBox.Show("请先选择设计组合！");
                return;
            }

            // 所有判断都通过了，再打开覆盖项窗口
            var members = selected.Select(name => Global.Data.Members[name]).ToList();
            var form = new OverWritesForm(members);
            form.ShowDialog();
        }
        private void BtnCombo_Click(object? sender, EventArgs e)
        {
            var form = new ComboSelectForm(Global.Data.Combos, Global.Data.AllCombos);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _lblStatus.Text = $"已选 {Global.Data.Combos.SelectedCombos.Count} 个设计组合";
            }
        }
        private void BtnCalcKz_Click(object? sender, EventArgs e)
        {
            try
            {
                // 计算Kz
                MemberBuilder.GetKz(Global.SapModel, Global.Data.Groups.KzGroups);

                // 保存Kz到sdb
                Persistence.SaveAllKz();

                _lblStatus.Text = "Kz计算完成";
                MessageBox.Show("Kz计算完成并已保存");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kz计算失败：" + ex.Message);
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
                int colCount = Global.Data.Members.Values.Count(m => m.Type == MemberType.Column);
                int beamCount = Global.Data.Members.Values.Count(m => m.Type == MemberType.Beam);
                _lblStatus.Text = $"验算完成：柱 {colCount} 根，梁 {beamCount} 根";

            }
            catch (Exception ex)
            {
                MessageBox.Show("验算出错：" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }
        private void BtnResult_Click(object? sender, EventArgs e)
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
                if (Global.Data.Members.TryGetValue(name, out var member))
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
            if (Global.Data.Members.TryGetValue(memberName, out var member))
            {
                var form = new MemberDetailForm(member);
                form.ShowDialog();
            }
        }
    }
}
