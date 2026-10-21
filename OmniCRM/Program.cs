using OmniCRM.Forms;

namespace OmniCRM;

static class Program
{
    /// <summary>
    /// The main entry point for the OmniCRM desktop application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }    
}