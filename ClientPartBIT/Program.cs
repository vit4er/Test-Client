using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClientPartBIT
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//var form = new MyForm();
			AppContext myContext = new AppContext();
			using(NotifyIcon icon = new NotifyIcon())
			{
				icon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
				icon.ContextMenu = new ContextMenu(
					new[]
					{
						new MenuItem("Stop it!", (s,e) => Application.Exit())
					}
				);

				icon.Visible = true;
				Application.Run(myContext);
				icon.Visible = false;
			}
		}
	}
}
