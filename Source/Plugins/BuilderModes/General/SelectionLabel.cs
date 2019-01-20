namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class SelectionLabel : LineLengthLabel
	{
		// Constructor
		public SelectionLabel() : base(false, true) { }
		
		// We don't want any changes here
		protected override void UpdateText() { }
	}
}
