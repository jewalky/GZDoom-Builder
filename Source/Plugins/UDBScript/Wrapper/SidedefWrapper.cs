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
	class SidedefWrapper : MapElementWrapper
	{
		#region ================== Variables

		private Sidedef sidedef;

		#endregion

		#region ================== Properties

		/// <summary>
		/// The sidedef's index. Read-only.
		/// </summary>
		public int index
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the isFront property can not be accessed.");

				return sidedef.Index;
			}
		}

		/// <summary>
		/// `true` if this sidedef is the front of its linedef, otherwise `false`. Read-only.
		/// </summary>
		public bool isFront
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the isFront property can not be accessed.");

				return sidedef.IsFront;
			}
		}

		/// <summary>
		/// The sector the sidedef belongs to. Read-only.
		/// </summary>
		public SectorWrapper sector
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the sector property can not be accessed.");

				return new SectorWrapper(sidedef.Sector);
			}
		}

		/// <summary>
		/// The sidedef on the other side of this sidedef's linedef. Returns `null` if there is no other. Read-only.
		/// </summary>
		public SidedefWrapper other
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the other property can not be accessed.");

				if (sidedef.Other == null)
					return null;

				return new SidedefWrapper(sidedef.Other);
			}
		}

		/// <summary>
		/// The sidedef's angle in degrees. Read-only.
		/// </summary>
		public double angle
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the angle property can not be accessed.");

				return Angle2D.RadToDeg(sidedef.Angle);
			}
		}

		/// <summary>
		/// The sidedef's angle in radians. Read-only.
		/// </summary>
		public double angleRad
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the angleRad property can not be accessed.");

				return sidedef.Angle;
			}
		}

		/// <summary>
		/// The x offset of the sidedef's textures.
		/// </summary>
		public int offsetX
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the offsetX property can not be accessed.");

				return sidedef.OffsetX;
			}
			set
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the offsetX property can not be accessed.");

				sidedef.OffsetX = value;
			}
		}

		/// <summary>
		/// The y offset of the sidedef's textures.
		/// </summary>
		public int offsetY
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the offsetY property can not be accessed.");

				return sidedef.OffsetY;
			}
			set
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the offsetY property can not be accessed.");

				sidedef.OffsetY = value;
			}
		}

		/// <summary>
		/// Sidedef flags. It's an object with the flags as properties. Only available in UDMF
		///
		/// ```
		/// s.flags['noattack'] = true; // Monsters in this sector don't attack
		/// s.flags.noattack = true; // Also works
		/// ```
		/// </summary>
		public ExpandoObject flags
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the flags property can not be accessed.");

				dynamic eo = new ExpandoObject();
				IDictionary<string, object> o = eo as IDictionary<string, object>;

				foreach (KeyValuePair<string, bool> kvp in sidedef.GetFlags())
				{
					o.Add(kvp.Key, kvp.Value);
				}

				// Create event that gets called when a property is changed. This sets the flag
				((INotifyPropertyChanged)eo).PropertyChanged += new PropertyChangedEventHandler((sender, ea) => {
					PropertyChangedEventArgs pcea = ea as PropertyChangedEventArgs;
					IDictionary<string, object> so = sender as IDictionary<string, object>;

					// Only allow known flags to be set
					if (!General.Map.Config.SectorFlags.Keys.Contains(pcea.PropertyName))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Flag name '" + pcea.PropertyName + "' is not valid.");

					// New value must be bool
					if (!(so[pcea.PropertyName] is bool))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Flag values must be bool.");

					sidedef.SetFlag(pcea.PropertyName, (bool)so[pcea.PropertyName]);
				});

				return eo;
			}
		}

		/// <summary>
		/// The sidedef's upper texture
		/// </summary>
		public string upperTexture
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the upperTexture property can not be accessed.");

				return sidedef.HighTexture;
			}
			set
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the upperTexture property can not be accessed.");

				sidedef.SetTextureHigh(value);

				// Make sure to update the used textures, so that they are shown immediately
				General.Map.Data.UpdateUsedTextures();
			}
		}

		/// <summary>
		/// The sidedef's middle texture
		/// </summary>
		public string middleTexture
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the middleTexture property can not be accessed.");

				return sidedef.MiddleTexture;
			}
			set
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the middleTexture property can not be accessed.");

				sidedef.SetTextureMid(value);

				// Make sure to update the used textures, so that they are shown immediately
				General.Map.Data.UpdateUsedTextures();
			}
		}

		/// <summary>
		/// The sidedef's middle texture
		/// </summary>
		public string lowerTexture
		{
			get
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the lowerTexture property can not be accessed.");

				return sidedef.LowTexture;
			}
			set
			{
				if (sidedef.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sidedef is disposed, the lowerTexture property can not be accessed.");

				sidedef.SetTextureLow(value);

				// Make sure to update the used textures, so that they are shown immediately
				General.Map.Data.UpdateUsedTextures();
			}
		}

		#endregion

		#region ================== Constructors

		internal SidedefWrapper(Sidedef sidedef) : base(sidedef)
		{
			this.sidedef = sidedef;
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
			return sidedef.ToString();
		}

		#endregion
	}
}
