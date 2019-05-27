#region ================== Namespaces

using System;
using System.Drawing;
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.GZBuilder.Data 
{
	public class EngineInfo : IDisposable
	{
		#region ================== Constants

		public const string DEFAULT_ENGINE_NAME = "Engine with no name";

		#endregion

		#region ================== Variables

		// Settings
		private string testprogramname;
		private string testprogram;
		private Bitmap icon;
		public string TestParameters;
		public bool CustomParameters;
		public int TestSkill;
		public bool TestShortPaths;
		public bool TestLinuxPaths;

		// Disposing
		private bool isdisposed;

		#endregion

		#region ================== Properties

		public string TestProgramName { get { return testprogramname; } set { testprogramname = value; CheckProgramName(); } }
		public string TestProgram { get { return testprogram; } set { testprogram = value; CheckProgramName(); } }
		public Bitmap TestProgramIcon { get { if(icon == null) UpdateIcon(); return icon; } }

		#endregion

		#region ================== Constructors / Disposer

		public EngineInfo()
		{
			testprogramname = DEFAULT_ENGINE_NAME;
		}

		public EngineInfo(EngineInfo other)
		{
			testprogramname = other.TestProgramName;
			testprogram = other.testprogram;
			TestParameters = other.TestParameters;
			CustomParameters = other.CustomParameters;
			TestSkill = other.TestSkill;
			TestShortPaths = other.TestShortPaths;
			TestLinuxPaths = other.TestLinuxPaths;

			UpdateIcon();
		}

		public void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				icon.Dispose();

				// Done
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Methods

		private void CheckProgramName() 
		{
			if(testprogramname == DEFAULT_ENGINE_NAME && !String.IsNullOrEmpty(testprogram)) 
			{
				// Get engine name from path
				testprogramname = Path.GetFileNameWithoutExtension(testprogram);
			}
		}

		private void UpdateIcon()
		{
			if(icon != null)
			{
				icon.Dispose();
				icon = null;
			}

			if(File.Exists(testprogram))
			{
				Icon i = Icon.ExtractAssociatedIcon(testprogram);
				icon = new Bitmap(i != null ? i.ToBitmap() : Properties.Resources.Question);
			}
			else
			{
				icon = new Bitmap(Properties.Resources.Warning);
			}
		}

		#endregion
	}
}
