
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

using System;
using System.Collections.Generic;
using System.IO;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	internal sealed class DirectoryFilesList
	{
		#region ================== Constants (mxd)

		#endregion

		#region ================== Variables

		private Dictionary<string, DirectoryFileEntry> entries; //mxd
		private List<string> wadentries; //mxd
		
		#endregion

		#region ================== Properties

		public int Count { get { return entries.Count; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor to fill list from directory and optionally subdirectories
		public DirectoryFilesList(string path, bool subdirectories)
		{
			path = Path.GetFullPath(path);
			string[] files = Directory.GetFiles(path, "*", subdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			Array.Sort(files); //mxd
			entries = new Dictionary<string, DirectoryFileEntry>(files.Length, StringComparer.OrdinalIgnoreCase);
			wadentries = new List<string>();
			
			foreach(string file in files) //mxd
			{
				var e = new DirectoryFileEntry(file, path);
				if(string.Compare(e.extension, "wad", true) == 0 && e.path.Length == 0)
				{
					wadentries.Add(file);
					continue;
				}

				if(General.Map.Config.IgnoredFileExtensions.Contains(e.extension)) continue;

				bool skipfolder = false;
				foreach(string ef in General.Map.Config.IgnoredDirectoryNames)
				{
					if(e.path.StartsWith(ef + Path.DirectorySeparatorChar))
					{
						skipfolder = true;
						break;
					}
				}
				if(skipfolder) continue;

				entries.Add(e.filepathname, e);
			}
		}

		// Constructor for custom list
		public DirectoryFilesList(string resourcename, ICollection<DirectoryFileEntry> sourceentries)
		{
			entries = new Dictionary<string, DirectoryFileEntry>(sourceentries.Count, StringComparer.OrdinalIgnoreCase);
			wadentries = new List<string>();
			foreach(DirectoryFileEntry e in sourceentries)
			{
				if(string.Compare(e.extension, "wad", true) == 0 && e.path.Length == 0)
				{
					wadentries.Add(e.filepathname);
					continue;
				}

				if(General.Map.Config.IgnoredFileExtensions.Contains(e.extension)) continue;

				bool skipfolder = false;
				foreach(string ef in General.Map.Config.IgnoredDirectoryNames)
				{
					if(e.path.StartsWith(ef + Path.DirectorySeparatorChar))
					{
						skipfolder = true;
						break;
					}
				}
				if(skipfolder) continue;

				if(entries.ContainsKey(e.filepathname))
				{
					General.ErrorLogger.Add(ErrorType.Warning, "Resource \"" + resourcename + "\" contains multiple files with the same filename. See: \"" + e.filepathname + "\"");
					continue;
				}

				entries.Add(e.filepathname, e);
			}
		}

		#endregion

		#region ================== Methods

		// This checks if a given file exists
		// The given file path must not be absolute
		public bool FileExists(string filepathname)
		{
			return entries.ContainsKey(filepathname.ToLowerInvariant());
		}

		// This returns file information for the given file
		// The given file path must not be absolute
		public DirectoryFileEntry GetFileInfo(string filepathname)
		{
			return entries[filepathname.ToLowerInvariant()];
		}

		//mxd. This returns a list of all wad files (filepathname)
		public List<string> GetWadFiles()
		{
			return wadentries;
		}
		
		// This returns a list of all files (filepathname)
		public List<string> GetAllFiles()
		{
			List<string> files = new List<string>(entries.Count);
			foreach(DirectoryFileEntry e in entries.Values) files.Add(e.filepathname);
			return files;
		}

		// This returns a list of all files optionally with subdirectories included
		public List<string> GetAllFiles(bool subdirectories)
		{
			if(subdirectories) return GetAllFiles();

			List<string> files = new List<string>(entries.Count);
			foreach(DirectoryFileEntry e in entries.Values)
				if(e.path.Length == 0) files.Add(e.filepathname);
			return files;
		}

		// This returns a list of all files that are in the given path and optionally in subdirectories
		public List<string> GetAllFiles(string path, bool subdirectories)
		{
			path = CorrectPath(path);
			if(subdirectories)
			{
				List<string> files = new List<string>(entries.Count);
				foreach(DirectoryFileEntry e in entries.Values)
					if(e.path.StartsWith(path)) files.Add(e.filepathname);
				return files;
			}
			else
			{
				List<string> files = new List<string>(entries.Count);
				foreach(DirectoryFileEntry e in entries.Values)
					if(e.path == path) files.Add(e.filepathname);
				return files;
			}
		}

		// This returns a list of all files that are in the given path and subdirectories and have the given title
		public List<string> GetAllFilesWithTitle(string path, string title)
		{
			path = CorrectPath(path).ToLowerInvariant();
			title = title.ToLowerInvariant();
			List<string> files = new List<string>(entries.Count);
			foreach(DirectoryFileEntry e in entries.Values)
				if(e.path.StartsWith(path) && e.filetitle == title) files.Add(e.filepathname);
			return files;
		}

		// This returns a list of all files that are in the given path (optionally in subdirectories) and have the given title
		public List<string> GetAllFilesWithTitle(string path, string title, bool subdirectories)
		{
			if(subdirectories) return GetAllFilesWithTitle(path, title);

			path = CorrectPath(path).ToLowerInvariant();
			title = title.ToLowerInvariant();
			List<string> files = new List<string>(entries.Count);
			foreach(DirectoryFileEntry e in entries.Values)
				if(e.path == path && e.filetitle == title) files.Add(e.filepathname);
			return files;
		}

		//mxd. This returns a list of all files that are in the given path and which names starts with title
		public List<string> GetAllFilesWhichTitleStartsWith(string path, string title)
		{
			path = CorrectPath(path).ToLowerInvariant();
			title = title.ToLowerInvariant();
			List<string> files = new List<string>(entries.Count);
			foreach(DirectoryFileEntry e in entries.Values)
				if(e.path.StartsWith(path) && e.filetitle.StartsWith(title)) files.Add(e.filepathname);
			return files;
		}

		//mxd. This returns a list of all files that are in the given path and which names starts with title
		public List<string> GetAllFilesWhichTitleStartsWith(string path, string title, bool subdirectories) 
		{
			if(subdirectories) return GetAllFilesWhichTitleStartsWith(path, title);

			path = CorrectPath(path).ToLowerInvariant();
			title = title.ToLowerInvariant();
			List<string> files = new List<string>(entries.Count);
			foreach(DirectoryFileEntry e in entries.Values)
				if(e.path == path && e.filetitle.StartsWith(title)) files.Add(e.filepathname);
			return files;
		}

		// This returns a list of all files that are in the given path and subdirectories and have the given extension
		public List<string> GetAllFiles(string path, string extension)
		{
			path = CorrectPath(path).ToLowerInvariant();
			extension = extension.ToLowerInvariant();
			List<string> files = new List<string>(entries.Count);
			foreach(DirectoryFileEntry e in entries.Values)
				if(e.path.StartsWith(path) && e.extension == extension) files.Add(e.filepathname);
			return files;
		}

		// This returns a list of all files that are in the given path (optionally in subdirectories) and have the given extension
		public List<string> GetAllFiles(string path, string extension, bool subdirectories)
		{
			if(subdirectories) return GetAllFiles(path, extension);

			path = CorrectPath(path).ToLowerInvariant();
			extension = extension.ToLowerInvariant();
			List<string> files = new List<string>(entries.Count);
			foreach(DirectoryFileEntry e in entries.Values)
				if(e.path == path && e.extension == extension) files.Add(e.filepathname);
			return files;
		}

		// This finds the first file that has the specified name, regardless of file extension
		public string GetFirstFile(string title, bool subdirectories)
		{
			title = title.ToLowerInvariant();
			if(subdirectories)
			{
				foreach(DirectoryFileEntry e in entries.Values)
					if(e.filetitle == title) return e.filepathname;
			}
			else
			{
				foreach(DirectoryFileEntry e in entries.Values)
					if(e.filetitle == title && e.path.Length == 0) return e.filepathname;
			}

			return null;
		}

		// This finds the first file that has the specified name and extension
		public string GetFirstFile(string title, bool subdirectories, string extension)
		{
			title = title.ToLowerInvariant();
			extension = extension.ToLowerInvariant();
			if(subdirectories)
			{
				foreach(DirectoryFileEntry e in entries.Values)
					if(e.filetitle == title && e.extension == extension) return e.filepathname;
			}
			else
			{
				foreach(DirectoryFileEntry e in entries.Values)
					if(e.filetitle == title && e.path.Length == 0 && e.extension == extension) return e.filepathname;
			}

			return null;
		}

		// This finds the first file that has the specified name, regardless of file extension, and is in the given path
		public string GetFirstFile(string path, string title, bool subdirectories)
		{
			title = title.ToLowerInvariant();
			path = CorrectPath(path).ToLowerInvariant();
			if(subdirectories)
			{
				foreach(DirectoryFileEntry e in entries.Values)
					if((e.filetitle.Length > DataManager.CLASIC_IMAGE_NAME_LENGTH ? e.filetitle.StartsWith(title) : e.filetitle == title)
						&& e.path.StartsWith(path)) return e.filepathname;
			}
			else
			{
				foreach(DirectoryFileEntry e in entries.Values)
					if((e.filetitle.Length > DataManager.CLASIC_IMAGE_NAME_LENGTH ? e.filetitle.StartsWith(title) : e.filetitle == title) 
						&& e.path == path) return e.filepathname;
			}

			return null;
		}

		// This finds the first file that has the specified name, is in the given path and has the given extension
		public string GetFirstFile(string path, string title, bool subdirectories, string extension)
		{
			title = title.ToLowerInvariant();
			path = CorrectPath(path).ToLowerInvariant();
			extension = extension.ToLowerInvariant();
			if(subdirectories)
			{
				foreach(DirectoryFileEntry e in entries.Values)
					if(e.filetitle == title && e.path.StartsWith(path) && e.extension == extension) return e.filepathname;
			}
			else
			{
				foreach(DirectoryFileEntry e in entries.Values)
					if(e.filetitle == title && e.path == path && e.extension == extension) return e.filepathname;
			}

			return null;
		}
		
		// This fixes a path so that it ends with the proper directory separator (mxd)
		private static string CorrectPath(string path)
		{
			if(path.Length > 0)
			{
				if(path[path.Length - 1] == Path.DirectorySeparatorChar) return path;
				if(path[path.Length - 1] == Path.AltDirectorySeparatorChar)
					path = path.Substring(0, path.Length - 1);
				return path + Path.DirectorySeparatorChar;
			}

			return path;
		}

		#endregion
	}
}
