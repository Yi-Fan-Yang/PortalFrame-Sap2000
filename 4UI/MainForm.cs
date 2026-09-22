using CSiAPIv1;
using PortalFrame._3Dispatch;
using System.Text;

namespace PortalFrame._4UI
{
    public class MainForm : Form
    {
        private cPluginCallback _pluginCallback = null!;
        private int _errorCode = 0;
        private _0Sap.SapModelReader _reader = null!;
        private Dispatch _dispatch = null!;

        private Button _btnReadModel = null!;
        private DoubleBufferedPanel _canvas = null!;
        private ModelRenderer _renderer = null!;
        private enum DragMode { None, Rotate, Pan }
        private DragMode _dragMode = DragMode.None;
        private Point _lastMousePos;             // 上一次鼠标位置


        public MainForm()
        {
            Text = "门式刚架验算插件";
            Width = 800;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            DefineControls();   // 第①步：创建控件
            LayoutControls();   // 第②步：摆位置
            SetupEvents();      // 第③步：绑事件
        }
        private void DefineControls()
        {
            _btnReadModel = new Button();
            _canvas = new DoubleBufferedPanel();
        }
        private void LayoutControls()
        {
            _btnReadModel.Text = "读取模型并显示";
            _btnReadModel.Size = new Size(160, 30);
            _btnReadModel.Location = new Point(10, 10);

            _canvas.BackColor = Color.White;
            _canvas.Location = new Point(10, 50);
            _canvas.Size = new Size(760, 510);
            _canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            Controls.Add(_btnReadModel);
            Controls.Add(_canvas);
        }


        private void SetupEvents()
        {
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
            _canvas.MouseWheel += Canvas_MouseWheel;


            _btnReadModel.Click += BtnReadModel_Click;
            _canvas.Paint += Canvas_Paint;
            FormClosing += MainForm_FormClosing;
        }

        public void Connect(ref cSapModel sapModel, ref cPluginCallback pluginCallback)
        {
            _pluginCallback = pluginCallback;
            _reader = new _0Sap.SapModelReader(sapModel);
            _dispatch = new Dispatch(_reader);
            _renderer = new ModelRenderer();
        }

        private void BtnReadModel_Click(object? sender, EventArgs e)
        {
            // 1. 读数据
            _dispatch.ReadModel();
            // 2. 把数据交给渲染器
            _renderer.SetData(_dispatch.GetJoints(), _dispatch.GetFrames());
            // 3. 请画布重画（会自动触发 Canvas_Paint）
            _canvas.Invalidate();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _pluginCallback.Finish(_errorCode);
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            // 把画笔和画布大小交给渲染器，让它去画
            _renderer.Draw(e.Graphics, _canvas.ClientSize.Width, _canvas.ClientSize.Height);
        }

        // 鼠标按下：记下来"按着了"，记下位置
        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Middle)
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
