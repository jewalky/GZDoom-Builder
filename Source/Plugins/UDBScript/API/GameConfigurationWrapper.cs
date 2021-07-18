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
#endregion

namespace CodeImp.DoomBuilder.UDBScript.API
{
	class GameConfigurationWrapper
	{
		#region ================== Properties

		/// <summary>
		/// Engine name, like `doom`, `boom`, `zdoom` etc. Used for the namespace in UDMF maps. Read-only.
		/// </summary>
		public string engineName
		{
			get
			{
				return General.Map.Config.EngineName;
			}
		}

		/// <summary>
		/// If the game configuration supports local sidedef texture offsets (distinct offsets for upper, middle, and lower sidedef parts).
		/// </summary>
		public bool hasLocalSidedefTextureOffsets
		{
			get
			{
				return General.Map.Config.UseLocalSidedefTextureOffsets;
			}
		}

		#endregion
	}
}