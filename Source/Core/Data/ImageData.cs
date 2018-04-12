
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;
using SlimDX;
using SlimDX.Direct3D9;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public abstract unsafe class ImageData : IDisposable
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		// Properties
		protected string name;
		protected long longname;
		protected int width;
		protected int height;
		protected Vector2D scale;
		protected bool worldpanning;
		private bool usecolorcorrection;
		protected string filepathname; //mxd. Absolute path to the image;
		protected string shortname; //mxd. Name in uppercase and clamped to DataManager.CLASIC_IMAGE_NAME_LENGTH
		protected string virtualname; //mxd. Path of this name is used in TextureBrowserForm
		protected string displayname; //mxd. Name to display in TextureBrowserForm
		protected bool isFlat; //mxd. If false, it's a texture
		protected bool istranslucent; //mxd. If true, has pixels with alpha > 0 && < 255 
		protected bool ismasked; //mxd. If true, has pixels with zero alpha
		protected bool hasLongName; //mxd. Texture name is longer than DataManager.CLASIC_IMAGE_NAME_LENGTH
		protected bool hasPatchWithSameName; //mxd

		//mxd. Hashing
		private static int hashcounter;
		private readonly int hashcode;
		
		// Loading
		private volatile ImageLoadState previewstate;
		private volatile ImageLoadState imagestate;
		private volatile int previewindex;
		protected volatile bool loadfailed;
		private volatile bool allowunload;
		
		// References
		private volatile bool usedinmap;
		private volatile int references;
		
		// GDI bitmap
		protected Bitmap bitmap;
		
		// Direct3D texture
		private int mipmaplevels;	// 0 = all mipmaps
		protected bool dynamictexture;
		private Texture texture;
		
		// Disposing
		protected bool isdisposed;

        // Dummy object used when we don't have a bitmap for locking
        private object bitmapLocker = new object();
		
		#endregion
		
		#region ================== Properties
		
		public string Name { get { return name; } }
		public long LongName { get { return longname; } }
		public string ShortName { get { return shortname; } } //mxd
		public string FilePathName { get { return filepathname; } } //mxd
		public string VirtualName { get { return virtualname; } } //mxd
		public string DisplayName { get { return displayname; } } //mxd
		public bool IsFlat { get { return isFlat; } } //mxd
		public bool IsTranslucent { get { return istranslucent; } } //mxd
		public bool IsMasked { get { return ismasked; } } //mxd
		public bool HasPatchWithSameName { get { return hasPatchWithSameName; } } //mxd
		internal bool HasLongName { get { return hasLongName; } } //mxd
		public bool UseColorCorrection { get { return usecolorcorrection; } set { usecolorcorrection = value; } }
		public Texture Texture { get { lock (this) lock (bitmap ?? bitmapLocker) { return texture; } } }
		public bool IsPreviewLoaded { get { return (previewstate == ImageLoadState.Ready); } }
		public bool IsImageLoaded { get { return (imagestate == ImageLoadState.Ready); } }
		public bool LoadFailed { get { return loadfailed; } }
		public bool IsDisposed { get { return isdisposed; } }
		public bool AllowUnload { get { return allowunload; } set { allowunload = value; } }
		public ImageLoadState ImageState { get { return imagestate; } internal set { imagestate = value; } }
		public ImageLoadState PreviewState { get { return previewstate; } internal set { previewstate = value; } }
		public bool IsReferenced { get { return (references > 0) || usedinmap; } }
		public bool UsedInMap { get { return usedinmap; } }
		public int MipMapLevels { get { return mipmaplevels; } set { mipmaplevels = value; } }
		public virtual int Width { get { return width; } }
		public virtual int Height { get { return height; } }
		internal int PreviewIndex { get { return previewindex; } set { previewindex = value; } }
		//mxd. Scaled texture size is integer in ZDoom.
		public virtual float ScaledWidth { get { return (float)Math.Round(width * scale.x); } }
		public virtual float ScaledHeight { get { return (float)Math.Round(height * scale.y); } }
		public virtual Vector2D Scale { get { return scale; } }
		public bool WorldPanning { get { return worldpanning; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		protected ImageData()
		{
			// Defaults
			usecolorcorrection = true;
			allowunload = true;

			//mxd. Hashing
			hashcode = hashcounter++;
		}

		// Destructor
		~ImageData()
		{
			this.Dispose();
		}
		
		// Disposer
		public virtual void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				lock (this) lock (bitmap ?? bitmapLocker)
				{
					// Clean up
					if(bitmap != null) bitmap.Dispose();
					if(texture != null) texture.Dispose();
					bitmap = null;
					texture = null;
					
					// Done
					usedinmap = false;
					imagestate = ImageLoadState.None;
					previewstate = ImageLoadState.None;
					isdisposed = true;
				}
			}
		}
		
		#endregion
		
		#region ================== Management
		
		// This sets the status of the texture usage in the map
		internal void SetUsedInMap(bool used)
		{
			if(used != usedinmap)
			{
				usedinmap = used;
				General.Map.Data.ProcessImage(this);
			}
		}
		
		// This adds a reference
		public void AddReference()
		{
			references++;
			if(references == 1) General.Map.Data.ProcessImage(this);
		}
		
		// This removes a reference
		public void RemoveReference()
		{
			references--;
			if(references < 0) General.Fail("FAIL! (references < 0) Somewhere this image is dereferenced more than it was referenced.");
			if(references == 0) General.Map.Data.ProcessImage(this);
		}
		
		// This sets the name
		protected virtual void SetName(string name)
		{
			this.name = name;
			this.filepathname = name; //mxd
			this.shortname = name; //mxd
			this.virtualname = name; //mxd
			this.displayname = name; //mxd
			this.longname = Lump.MakeLongName(name); //mxd
		}
		
		// This unloads the image
		public virtual void UnloadImage()
		{
            lock (this) lock (bitmap ?? bitmapLocker)
            {
				if(bitmap != null) bitmap.Dispose();
				bitmap = null;
				imagestate = ImageLoadState.None;
			}
		}

		// This returns the bitmap image
		public Bitmap GetBitmap()
		{
            lock (this) lock (bitmap ?? bitmapLocker)
            {
				// Image loaded successfully?
				if(!loadfailed && (imagestate == ImageLoadState.Ready) && (bitmap != null))
					return bitmap;
				
				// Image loading failed?
				return (loadfailed ? Properties.Resources.Failed : Properties.Resources.Hourglass);
			}
		}
		
		// This loads the image
		public virtual void LoadImage()
		{
			// Do the loading
			LocalLoadImage();

			// Notify the main thread about the change so that sectors can update their buffers
			IntPtr strptr = Marshal.StringToCoTaskMemAuto(this.name);
			General.SendMessage(General.MainWindow.Handle, (int)MainForm.ThreadMessages.ImageDataLoaded, strptr, IntPtr.Zero);
		}
		
		// This requests loading the image
		protected virtual void LocalLoadImage()
		{
            lock (this) lock (bitmap ?? bitmapLocker)
            {
				// Bitmap loaded successfully?
				if(bitmap != null)
				{
					// Bitmap has incorrect format?
					if(bitmap.PixelFormat != PixelFormat.Format32bppArgb)
					{
						if(dynamictexture)
							throw new Exception("Dynamic images must be in 32 bits ARGB format.");
						
						//General.ErrorLogger.Add(ErrorType.Warning, "Image '" + name + "' does not have A8R8G8B8 pixel format. Conversion was needed.");
						Bitmap oldbitmap = bitmap;
						try
						{
							// Convert to desired pixel format
							bitmap = new Bitmap(oldbitmap.Size.Width, oldbitmap.Size.Height, PixelFormat.Format32bppArgb);
							Graphics g = Graphics.FromImage(bitmap);
							g.PageUnit = GraphicsUnit.Pixel;
							g.CompositingQuality = CompositingQuality.HighQuality;
							g.InterpolationMode = InterpolationMode.NearestNeighbor;
							g.SmoothingMode = SmoothingMode.None;
							g.PixelOffsetMode = PixelOffsetMode.None;
							g.Clear(Color.Transparent);
							g.DrawImage(oldbitmap, 0, 0, oldbitmap.Size.Width, oldbitmap.Size.Height);
							g.Dispose();
							oldbitmap.Dispose();
						}
						catch(Exception e)
						{
							bitmap = oldbitmap;
							General.ErrorLogger.Add(ErrorType.Warning, "Cannot lock image \"" + name + "\" for pixel format conversion. The image may not be displayed correctly.\n" + e.GetType().Name + ": " + e.Message);
						}
					}
					
					// This applies brightness correction on the image
					if(usecolorcorrection)
					{
						BitmapData bmpdata = null;
						
						try
						{
							// Try locking the bitmap
							bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
						}
						catch(Exception e)
						{
							General.ErrorLogger.Add(ErrorType.Warning, "Cannot lock image \"" + name + "\" for color correction. The image may not be displayed correctly.\n" + e.GetType().Name + ": " + e.Message);
						}

						// Bitmap locked?
						if(bmpdata != null)
						{
							// Apply color correction
							PixelColor* pixels = (PixelColor*)(bmpdata.Scan0.ToPointer());
							General.Colors.ApplyColorCorrection(pixels, bmpdata.Width * bmpdata.Height);
							bitmap.UnlockBits(bmpdata);
						}
					}
				}
				else
				{
					// Loading failed
					// We still mark the image as ready so that it will
					// not try loading again until Reload Resources is used
					loadfailed = true;
					bitmap = new Bitmap(Properties.Resources.Failed);
				}

				if(bitmap != null)
				{
					width = bitmap.Size.Width;
					height = bitmap.Size.Height;

					if(dynamictexture)
					{
						if((width != General.NextPowerOf2(width)) || (height != General.NextPowerOf2(height)))
							throw new Exception("Dynamic images must have a size in powers of 2.");
					}

					// Do we still have to set a scale?
					if((scale.x == 0.0f) && (scale.y == 0.0f))
					{
						if((General.Map != null) && (General.Map.Config != null))
						{
							scale.x = General.Map.Config.DefaultTextureScale;
							scale.y = General.Map.Config.DefaultTextureScale;
						}
						else
						{
							scale.x = 1.0f;
							scale.y = 1.0f;
						}
					}

					if(!loadfailed)
					{
						//mxd. Check translucency and calculate average color?
						if(General.Map != null && General.Map.Data != null && General.Map.Data.GlowingFlats != null &&
						   General.Map.Data.GlowingFlats.ContainsKey(longname) &&
						   General.Map.Data.GlowingFlats[longname].CalculateTextureColor)
						{
							BitmapData bmpdata = null;
							try
							{
								bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
							}
							catch(Exception e)
							{
								General.ErrorLogger.Add(ErrorType.Error, "Cannot lock image \"" + this.filepathname + "\" for glow color calculation. " + e.GetType().Name + ": " + e.Message);
							}

							if(bmpdata != null)
							{
								PixelColor* pixels = (PixelColor*)(bmpdata.Scan0.ToPointer());
								int numpixels = bmpdata.Width * bmpdata.Height;
								uint r = 0;
								uint g = 0;
								uint b = 0;

								for(PixelColor* cp = pixels + numpixels - 1; cp >= pixels; cp--)
								{
									r += cp->r;
									g += cp->g;
									b += cp->b;

									// Also check alpha
									if(cp->a > 0 && cp->a < 255) istranslucent = true;
									else if(cp->a == 0) ismasked = true;
								}

								// Update glow data
								int br = (int)(r / numpixels);
								int bg = (int)(g / numpixels);
								int bb = (int)(b / numpixels);

								int max = Math.Max(br, Math.Max(bg, bb));

								// Black can't glow...
								if(max == 0)
								{
									General.Map.Data.GlowingFlats.Remove(longname);
								}
								else
								{
									// That's how it's done in GZDoom (and I may be totally wrong about this)
									br = Math.Min(255, br * 153 / max);
									bg = Math.Min(255, bg * 153 / max);
									bb = Math.Min(255, bb * 153 / max);

									General.Map.Data.GlowingFlats[longname].Color = new PixelColor(255, (byte)br, (byte)bg, (byte)bb);
									General.Map.Data.GlowingFlats[longname].CalculateTextureColor = false;
									if(!General.Map.Data.GlowingFlats[longname].Fullbright) General.Map.Data.GlowingFlats[longname].Brightness = (br + bg + bb) / 3;
								}

								// Release the data
								bitmap.UnlockBits(bmpdata);
							}
						}
						//mxd. Check if the texture is translucent
						else
						{
							BitmapData bmpdata = null;
							try
							{
								bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
							}
							catch(Exception e)
							{
								General.ErrorLogger.Add(ErrorType.Error, "Cannot lock image \"" + this.filepathname + "\" for translucency check. " + e.GetType().Name + ": " + e.Message);
							}

							if(bmpdata != null)
							{
								PixelColor* pixels = (PixelColor*)(bmpdata.Scan0.ToPointer());
								int numpixels = bmpdata.Width * bmpdata.Height;

								for(PixelColor* cp = pixels + numpixels - 1; cp >= pixels; cp--)
								{
									// Check alpha
									if(cp->a > 0 && cp->a < 255) istranslucent = true;
									else if(cp->a == 0) ismasked = true;
								}

								// Release the data
								bitmap.UnlockBits(bmpdata);
							}
						}
					}
				}
				
				// Image is ready
				imagestate = ImageLoadState.Ready;
			}
		}
		
		// This creates the Direct3D texture
		public virtual void CreateTexture()
		{
            lock (this) lock (bitmap ?? bitmapLocker)
            {
				// Only do this when texture is not created yet
				if(((texture == null) || (texture.Disposed)) && this.IsImageLoaded && !loadfailed)
				{
					Image img = bitmap;
					if(loadfailed) img = Properties.Resources.Failed;
					
					// Write to memory stream and read from memory
					MemoryStream memstream = new MemoryStream((img.Size.Width * img.Size.Height * 4) + 4096);
					img.Save(memstream, ImageFormat.Bmp);
					memstream.Seek(0, SeekOrigin.Begin);
					if(dynamictexture)
					{
						texture = Texture.FromStream(General.Map.Graphics.Device, memstream, (int)memstream.Length,
										img.Size.Width, img.Size.Height, mipmaplevels, Usage.Dynamic, Format.A8R8G8B8,
										Pool.Default, General.Map.Graphics.PostFilter, General.Map.Graphics.MipGenerateFilter, 0);
					}
					else
					{
						texture = Texture.FromStream(General.Map.Graphics.Device, memstream, (int)memstream.Length,
										img.Size.Width, img.Size.Height, mipmaplevels, Usage.None, Format.Unknown,
										Pool.Managed, General.Map.Graphics.PostFilter, General.Map.Graphics.MipGenerateFilter, 0);
					}
					memstream.Dispose();
					
					if(dynamictexture)
					{
						if((width != texture.GetLevelDescription(0).Width) || (height != texture.GetLevelDescription(0).Height))
							throw new Exception("Could not create a texture with the same size as the image.");
					}

#if DEBUG
					texture.Tag = name; //mxd. Helps with tracking undisposed resources...
#endif
				}
			}
		}

		// This updates a dynamic texture
		public void UpdateTexture()
		{
			if(!dynamictexture)
				throw new Exception("The image must be a dynamic image to support direct updating.");

            lock (this) lock (bitmap ?? bitmapLocker)
            {
				if((texture != null) && !texture.Disposed)
				{
					// Lock the bitmap and texture
					BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
					DataRectangle texdata = texture.LockRectangle(0, LockFlags.Discard);

					// Copy data
					int* bp = (int*)bmpdata.Scan0.ToPointer();
					int* tp = (int*)texdata.Data.DataPointer.ToPointer();
					for(int y = 0; y < bmpdata.Height; y++)
					{
						for(int x = 0; x < bmpdata.Width; x++)
						{
							*tp = *bp;
							bp++;
							tp++;
						}

						// Skip extra data in texture
						int extrapitch = (texdata.Pitch >> 2) - bmpdata.Width;
						tp += extrapitch;
					}

					// Unlock
					texture.UnlockRectangle(0);
					bitmap.UnlockBits(bmpdata);
				}
			}
		}
		
		// This destroys the Direct3D texture
		public void ReleaseTexture()
		{
            lock (this) lock (bitmap ?? bitmapLocker)
            {
				// Trash it
				if(texture != null) texture.Dispose();
				texture = null;
			}
		}

		// This draws a preview
		public virtual void DrawPreview(Graphics target, Point targetpos)
		{
            lock (this) lock (bitmap ?? bitmapLocker)
            {
				// Preview ready?
				if(!loadfailed && (previewstate == ImageLoadState.Ready))
				{
					// Draw preview
					General.Map.Data.Previews.DrawPreview(previewindex, target, targetpos);
				}
				// Loading failed?
				else if(loadfailed)
				{
					// Draw error bitmap
					targetpos = new Point(targetpos.X + ((PreviewManager.MAX_PREVIEW_SIZE - Properties.Resources.Hourglass.Width) >> 1),
										  targetpos.Y + ((PreviewManager.MAX_PREVIEW_SIZE - Properties.Resources.Hourglass.Height) >> 1));
					target.DrawImageUnscaled(Properties.Resources.Failed, targetpos);
				}
				else
				{
					// Draw loading bitmap
					targetpos = new Point(targetpos.X + ((PreviewManager.MAX_PREVIEW_SIZE - Properties.Resources.Hourglass.Width) >> 1),
										  targetpos.Y + ((PreviewManager.MAX_PREVIEW_SIZE - Properties.Resources.Hourglass.Height) >> 1));
					target.DrawImageUnscaled(Properties.Resources.Hourglass, targetpos);
				}
			}
		}
		
		// This returns a preview image
		public virtual Image GetPreview()
		{
            lock (this) lock (bitmap ?? bitmapLocker)
            {
				// Preview ready?
				if(previewstate == ImageLoadState.Ready)
				{
					// Make a copy
					return General.Map.Data.Previews.GetPreviewCopy(previewindex);
				}
				
				// Loading failed?
				if(loadfailed)
				{
					// Return error bitmap
					return Properties.Resources.Failed;
				}

				// Return loading bitmap
				return Properties.Resources.Hourglass;
			}
		}

		//mxd. This greatly speeds up Dictionary lookups
		public override int GetHashCode()
		{
			return hashcode;
		}
		
		#endregion
	}
}
