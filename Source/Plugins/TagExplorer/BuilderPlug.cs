using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Plugins;

namespace CodeImp.DoomBuilder.TagExplorer
{
	public sealed class BuilderPlug : Plug
	{
		private static BuilderPlug me;

		// Docker
		private TagExplorer tagExplorer;
		private Docker docker;

		// Static property to access the BuilderPlug
		public static BuilderPlug Me { get { return me; } }

		// This event is called when the plugin is initialized
		public override void OnInitialize() 
		{
			base.OnInitialize();

			// Keep a static reference
			me = this;
		}

		// When a map is created
		public override void OnMapNewEnd() 
		{
			OnMapOpenEnd();
		}

		// This is called after a map has been successfully opened
		public override void OnMapOpenEnd() 
		{
			if(tagExplorer == null) 
			{
				tagExplorer = new TagExplorer();
				docker = new Docker("tagexplorerdockerpanel", "Tag Explorer", tagExplorer);
				General.Interface.AddDocker(docker);
				tagExplorer.Setup();
			}
		}

		// This is called after a map has been closed
		public override void OnMapCloseBegin() 
		{
			// If we have a Tag Explorer panel, remove it
			if(tagExplorer != null) 
			{
				tagExplorer.Terminate();
				General.Interface.RemoveDocker(docker);
				docker = null;
				tagExplorer.Dispose();
				tagExplorer = null;
			}
		}

		// Geometry pasted
		public override void OnPasteEnd(PasteOptions options) 
		{
			if(tagExplorer != null) tagExplorer.UpdateTreeSoon();
		}

		// Undo performed
		public override void OnUndoEnd() 
		{
			if(tagExplorer != null) tagExplorer.UpdateTreeSoon();
		}

		// Redo performed
		public override void OnRedoEnd() 
		{
			if(tagExplorer != null) tagExplorer.UpdateTreeSoon();
		}

		// Edit performed
		public override void OnEditAccept()
		{
			if(tagExplorer != null) tagExplorer.UpdateTreeSoon();
		}

		public override void OnActionEnd(Actions.Action action) 
		{
			if(tagExplorer != null && action.Name == "builder_deleteitem")
				tagExplorer.UpdateTreeSoon();
		}
	}
}
