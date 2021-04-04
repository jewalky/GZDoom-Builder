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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	internal class ThingWrapper : MapElementWrapper
	{
		#region ================== Variables

		private Thing thing;
		private MapElementArgumentsWrapper elementargs;

		#endregion

		#region ================== Properties

		/// <summary>
		/// Index of the thing. Read-only
		/// </summary>
		public int index
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the index property can not be accessed.");

				return thing.Index;
			}
		}

		/// <summary>
		/// Type of the thing
		/// </summary>
		public int type
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the type property can not be accessed.");

				return thing.Type;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the type property can not be accessed.");

				thing.Type = value;
				thing.UpdateConfiguration();
			}
		}

		/// <summary>
		/// Angle of the thing in degrees, see https://doomwiki.org/wiki/Angle
		/// </summary>
		public int angle
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the angle property can not be accessed.");

				return thing.AngleDoom;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the angle property can not be accessed.");

				thing.Rotate(value);
			}
		}

		/// <summary>
		/// Angle of the thing in radians
		/// </summary>
		public double angleRad
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the angleRad property can not be accessed.");

				return thing.Angle;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the angleRad property can not be accessed.");

				thing.Rotate(value);
			}
		}

		/// <summary>
		/// Array of arguments of the thing. Number of arguments depends on game config (usually 5). Hexen format and UDMF only.
		/// </summary>
		public MapElementArgumentsWrapper args
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the args property can not be accessed.");

				return elementargs;
			}
		}

		/// <summary>
		/// Thing action. Hexen and UDMF only.
		/// </summary>
		public int action
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the action property can not be accessed.");

				return thing.Action;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the angle property can not be accessed.");

				thing.Action = value;
			}
		}

		/// <summary>
		/// Thing tag. UDMF only.
		/// </summary>
		public int tag
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the tag property can not be accessed.");

				return thing.Tag;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the tag property can not be accessed.");

				thing.Tag = value;
			}
		}

		/// <summary>
		/// If the thing is selected or not
		/// </summary>
		public bool selected
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the selected property can not be accessed.");

				return thing.Selected;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the selected property can not be accessed.");

				thing.Selected = value;
			}
		}

		/// <summary>
		/// If the thing is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).
		/// </summary>
		public bool marked
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the marked property can not be accessed.");

				return thing.Marked;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the marked property can not be accessed.");

				thing.Marked = value;
			}
		}

		/// <summary>
		/// Thing flags. It's an object with the flags as properties. In Doom format and Hexen format they are identified by numbers, in UDMF by their name.
		/// Doom and Hexen:
		/// ```
		/// t.flags["8"] = true; // Set the ambush flag
		/// ```
		/// UDMF:
		/// ```
		/// t.flags['ambush'] = true; // Set the ambush flag
		/// t.flags.ambush = true; // Also works
		/// ```
		/// </summary>
		public ExpandoObject flags
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the flags property can not be accessed.");

				dynamic eo = new ExpandoObject();
				IDictionary<string, object> o = eo as IDictionary<string, object>;

				foreach (KeyValuePair<string, bool> kvp in thing.GetFlags())
				{
					o.Add(kvp.Key, kvp.Value);
				}

				// Create event that gets called when a property is changed. This sets the flag
				((INotifyPropertyChanged)eo).PropertyChanged += new PropertyChangedEventHandler((sender, ea) => {
					PropertyChangedEventArgs pcea = ea as PropertyChangedEventArgs;
					IDictionary<string, object> so = sender as IDictionary<string, object>;

					// Only allow known flags to be set
					if(!General.Map.Config.ThingFlags.Keys.Contains(pcea.PropertyName))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Flag name '" + pcea.PropertyName + "' is not valid.");

					// New value must be bool
					if (!(so[pcea.PropertyName] is bool))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Flag values must be bool.");

					thing.SetFlag(pcea.PropertyName, (bool)so[pcea.PropertyName]);
				});

				return eo;
			}
		}

		/// <summary>
		/// Position of the thing. It's an object with `x`, `y`, and `z` properties. The latter is only relevant in Hexen format and UDMF.
		/// The `x`, `y`, and `z` accept numbers:
		/// ```
		/// t.position.x = 32;
		/// t.position.y = 64;
		/// ```
		/// It's also possible to set all fields immediately by assigning either a `Vector2D`, `Vector3D`, or an array of numbers:
		/// ```
		/// t.position = new Vector2D(32, 64);
		/// t.position = new Vector3D(32, 64, 128);
		/// t.position = [ 32, 64 ];
		/// t.position = [ 32, 64, 128 ];
		/// ```
		/// </summary>
		public object position
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the position property can not be accessed.");

				dynamic eo = new ExpandoObject();
				IDictionary<string, object> o = eo as IDictionary<string, object>;

				// Create x, y, and z properties
				o.Add("x", thing.Position.x);
				o.Add("y", thing.Position.y);
				o.Add("z", thing.Position.z);

				// Create event that gets called when a property is changed. This moves the thing to the given position
				((INotifyPropertyChanged)eo).PropertyChanged += new PropertyChangedEventHandler((sender, ea) => {
					string[] allowedproperties = new string[] { "x", "y", "z" };

					// Give us easier access to the variables
					var pcea = ea as PropertyChangedEventArgs;
					IDictionary<string, object> so = sender as IDictionary<string, object>;
					
					// Make sure the changed property is a valid one
					if (!allowedproperties.Contains(pcea.PropertyName))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Invalid property '" + pcea.PropertyName + "' given. Only x, y, and z are allowed.");

					if (!(so[pcea.PropertyName] is double))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Position values must be a number.");

					double x = (double)so["x"];
					double y = (double)so["y"];
					double z = (double)so["z"];

					thing.Move(x, y, z);

				});

				return eo;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the position property can not be accessed.");

				if (value is Vector2D)
					thing.Move((Vector2D)value);
				else if (value is Vector3D)
					thing.Move((Vector3D)value);
				else if(value.GetType().IsArray)
				{
					object[] vals = (object[])value;

					// Make sure all values in the array are doubles
					foreach(object v in vals)
						if(!(v is double))
							throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Values in position array must be numbers.");

					if (vals.Length == 2)
						thing.Move((double)vals[0], (double)vals[1], thing.Position.z);
					else if(vals.Length == 3)
						thing.Move((double)vals[0], (double)vals[1], (double)vals[2]);
					else
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Position array must contain 2 or 3 values.");
				}					
				else
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Position values must be a Vector2D, Vector3D, or an array of numbers.");
			}
		}

		/// <summary>
		/// Pitch of the thing in degrees. Only valid for supporting game configurations.
		/// </summary>
		public int pitch
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the pitch property can not be accessed.");

				return thing.Pitch;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the pitch property can not be accessed.");

				thing.SetPitch(value);
			}
		}

		/// <summary>
		/// Roll of the thing in degrees. Only valid for supporting game configurations.
		/// </summary>
		public int roll
		{
			get
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the roll property can not be accessed.");

				return thing.Roll;
			}
			set
			{
				if (thing.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the roll property can not be accessed.");

				thing.SetRoll(value);
			}
		}

		#endregion

		#region ================== Constructors

		internal ThingWrapper(Thing thing) : base(thing)
		{
			this.thing = thing;
			elementargs = new MapElementArgumentsWrapper(thing);
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
			return thing.ToString();
		}

		/// <summary>
		/// Copies the properties from this thing to another.
		/// </summary>
		/// <param name="t">the thing to copy the properties to</param>
		public void copyPropertiesTo(ThingWrapper t)
		{
			if (thing.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the copyPropertiesTo method can not be accessed.");

			thing.CopyPropertiesTo(t.thing);
		}

		/// <summary>
		/// Clears all flags.
		/// </summary>
		public void clearFlags()
		{
			if (thing.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the cleaFlags method can not be accessed.");

			thing.ClearFlags();
		}

		/// <summary>
		/// Snaps the thing's position to the grid.
		/// </summary>
		public void snapToGrid()
		{
			if (thing.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the snapToGrid method can not be accessed.");

			thing.SnapToGrid();
		}

		/// <summary>
		/// Snaps the thing's position to the map format's accuracy
		/// </summary>
		public void snapToAccuracy()
		{
			if (thing.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the snapToAccuracy method can not be accessed.");

			thing.SnapToAccuracy();
		}

		/// <summary>
		/// Gets the squared distance between this thing and the given point.
		/// The point can be either a `Vector2D` or an array of numbers.
		/// ```
		/// t.distanceToSq(new Vector2D(32, 64));
		/// t.distanceToSq([ 32, 64 ]);
		/// ```
		/// </summary>
		/// <param name="pos">Point to calculate the squared distance to.</param>
		/// <returns>Distance to `pos`</returns>
		public double distanceToSq(Vector2D pos)
		{
			if (thing.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the distanceToSq method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
				return thing.DistanceToSq(pos);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the distance between this thing and the given point.
		/// The point can be either a `Vector2D` or an array of numbers.
		/// ```
		/// t.distanceToSq(new Vector2D(32, 64));
		/// t.distanceToSq([ 32, 64 ]);
		/// ```
		/// </summary>
		/// <param name="pos">Point to calculate the distance to.</param>
		/// <returns>Distance to `pos`</returns>
		public double distanceTo(Vector2D pos)
		{
			if (thing.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the distanceTo method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
				return thing.DistanceTo(pos);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Deletes the thing
		/// </summary>
		public void delete()
		{
			//if (thing.IsDisposed)
			//	throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing is disposed, the dispose method can not be accessed.");

			if (!thing.IsDisposed)
			{
				thing.Dispose();
				General.Map.ThingsFilter.Update();
			}
		}

		#endregion
	}
}
