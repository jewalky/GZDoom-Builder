
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

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	internal class ShaderManager : ID3DResource, IDisposable
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Settings
		private readonly string shadertechnique;
		
		// Shaders
		private Display2DShader display2dshader;
		private Things2DShader things2dshader;
		private World3DShader world3dshader;
		
		// Device
		private D3DDevice device;
		
		// Disposing
		private bool isdisposed;

		#endregion

		#region ================== Properties

		public string ShaderTechnique { get { return shadertechnique; } }
		public Display2DShader Display2D { get { return display2dshader; } }
		public Things2DShader Things2D { get { return things2dshader; } }
		public World3DShader World3D { get { return world3dshader; } }
		public bool IsDisposed { get { return isdisposed; } }
		internal D3DDevice D3DDevice { get { return device; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public ShaderManager(D3DDevice device)
		{
			// Initialize
			this.device = device;
			shadertechnique = "SM20"; //mxd
			
			// Load
			ReloadResource();

			// Register as resource
			device.RegisterResource(this);
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				UnloadResource();

				// Unregister as resource
				device.UnregisterResource(this);
				
				// Done
				device = null;
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Resources

		// Clean up resources
		public void UnloadResource()
		{
			display2dshader.Dispose();
			things2dshader.Dispose();
			world3dshader.Dispose();
		}

		// Load resources
		public void ReloadResource()
		{
			// Initialize effects
			display2dshader = new Display2DShader(this);
			things2dshader = new Things2DShader(this);
			world3dshader = new World3DShader(this);
		}
		
		#endregion
	}
}
