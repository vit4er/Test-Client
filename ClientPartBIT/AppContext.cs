using System.Windows.Forms;

namespace ClientPartBIT
{
	class AppContext: ApplicationContext
	{
		public AppContext()
		{
			new Receiver().RunMonitor();
		}
	}
}
