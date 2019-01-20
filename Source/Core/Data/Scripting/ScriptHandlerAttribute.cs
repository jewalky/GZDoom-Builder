#region ================== Namespaces

using System;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Data.Scripting
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public sealed class ScriptHandlerAttribute : Attribute
	{
		#region ================== Variables

		private Type type;
		private ScriptType scripttype;
		
		#endregion

		#region ================== Properties

		public Type Type { get { return type; } set { type = value; } }
		public ScriptType ScriptType { get { return scripttype; } }
		
		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ScriptHandlerAttribute(ScriptType scripttype)
		{
			// Initialize
			this.scripttype = scripttype;
		}

		#endregion
	}
}
