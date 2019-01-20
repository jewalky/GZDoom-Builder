using System;
using System.Diagnostics;
using System.IO;

namespace mxd.VersionFromEXE
{
	class Program
	{
		static int Main(string[] args)
		{
			if(args.Length != 2 || !File.Exists(args[0]))
			{
				Console.WriteLine("Creates a bath file, which sets EXEREVISIONNUMBER environment variable to the exe revision number.");
				Console.WriteLine("Usage: VersionFromEXE.exe other.exe output.bat");
				return 1;
			}

			int rev = FileVersionInfo.GetVersionInfo(args[0]).ProductPrivatePart;
			File.AppendAllText(args[1], "SET EXEREVISIONNUMBER=" + rev + "\n");
			return 0;
		}
	}
}
