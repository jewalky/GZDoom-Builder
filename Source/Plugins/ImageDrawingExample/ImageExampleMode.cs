
#region ================== Copyright (c) 2009 Pascal vd Heiden

/*
 * Copyright (c) 2009 Pascal vd Heiden, www.codeimp.com
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
using System.Drawing;
using System.Globalization;
using System.Text;
using System.IO;
using System.Reflection;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Statistics
{
	//
	// This class defines an classic editing mode. You can also inherit from VisualMode for a
	// visual editing mode or inherit from EditMode if you feel creative. The ClassicMode
	// provides basic 2D editing mode features such as zooming in/out and panning the view as
	// well as a bunch of usefull events.
	//
	
	//
	// This EditMode attribute allows you to link the editing mode into Doom Builder. The core
	// only sees your editing mode with this attribute and uses the settings to register it.
	//
	// DisplayName: This is the name you want to display to the user for this mode.
	//
	// SwitchAction: This is the action (see Actions.cfg) that engages this mode.
	//
	// ButtonImage: If set, creates a button on the toolbar for this mode and uses this image.
	//              Note that the image is included in this project as "Embedded Resource".
	//
	// ButtonOrder: Ordering number for the position of the button on the toolbar.
	//
	// ButtonGroup: Group in which to put the button on the toolbar.
	//              (Groups are buttons that are grouped together between separators)
	//
	// UseByDefault: THIS OPTION MAY BE INTRUSIVE TO THE USER, USE WITH GREAT CARE!
	//               Set this to enable this editing mode for use in all game configurations
	//               by default. This only applies the first time a plugin is loaded and can
	//               still be changed by the user. But it may not be desired and may conflict
	//               with other editing modes.
	//
	// Volatile: Set this to make this mode a volatile mode. When volatile, the mode is
	//           automatically cancelled when certain major actions are performed (such as
	//           when the map is saved)
	//

	[EditMode(DisplayName = "Image Example",
			  SwitchAction = "imageexamplemode",
			  ButtonImage = "ImageIcon.png",
			  ButtonOrder = 300,
			  ButtonGroup = "002_tools",
			  UseByDefault = true)]
	
	public class ImageExampleMode : ClassicMode
	{
		// This is the image we want to display. We keep it here, because we don't want to load/unload
		// it all the time when it needs to be drawn. You may even want to consider placing this in a
		// class of program scope such as the Plug and load the image only once when the plugin is loaded.
		private ImageData exampleimage;

		//
		// Order in which events occur when switching editing modes:
		// 
		// - Constructor of new mode is called
		// - OnDisengage() of old mode is called
		// ----- Mode switches -----
		// - OnEngage() of new mode is called
		// - Dispose() of old mode is called
		//
		// This function is called when this editing mode is engaged
		public override void OnEngage()
		{
			base.OnEngage();
			
			// Here we load our image. The image is loaded from resource that is embedded
			// in this project. This is done by simply adding the PNG file to the project
			// and set the Build Action property of the image to "Embedded Resource".
			// Embedded means that the image will be compiled into your plugin .DLL file so
			// you don't have to distribute this image separately.
			exampleimage = new ResourceImage("CodeImp.DoomBuilder.Plugins.ImageDrawingExample.exampleimage.png");

			// The image is not always directly loaded. Call this to ensure that the image is loaded immediately.
			exampleimage.LoadImage();
			if(exampleimage.LoadFailed) throw new Exception("Unable to load the exampleimage.png resource!");

			// After loading, the image is usable by the GDI (GetBitmap() function) but we want to use
			// it for rendering in the working area. We must call CreateTexture to tranfer the image to
			// the vido memory as texture.
			exampleimage.CreateTexture();
			
			// This tells the renderer how to display the map.
			// The renderer works with several different layers, each with its own purpose
			// and features. A "presentation" defines how to combine these layers when
			// presented to the user on the display. Here I make a special presentation
			// that includes only the layer we need: the Overlay layer.
			CustomPresentation p = new CustomPresentation();
			p.AddLayer(new PresentLayer(RendererLayer.Overlay, BlendingMode.None, 1.0f, false));
			renderer.SetPresentation(p);
		}
		
		// This function is called when this editing mode is disengaged
		public override void OnDisengage()
		{
			// We no longer need the image, make sure it is removed from memory!
			exampleimage.Dispose();
			
			base.OnDisengage();
		}

		// Event called when the user (or the core) wants to cancel this editing mode
		public override void OnCancel()
		{
			base.OnCancel();

			// Return to previous stable mode
			General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
		}
		
		// MANDATORY: You must override this event and handle it to draw the map
		// In this example, we're not going to draw the map at all, but the example image instead.
		public override void OnRedrawDisplay()
		{
			base.OnRedrawDisplay();
			
			// We don't have to clear the other layers or anything, because they are not used
			// anyway (see how the presentation above is configured to show only the Overlay layer)

			// Clear the overlay and begin rendering to it.
			if(renderer.StartOverlay(true))
			{
				// Rectangle of coordinates where to draw the image.
				// We use untranslated coordinates: this means the coordinates here
				// are already in screen space.
				RectangleF r = new RectangleF(20.0f, 20.0f, 428.0f, 332.0f);
				
				// Show the picture!
				renderer.RenderRectangleFilled(r, PixelColor.FromColor(Color.White), false, exampleimage);
				
				// Finish our rendering to this layer.
				renderer.Finish();
			}
			
			// We now present it to the user on the display,
			// with the previously defined presentation settings.
			renderer.Present();
		}
	}
}
