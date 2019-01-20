
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Geometry;
using SlimDX;
using SlimDX.Direct3D9;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	internal class D3DDevice : IDisposable
	{
		#region ================== Constants

		// NVPerfHUD device name
		private const string NVPERFHUD_ADAPTER = "NVPerfHUD";

		//mxd. Anisotropic filtering steps
		public static readonly List<float> AF_STEPS = new List<float> { 1.0f, 2.0f, 4.0f, 8.0f, 16.0f }; 
		
		//mxd. Antialiasing steps
		public static readonly List<int> AA_STEPS = new List<int> { 0, 2, 4, 8 };

		#endregion

		#region ================== Variables

		// Settings
		private int adapter;
		private Filter postfilter;
		private Filter mipgeneratefilter;
		private static bool isrendering; //mxd
		
		// Main objects
		private static Direct3D d3d;
		private RenderTargetControl rendertarget;
		private Capabilities devicecaps;
		private Device device;
		private Viewport viewport;
		private readonly HashSet<ID3DResource> resources;
		private ShaderManager shaders;
		private Surface backbuffer;
		private Surface depthbuffer;
		
		// Disposing
		private bool isdisposed;

		#endregion

		#region ================== Properties

		internal Device Device { get { return device; } }
		public bool IsDisposed { get { return isdisposed; } }
		public static bool IsRendering { get { return isrendering; } } //mxd
		internal RenderTargetControl RenderTarget { get { return rendertarget; } }
		internal Viewport Viewport { get { return viewport; } }
		internal ShaderManager Shaders { get { return shaders; } }
		internal Surface BackBuffer { get { return backbuffer; } }
		internal Surface DepthBuffer { get { return depthbuffer; } }
		internal Filter PostFilter { get { return postfilter; } }
		internal Filter MipGenerateFilter { get { return mipgeneratefilter; } }
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal D3DDevice(RenderTargetControl rendertarget)
		{
			// Set render target
			this.rendertarget = rendertarget;

			// Create resources list
			resources = new HashSet<ID3DResource>();
			
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
				foreach(ID3DResource res in resources) res.UnloadResource();
				if(shaders != null) shaders.Dispose();
				rendertarget = null;
				if(backbuffer != null) backbuffer.Dispose();
				if(depthbuffer != null) depthbuffer.Dispose();
				if(device != null) device.Dispose();

				if(ObjectTable.Objects.Count > 1) //mxd. Direct3D itself is not disposed while the editor is running
				{
					//mxd. Get rid of any remaining D3D objects...
					foreach(ComObject o in ObjectTable.Objects) 
					{
						if(o is Direct3D) continue; // Don't dispose the device itself...
						General.WriteLogLine("WARNING: D3D resource " + o
							+ (o.Tag != null ? " (" + o.Tag + ")" : string.Empty) + " was not disposed properly!"
							+ (o.CreationSource != null ? " Stack trace: " + o.CreationSource : string.Empty));
						o.Dispose();
					}

#if DEBUG
					General.ShowWarningMessage("Some D3D resources were not disposed properly! See the event log for more details.",
					                           MessageBoxButtons.OK);
#endif
				}
				
				// Done
				isrendering = false; //mxd
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Renderstates

		// This completes initialization after the device has started or has been reset
		public void SetupSettings()
		{
			// Setup renderstates
			device.SetRenderState(RenderState.AlphaBlendEnable, false);
			device.SetRenderState(RenderState.AlphaBlendEnable, false);
			device.SetRenderState(RenderState.AlphaFunc, Compare.GreaterEqual);
			device.SetRenderState(RenderState.AlphaRef, 0x0000007E);
			device.SetRenderState(RenderState.AlphaTestEnable, false);
			device.SetRenderState(RenderState.Ambient, Color.White.ToArgb());
			device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Material);
			device.SetRenderState(RenderState.AntialiasedLineEnable, false);
			device.SetRenderState(RenderState.Clipping, true);
			device.SetRenderState(RenderState.ColorVertex, false);
			device.SetRenderState(RenderState.ColorWriteEnable, ColorWriteEnable.Red | ColorWriteEnable.Green | ColorWriteEnable.Blue | ColorWriteEnable.Alpha);
			device.SetRenderState(RenderState.CullMode, Cull.None);
			device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
			device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Color1);
			//device.SetRenderState(RenderState.DitherEnable, true);
			device.SetRenderState(RenderState.FillMode, FillMode.Solid);
			device.SetRenderState(RenderState.FogEnable, false);
			device.SetRenderState(RenderState.FogTableMode, FogMode.Linear);
			device.SetRenderState(RenderState.Lighting, false);
			device.SetRenderState(RenderState.LocalViewer, false);
			device.SetRenderState(RenderState.MultisampleAntialias, (General.Settings.AntiAliasingSamples > 0)); //mxd
			device.SetRenderState(RenderState.NormalizeNormals, false);
			device.SetRenderState(RenderState.PointSpriteEnable, false);
			device.SetRenderState(RenderState.RangeFogEnable, false);
			device.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);
			device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
			device.SetRenderState(RenderState.SpecularEnable, false);
			device.SetRenderState(RenderState.StencilEnable, false);
			device.SetRenderState(RenderState.TextureFactor, -1);
			device.SetRenderState(RenderState.ZEnable, false);
			device.SetRenderState(RenderState.ZWriteEnable, false);
			device.PixelShader = null;
			device.VertexShader = null;

			// Matrices
			device.SetTransform(TransformState.World, Matrix.Identity);
			device.SetTransform(TransformState.View, Matrix.Identity);
			device.SetTransform(TransformState.Projection, Matrix.Identity);
			
			// Texture addressing
			device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
			device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
			device.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Wrap);
			
			// Setup material
			device.Material = new Material {
				Ambient = new Color4(Color.White),
				Diffuse = new Color4(Color.White),
				Specular = new Color4(Color.White)
			};
			
			// Shader settings
			shaders.World3D.SetConstants(General.Settings.VisualBilinear, Math.Min(devicecaps.MaxAnisotropy, General.Settings.FilterAnisotropy));
			
			// Texture filters
			postfilter = Filter.Point;
			mipgeneratefilter = Filter.Box;
			
			// Initialize presentations
			Presentation.Initialize();
		}

		#endregion

		#region ================== Initialization
		
		// This starts up Direct3D
		public static void Startup()
		{
			d3d = new Direct3D();
		}
		
		// This terminates Direct3D
		public static void Terminate()
		{
			if(d3d != null)
			{
				d3d.Dispose();
				d3d = null;
			}
		}
		
		// This initializes the graphics
		public bool Initialize()
		{
			// Use default adapter
			this.adapter = 0; // Manager.Adapters.Default.Adapter;

			try
			{
				// Make present parameters
				PresentParameters displaypp = CreatePresentParameters(adapter);

				// Determine device type for compatability with NVPerfHUD
				DeviceType devtype;
				if(d3d.Adapters[adapter].Details.Description.EndsWith(NVPERFHUD_ADAPTER))
					devtype = DeviceType.Reference;
				else
					devtype = DeviceType.Hardware;

				//mxd. Check maximum supported AA level...
				for(int i = AA_STEPS.Count - 1; i > 0; i--)
				{
					if(General.Settings.AntiAliasingSamples < AA_STEPS[i]) continue;
					if(d3d.CheckDeviceMultisampleType(this.adapter, devtype, d3d.Adapters[adapter].CurrentDisplayMode.Format, displaypp.Windowed, (MultisampleType)AA_STEPS[i]))
						break;

					if(General.Settings.AntiAliasingSamples > AA_STEPS[i - 1])
					{
						General.Settings.AntiAliasingSamples = AA_STEPS[i - 1];
						
						// TODO: looks like setting Multisample here just resets it to MultisampleType.None, 
						// regardless of value in displaypp.Multisample. Why?..
						displaypp.Multisample = (MultisampleType)General.Settings.AntiAliasingSamples;
					}
				}

				// Get the device capabilities
				devicecaps = d3d.GetDeviceCaps(adapter, devtype);

				// Check if this adapter supports TnL
				if((devicecaps.DeviceCaps & DeviceCaps.HWTransformAndLight) != 0)
				{
					// Initialize with hardware TnL
					device = new Device(d3d, adapter, devtype, rendertarget.Handle,
								CreateFlags.HardwareVertexProcessing, displaypp);
				}
				else
				{
					// Initialize with software TnL
					device = new Device(d3d, adapter, devtype, rendertarget.Handle,
								CreateFlags.SoftwareVertexProcessing, displaypp);
				}
			}
			catch(Exception)
			{
				// Failed
				MessageBox.Show(General.MainWindow, "Unable to initialize the Direct3D video device. Another application may have taken exclusive mode on this video device or the device does not support Direct3D at all.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			//mxd. Check if we can use shaders
			if(device.Capabilities.PixelShaderVersion.Major < 2)
			{
				// Failed
				MessageBox.Show(General.MainWindow, "Unable to initialize the Direct3D video device. Video device with Shader Model 2.0 support is required.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			// Add event to cancel resize event
			//device.DeviceResizing += new CancelEventHandler(CancelResize);

			// Keep a reference to the original buffers
			backbuffer = device.GetBackBuffer(0, 0);
			depthbuffer = device.DepthStencilSurface;

			// Get the viewport
			viewport = device.Viewport;

			// Create shader manager
			shaders = new ShaderManager(this);
			
			// Initialize settings
			SetupSettings();
			
			// Done
			return true;
		}

		// This is to disable the automatic resize reset
		/*private static void CancelResize(object sender, CancelEventArgs e)
		{
			// Cancel resize event
			e.Cancel = true;
		}*/
		
		// This creates present parameters
		private PresentParameters CreatePresentParameters(int adapter)
		{
			PresentParameters displaypp = new PresentParameters();

			// Get current display mode
			DisplayMode currentmode = d3d.Adapters[adapter].CurrentDisplayMode;

			// Make present parameters
			displaypp.Windowed = true;
			displaypp.SwapEffect = SwapEffect.Discard;
			displaypp.BackBufferCount = 1;
			displaypp.BackBufferFormat = currentmode.Format;
			displaypp.BackBufferWidth = rendertarget.ClientSize.Width;
			displaypp.BackBufferHeight = rendertarget.ClientSize.Height;
			displaypp.EnableAutoDepthStencil = true;
			displaypp.AutoDepthStencilFormat = Format.D24X8; //Format.D16;
			displaypp.Multisample = (MultisampleType)General.Settings.AntiAliasingSamples;
			displaypp.PresentationInterval = PresentInterval.Immediate;

			// Return result
			return displaypp;
		}
		
		#endregion

		#region ================== Resetting

		// This registers a resource
		internal void RegisterResource(ID3DResource res)
		{
			// Add resource
			resources.Add(res);
		}

		// This unregisters a resource
		internal void UnregisterResource(ID3DResource res)
		{
			// Remove resource
			resources.Remove(res);
		}
		
		// This resets the device and returns true on success
		internal bool Reset()
		{
			// Unload all Direct3D resources
			foreach(ID3DResource res in resources) res.UnloadResource();

			// Lose backbuffers
			if(backbuffer != null) backbuffer.Dispose();
			if(depthbuffer != null) depthbuffer.Dispose();
			backbuffer = null;
			depthbuffer = null;

			try
			{
				// Make present parameters
				PresentParameters displaypp = CreatePresentParameters(adapter);
				
				// Reset the device
				device.Reset(displaypp);
			}
#if DEBUG
			catch(Exception e)
			{
				// Failed to re-initialize
				Console.WriteLine("Device reset failed: " + e.Message);
				return false;
			}
#else
			catch(Exception) 
			{
				// Failed to re-initialize
				return false;
			}
#endif

			// Keep a reference to the original buffers
			backbuffer = device.GetBackBuffer(0, 0);
			depthbuffer = device.DepthStencilSurface;

			// Get the viewport
			viewport = device.Viewport;

			// Reload all Direct3D resources
			foreach(ID3DResource res in resources) res.ReloadResource();

			// Re-apply settings
			SetupSettings();
			
			// Success
			return true;
		}

        #endregion

        #region ================== Rendering

		// This begins a drawing session
		public bool StartRendering(bool clear, Color4 backcolor, Surface target, Surface depthbuffer)
		{
			// Check if we can render
			if(CheckAvailability() && !isrendering) //mxd. Added isrendering check
			{
				// Set rendertarget
				device.DepthStencilSurface = depthbuffer;
				device.SetRenderTarget(0, target);
				
				// Clear the screen
				if(clear)
				{
					if(depthbuffer != null)
						device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, backcolor, 1f, 0);
					else
						device.Clear(ClearFlags.Target, backcolor, 1f, 0);
				}

                // Ready to render
                // [ZZ] Sometimes (apparently during massive lag) this may cause invalid call exception.
                //      Let's put this code here until proper fix is available.
                //      For now, eat the initial exception and try to recover.
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        device.BeginScene();
                        break;
                    }
                    catch (SlimDXException e)
                    {
                        if (i != 0) throw e;
                        else
                        {
                            if (!CheckAvailability())
                            {
                                isrendering = false;
                                return false;
                            }

                            Reset();
                        }
                    }
                }

				isrendering = true; //mxd
				return true;
			}
			else
			{
				// Minimized, you cannot see anything
				isrendering = false; //mxd
				return false;
			}
		}

		// This clears a target
		public void ClearRendertarget(Color4 backcolor, Surface target, Surface depthbuffer)
		{
			// Set rendertarget
			device.DepthStencilSurface = depthbuffer;
			device.SetRenderTarget(0, target);

			if(depthbuffer != null)
				device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, backcolor, 1f, 0);
			else
				device.Clear(ClearFlags.Target, backcolor, 1f, 0);
		}

		// This ends a drawing session
		public void FinishRendering()
		{
			try
			{
				// Done
				device.EndScene();
				isrendering = false; //mxd
			}
			// Errors are not a problem here
			catch(Exception) { }
		}

		// This presents what has been drawn
		public void Present()
		{
			try
			{
				device.Present();
				isrendering = false; //mxd
			}
			// Errors are not a problem here
			catch(Exception) { }
		}
		
		// This checks if we can use the hardware at this moment
		public bool CheckAvailability()
		{
			// When minimized, the hardware is not available
			if(General.MainWindow.WindowState != FormWindowState.Minimized)
			{
				// Test the cooperative level
				Result coopresult = device.TestCooperativeLevel();

				// Check if device must be reset
				if(!coopresult.IsSuccess)
				{
					// Should we reset?
					if(coopresult.Name == "D3DERR_DEVICENOTRESET")
					{
						// Device is lost and must be reset now
						Reset();
					}

					// Impossible to render at this point
					return false;
				}
				else
				{
					// Ready to go!
					return true;
				}
			}
			else
			{
				// Minimized
				return false;
			}
		}
		
		#endregion

		#region ================== Tools

		// Make a color from ARGB
		public static int ARGB(float a, float r, float g, float b)
		{
			return Color.FromArgb((int)(a * 255f), (int)(r * 255f), (int)(g * 255f), (int)(b * 255f)).ToArgb();
		}

		// Make a color from RGB
		public static int RGB(int r, int g, int b)
		{
			return Color.FromArgb(255, r, g, b).ToArgb();
		}

		// This makes a Vector3 from Vector3D
		public static Vector3 V3(Vector3D v3d)
		{
			return new Vector3(v3d.x, v3d.y, v3d.z);
		}

		// This makes a Vector3D from Vector3
		public static Vector3D V3D(Vector3 v3)
		{
			return new Vector3D(v3.X, v3.Y, v3.Z);
		}

		// This makes a Vector2 from Vector2D
		public static Vector2 V2(Vector2D v2d)
		{
			return new Vector2(v2d.x, v2d.y);
		}

		// This makes a Vector2D from Vector2
		public static Vector2D V2D(Vector2 v2)
		{
			return new Vector2D(v2.X, v2.Y);
		}

		#endregion
	}
}
