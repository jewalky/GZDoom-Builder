
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

using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Geometry
{
	/// <summary>
	/// This is used to indicate a side of a line without the need for a sidedef.
	/// </summary>
	public sealed class LinedefSide
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private Linedef line;
		private bool front;
		private bool ignore; //mxd
		
		#endregion

		#region ================== Properties

		public Linedef Line { get { return line; } set { line = value; } }
		public bool Front { get { return front; } set { front = value; } }
		public bool Ignore { get { return ignore; } set { ignore = value; } } //mxd

		#endregion

		#region ================== Constructor / Disposer

		/// <summary>
		/// This is used to indicate a side of a line without the need for a sidedef.
		/// </summary>
		public LinedefSide(Linedef line, bool front)
		{
			// Initialize
			this.line = line;
			this.front = front;
		}

		/// <summary>
		/// This makes a copy of the linedef side.
		/// </summary>
		public LinedefSide(LinedefSide original)
		{
			// Initialize
			this.line = original.line;
			this.front = original.front;
		}

		#endregion

		#region ================== Methods

		// This compares a linedef side
		public static bool operator ==(LinedefSide a, LinedefSide b)
		{
			if((object.Equals(a, null)) && (object.Equals(b, null))) return true;
			if((!object.Equals(a, null)) && (object.Equals(b, null))) return false;
			if(object.Equals(a, null)) return false;
			return (a.line == b.line) && (a.front == b.front);
		}

		// This compares a linedef side
		public static bool operator !=(LinedefSide a, LinedefSide b)
		{
			if((object.Equals(a, null)) && (object.Equals(b, null))) return false;
			if((!object.Equals(a, null)) && (object.Equals(b, null))) return true;
			if(object.Equals(a, null)) return true;
			return (a.line != b.line) || (a.front != b.front);
		}

		//mxd. Addeed to make compiler a bit more happy...
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//mxd. Addeed to make compiler a bit more happy...
		public override bool Equals(object obj) 
		{
			if(object.Equals(obj, null)) return false;
			LinedefSide other = (LinedefSide)obj;
			return (this.line == other.line) && (this.front == other.front);
		}

#if DEBUG
		//mxd. Useful when debugging...
		public override string ToString()
		{
			Sidedef side = (front ? line.Front : line.Back);
			Sector sector = (side != null ? side.Sector : null);
			return line + " (" + (front ? "front" : "back") + ")" + (sector != null ? ", Sector " + sector.Index : ", no sector");
		}
#endif

		#endregion
	}
}
