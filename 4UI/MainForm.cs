using CSiAPIv1;
using PortalFrame._0Sap;
using PortalFrame._1Model;
using PortalFrame._2Check;
using PortalFrame._3Dispatch;
using PortalFrame._5data;
using System.Linq;
using System.Text;


namespace PortalFrame._4UI
{
    public class MainForm : Form
    {
        private cPluginCallback _pluginCallback = null!;
        private int _errorCode = 0;
        private Dispatch _dispatch = null!;

        private PreReader _pre = null!;
        private PostReader _post = null!;
        private PreData _preData = new();
        private Preferences _prefer = new();
        private OverWrites _Writes = new();
        private DesignCombos _Combos = new();
        private PostData _postData = new();


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


        private ModelRenderer _renderer = null!;
        private CanvasInteraction _canvasInteraction = null!;


        public MainForm()
        {
            Text = "门式刚架验算插件";
            Width = 800;
            Height = 700;
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
            _canvas.Size = new Size(760, 480);
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
            _canvasInteraction.MemberRightClicked += OnMemberRightClicked;


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
            _pre = new PreReader(ref sapModel);
            _post = new PostReader(ref sapModel);
            _dispatch = new Dispatch(_pre, _post, _preData,_prefer,_Writes,_Combos,_postData);
            _renderer = new ModelRenderer();
            // 初始化画布交互
            _canvasInteraction = new CanvasInteraction(_renderer, _canvas);
            _canvasInteraction.SelectionChanged += UpdateStatusBar;
        }
        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            _renderer.Draw(e.Graphics, _canvas.ClientSize.Width, _canvas.ClientSize.Height);
        }
        private void BtnReadModel_Click(object? sender, EventArgs e)
        {
            _dispatch.ReadAllData();           // ← 只调这一行，具体逻辑在 Dispatch 里

            _renderer.SetData(_preData.Joints, _preData.Frames);
            _canvas.Invalidate();

            MessageBox.Show(
                $"读取完成：\n" +
                $"节点 {_preData.Joints.Count}，杆件 {_preData.Frames.Count}\n" +
                $"截面 {_preData.Sections.Count}，材料 {_preData.Materials.Count}\n" +
                $"组合 {_preData.Combos.Count}");
        }
        private void btnPreferences_Click(object? sender, EventArgs e)
        {
            var form = new PreferencesForm(_prefer);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _lblStatus.Text = "首选项已更新";
            }
        }
        private void btnOverwrite_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("覆盖项窗口（后面做）");
        }
        private void btnCombo_Click(object? sender, EventArgs e)
        {
            var form = new ComboSelectForm(_Combos, _preData.Combos);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _lblStatus.Text = $"已选 {_Combos.SelectedCombos.Count} 个设计组合";
            }
        }
        private void BtnRunCheck_Click(object? sender, EventArgs e)
        {
            _lblStatus.Text = "验算中...";
            Application.DoEvents();

            //只验算选中的构件
            var selected = _renderer.GetSelected();
            _dispatch.RunCheck(selected.Count > 0 ? selected : null);

            // 把结果传给渲染器着色
            var utils = _dispatch.GetMemberUtilsByCheckType("汇总");
            _renderer.SetResults(utils);
            _canvas.Invalidate();
            _lblStatus.Text = $"验算完成：柱 {_postData.ColumnResults.Count} 根，梁 {_postData.BeamResults.Count} 根";
        }
        private void btnResult_Click(object? sender, EventArgs e)
        {
            var form = new ResultSelectForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var utils = _dispatch.GetMemberUtilsByCheckType(form.SelectedCheckType);
                _renderer.SetResults(utils);
                _canvas.Invalidate();

                _lblStatus.Text = $"显示结果：{form.SelectedCheckType}";
            }
        }
        private void UpdateStatusBar()
        {
            var selected = _renderer.GetSelected();
            int columnCount = 0;
            int beamCount = 0;

            foreach (var name in selected)
            {
                if (_preData.Members.TryGetValue(name, out var member))
                {
                    if (member.Type == MemberType.Column) columnCount++;
                    else beamCount++;
                }
            }

            _lblSelected.Text = $"已选择：柱 {columnCount} 根，梁 {beamCount} 根";
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _pluginCallback.Finish(_errorCode);
        }

        private void OnMemberRightClicked(string memberName)
        {
            // 判断有没有这根构件的验算结果
            if (_postData.ColumnResults.ContainsKey(memberName))
            {
                var form = new MemberDetailForm(memberName, _postData);
                form.ShowDialog();
            }
            else if (_postData.BeamResults.ContainsKey(memberName))
            {
                var form = new MemberDetailForm(memberName, _postData);
                form.ShowDialog();
            }
            // 没结果的话什么都不做
        }


    }
}
