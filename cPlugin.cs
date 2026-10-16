using CSiAPIv1;

namespace PortalFrame
{
    public class cPlugin : cPluginContract
    {
        private static string _versionString = "0.1";

        public int Info(ref string Text)
        {
            Text = "门式刚架构件验算插件。版本 " + _versionString;
            return 0;
        }

        public void Main(ref cSapModel sapMode, ref cPluginCallback pluginCallback)
        {
            var form = new _4UI.MainForm();
            try
            {
                form.Connect(ref sapMode, ref pluginCallback);
                form.Show();

                // 加这行测试，马上调用 Finish
                pluginCallback.Finish(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("插件启动失败：" + ex.Message);
                pluginCallback.Finish(1);
            }
        }

    }
}
