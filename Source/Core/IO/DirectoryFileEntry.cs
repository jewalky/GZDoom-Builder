
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System.IO;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	internal struct DirectoryFileEntry
	{
										// Example for:   C:\WADs\Foo\Bar.WAD
										// Created from:  C:\WADs
		// Members
		public string filename;			// bar.wad
		public string filetitle;		// bar
		public string extension;		// wad
		public string path;				// foo\
		public string filepathname;		// Foo\Bar.WAD
		public string filepathtitle;	// Foo\Bar

		// Constructor
		public DirectoryFileEntry(string fullname, string frompath)
		{
			// Get the information we need
			filename = Path.GetFileName(fullname);
			filetitle = Path.GetFileNameWithoutExtension(fullname);
			extension = Path.GetExtension(fullname);
			if(extension.Length > 1)
				extension = extension.Substring(1);
			else
				extension = "";
			path = Path.GetDirectoryName(fullname);
			if(path.Length > (frompath.Length + 1))
				path = path.Substring(frompath.Length + 1) + Path.DirectorySeparatorChar;
			else
				path = "";
			filepathname = Path.Combine(path, filename);
			filepathtitle = Path.Combine(path, filetitle);

			// Make some lowercase
			filename = filename.ToLowerInvariant();
			filetitle = filetitle.ToLowerInvariant();
			extension = extension.ToLowerInvariant();
			path = path.ToLowerInvariant();
		}

		// Constructor
		public DirectoryFileEntry(string fullname)
		{
			// Get the information we need
			filename = Path.GetFileName(fullname);
			filetitle = Path.GetFileNameWithoutExtension(fullname);
			extension = Path.GetExtension(fullname);
			if(extension.Length > 1)
				extension = extension.Substring(1);
			else
				extension = "";
			path = Path.GetDirectoryName(fullname);
			if(!string.IsNullOrEmpty(path)) path += Path.DirectorySeparatorChar; //mxd
			filepathname = Path.Combine(path, filename);
			filepathtitle = Path.Combine(path, filetitle);

			// Make some lowercase
			filename = filename.ToLowerInvariant();
			filetitle = filetitle.ToLowerInvariant();
			extension = extension.ToLowerInvariant();
			path = path.ToLowerInvariant();
		}
	}
}
