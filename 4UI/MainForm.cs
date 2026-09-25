using CSiAPIv1;
using PortalFrame._0Sap;
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
        private cSapModel _sapModel = null!;
        private Dispatch _dispatch = null!;

        private PreReader _pre = null!;
        private PostReader _post = null!;
        private PreData _preData = new();
        private Preferences _prefer = new();
        private OverWrites _Writes = new();
        private DesignCombos _Combos = new();
        private PostData _postData = new();


        private Button _btnReadModel = null!;
        private Button _btnPreferences = null!;
        private Button _btnOverwrite = null!;
        private Button _btnCombo = null!;
        private Button _btnRunCheck = null!;
        private Button _btnResult = null!;
        private DoubleBufferedPanel _canvas = null!;
        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _lblTotal = null!;
        private ToolStripStatusLabel _lblSelected = null!;
        private ToolStripStatusLabel _lblStatus = null!;





        private ModelRenderer _renderer = null!;
        private enum DragMode { None, Rotate, Pan }
        private DragMode _dragMode = DragMode.None;
        private Point _lastMousePos;             // 上一次鼠标位置


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
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
            _canvas.MouseWheel += Canvas_MouseWheel;
            _canvas.Paint += Canvas_Paint;

            _btnReadModel.Click += BtnReadModel_Click;
            _btnPreferences.Click += btnPreferences_Click;
            _btnOverwrite.Click += (s, e) => MessageBox.Show("覆盖项窗口（后面做）");
            _btnCombo.Click += (s, e) => MessageBox.Show("组合窗口（后面做）");
            _btnRunCheck.Click += BtnRunCheck_Click;
            _btnResult.Click += (s, e) => MessageBox.Show("结果查看（后面做）");

            FormClosing += MainForm_FormClosing;
        }

        public void Connect(ref cSapModel sapModel, ref cPluginCallback pluginCallback)
        {
            _sapModel = sapModel;
            _pluginCallback = pluginCallback;
            _pre = new PreReader(ref sapModel);
            _post = new PostReader(ref sapModel);
            _dispatch = new Dispatch(_pre, _post, _preData,_prefer,_Writes,_Combos,_postData);
            _renderer = new ModelRenderer();
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
        private void BtnRunCheck_Click(object? sender, EventArgs e)
        {
            _lblStatus.Text = "验算中...";
            Application.DoEvents();

            _dispatch.RunCheck();

            _lblStatus.Text = $"验算完成：柱 {_postData.ColumnResults.Count} 根，梁 {_postData.BeamResults.Count} 根";
        }




        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _pluginCallback.Finish(_errorCode);
        }




        //=====================画布相关=========================
        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            // 把画笔和画布大小交给渲染器，让它去画
            _renderer.Draw(e.Graphics, _canvas.ClientSize.Width, _canvas.ClientSize.Height);
        }
        // 鼠标按下：记下来"按着了"，记下位置
        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _lastMousePos = e.Location;
                if ((ModifierKeys & Keys.Shift) != 0) _dragMode = DragMode.Rotate;
                else _dragMode = DragMode.Pan;
            }
        }
        // 鼠标移动
        private void Canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_dragMode == DragMode.None) return;

            double dx = e.X - _lastMousePos.X;
            double dy = e.Y - _lastMousePos.Y;
            if (_dragMode == DragMode.Rotate) _renderer.Rotate(dx, dy);
            else if (_dragMode == DragMode.Pan) _renderer.Pan(dx, dy);

            _lastMousePos = e.Location;
            _canvas.Invalidate();

        }
        // 鼠标松开：记下来"不按了"
        private void Canvas_MouseUp(object? sender, MouseEventArgs e)
        {
            _dragMode = DragMode.None;
        }
        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            _renderer.Zoom(e.Delta, e.X, e.Y);   // e.Delta 是滚轮滚动量，向上为正;e.X/e.Y 就是鼠标在画布上的位置
            _canvas.Invalidate();      // 重画
        }

    }
}
