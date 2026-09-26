using Microsoft.VisualBasic.Devices;
using PortalFrame._1Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalFrame._3Dispatch
{
    public class ModelRenderer
    {
        // 要画的数据
        private Dictionary<string, JointData> _joints = new();
        private Dictionary<string, FrameData> _frames = new();
        //画笔
        private DrawPens _pens = new DrawPens();
        //投影参数
        private ProjectionParams _proj = new ProjectionParams();
        
        // 每根杆件的屏幕坐标（Draw的时候存下来，选的时候用）
        private Dictionary<string, (PointF p1, PointF p2)> _memberScreenCoords = new();
        private HashSet<string> _selectedMembers = new();   // 选中的杆件名
        private bool _isBoxSelecting;                        // 是否正在拉框
        private Point _boxStart;                             // 拉框起点
        private Point _boxEnd;                               // 拉框终点

        // 验算结果：杆件名→最大利用率
        private Dictionary<string, double> _currentUtils = new();


        /// <summary>
        /// =============================画到指定画布上,g 是画笔=========================
        /// </summary>
        public class DrawPens
        {
            public Pen Normal = new Pen(Color.Black, 2);
            public Pen Selected = new Pen(Color.Blue, 3);
            public Pen Green = new Pen(Color.Green, 2);
            public Pen Yellow = new Pen(Color.Yellow, 2);
            public Pen Red = new Pen(Color.Red, 2);
        }
        public void Draw(Graphics g, int canvasWidth, int canvasHeight)
        {
            if (_joints.Count == 0) return;

            CalcProjection(canvasWidth, canvasHeight);   // 1. 算投影参数
            DrawMembers(g);                               // 2. 画杆件
            DrawJoints(g);                                // 3. 画节点
            if (_isBoxSelecting) DrawSelectionBox(g, _boxStart, _boxEnd);  // 画拉框虚线
        }

        private Pen GetMemberPen(string memberName)
        {
            if (_selectedMembers.Contains(memberName))
                return _pens.Selected;

            if (_currentUtils.TryGetValue(memberName, out double util))
            {
                if (util <= 0.8) return _pens.Green;
                if (util <= 1.0) return _pens.Yellow;
                return _pens.Red;
            }

            return _pens.Normal;
        }
        // 坐标变换：模型坐标 → 屏幕坐标
        private PointF ToScreen(JointData j)
        {
            // ① 先绕 Y 轴左右转（方位角）
            double x1 = j.X * Math.Cos(_proj.AngleY) + j.Z * Math.Sin(_proj.AngleY);
            double z1 = -j.X * Math.Sin(_proj.AngleY) + j.Z * Math.Cos(_proj.AngleY);

            // ② 再绕 X 轴上下转（俯仰角）
            double y2 = j.Y * Math.Cos(_proj.AngleX) - z1 * Math.Sin(_proj.AngleX);

            // ③ 投影到屏幕
            float sx = (float)(_proj.Pad + (x1 - _proj.MinX) * _proj.S + _proj.PanX);
            float sy = (float)(_proj.CanvasHeight - _proj.Pad - (y2 - _proj.MinZ) * _proj.S + _proj.PanY);

            return new PointF(sx, sy);
        }
        // 算投影参数
        private void CalcProjection(int canvasWidth, int canvasHeight)
        {
            // 模型范围
            _proj.MinX = _joints.Values.Min(j => j.X);
            _proj.MaxX = _joints.Values.Max(j => j.X);
            _proj.MinZ = _joints.Values.Min(j => j.Z);
            _proj.MaxZ = _joints.Values.Max(j => j.Z);
            _proj.CanvasHeight = canvasHeight;

            // 缩放
            float cw = canvasWidth - 2 * _proj.Pad;
            float ch = canvasHeight - 2 * _proj.Pad;

            double sx = (_proj.MaxX > _proj.MinX) ? cw / (_proj.MaxX - _proj.MinX) : 1;
            double sz = (_proj.MaxZ > _proj.MinZ) ? ch / (_proj.MaxZ - _proj.MinZ) : 1;
            _proj.S = Math.Min(sx, sz) * _proj.Zoom;
        }
        // 画杆件
        private void DrawMembers(Graphics g)
        {
            _memberScreenCoords.Clear();

            foreach (var f in _frames.Values)
            {
                if (!_joints.TryGetValue(f.StartJoint, out var j1)) continue;
                if (!_joints.TryGetValue(f.EndJoint, out var j2)) continue;

                var p1 = ToScreen(j1);
                var p2 = ToScreen(j2);
                _memberScreenCoords[f.Name] = (p1, p2);

                g.DrawLine(GetMemberPen(f.Name), p1, p2);
            }
        }
        // 画节点
        private void DrawJoints(Graphics g)
        {
            using var brush = new SolidBrush(Color.Red);
            foreach (var j in _joints.Values)
            {
                var p = ToScreen(j);
                g.FillEllipse(brush, p.X - 3, p.Y - 3, 6, 6);
            }
        }






        // 更新数据（每次读完模型调一次）
        public void SetData(Dictionary<string, JointData> joints, Dictionary<string, FrameData> frames)
        {
            _joints = joints;
            _frames = frames;
        }


        /// <summary>
        /// =============================旋转、平移、缩放=========================
        /// </summary>
        //投影参数集中管理
        public class ProjectionParams
        {
            // 模型范围
            public double MinX, MaxX, MinZ, MaxZ;

            // 视图变换
            public double AngleX = 0;    // 俯仰角
            public double AngleY = 0;    // 方位角
            public double Zoom = 1.0;    // 缩放
            public double PanX = 0;      // 水平平移
            public double PanY = 0;      // 竖直平移

            // 画的时候算出来的
            public double S;             // 缩放系数
            public float Pad = 30;       // 边距
            public int CanvasHeight;     // 画布高度
        }
        public void Rotate(double dx, double dy)
        {
            // 每 100 像素转 1 弧度（手感可以调）
            _proj.AngleY += dx / 100.0;   // 左右拖 → 方位角
            _proj.AngleX += dy / 100.0;   // 上下拖 → 俯仰角
        }
        public void Zoom(double delta, double mouseX, double mouseY)
        {
            double oldZoom = _proj.Zoom;
            _proj.Zoom *= (delta > 0) ? 1.1 : 0.9;
            if (_proj.Zoom < 0.1) _proj.Zoom = 0.1;
            if (_proj.Zoom > 10) _proj.Zoom = 10;
            double ratio = _proj.Zoom / oldZoom;

            // 关键：缩放的同时微调平移，把鼠标指着的结构点拽回鼠标位置
            _proj.PanX = mouseX - _proj.Pad - (mouseX - _proj.Pad - _proj.PanX) * ratio;
            // y 方向有翻转（屏幕上方=结构下方），公式符号和 x 不同
            _proj.PanY = mouseY - _proj.CanvasHeight + _proj.Pad
                + (_proj.CanvasHeight - _proj.Pad - mouseY + _proj.PanY) * ratio;

        }
        public void Pan(double dx, double dy)
        {
            _proj.PanX += dx;
            _proj.PanY += dy;
        }


        /// <summary>
        /// =============================选择=========================
        /// </summary>
        public string? HitTest(Point screenPos)
        {
            string? bestMatch = null;
            double minDist = 5.0;

            foreach (var (name, (p1, p2)) in _memberScreenCoords)
            {
                double dist = DistancePointToLine(screenPos, p1, p2);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestMatch = name;
                }
            }

            return bestMatch;
        }
        /// <param name="rect">拉框的屏幕矩形</param>
        /// <param name="isWindowMode">true=从左到右（窗口选，全在框里才选中）；false=从右到左（交叉选，碰到就选中）</param>
        public HashSet<string> BoxSelect(Rectangle rect, bool isWindowMode)
        {
            var result = new HashSet<string>();

            foreach (var (name, (p1, p2)) in _memberScreenCoords)
            {
                if (isWindowMode)
                {
                    // 窗口选：两个端点都在框里
                    if (rect.Contains(new Point((int)p1.X, (int)p1.Y)) && rect.Contains(new Point((int)p2.X, (int)p2.Y)))
                    {
                        result.Add(name);
                    }
                }
                else
                {
                    // 交叉选：线段和框相交
                    if (LineIntersectsRect(p1, p2, rect))
                    {
                        result.Add(name);
                    }
                }
            }

            return result;
        }

        // 判断线段是否和矩形相交
        private bool LineIntersectsRect(PointF a, PointF b, Rectangle rect)
        {
            // 两个端点有一个在框里就行
            if (rect.Contains(new Point((int)a.X, (int)a.Y)) ||rect.Contains(new Point((int)b.X, (int)b.Y))) return true;

            // 线段和矩形四条边相交
            return LineIntersectsLine(a, b, new PointF(rect.Left, rect.Top), new PointF(rect.Right, rect.Top)) ||
                   LineIntersectsLine(a, b, new PointF(rect.Left, rect.Bottom), new PointF(rect.Right, rect.Bottom)) ||
                   LineIntersectsLine(a, b, new PointF(rect.Left, rect.Top), new PointF(rect.Left, rect.Bottom)) ||
                   LineIntersectsLine(a, b, new PointF(rect.Right, rect.Top), new PointF(rect.Right, rect.Bottom));
        }
        // 判断两条线段是否相交
        private bool LineIntersectsLine(PointF a1, PointF a2, PointF b1, PointF b2)
        {
            double cross(PointF p, PointF a, PointF b) =>
                (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);

            double d1 = cross(b1, a1, a2);
            double d2 = cross(b2, a1, a2);
            double d3 = cross(a1, b1, b2);
            double d4 = cross(a2, b1, b2);

            return ((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                   ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0));
        }
        // 算点到线段的垂直距离
        private double DistancePointToLine(PointF p, PointF a, PointF b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.001) return double.MaxValue;

            // 投影长度
            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (len * len);
            t = Math.Max(0, Math.Min(1, t));   // 限制在线段范围内

            // 投影点
            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;

            // 距离
            return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
        }
        // 选中杆件
        public void Select(string name)
        {
            _selectedMembers.Add(name);
        }
        // 取消选中
        public void Deselect(string name)
        {
            _selectedMembers.Remove(name);
        }
        // 清空所有选择
        public void ClearSelection()
        {
            _selectedMembers.Clear();
        }
        // 拿当前选中的杆件
        public HashSet<string> GetSelected()
        {
            return _selectedMembers;
        }
        // 画拉框的虚线矩形
        public void DrawSelectionBox(Graphics g, Point start, Point end)
        {
            if (start == end) return;

            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int w = Math.Abs(start.X - end.X);
            int h = Math.Abs(start.Y - end.Y);

            using var pen = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            g.DrawRectangle(pen, x, y, w, h);
        }
        // 开始拉框
        public void StartBoxSelect(Point start)
        {
            _isBoxSelecting = true;
            _boxStart = start;
            _boxEnd = start;
        }
        // 更新拉框终点
        public void UpdateBoxSelect(Point end)
        {
            _boxEnd = end;
        }
        // 结束拉框
        public void EndBoxSelect()
        {
            _isBoxSelecting = false;
        }

        /// <summary>
        /// =============================结果显示=========================
        /// </summary>
        public void SetResults(Dictionary<string, double> utils)
        {
            _currentUtils = utils;
        }



    }
}
