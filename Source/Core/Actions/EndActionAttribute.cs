
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

namespace CodeImp.DoomBuilder.Actions
{
	/// <summary>
	/// This binds a method to an action which is then called when the action is stopped.
	/// </summary>
	public sealed class EndActionAttribute : ActionAttribute
	{
		/// <summary>
		/// This binds a method to an action which is then called when the action is stopped.
		/// </summary>
		/// <param name="action">The action name as defined in Actions.cfg resource.</param>
		public EndActionAttribute(string action) : base(action)
		{
		}
	}
}
