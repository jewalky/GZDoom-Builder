#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	class LinedefWrapper : MapElementWrapper, IMoreTags, IEquatable<LinedefWrapper>
	{
		#region ================== Variables

		private Linedef linedef;
		private MapElementArgumentsWrapper elementargs;

		#endregion

		#region IEquatable<LinedefWrapper> members

		public bool Equals(LinedefWrapper other)
		{
			return linedef == other.linedef;
		}

		public override bool Equals(object obj)
		{
			return Equals((LinedefWrapper)obj);
		}

		public override int GetHashCode()
		{
			return linedef.GetHashCode();
		}

		#endregion

		#region ================== Properties

		/// <summary>
		/// The linedef's index. Read-only.
		/// </summary>
		public int index
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the index property can not be accessed.");

				return linedef.Index;
			}
		}

		/// <summary>
		/// The linedef's start `Vertex`.
		/// </summary>
		public VertexWrapper start
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the start property can not be accessed.");

				return new VertexWrapper(linedef.Start);
			}
		}

		/// <summary>
		/// The linedef's end `Vertex`.
		/// </summary>
		public VertexWrapper end
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the end property can not be accessed.");

				return new VertexWrapper(linedef.End);
			}
		}

		/// <summary>
		/// The `Linedef`'s front `Sidedef`. Is `null` when there is no front (should not happen).
		/// </summary>
		public SidedefWrapper front
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the front property can not be accessed.");

				if (linedef.Front == null)
					return null;

				return new SidedefWrapper(linedef.Front);
			}
		}

		/// <summary>
		/// The `Linedef`'s back `Sidedef`. Is `null` when there is no back.
		/// </summary>
		public SidedefWrapper back
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the back property can not be accessed.");

				if (linedef.Back == null)
					return null;

				return new SidedefWrapper(linedef.Back);
			}
		}

		/// <summary>
		/// The `Line2D` from the `start` to the `end` `Vertex`.
		/// </summary>
		public Line2DWrapper line
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the line property can not be accessed.");

				return new Line2DWrapper(linedef.Line);
			}
		}

		/// <summary>
		/// If the `Linedef` is selected or not.
		/// </summary>
		public bool selected
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the selected property can not be accessed.");

				return linedef.Selected;
			}
			set
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the selected property can not be accessed.");

				linedef.Selected = value;
			}
		}

		/// <summary>
		/// If the `Linedef` is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).
		/// </summary>
		public bool marked
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the marked property can not be accessed.");

				return linedef.Marked;
			}
			set
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the marked property can not be accessed.");

				linedef.Marked = value;
			}
		}

		/// <summary>
		/// `Linedef` flags. It's an object with the flags as properties. In Doom format and Hexen format they are identified by numbers, in UDMF by their name.
		/// Doom and Hexen:
		/// ```
		/// ld.flags['64'] = true; // Set the block sound flag
		/// ```
		/// UDMF:
		/// ```
		/// ld.flags['blocksound'] = true; // Set the block sound flag
		/// ld.flags.blocksound = true; // Also works
		/// ```
		/// </summary>
		public ExpandoObject flags
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the flags property can not be accessed.");

				dynamic eo = new ExpandoObject();
				IDictionary<string, object> o = eo as IDictionary<string, object>;

				foreach (KeyValuePair<string, bool> kvp in linedef.GetFlags())
				{
					o.Add(kvp.Key, kvp.Value);
				}

				// Create event that gets called when a property is changed. This sets the flag
				((INotifyPropertyChanged)eo).PropertyChanged += new PropertyChangedEventHandler((sender, ea) => {
					PropertyChangedEventArgs pcea = ea as PropertyChangedEventArgs;
					IDictionary<string, object> so = sender as IDictionary<string, object>;

					// Only allow known flags to be set
					if (!General.Map.Config.LinedefFlags.Keys.Contains(pcea.PropertyName))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Flag name '" + pcea.PropertyName + "' is not valid.");

					// New value must be bool
					if (!(so[pcea.PropertyName] is bool))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Flag values must be bool.");

					linedef.SetFlag(pcea.PropertyName, (bool)so[pcea.PropertyName]);
				});

				return eo;
			}
		}

		/// <summary>
		/// `Array` of arguments of the `Linedef`. Number of arguments depends on game config (usually 5). Hexen format and UDMF only.
		/// </summary>
		public MapElementArgumentsWrapper args
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the args property can not be accessed.");

				return elementargs;
			}
		}

		/// <summary>
		/// `Linedef` action.
		/// </summary>
		public int action
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the action property can not be accessed.");

				return linedef.Action;
			}
			set
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the action property can not be accessed.");

				linedef.Action = value;
			}
		}

		/// <summary>
		/// `Linedef` tag. UDMF only.
		/// </summary>
		public int tag
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the tag property can not be accessed.");

				return linedef.Tag;
			}
			set
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the tag property can not be accessed.");

				linedef.Tag = value;
			}
		}

		/// <summary>
		/// The `Linedef`'s squared length. Read-only.
		/// </summary>
		public double lengthSq
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the lengthSq property can not be accessed.");

				return linedef.LengthSq;
			}
		}

		/// <summary>
		/// The `Linedef`'s length. Read-only.
		/// </summary>
		public double length
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the length property can not be accessed.");

				return linedef.Length;
			}
		}

		/// <summary>
		/// 1.0 / length. Read-only.
		/// </summary>
		public double lengthInv
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the lengthInv property can not be accessed.");

				return linedef.LengthInv;
			}
		}

		/// <summary>
		/// The `Linedef`'s angle in degree. Read-only.
		/// </summary>
		public int angle
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the angle property can not be accessed.");

				return linedef.AngleDeg;
			}
		}

		/// <summary>
		/// The `Linedef`'s angle in radians. Read-only.
		/// </summary>
		public double angleRad
		{
			get
			{
				if (linedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the angleRad property can not be accessed.");

				return linedef.Angle;
			}
		}

		#endregion

		#region ================== Constructors

		internal LinedefWrapper(Linedef linedef) : base(linedef)
		{
			this.linedef = linedef;
			elementargs = new MapElementArgumentsWrapper(linedef);
		}

		#endregion

		#region ================== Update

		internal override void AfterFieldsUpdate()
		{
		}

		#endregion

		#region ================== Methods

		public override string ToString()
		{
			return linedef.ToString();
		}

		/// <summary>
		/// Copies the properties of this `Linedef` to another `Linedef`.
		/// </summary>
		/// <param name="other">The `Linedef` to copy the properties to</param>
		public void copyPropertiesTo(LinedefWrapper other)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the copyPropertiesTo method can not be accessed.");

			linedef.CopyPropertiesTo(other.linedef);
		}

		/// <summary>
		/// Clears all flags.
		/// </summary>
		public void clearFlags()
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the clearFlags medhot can not be accessed.");

			linedef.ClearFlags();
		}

		/// <summary>
		/// Flips the `Linedef`'s vertex attachments.
		/// </summary>
		public void flipVertices()
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the flipVertices method can not be accessed.");

			linedef.FlipVertices();
		}

		/// <summary>
		/// Flips the `Linedef`'s `Sidedefs`.
		/// </summary>
		public void flipSidedefs()
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the flipSidedefs method can not be accessed.");

			linedef.FlipSidedefs();
		}

		/// <summary>
		/// Gets a `Vector2D` for testing on one side. The `Vector2D` is on the front when `true` is passed, otherwise on the back.
		/// </summary>
		/// <param name="front">`true` for front, `false` for back</param>
		/// <returns>`Vector2D` that's either on the front of back of the Linedef</returns>
		public Vector2DWrapper getSidePoint(bool front)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the getSidePoint method can not be accessed.");

			return new Vector2DWrapper(linedef.GetSidePoint(front));
		}

		/// <summary>
		/// Gets a `Vector2D` that's in the center of the `Linedef`.
		/// </summary>
		/// <returns>`Vector2D` in the center of the `Linedef`</returns>
		public Vector2DWrapper getCenterPoint()
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the getCenterPoint method can not be accessed.");

			return new Vector2DWrapper(linedef.GetCenterPoint());
		}

		/// <summary>
		/// Automatically sets the blocking and two-sided flags based on the existing `Sidedef`s.
		/// </summary>
		public void applySidedFlags()
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the applySidedFlags method can not be accessed.");

			linedef.ApplySidedFlags();
		}

		/// <summary>
		/// Get a `Vector2D` that's *on* the line, closest to `pos`. `pos` can either be a `Vector2D`, or an array of numbers.
		/// ```
		/// var v1 = ld.nearestOnLine(new Vector2D(32, 64));
		/// var v2 = ld.nearestOnLine([ 32, 64 ]);
		/// ```
		/// </summary>
		/// <param name="pos">Point to check against</param>
		/// <returns>`Vector2D` that's on the linedef</returns>
		public Vector2DWrapper nearestOnLine(object pos)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the nearestOnLine method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
				return new Vector2DWrapper(linedef.NearestOnLine(v));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the shortest "safe" squared distance from `pos` to the line. If `bounded` is `true` that means that the not the whole line's length will be used, but `lengthInv` less at the start and end.
		/// </summary>
		/// <param name="pos">Point to check against</param>
		/// <param name="bounded">`true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used</param>
		/// <returns>Squared distance to the line</returns>
		public double safeDistanceToSq(object pos, bool bounded)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the safeDistanceToSq method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
				return linedef.SafeDistanceToSq(v, bounded);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the shortest "safe" distance from `pos` to the line. If `bounded` is `true` that means that the not the whole line's length will be used, but `lengthInv` less at the start and end.
		/// </summary>
		/// <param name="pos">Point to check against</param>
		/// <param name="bounded">`true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used</param>
		/// <returns>Distance to the line</returns>
		public double safeDistanceTo(object pos, bool bounded)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the safeDistanceTo method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
				return linedef.SafeDistanceTo(v, bounded);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the shortest squared distance from `pos` to the line.
		/// </summary>
		/// <param name="pos">Point to check against</param>
		/// <param name="bounded">`true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used</param>
		/// <returns>Squared distance to the line</returns>
		public double distanceToSq(object pos, bool bounded)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the distanceToSq method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
				return linedef.DistanceToSq(v, bounded);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the shortest distance from `pos` to the line.
		/// </summary>
		/// <param name="pos">Point to check against</param>
		/// <param name="bounded">`true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used</param>
		/// <returns>Distance to the line</returns>
		public double distanceTo(object pos, bool bounded)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the distanceTo method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
				return linedef.DistanceTo(v, bounded);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Tests which side of the `Linedef` `pos` is on. Returns &lt; 0 for front (right) side, &gt; for back (left) side, and 0 if `pos` is on the line.
		/// </summary>
		/// <param name="pos">Point to check against</param>
		/// <returns>&lt; 0 for front (right) side, &gt; for back (left) side, and 0 if `pos` is on the line</returns>
		public double sideOfLine(object pos)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the sideOfLine method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
				return linedef.SideOfLine(v);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Splits the `Linedef` at the given `Vertex`. The result will be two lines, from the start `Vertex` of the `Linedef` to `v`, and from `v` to the end `Vertex` of the `Linedef`.
		/// </summary>
		/// <param name="v">`Vertex` to split by</param>
		/// <returns>The newly created `Linedef`</returns>
		public LinedefWrapper split(VertexWrapper v)
		{
			if (linedef.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Linedef is disposed, the split method can not be accessed.");

			return new LinedefWrapper(linedef.Split(v.Vertex));
		}

		/// <summary>
		/// Deletes the `Linedef`. Note that this will result in unclosed `Sector`s unless it has the same `Sector`s on both sides.
		/// </summary>
		public void delete()
		{
			if (linedef.IsDisposed)
				return;

			linedef.Dispose();
		}

		#endregion

		#region ================== Interface implementations

		/// <summary>
		/// Returns an `Array` of the `Linedef`'s tags. UDMF only. Supported game configurations only.
		/// </summary>
		/// <returns>`Array` of tags</returns>
		public int[] getTags()
		{
			return linedef.Tags.ToArray();
		}

		/// <summary>
		/// Adds a tag to the `Linedef`. UDMF only. Supported game configurations only.
		/// </summary>
		/// <param name="tag">Tag to add</param>
		/// <returns>`true` when the tag was added, `false` when the tag already exists</returns>
		public bool addTag(int tag)
		{
			if (linedef.Tags.Contains(tag))
				return false;

			linedef.Tags.Add(tag);

			return true;
		}

		/// <summary>
		/// Removes a tag from the `Linedef`. UDMF only. Supported game configurations only.
		/// </summary>
		/// <param name="tag">Tag to remove</param>
		/// <returns>`true` when the tag was removed successfully, `false` when the tag did not exist</returns>
		public bool removeTag(int tag)
		{
			return linedef.Tags.Remove(tag);
		}

		#endregion
	}
}
