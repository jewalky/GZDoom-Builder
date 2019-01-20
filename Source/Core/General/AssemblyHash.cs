using System;

//mxd. Attribute to store git short hash string
namespace CodeImp.DoomBuilder
{
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	public class AssemblyHashAttribute : Attribute
	{
		private String commithash;
		public String CommitHash { get { return commithash; } }

		public AssemblyHashAttribute(String commithash) 
        {
			this.commithash = commithash;
        }
	}
}
