namespace CodeImp.DoomBuilder.Rendering
{
	public struct CommentType
	{
		private const string REGULAR = "";
		private const string INFO = "[i]";
		private const string QUESTION = "[?]";
		private const string PROBLEM = "[!]";
		private const string SMILE = "[:]";

		public static readonly string[] Types = new[] { REGULAR, INFO, QUESTION, PROBLEM, SMILE };
	}
}
