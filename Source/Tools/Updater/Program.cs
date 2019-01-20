using System;
using System.Reflection;
using System.Windows.Forms;

namespace mxd.GZDBUpdater
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			EmbeddedAssembly.Load("mxd.GZDBUpdater.SharpCompressStripped.dll", "SharpCompressStripped.dll");
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			MainForm form = new MainForm();
			if(!form.IsDisposed) Application.Run(form);
		}

		private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			return EmbeddedAssembly.Get(args.Name);
		}
	}
}