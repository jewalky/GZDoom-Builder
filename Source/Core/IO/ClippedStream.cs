
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
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	internal class ClippedStream : Stream
	{
		#region ================== Variables

		// Base stream
		private Stream basestream;

		// Data limit
		private int offset;
		private int length;
		private long position;

		// Disposing
		private bool isdisposed;
		
		#endregion

		#region ================== Properties

        public Stream BaseStream { get { return basestream; } }
		public override long Length { get { return length; } }
		public override long Position { get { return position; } set { this.Seek(value, SeekOrigin.Begin); } }
		public override bool CanRead { get { return basestream.CanRead; } }
		public override bool CanSeek { get { return basestream.CanSeek; } }
		public override bool CanWrite { get { return basestream.CanWrite; }	}
		public bool IsDisposed { get { return isdisposed; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public ClippedStream(Stream basestream, int offset, int length)
		{
			// Can only create from a stream that can seek
			if(!basestream.CanSeek) throw new ArgumentException("ClippedStream can only be created with a Stream that allows Seeking.");

			// Initialize
			this.basestream = basestream;
			this.position = 0;
			this.offset = offset;
			this.length = length;

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public new void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Already set isdisposed to prevent recursion
				isdisposed = true;
				
				// Clean up
				basestream = null;
				
				// Dispose base
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		// This flushes the written changes
		public override void Flush()
		{
			// Flush base stream
			basestream.Flush();
		}

		// This reads from the stream
		public override int Read(byte[] buffer, int offset, int count)
		{
			// Check if this exceeds limits
			if((this.position + count) > (this.length + 1))
			{
				// Read only within limits
				count = this.length - (int)this.position;
			}

			// Anything to read?
			if(count > 0)
			{
				// Seek if needed
				if(basestream.Position != (this.offset + this.position))
					basestream.Seek(this.offset + this.position, SeekOrigin.Begin);

				// Read from base stream
				position += count;
				return basestream.Read(buffer, offset, count);
			}
			else
			{
				return 0;
			}
		}

		// This writes to the stream
		public override void Write(byte[] buffer, int offset, int count)
		{
			// Check if this exceeds limits
			if((this.position + count) > (this.length + 1))
				throw new ArgumentException("Attempted to write outside the range of the stream.");

			// Seek if needed
			if(basestream.Position != (this.offset + this.position))
				basestream.Seek(this.offset + this.position, SeekOrigin.Begin);

			// Read from base stream
			position += count;
			basestream.Write(buffer, offset, count);
		}
		
		// Seek within clipped buffer
		public override long Seek(long offset, SeekOrigin origin)
		{
			// Seeking from beginning
			if(origin == SeekOrigin.Begin)
			{
				// Check if this exceeds limits
				if((offset > this.length) || (offset < 0))
					throw new ArgumentException("Attempted to seek outside the range of the stream.");
				
				// Seek
				position = basestream.Seek(this.offset + offset, SeekOrigin.Begin) - this.offset;
			}
			// Seeking from current position
			else if(origin == SeekOrigin.Current)
			{
				// Check if this exceeds limits
				if((this.position + offset > this.length) || (this.position + offset < 0))
					throw new ArgumentException("Attempted to seek outside the range of the stream.");

				// Seek
				position = basestream.Seek(this.offset + this.position + offset, SeekOrigin.Begin) - this.offset;
			}
			// Seeking from end
			else
			{
				// Check if this exceeds limits
				if((offset > 0) || (this.length + offset < 0))
					throw new ArgumentException("Attempted to seek outside the range of the stream.");

				// Seek
				position = basestream.Seek(this.offset + this.length + offset, SeekOrigin.Begin) - this.offset;
			}

			// Return new position
			return position;
		}

		// Change the length of the steam
		public override void SetLength(long value)
		{
			// Not supported
			throw new NotSupportedException("This operation is not supported.");
		}

		// Asynchronous read from stream
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			// Check if this exceeds limits
			if((this.position + count) > (this.length + 1))
				throw new ArgumentException("Attempted to read outside the range of the stream.");

			// Seek if needed
			if(basestream.Position != (this.offset + this.position))
				basestream.Seek(this.offset + this.position, SeekOrigin.Begin);
			
			// Read
			position += count;
			return base.BeginRead(buffer, offset, count, callback, state);
		}

		// Asynchronous write to stream
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			// Check if this exceeds limits
			if((this.position + count) > (this.length + 1))
				throw new ArgumentException("Attempted to write outside the range of the stream.");

			// Seek if needed
			if(basestream.Position != (this.offset + this.position))
				basestream.Seek(this.offset + this.position, SeekOrigin.Begin);
			
			// Write
			position += count;
			return base.BeginWrite(buffer, offset, count, callback, state);
		}

		// This closes the stream
		public override void Close()
		{
			basestream = null;
			base.Close();
		}
		
		// This reads a single byte from the stream
		public override int ReadByte()
		{
			// Check if this exceeds limits
			if((this.position + 1) > (this.length + 1))
				throw new ArgumentException("Attempted to read outside the range of the stream.");

			// Seek if needed
			if(basestream.Position != (this.offset + this.position))
				basestream.Seek(this.offset + this.position, SeekOrigin.Begin);

			// Read from base stream
			position++;
			return basestream.ReadByte();
		}

		// This writes a single byte to the stream
		public override void WriteByte(byte value)
		{
			// Check if this exceeds limits
			if((this.position + 1) > (this.length + 1))
				throw new ArgumentException("Attempted to write outside the range of the stream.");

			// Seek if needed
			if(basestream.Position != (this.offset + this.position))
				basestream.Seek(this.offset + this.position, SeekOrigin.Begin);

			// Read from base stream
			position++;
			basestream.WriteByte(value);
		}
		
		// This returns all the bytes in the stream
		public byte[] ReadAllBytes()
		{
			byte[] bytes = new byte[length];
			Seek(0, SeekOrigin.Begin);
			Read(bytes, 0, length);
			return bytes;
		}
		
		#endregion
	}
}
