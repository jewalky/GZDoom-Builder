
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
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using CodeImp.DoomBuilder.Rendering;
using System.Drawing.Imaging;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	//mxd
	internal struct DevilImageType
	{
		internal const int IL_TYPE_UNKNOWN = 0x0000;
		//internal const int IL_DOOM = 0x0422;  //!< DooM walls - no specific extension
		//internal const int IL_DOOM_FLAT = 0x0423;  //!< DooM flats - no specific extension

		internal const int IL_JPG = 0x0425;  //!< JPEG - .jpg, .jpe and .jpeg extensions
		internal const int IL_PCX = 0x0428;  //!< ZSoft PCX - .pcx extension
		internal const int IL_PNG = 0x042A;  //!< Portable Network Graphics - .png extension
		internal const int IL_TGA = 0x042D;  //!< TrueVision Targa File - .tga, .vda, .icb and .vst extensions
		internal const int IL_GIF = 0x0436;  //!< Graphics Interchange Format - .gif extension
		internal const int IL_DDS = 0x0437;  //!< DirectDraw Surface - .dds extension
	}

    // [ZZ]
    internal enum DevilError
    {
        IL_NO_ERROR             = 0x0000,
        IL_INVALID_ENUM         = 0x0501,
        IL_OUT_OF_MEMORY        = 0x0502,
        IL_FORMAT_NOT_SUPPORTED = 0x0503,
        IL_INTERNAL_ERROR       = 0x0504,
        IL_INVALID_VALUE        = 0x0505,
        IL_ILLEGAL_OPERATION    = 0x0506,
        IL_ILLEGAL_FILE_VALUE   = 0x0507,
        IL_INVALID_FILE_HEADER  = 0x0508,
        IL_INVALID_PARAM        = 0x0509,
        IL_COULD_NOT_OPEN_FILE  = 0x050A,
        IL_INVALID_EXTENSION    = 0x050B,
        IL_FILE_ALREADY_EXISTS  = 0x050C,
        IL_OUT_FORMAT_SAME      = 0x050D,
        IL_STACK_OVERFLOW       = 0x050E,
        IL_STACK_UNDERFLOW      = 0x050F,
        IL_INVALID_CONVERSION   = 0x0510,
        IL_BAD_DIMENSIONS       = 0x0511,
        IL_FILE_READ_ERROR      = 0x0512,  // 05/12/2002: Addition by Sam.
        IL_FILE_WRITE_ERROR     = 0x0512,

        IL_LIB_GIF_ERROR  = 0x05E1,
        IL_LIB_JPEG_ERROR = 0x05E2,
        IL_LIB_PNG_ERROR  = 0x05E3,
        IL_LIB_TIFF_ERROR = 0x05E4,
        IL_LIB_MNG_ERROR  = 0x05E5,
        IL_UNKNOWN_ERROR  = 0x05FF
    }

    internal unsafe class FileImageReader : IImageReader
	{
		#region ================== APIs

		[DllImport("devil.dll")]
		private static extern void ilGenImages(int num, IntPtr images);

		[DllImport("devil.dll")]
		private static extern void ilBindImage(uint image);

		[DllImport("devil.dll")]
		private static extern void ilDeleteImages(int num, IntPtr images);

		[DllImport("devil.dll")]
		private static extern bool ilLoadL(uint type, IntPtr lump, uint size);

		[DllImport("devil.dll")]
		private static extern int ilGetInteger(uint mode);

        [DllImport("devil.dll")]
        private static extern int ilGetError();

		[DllImport("devil.dll")]
		private static extern int ilConvertImage(uint destformat, uint desttype);

		[DllImport("devil.dll")]
		private static extern uint ilCopyPixels(uint xoff, uint yoff, uint zoff, uint width, uint height, uint depth, uint format, uint type, IntPtr data);
		
		//mxd. Look's like we don't need many of those...
		//  Matches OpenGL's right now.
		//! Data formats \link Formats Formats\endlink
		//private const int IL_COLOUR_INDEX     = 0x1900;
		//private const int IL_COLOR_INDEX      = 0x1900;
		//private const int IL_ALPHA			= 0x1906;
		//private const int IL_RGB              = 0x1907;
		//private const int IL_RGBA             = 0x1908;
		//private const int IL_BGR              = 0x80E0;
		private const int IL_BGRA             = 0x80E1;
		//private const int IL_LUMINANCE        = 0x1909;
		//private const int IL_LUMINANCE_ALPHA  = 0x190A;

		//! Data types \link Types Types\endlink
		//private const int IL_BYTE           = 0x1400;
		private const int IL_UNSIGNED_BYTE  = 0x1401;
		/*private const int IL_SHORT          = 0x1402;
		private const int IL_UNSIGNED_SHORT = 0x1403;
		private const int IL_INT            = 0x1404;
		private const int IL_UNSIGNED_INT   = 0x1405;
		private const int IL_FLOAT          = 0x1406;
		private const int IL_DOUBLE         = 0x140A;
		private const int IL_HALF           = 0x140B;*/

		// Image types
		//private const int IL_TYPE_UNKNOWN = 0x0000;
		/*private const int IL_BMP          = 0x0420;  //!< Microsoft Windows Bitmap - .bmp extension
		private const int IL_CUT          = 0x0421;  //!< Dr. Halo - .cut extension
		private const int IL_DOOM         = 0x0422;  //!< DooM walls - no specific extension
		private const int IL_DOOM_FLAT    = 0x0423;  //!< DooM flats - no specific extension
		private const int IL_ICO          = 0x0424;  //!< Microsoft Windows Icons and Cursors - .ico and .cur extensions
		private const int IL_JPG          = 0x0425;  //!< JPEG - .jpg, .jpe and .jpeg extensions
		private const int IL_JFIF         = 0x0425;  //!<
		private const int IL_ILBM         = 0x0426;  //!< Amiga IFF (FORM ILBM) - .iff, .ilbm, .lbm extensions
		private const int IL_PCD          = 0x0427;  //!< Kodak PhotoCD - .pcd extension
		private const int IL_PCX          = 0x0428;  //!< ZSoft PCX - .pcx extension
		private const int IL_PIC          = 0x0429;  //!< PIC - .pic extension
		private const int IL_PNG          = 0x042A;  //!< Portable Network Graphics - .png extension
		private const int IL_PNM          = 0x042B;  //!< Portable Any Map - .pbm, .pgm, .ppm and .pnm extensions
		private const int IL_SGI          = 0x042C;  //!< Silicon Graphics - .sgi, .bw, .rgb and .rgba extensions
		private const int IL_TGA          = 0x042D;  //!< TrueVision Targa File - .tga, .vda, .icb and .vst extensions
		private const int IL_TIF          = 0x042E;  //!< Tagged Image File Format - .tif and .tiff extensions
		private const int IL_CHEAD        = 0x042F;  //!< C-Style Header - .h extension
		private const int IL_RAW          = 0x0430;  //!< Raw Image Data - any extension
		private const int IL_MDL          = 0x0431;  //!< Half-Life Model Texture - .mdl extension
		private const int IL_WAL          = 0x0432;  //!< Quake 2 Texture - .wal extension
		private const int IL_LIF          = 0x0434;  //!< Homeworld Texture - .lif extension
		private const int IL_MNG          = 0x0435;  //!< Multiple-image Network Graphics - .mng extension
		private const int IL_JNG          = 0x0435;  //!< 
		private const int IL_GIF          = 0x0436;  //!< Graphics Interchange Format - .gif extension
		private const int IL_DDS          = 0x0437;  //!< DirectDraw Surface - .dds extension
		private const int IL_DCX          = 0x0438;  //!< ZSoft Multi-PCX - .dcx extension
		private const int IL_PSD          = 0x0439;  //!< Adobe PhotoShop - .psd extension
		private const int IL_EXIF         = 0x043A;  //!< 
		private const int IL_PSP          = 0x043B;  //!< PaintShop Pro - .psp extension
		private const int IL_PIX          = 0x043C;  //!< PIX - .pix extension
		private const int IL_PXR          = 0x043D;  //!< Pixar - .pxr extension
		private const int IL_XPM          = 0x043E;  //!< X Pixel Map - .xpm extension
		private const int IL_HDR          = 0x043F;  //!< Radiance High Dynamic Range - .hdr extension
		private const int IL_ICNS = 0x0440;  //!< Macintosh Icon - .icns extension
		private const int IL_JP2 = 0x0441;  //!< Jpeg 2000 - .jp2 extension
		private const int IL_EXR = 0x0442;  //!< OpenEXR - .exr extension
		private const int IL_WDP = 0x0443;  //!< Microsoft HD Photo - .wdp and .hdp extension
		private const int IL_VTF = 0x0444;  //!< Valve Texture Format - .vtf extension
		private const int IL_WBMP = 0x0445;  //!< Wireless Bitmap - .wbmp extension
		private const int IL_SUN = 0x0446;  //!< Sun Raster - .sun, .ras, .rs, .im1, .im8, .im24 and .im32 extensions
		private const int IL_IFF = 0x0447;  //!< Interchange File Format - .iff extension
		private const int IL_TPL = 0x0448;  //!< Gamecube Texture - .tpl extension
		private const int IL_FITS = 0x0449;  //!< Flexible Image Transport System - .fit and .fits extensions
		private const int IL_DICOM = 0x044A;  //!< Digital Imaging and Communications in Medicine (DICOM) - .dcm and .dicom extensions
		private const int IL_IWI = 0x044B;  //!< Call of Duty Infinity Ward Image - .iwi extension
		private const int IL_BLP = 0x044C;  //!< Blizzard Texture Format - .blp extension
		private const int IL_FTX = 0x044D;  //!< Heavy Metal: FAKK2 Texture - .ftx extension
		private const int IL_ROT = 0x044E;  //!< Homeworld 2 - Relic Texture - .rot extension
		private const int IL_TEXTURE = 0x044F;  //!< Medieval II: Total War Texture - .texture extension
		private const int IL_DPX = 0x0450;  //!< Digital Picture Exchange - .dpx extension
		private const int IL_UTX = 0x0451;  //!< Unreal (and Unreal Tournament) Texture - .utx extension
		private const int IL_MP3 = 0x0452;  //!< MPEG-1 Audio Layer 3 - .mp3 extension*/

		//private const int IL_JASC_PAL     = 0x0475;  //!< PaintShop Pro Palette


		// Error Types
		/*private const int IL_NO_ERROR             = 0x0000;
		private const int IL_INVALID_ENUM         = 0x0501;
		private const int IL_OUT_OF_MEMORY        = 0x0502;
		private const int IL_FORMAT_NOT_SUPPORTED = 0x0503;
		private const int IL_INTERNAL_ERROR       = 0x0504;
		private const int IL_INVALID_VALUE        = 0x0505;
		private const int IL_ILLEGAL_OPERATION    = 0x0506;
		private const int IL_ILLEGAL_FILE_VALUE   = 0x0507;
		private const int IL_INVALID_FILE_HEADER  = 0x0508;
		private const int IL_INVALID_PARAM        = 0x0509;
		private const int IL_COULD_NOT_OPEN_FILE  = 0x050A;
		private const int IL_INVALID_EXTENSION    = 0x050B;
		private const int IL_FILE_ALREADY_EXISTS  = 0x050C;
		private const int IL_OUT_FORMAT_SAME      = 0x050D;
		private const int IL_STACK_OVERFLOW       = 0x050E;
		private const int IL_STACK_UNDERFLOW      = 0x050F;
		private const int IL_INVALID_CONVERSION   = 0x0510;
		private const int IL_BAD_DIMENSIONS       = 0x0511;
		private const int IL_FILE_READ_ERROR      = 0x0512;  // 05/12/2002: Addition by Sam.
		private const int IL_FILE_WRITE_ERROR     = 0x0512;*/

		/*private const int IL_LIB_GIF_ERROR  = 0x05E1;
		private const int IL_LIB_JPEG_ERROR = 0x05E2;
		private const int IL_LIB_PNG_ERROR  = 0x05E3;
		private const int IL_LIB_TIFF_ERROR = 0x05E4;
		private const int IL_LIB_MNG_ERROR  = 0x05E5;
		private const int IL_LIB_JP2_ERROR  = 0x05E6;
		private const int IL_LIB_EXR_ERROR  = 0x05E7;
		private const int IL_UNKNOWN_ERROR  = 0x05FF;*/


		// Origin Definitions
		/*private const int IL_ORIGIN_SET        = 0x0600;
		private const int IL_ORIGIN_LOWER_LEFT = 0x0601;
		private const int IL_ORIGIN_UPPER_LEFT = 0x0602;
		private const int IL_ORIGIN_MODE       = 0x0603;*/


		// Format and Type Mode Definitions
		/*private const int IL_FORMAT_SET  = 0x0610;
		private const int IL_FORMAT_MODE = 0x0611;
		private const int IL_TYPE_SET    = 0x0612;
		private const int IL_TYPE_MODE   = 0x0613;*/


		// File definitions
		/*private const int IL_FILE_OVERWRITE	= 0x0620;
		private const int IL_FILE_MODE		= 0x0621;
		*/

		// Palette definitions
		//private const int IL_CONV_PAL			= 0x0630;


		// Load fail definitions
		//private const int IL_DEFAULT_ON_FAIL	= 0x0632;


		// Key colour and alpha definitions
		/*private const int IL_USE_KEY_COLOUR	= 0x0635;
		private const int IL_USE_KEY_COLOR	= 0x0635;
		private const int IL_BLIT_BLEND		= 0x0636;*/


		// Interlace definitions
		/*private const int IL_SAVE_INTERLACED	= 0x0639;
		private const int IL_INTERLACE_MODE	= 0x063A;*/


		// Quantization definitions
		/*private const int IL_QUANTIZATION_MODE = 0x0640;
		private const int IL_WU_QUANT          = 0x0641;
		private const int IL_NEU_QUANT         = 0x0642;
		private const int IL_NEU_QUANT_SAMPLE  = 0x0643;
		private const int IL_MAX_QUANT_INDEXS  = 0x0644; //XIX : ILint : Maximum number of colors to reduce to, default of 256. and has a range of 2-256
		private const int IL_MAX_QUANT_INDICES = 0x0644; // Redefined, since the above private const int is misspelled*/


		// Hints
		/*private const int IL_FASTEST          = 0x0660;
		private const int IL_LESS_MEM         = 0x0661;
		private const int IL_DONT_CARE        = 0x0662;
		private const int IL_MEM_SPEED_HINT   = 0x0665;
		private const int IL_USE_COMPRESSION  = 0x0666;
		private const int IL_NO_COMPRESSION   = 0x0667;
		private const int IL_COMPRESSION_HINT = 0x0668;*/


		// Compression
		/*private const int IL_NVIDIA_COMPRESS	= 0x0670;
		private const int IL_SQUISH_COMPRESS	= 0x0671;*/


		// Subimage types
		/*private const int IL_SUB_NEXT   = 0x0680;
		private const int IL_SUB_MIPMAP = 0x0681;
		private const int IL_SUB_LAYER  = 0x0682;*/
		

		// Compression definitions
		/*private const int IL_COMPRESS_MODE = 0x0700;
		private const int IL_COMPRESS_NONE = 0x0701;
		private const int IL_COMPRESS_RLE  = 0x0702;
		private const int IL_COMPRESS_LZO  = 0x0703;
		private const int IL_COMPRESS_ZLIB = 0x0704;*/


		// File format-specific values
		/*private const int IL_TGA_CREATE_STAMP        = 0x0710;
		private const int IL_JPG_QUALITY             = 0x0711;
		private const int IL_PNG_INTERLACE           = 0x0712;
		private const int IL_TGA_RLE                 = 0x0713;
		private const int IL_BMP_RLE                 = 0x0714;
		private const int IL_SGI_RLE                 = 0x0715;
		private const int IL_TGA_ID_STRING           = 0x0717;
		private const int IL_TGA_AUTHNAME_STRING     = 0x0718;
		private const int IL_TGA_AUTHCOMMENT_STRING  = 0x0719;
		private const int IL_PNG_AUTHNAME_STRING     = 0x071A;
		private const int IL_PNG_TITLE_STRING        = 0x071B;
		private const int IL_PNG_DESCRIPTION_STRING  = 0x071C;
		private const int IL_TIF_DESCRIPTION_STRING  = 0x071D;
		private const int IL_TIF_HOSTCOMPUTER_STRING = 0x071E;
		private const int IL_TIF_DOCUMENTNAME_STRING = 0x071F;
		private const int IL_TIF_AUTHNAME_STRING     = 0x0720;
		private const int IL_JPG_SAVE_FORMAT         = 0x0721;
		private const int IL_CHEAD_HEADER_STRING     = 0x0722;
		private const int IL_PCD_PICNUM              = 0x0723;
		private const int IL_PNG_ALPHA_INDEX = 0x0724; //XIX : ILint : the color in the palette at this index value (0-255) is considered transparent, -1 for no trasparent color
		private const int IL_JPG_PROGRESSIVE         = 0x0725;
		private const int IL_VTF_COMP                = 0x0726;*/


		// DXTC definitions
		/*private const int IL_DXTC_FORMAT      = 0x0705;
		private const int IL_DXT1             = 0x0706;
		private const int IL_DXT2             = 0x0707;
		private const int IL_DXT3             = 0x0708;
		private const int IL_DXT4             = 0x0709;
		private const int IL_DXT5             = 0x070A;
		private const int IL_DXT_NO_COMP      = 0x070B;
		private const int IL_KEEP_DXTC_DATA   = 0x070C;
		private const int IL_DXTC_DATA_FORMAT = 0x070D;
		private const int IL_3DC              = 0x070E;
		private const int IL_RXGB             = 0x070F;
		private const int IL_ATI1N            = 0x0710;
		private const int IL_DXT1A            = 0x0711; // Normally the same as IL_DXT1, except for nVidia Texture Tools.*/

		// Environment map definitions
		/*private const int IL_CUBEMAP_POSITIVEX = 0x00000400;
		private const int IL_CUBEMAP_NEGATIVEX = 0x00000800;
		private const int IL_CUBEMAP_POSITIVEY = 0x00001000;
		private const int IL_CUBEMAP_NEGATIVEY = 0x00002000;
		private const int IL_CUBEMAP_POSITIVEZ = 0x00004000;
		private const int IL_CUBEMAP_NEGATIVEZ = 0x00008000;
		private const int IL_SPHEREMAP         = 0x00010000;*/


		// Values
		//private const int IL_VERSION_NUM           = 0x0DE2;
		private const int IL_IMAGE_WIDTH           = 0x0DE4;
		private const int IL_IMAGE_HEIGHT          = 0x0DE5;
		/*private const int IL_IMAGE_DEPTH           = 0x0DE6;
		private const int IL_IMAGE_SIZE_OF_DATA    = 0x0DE7;
		private const int IL_IMAGE_BPP             = 0x0DE8;
		private const int IL_IMAGE_BYTES_PER_PIXEL = 0x0DE8;
		private const int IL_IMAGE_BITS_PER_PIXEL  = 0x0DE9;
		private const int IL_IMAGE_FORMAT          = 0x0DEA;
		private const int IL_IMAGE_TYPE            = 0x0DEB;
		private const int IL_PALETTE_TYPE          = 0x0DEC;
		private const int IL_PALETTE_SIZE          = 0x0DED;
		private const int IL_PALETTE_BPP           = 0x0DEE;
		private const int IL_PALETTE_NUM_COLS      = 0x0DEF;
		private const int IL_PALETTE_BASE_TYPE     = 0x0DF0;
		private const int IL_NUM_FACES             = 0x0DE1;
		private const int IL_NUM_IMAGES            = 0x0DF1;
		private const int IL_NUM_MIPMAPS           = 0x0DF2;
		private const int IL_NUM_LAYERS            = 0x0DF3;
		private const int IL_ACTIVE_IMAGE          = 0x0DF4;
		private const int IL_ACTIVE_MIPMAP         = 0x0DF5;
		private const int IL_ACTIVE_LAYER          = 0x0DF6;
		private const int IL_ACTIVE_FACE           = 0x0E00;
		private const int IL_CUR_IMAGE             = 0x0DF7;
		private const int IL_IMAGE_DURATION        = 0x0DF8;
		private const int IL_IMAGE_PLANESIZE       = 0x0DF9;
		private const int IL_IMAGE_BPC             = 0x0DFA;
		private const int IL_IMAGE_OFFX            = 0x0DFB;
		private const int IL_IMAGE_OFFY            = 0x0DFC;
		private const int IL_IMAGE_CUBEFLAGS       = 0x0DFD;
		private const int IL_IMAGE_ORIGIN          = 0x0DFE;
		private const int IL_IMAGE_CHANNELS        = 0x0DFF;*/

		//mxd
		private readonly uint imagetype;
        // [ZZ]
        private byte[] imagebytes;
        // [ZZ] basically, the logic here is: if the image is not loaded correctly,
        //      then it might be misinterpreted Doom headerless image.
        //      so here we just allocate a DoomFlat/Colormap/Image reader and proxy calls
        private readonly int guesstype;
        private readonly Playpal guesspalette;
        private IImageReader proxyreader;
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public FileImageReader()
		{
			imagetype = DevilImageType.IL_TYPE_UNKNOWN;//mxd
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		//mxd
		public FileImageReader(uint devilImagetype) 
		{
			imagetype = devilImagetype;//mxd
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

        // [ZZ]
        public FileImageReader(uint devilImagetype, int guesstype, Playpal guesspalette)
        {
            imagetype = devilImagetype;
            this.guesstype = guesstype;
            this.guesspalette = guesspalette;

            // We have no destructor
            GC.SuppressFinalize(this);
        }

		#endregion

		#region ================== Methods

		// This creates a Bitmap from the given data
		// Returns null on failure
		public Bitmap ReadAsBitmap(Stream stream, out int offsetx, out int offsety)
		{
            if (proxyreader != null)
                return proxyreader.ReadAsBitmap(stream, out offsetx, out offsety);

			offsetx = int.MinValue;
			offsety = int.MinValue;

		    Bitmap bmp = ReadAsBitmap(stream);

			//mxd. Read PNG offsets
			if(imagetype == DevilImageType.IL_PNG && bmp != null)
			{
				stream.Position = 8;
				using(BinaryReader reader = new BinaryReader(stream))
				{
					// Read chunks untill we encounter either "grAb" or "IDAT"
					while(reader.BaseStream.Position < reader.BaseStream.Length)
					{
						// Big Endian!
						int chunklength = ToInt32BigEndian(reader.ReadBytes(4));
						string chunkname = new string(reader.ReadChars(4));

						if(chunkname == "grAb")
						{
							offsetx = ToInt32BigEndian(reader.ReadBytes(4));
							offsety = ToInt32BigEndian(reader.ReadBytes(4));
							break;
						}
						else if(chunkname == "IDAT")
						{
							break;
						}
						else
						{
							// Skip the rest of the chunk
							reader.BaseStream.Position += chunklength + 4;
						}
					}
				}
			}

			// Return the image
			return bmp;
		}

		// This reads the image and returns a Bitmap
		public Bitmap ReadAsBitmap(Stream stream)
		{
            if (proxyreader != null)
                return proxyreader.ReadAsBitmap(stream);

            try
			{
				// Create an image in DevIL
				uint imageid = 0;
				ilGenImages(1, new IntPtr(&imageid));
				ilBindImage(imageid);

                // Read image data from stream
                byte[] bytes;
                if (imagebytes == null)
                {
                    bytes = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(bytes, 0, bytes.Length);
                    imagebytes = bytes;
                }
                else bytes = imagebytes;

                fixed (byte* bptr = bytes)
				{
					if(!ilLoadL(imagetype, new IntPtr(bptr), (uint)bytes.Length))
						throw new BadImageFormatException();
				}

                // [ZZ] check if there was any error code
                DevilError ilerror = (DevilError)ilGetError();
                if (ilerror != DevilError.IL_NO_ERROR)
                    throw new BadImageFormatException("DevIL error: " + ilerror.ToString());

				// Get the image properties
				int width = ilGetInteger(IL_IMAGE_WIDTH);
				int height = ilGetInteger(IL_IMAGE_HEIGHT);
				if((width < 1) || (height < 1))
					throw new BadImageFormatException();
				
				// Convert the image to ARGB if needed
				ilConvertImage(IL_BGRA, IL_UNSIGNED_BYTE);

				// Copy the image pixels to a Bitmap
				Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				ilCopyPixels(0, 0, 0, (uint)width, (uint)height, 1, IL_BGRA, IL_UNSIGNED_BYTE, bmpdata.Scan0);
				bmp.UnlockBits(bmpdata);

				// Clean up
				ilDeleteImages(1, new IntPtr(&imageid));

				//mxd. TGA fix
				if(imagetype == DevilImageType.IL_TGA)
					bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

				return bmp;
			}
			catch(Exception e)
			{
                // [ZZ] try to make a guessed reader
                switch (guesstype)
                {
                    case ImageDataFormat.DOOMPICTURE:
                        // Check if data is valid for a doom picture
                        stream.Seek(0, SeekOrigin.Begin);
                        DoomPictureReader picreader = new DoomPictureReader(guesspalette);
                        if (picreader.Validate(stream)) proxyreader = picreader;
                        break;

                    case ImageDataFormat.DOOMFLAT:
                        // Check if data is valid for a doom flat
                        stream.Seek(0, SeekOrigin.Begin);
                        DoomFlatReader flatreader = new DoomFlatReader(guesspalette);
                        if (flatreader.Validate(stream)) proxyreader = flatreader;
                        break;

                    case ImageDataFormat.DOOMCOLORMAP:
                        // Check if data is valid for a doom colormap
                        stream.Seek(0, SeekOrigin.Begin);
                        DoomColormapReader colormapreader = new DoomColormapReader(guesspalette);
                        if (colormapreader.Validate(stream)) proxyreader = colormapreader;
                        break;
                }

                if (proxyreader != null)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return proxyreader.ReadAsBitmap(stream);
                }

                // Unable to make bitmap
                General.ErrorLogger.Add(ErrorType.Error, "Unable to make file image. " + e.GetType().Name + ": " + e.Message);
				return null;
			}
		}

		// This draws the picture to the given pixel color data
		// Throws exception on failure
		public void DrawToPixelData(Stream stream, PixelColor* target, int targetwidth, int targetheight, int x, int y)
		{
			// Get bitmap
			Bitmap bmp = ReadAsBitmap(stream);
			if(bmp != null)
			{
				int width = bmp.Size.Width;
				int height = bmp.Size.Height;

				// Lock bitmap pixels
				BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				PixelColor* pixels = (PixelColor*)bmpdata.Scan0.ToPointer();

				// Go for all pixels in the original image
				for(int ox = 0; ox < width; ox++)
				{
					for(int oy = 0; oy < height; oy++)
					{
						// Copy this pixel?
						if(pixels[oy * width + ox].a > 0.5f)
						{
							// Calculate target pixel and copy when within bounds
							int tx = x + ox;
							int ty = y + oy;
							if((tx >= 0) && (tx < targetwidth) && (ty >= 0) && (ty < targetheight))
								target[ty * targetwidth + tx] = pixels[oy * width + ox];
						}
					}
				}

				// Done
				bmp.UnlockBits(bmpdata);
				bmp.Dispose();
			}
			else
			{
				throw new InvalidDataException();
			}
		}

		//mxd
		private static int ToInt32BigEndian(byte[] buffer)
		{
			return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
		}

        public bool Validate(Stream stream)
        {
            uint _vimageid = 0;
            try
            {
                byte[] bytes;
                if (imagebytes == null)
                {
                    bytes = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(bytes, 0, bytes.Length);
                    imagebytes = bytes;
                }
                else bytes = imagebytes;

                // Create an image in DevIL
                ilGenImages(1, new IntPtr(&_vimageid));
                ilBindImage(_vimageid);

                // Read image data from stream
                fixed (byte* bptr = bytes)
                {
                    if (!ilLoadL(imagetype, new IntPtr(bptr), (uint)bytes.Length))
                        throw new BadImageFormatException();
                }

                // [ZZ] check if there was any error code
                DevilError ilerror = (DevilError)ilGetError();
                if (ilerror != DevilError.IL_NO_ERROR)
                    throw new BadImageFormatException("DevIL error: " + ilerror.ToString());

                // Get the image properties
                int width = ilGetInteger(IL_IMAGE_WIDTH);
                int height = ilGetInteger(IL_IMAGE_HEIGHT);
                if ((width < 1) || (height < 1))
                    throw new BadImageFormatException();

                // Clean up
                ilDeleteImages(1, new IntPtr(&_vimageid));
                _vimageid = 0;
            }
            catch (Exception /*e*/)
            {
                //General.ErrorLogger.Add(ErrorType.Warning, e.ToString());
                return false;
            }
            finally
            {
                if (_vimageid != 0)
                {
                    try
                    {
                        ilDeleteImages(1, new IntPtr(&_vimageid));
                    }
                    catch (Exception /*e_nested*/)
                    {
                        /* really really failed */
                    }
                }
            }

            return true;
        }
		
		#endregion
	}
}
