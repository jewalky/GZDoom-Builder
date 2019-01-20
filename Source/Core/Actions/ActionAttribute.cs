
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
using System.Reflection;

#endregion

namespace CodeImp.DoomBuilder.Actions
{
	/// <summary>
	/// This binds a method to an action.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class ActionAttribute : Attribute
	{
		#region ================== Variables

		// The action to bind to
		protected string action;
		protected bool baseaction;
		protected string library;

		#endregion

		#region ================== Properties

		/// <summary>
		/// Set to true to indicate this is a core Doom Builder action when used within a plugin.
		/// </summary>
		public bool BaseAction { get { return baseaction; } set { baseaction = value; } }

		/// <summary>
		/// Set this to the name of the plugin library when this action is defined by another plugin. The library name is the filename without extension.
		/// </summary>
		public string Library { get { return library; } set { library = value; } }

		internal string ActionName { get { return action; } }

		#endregion

		#region ================== Constructor / Disposer

		/// <summary>
		/// This binds a method to an action.
		/// </summary>
		/// <param name="action">The action name as defined in Actions.cfg resource.</param>
		public ActionAttribute(string action)
		{
			// Initialize
			this.action = action;
			this.baseaction = false;
			this.library = "";
		}

		#endregion

		#region ================== Methods

		// This makes the proper name
		public string GetFullActionName(Assembly asm)
		{
			string asmname;

			if(library.Length > 0)
				asmname = library.ToLowerInvariant();
			else if(baseaction)
				asmname = General.ThisAssembly.GetName().Name.ToLowerInvariant();
			else
				asmname = asm.GetName().Name.ToLowerInvariant();

			return asmname + "_" + action;
		}

		#endregion
	}
}
