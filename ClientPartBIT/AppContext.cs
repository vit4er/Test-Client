using System.Windows.Forms;

namespace ClientPartBIT
{
	class AppContext: ApplicationContext
	{
		public MyForm form;
		public AppContext()
		{
			form = new MyForm();
		}
	}
}
