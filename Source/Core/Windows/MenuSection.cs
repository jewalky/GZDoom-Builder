
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

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	public enum MenuSection
	{
		FileNewOpenClose,
		FileSave,
		FileImport, //mxd
		FileExport, //mxd
		FileRecent,
		FileExit,
		EditUndoRedo,
		EditCopyPaste,
		EditGeometry,
		EditGrid,
		EditMapOptions,
		ViewThings,
		ViewViews,
		ViewHelpers, //mxd
		ViewRendering, //mxd
		ViewZoom,
		ViewScriptEdit,
		PrefabsInsert,
		PrefabsCreate,
		ToolsResources,
		ToolsConfiguration,
		ToolsTesting,
		HelpManual,
		HelpAbout,
		Top
	}
}
