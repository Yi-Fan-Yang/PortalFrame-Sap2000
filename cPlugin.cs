using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSiAPIv1;

namespace PortalFrame
{
    public class cPlugin:cPluginContract
    {
        private readonly string _version = "0.1";
        // SAP 先问：你是谁？这段文字会显示在插件说明里
        public int Info(ref string Text)
        {
            Text= "门式刚架构件验算插件（GB51022-2015 第7.1节）。版本 " + _version;
            return 0;
        }
        // SAP 先问：你是谁？这段文字会显示在插件说明里
        public void Main(ref cSapModel sapMode, ref cPluginCallback pluginCallback)
        {
            var form = new _4UI.MainForm();
            try
            {
                form.Connect(ref sapMode, ref pluginCallback);
                form.Show();// 非模态：SAP 主窗口保持可用
            }
            catch (Exception ex)
            {
                MessageBox.Show("插件启动失败：" + ex.Message);
                pluginCallback.Finish(1);
            }
        }
    }
}
