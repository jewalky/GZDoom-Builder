
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
using SlimDX.Direct3D9;
using SlimDX;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	internal sealed class Display2DShader : D3DShader
	{
		#region ================== Variables

		// Property handlers
		private readonly EffectHandle texture1;
		private readonly EffectHandle rendersettings;
		private readonly EffectHandle transformsettings;
		private readonly EffectHandle filtersettings;
        private readonly EffectHandle desaturationHandle;

        #endregion

        #region ================== Properties

        public Texture Texture1 { set { effect.SetTexture(texture1, value); settingschanged = true; } }

        // [ZZ]
        private float desaturation;
        public float Desaturation
        {
            set
            {
                if (desaturation != value)
                {
                    effect.SetValue(desaturationHandle, value);
                    desaturation = value;
                    settingschanged = true;
                }
            }
        }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        public Display2DShader(ShaderManager manager) : base(manager)
		{
			// Load effect from file
			effect = LoadEffect("display2d.fx");

			// Get the property handlers from effect
			if(effect != null)
			{
				texture1 = effect.GetParameter(null, "texture1");
				rendersettings = effect.GetParameter(null, "rendersettings");
				transformsettings = effect.GetParameter(null, "transformsettings");
				filtersettings = effect.GetParameter(null, "filtersettings");
                desaturationHandle = effect.GetParameter(null, "desaturation"); // [ZZ]
            }
			
			// Initialize world vertex declaration
			VertexElement[] elements = new[]
			{
				new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
				new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
				new VertexElement(0, 16, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
				VertexElement.VertexDeclarationEnd
			};
			vertexdecl = new VertexDeclaration(General.Map.Graphics.Device, elements);

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				if(texture1 != null) texture1.Dispose();
				if(rendersettings != null) rendersettings.Dispose();
				if(transformsettings != null) transformsettings.Dispose();
				if(filtersettings != null) filtersettings.Dispose();
                if(desaturationHandle != null) desaturationHandle.Dispose();
				
				// Done
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		// This sets the settings
		public void SetSettings(float texelx, float texely, float fsaafactor, float alpha, bool bilinear)
		{
			Vector4 values = new Vector4(texelx, texely, fsaafactor, alpha);
			effect.SetValue(rendersettings, values);
			Matrix world = manager.D3DDevice.Device.GetTransform(TransformState.World);
			Matrix view = manager.D3DDevice.Device.GetTransform(TransformState.View);
			effect.SetValue(transformsettings, world * view);
			TextureFilter filter = (bilinear ? TextureFilter.Linear : TextureFilter.Point);
			effect.SetValue(filtersettings, (int)filter);
			settingschanged = true; //mxd
		}
		
		#endregion
	}
}
