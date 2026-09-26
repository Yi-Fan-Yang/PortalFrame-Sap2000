using PortalFrame._3Dispatch;

namespace PortalFrame._4UI
{
    /// <summary>
    /// 画布鼠标交互：旋转/平移/缩放/点选/框选
    /// </summary>
    public class CanvasInteraction
    {
        private ModelRenderer _renderer;
        private DoubleBufferedPanel _canvas;
        // 选择变化的时候触发
        public event Action? SelectionChanged;
        // 右键点构件的时候触发
        public event Action<string>? MemberRightClicked;



        // 拖拽模式
        private enum DragMode { None, Rotate, Pan }
        private DragMode _dragMode = DragMode.None;
        private Point _lastMousePos;

        // 选择模式
        private enum SelectMode { None, Pick, Box }
        private SelectMode _selectMode = SelectMode.None;
        private Point _leftMouseDownPos;

        public CanvasInteraction(ModelRenderer renderer, DoubleBufferedPanel canvas)
        {
            _renderer = renderer;
            _canvas = canvas;
            BindEvents();
        }

        private void BindEvents()
        {
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
            _canvas.MouseWheel += Canvas_MouseWheel;
        }


        //======================
        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _lastMousePos = e.Location;
                if ((Control.ModifierKeys & Keys.Shift) != 0) _dragMode = DragMode.Rotate;
                else _dragMode = DragMode.Pan;
            }
            else if (e.Button == MouseButtons.Left)
            {
                _leftMouseDownPos = e.Location;
                _selectMode = SelectMode.Pick;
            }
        }
        private void Canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            // 中键拖动：旋转/平移
            if (_dragMode != DragMode.None)
            {
                double dx = e.X - _lastMousePos.X;
                double dy = e.Y - _lastMousePos.Y;
                if (_dragMode == DragMode.Rotate) _renderer.Rotate(dx, dy);
                else if (_dragMode == DragMode.Pan) _renderer.Pan(dx, dy);

                _lastMousePos = e.Location;
                _canvas.Invalidate();
                return;
            }

            // 左键拖动：判断是否进入框选模式
            if (_selectMode == SelectMode.Pick && e.Button == MouseButtons.Left)
            {
                int dx = Math.Abs(e.X - _leftMouseDownPos.X);
                int dy = Math.Abs(e.Y - _leftMouseDownPos.Y);

                if (dx > 5 || dy > 5)
                {
                    _selectMode = SelectMode.Box;
                    _renderer.StartBoxSelect(e.Location);
                }
            }

            // 正在拉框：更新终点，重画
            if (_selectMode == SelectMode.Box)
            {
                _renderer.UpdateBoxSelect(e.Location);
                _canvas.Invalidate();
            }
        }
        private void Canvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _dragMode = DragMode.None;
                return;
            }

            if (e.Button != MouseButtons.Left) return;
            if (e.Button == MouseButtons.Right)
            {
                var member = _renderer.HitTest(e.Location);
                if (member != null)
                {
                    MemberRightClicked?.Invoke(member);
                }
                return;
            }


            if (_selectMode == SelectMode.Pick)
                DoPickSelect(e.Location);
            else if (_selectMode == SelectMode.Box)
                DoBoxSelect(_leftMouseDownPos, e.Location);

            _selectMode = SelectMode.None;
            _renderer.EndBoxSelect();
            _canvas.Invalidate();
        }
        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            _renderer.Zoom(e.Delta, e.X, e.Y);
            _canvas.Invalidate();
        }
        // 点选
        private void DoPickSelect(Point mousePos)
        {
            var member = _renderer.HitTest(mousePos);
            bool ctrl = (Control.ModifierKeys & Keys.Control) != 0;

            if (member == null)
            {
                if (!ctrl) _renderer.ClearSelection();
                return;
            }

            if (ctrl)
            {
                if (_renderer.GetSelected().Contains(member))
                    _renderer.Deselect(member);
                else
                    _renderer.Select(member);
            }
            else
            {
                _renderer.ClearSelection();
                _renderer.Select(member);
            }
            SelectionChanged?.Invoke();
        }
        // 框选
        private void DoBoxSelect(Point start, Point end)
        {
            Rectangle rect;
            bool isWindowMode;

            if (start.X < end.X)
            {
                rect = new Rectangle(start.X, start.Y, end.X - start.X, end.Y - start.Y);
                isWindowMode = true;
            }
            else
            {
                int x = Math.Min(start.X, end.X);
                int y = Math.Min(start.Y, end.Y);
                rect = new Rectangle(x, y, Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                isWindowMode = false;
            }

            var selected = _renderer.BoxSelect(rect, isWindowMode);
            bool ctrl = (Control.ModifierKeys & Keys.Control) != 0;

            if (!ctrl) _renderer.ClearSelection();
            foreach (var name in selected)
                _renderer.Select(name);
            SelectionChanged?.Invoke();
        }

    }
}
