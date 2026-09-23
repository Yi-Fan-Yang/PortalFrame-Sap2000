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
        private PreReader _pre = null!;
        private PostReader _post = null!;
        private Dispatch _dispatch = null!;
        private DataStore _store = new();


        private Button _btnReadModel = null!;
        private Button _btnRunCheck = null!;
        private DoubleBufferedPanel _canvas = null!;
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

            DefineControls();   // 第①步：创建控件
            LayoutControls();   // 第②步：摆位置
            SetupEvents();      // 第③步：绑事件
        }
        private void DefineControls()
        {
            _btnReadModel = new Button();
            _canvas = new DoubleBufferedPanel();
            _btnRunCheck = new Button();

        }
        private void LayoutControls()
        {
            _btnReadModel.Text = "读取模型并显示";
            _btnReadModel.Size = new Size(160, 30);
            _btnReadModel.Location = new Point(10, 10);

            _btnRunCheck.Text = "运行验算";
            _btnRunCheck.Size = new Size(160, 30);
            _btnRunCheck.Location = new Point(180, 10);

            _canvas.BackColor = Color.White;
            _canvas.Location = new Point(10, 50);
            _canvas.Size = new Size(760, 510);
            _canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            Controls.Add(_btnReadModel);
            Controls.Add(_btnRunCheck);
            Controls.Add(_canvas);
        }


        private void SetupEvents()
        {
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
            _canvas.MouseWheel += Canvas_MouseWheel;


            _btnReadModel.Click += BtnReadModel_Click;
            _btnRunCheck.Click += BtnRunCheck_Click;
            _canvas.Paint += Canvas_Paint;
            FormClosing += MainForm_FormClosing;
        }

        public void Connect(ref cSapModel sapModel, ref cPluginCallback pluginCallback)
        {
            _sapModel = sapModel;
            _pluginCallback = pluginCallback;
            _pre = new PreReader(ref sapModel);
            _post = new PostReader(ref sapModel);
            _dispatch = new Dispatch(_pre, _post, _store);
            _renderer = new ModelRenderer();
        }

        private void BtnReadModel_Click(object? sender, EventArgs e)
        {
            _dispatch.ReadAllData();           // ← 只调这一行，具体逻辑在 Dispatch 里

            _renderer.SetData(_store.Joints, _store.Frames);
            _canvas.Invalidate();

            MessageBox.Show(
                $"读取完成：\n" +
                $"节点 {_store.Joints.Count}，杆件 {_store.Frames.Count}\n" +
                $"截面 {_store.Sections.Count}，材料 {_store.Materials.Count}\n" +
                $"组合 {_store.Combos.Count}");
        }
        private void BtnRunCheck_Click(object? sender, EventArgs e)
        {
            string combo = _store.Combos.First();   // 临时：先取第一个组合
            _dispatch.RunCheck(combo);

            // 临时验证：找最大利用率
            var maxUtil = _store.CheckResults.Values
                .SelectMany(r => r)
                .Max(c => c.Utilization);

            MessageBox.Show($"验算完成\n最大利用率：{maxUtil:F3}");
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
