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
        // 旋转角度（弧度）
        private double _angleX = 0;   // 俯仰角：上下转
        private double _angleY = 0;   // 方位角：左右转
        private double _zoom = 1.0;   // 用户缩放系数，1.0 是默认大小
        private double _panX = 0;   // 水平平移（像素）
        private double _panY = 0;   // 竖直平移（像素）
        private float _currentPad;        // 当前边距
        private double _currentS;         // 当前缩放系数
        private int _currentCanvasHeight;  // 当前画布高度



        // 更新数据（每次读完模型调一次）
        public void SetData(Dictionary<string, JointData> joints, Dictionary<string, FrameData> frames)
        {
            _joints = joints;
            _frames = frames;
        }

        // 画到指定画布上,g 是画笔，canvasWidth/canvasHeight 是画布大小（像素）
        public void Draw(Graphics g, int canvasWidth, int canvasHeight)
        {
            if (_joints.Count == 0) return;
            // ① 找模型的范围：最左、最右、最低、最高
            double minX = _joints.Values.Min(j => j.X);
            double maxX = _joints.Values.Max(j => j.X);
            double minZ = _joints.Values.Min(j => j.Z);
            double maxZ = _joints.Values.Max(j => j.Z);

            float pad = 30;   // 四周留 30 像素边距，防止线贴边
            float cw = canvasWidth - 2 * pad;   // 画布可用宽度
            float ch = canvasHeight - 2 * pad; // 画布可用高度

            double sx = (maxX > minX) ? cw / (maxX - minX) : 1;   // 横向缩放
            double sz = (maxZ > minZ) ? ch / (maxZ - minZ) : 1;   // 纵向缩放
            double s = Math.Min(sx, sz)*_zoom;                    // 取小的那个
            _currentPad = pad;
            _currentS = s;
            _currentCanvasHeight = canvasHeight;


            PointF ToScreen(JointData j)//坐标变换
            {
                // ① 先绕 Y 轴左右转（方位角）
                double x1 = j.X * Math.Cos(_angleY) + j.Z * Math.Sin(_angleY);
                double z1 = -j.X * Math.Sin(_angleY) + j.Z * Math.Cos(_angleY);

                // ② 再绕 X 轴上下转（俯仰角）
                double y2 = j.Y * Math.Cos(_angleX) - z1 * Math.Sin(_angleX);
                double z2 = j.Y * Math.Sin(_angleX) + z1 * Math.Cos(_angleX);

                // ③ 投影到屏幕：用旋转后的 x 和 y
                float sx = (float)(pad + (x1 - minX) * s + _panX);
                float sy = (float)(canvasHeight - pad - (y2 - minZ) * s + _panY);

                return new PointF(sx, sy);
            }

            using(var pen =new Pen(Color.Black,2))
            {
                foreach (var f in _frames.Values)
                {
                    if(_joints.TryGetValue(f.StartJoint,out var j1) &&
                        _joints.TryGetValue(f.EndJoint,out var j2))
                    {
                        g.DrawLine(pen, ToScreen(j1), ToScreen(j2));
                    }
                }
            }

            using (var brush = new SolidBrush(Color.Red))
            {
                foreach (var j in _joints.Values)
                {
                    var p = ToScreen(j);
                    g.FillEllipse(brush, p.X - 3, p.Y - 3, 6, 6);
                }
            }
        }

        // 鼠标拖动时调：左右转 dx、上下转 dy（像素）
        public void Rotate(double dx, double dy)
        {
            // 每 100 像素转 1 弧度（手感可以调）
            _angleY += dx / 100.0;   // 左右拖 → 方位角
            _angleX += dy / 100.0;   // 上下拖 → 俯仰角
        }

        public void Zoom(double delta, double mouseX, double mouseY)
        {
            double oldZoom = _zoom;
            _zoom *= (delta > 0) ? 1.1 : 0.9;
            if (_zoom < 0.1) _zoom = 0.1;
            if (_zoom > 10) _zoom = 10;
            double ratio = _zoom / oldZoom;

            // 关键：缩放的同时微调平移，把鼠标指着的结构点拽回鼠标位置
            _panX = mouseX - _currentPad - (mouseX - _currentPad - _panX) * ratio;
            // y 方向有翻转（屏幕上方=结构下方），公式符号和 x 不同
            _panY = mouseY - _currentCanvasHeight + _currentPad
                    + (_currentCanvasHeight - _currentPad - mouseY + _panY) * ratio;

        }
        public void Pan(double dx, double dy)
        {
            _panX += dx;
            _panY += dy;
        }


    }
}
