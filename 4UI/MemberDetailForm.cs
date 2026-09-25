using PortalFrame._5data;

namespace PortalFrame._4UI
{
    public class MemberDetailForm : Form
    {
        // 左栏
        private GroupBox _grpCheckType = new() { Text = "验算类型" };
        private ListBox _lstCheckType = new();

        private GroupBox _grpStation = new() { Text = "测站" };
        private ListView _lstStation = new();

        // 中栏
        private GroupBox _grpCombo = new() { Text = "荷载组合" };
        private ListView _lstCombo = new();

        // 右栏
        private GroupBox _grpSec = new() { Text = "截面信息" };
        private ListView _lstSec = new();
        private GroupBox _grpSlend = new() { Text = "长细比" };
        private ListView _lstSlend = new();
        private GroupBox _grpLocal = new() { Text = "板件宽厚比" };
        private ListView _lstLocal = new();

        // 按钮
        private Button _btnDetail = new() { Text = "显示详细文件" };
        private Button _btnBack = new() { Text = "返回" };

        public MemberDetailForm()
        {
            Text = "构件验算详情";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            SetupLayout();
            SetupEvents();
        }

        private void SetupLayout()
        {
            // 左栏上：验算类型
            _grpCheckType.Location = new Point(10, 10);
            _grpCheckType.Size = new Size(200, 200);
            _lstCheckType.Dock = DockStyle.Fill;
            _grpCheckType.Controls.Add(_lstCheckType);

            // 左栏下：测站
            _grpStation.Location = new Point(10, 220);
            _grpStation.Size = new Size(200, 300);
            _lstStation.Dock = DockStyle.Fill;
            _lstStation.View = View.Details;
            _lstStation.FullRowSelect = true;
            _lstStation.Columns.Add("测站", 80);
            _lstStation.Columns.Add("利用率", 80);
            _grpStation.Controls.Add(_lstStation);

            // 中栏：组合
            _grpCombo.Location = new Point(220, 10);
            _grpCombo.Size = new Size(350, 510);
            _lstCombo.Dock = DockStyle.Fill;
            _lstCombo.View = View.Details;
            _lstCombo.FullRowSelect = true;
            _lstCombo.Columns.Add("组合名", 250);
            _lstCombo.Columns.Add("利用率", 80);
            _grpCombo.Controls.Add(_lstCombo);

            // 右栏：静态结果
            // 右栏：三个 GroupBox
            // 1. 截面属性
            _grpSec.Location = new Point(580, 10);
            _grpSec.Size = new Size(250, 250);
            _lstSec.Dock= DockStyle.Fill;
            _lstSec.View = View.Details;
            _lstSec.FullRowSelect = true;
            _lstSec.Columns.Add("项目", 100);
            _lstSec.Columns.Add("数值", 130);
            _grpSec.Controls.Add(_lstSec);


            // 2. 长细比
            _grpSlend.Location = new Point(580, 270);
            _grpSlend.Size = new Size(250, 100);
            _lstSlend.Dock = DockStyle.Fill;
            _lstSlend.View = View.Details;
            _lstSlend.FullRowSelect = true;
            _lstSlend.Columns.Add("项目", 100);
            _lstSlend.Columns.Add("数值", 130);
            _grpSlend.Controls.Add(_lstSlend);

            // 3. 局部稳定（宽厚比）
            _grpLocal.Location = new Point(580, 380);
            _grpLocal.Size = new Size(250, 100);
            _lstLocal.Dock = DockStyle.Fill;
            _lstLocal.View = View.Details;
            _lstLocal.FullRowSelect = true;
            _lstLocal.Columns.Add("项目", 100);
            _lstLocal.Columns.Add("数值", 130);
            _grpLocal.Controls.Add(_lstLocal);

            // 按钮
            _btnDetail.Location = new Point(600, 490);
            _btnBack.Location = new Point(720, 490);

            Controls.Add(_grpCheckType);
            Controls.Add(_grpStation);
            Controls.Add(_grpCombo);
            Controls.Add(_grpSec);
            Controls.Add(_grpSlend);
            Controls.Add(_grpLocal);
            Controls.Add(_btnDetail);
            Controls.Add(_btnBack);
        }

        private void SetupEvents()
        {
            // 后面加事件
        }
    }
}
