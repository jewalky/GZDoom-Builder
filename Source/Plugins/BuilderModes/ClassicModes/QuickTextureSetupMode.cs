
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

using System.Windows.Forms;
using CodeImp.DoomBuilder.BuilderModes;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Editing
{

    [EditMode(DisplayName = "Quick Texture Setup",
          SwitchAction = "quicktexturesetup",
          ButtonImage = "QuickTextures.png",
          ButtonOrder = 101,
          ButtonGroup = "002_tools",
          AllowCopyPaste = false,
          Volatile = true,
          UseByDefault = true)]

    public class QuickTextureSetupMode : BaseClassicMode
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        #endregion

        #region ================== Properties

        internal bool Volatile { get { return attributes.Volatile; } set { attributes.Volatile = value; } }

        #endregion

        #region ================== Events

        public override void OnHelp()
        {
            General.ShowHelp("e_quicktextures.html");
        }

        // Cancelled
        public override void OnCancel()
        {
            // Cancel base class
            base.OnCancel();

            // Return to base mode
            General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
        }

        // Mode engages
        public override void OnEngage()
        {
            base.OnEngage();
            renderer.SetPresentation(Presentation.Standard);
            General.Map.Map.SelectionType = SelectionType.All;

            // Show toolbox window
            BuilderPlug.Me.QuickTextureForm.Show((Form)General.Interface);
        }

        // Disenagaging
        public override void OnDisengage()
        {
            base.OnDisengage();

            // Hide object info
            General.Interface.HideInfo();

            // Hide toolbox window
            BuilderPlug.Me.QuickTextureForm.Hide();
        }

        // This applies the curves and returns to the base mode
        public override void OnAccept()
        {
            // Snap to map format accuracy
            General.Map.Map.SnapAllToAccuracy();

            // Update caches
            General.Map.Map.Update();
            General.Map.IsChanged = true;

            // Return to base mode
            General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
        }

        // Redrawing display
        public override void OnRedrawDisplay()
        {
            // Get the selection
            FindReplaceObject[] selection = BuilderPlug.Me.FindReplaceForm.GetSelection();

            renderer.RedrawSurface();

            // Render lines
            if (renderer.StartPlotter(true))
            {
                renderer.PlotLinedefSet(General.Map.Map.Linedefs);
                if (BuilderPlug.Me.FindReplaceForm.Finder != null)
                    BuilderPlug.Me.FindReplaceForm.Finder.PlotSelection(renderer, selection);
                renderer.PlotVerticesSet(General.Map.Map.Vertices);
                renderer.Finish();
            }

            // Render things
            if (renderer.StartThings(true))
            {
                renderer.RenderThingSet(General.Map.Map.Things, General.Settings.ActiveThingsAlpha);
                if (BuilderPlug.Me.FindReplaceForm.Finder != null)
                    BuilderPlug.Me.FindReplaceForm.Finder.RenderThingsSelection(renderer, selection);
                renderer.Finish();
            }

            // Render overlay
            if (renderer.StartOverlay(true))
            {
                if (BuilderPlug.Me.FindReplaceForm.Finder != null)
                    BuilderPlug.Me.FindReplaceForm.Finder.RenderOverlaySelection(renderer, selection);
                renderer.Finish();
            }

            renderer.Present();
        }

        #endregion
	}
}
