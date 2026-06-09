namespace QuickTxt;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, name: "QuickTxt.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "QuickTxt 已经在后台运行。请在系统托盘中找到 QuickTxt 图标。",
                "QuickTxt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new QuickTxtApplicationContext());
    }
}
