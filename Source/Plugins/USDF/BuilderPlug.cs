
#region ================== Copyright (c) 2010 Pascal vd Heiden

/*
 * Copyright (c) 2010 Pascal vd Heiden
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.USDF
{
	public class BuilderPlug : Plug
	{
		#region ================== Variables

		// Static instance
		private static BuilderPlug me;
		
		// Tools form
		private ToolsForm toolsform;
		
		// Main form
		private MainForm mainform;
		
		#endregion
		
		#region ================== Properties

		// Static property to access the BuilderPlug
		public static BuilderPlug Me { get { return me; } }
		
		// Is the editor opened?
		public bool EditorOpen { get { return (mainform != null) && !mainform.IsDisposed; } }
		
		#endregion
		
		#region ================== Methods

		// This loads what is needed to support USDF
		private void Load()
		{
			// Check if the map format has a DIALOGUE lump we can edit
			bool editlump = false;
			foreach(KeyValuePair<string, MapLumpInfo> lump in General.Map.Config.MapLumps)
			{
				if(lump.Key.Trim().ToUpperInvariant() == "DIALOGUE")
					editlump = true;
			}
			
			if(editlump)
			{
				// Load tools (this adds our button to the toolbar)
				if(toolsform == null)
					toolsform = new ToolsForm();
			}
		}
		
		// This unloads everything
		private void Unload()
		{
			if(mainform != null)
				mainform.Dispose();
			
			if(toolsform != null)
				toolsform.Dispose();
			
			toolsform = null;
		}
		
		#endregion
		
		#region ================== Events

		// This event is called when the plugin is initialized
		public override void OnInitialize()
		{
			base.OnInitialize();

			// Keep a static reference
            me = this;

			General.Actions.BindMethods(this);
		}
		
		// This is called when the plugin is terminated
		public override void Dispose()
		{
			General.Actions.UnbindMethods(this);
			Unload();
			base.Dispose();
        }

		// New map started
		public override void OnMapNewEnd()
		{
			base.OnMapNewEnd();
			Load();
		}

		// Map opened
		public override void OnMapOpenEnd()
		{
			base.OnMapOpenEnd();
			Load();
		}
		
		// Map is being saved
		public override void OnMapSaveBegin(SavePurpose purpose)
		{
			base.OnMapSaveBegin(purpose);
			if(this.EditorOpen)
				mainform.SaveData();
		}
		
		// Map closed
		public override void OnMapCloseEnd()
		{
			base.OnMapCloseEnd();
			Unload();
		}

		// Map config changed
		public override void OnMapReconfigure()
		{
			base.OnMapReconfigure();
			Unload();
			Load();
		}
		
		#endregion
		
		#region ================== Actions
		
		[BeginAction("opendialogeditor")]
		public void OpenConversationEditor()
		{
			if(!this.EditorOpen)
			{
				mainform = new MainForm();
				
				if(General.Settings.ScriptOnTop)
					mainform.Show(General.Interface);
				else
					mainform.Show();
			}
			else
			{
				mainform.Activate();
			}
		}
		
		#endregion
	}
}
