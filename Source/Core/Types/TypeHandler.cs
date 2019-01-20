
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
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	/// <summary>
	/// Type Handler base class. A Type Handler takes care of editing, validating and
	/// displaying values of different types for UDMF fields and hexen arguments.
	/// </summary>
	internal abstract class TypeHandler
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		protected int index;
		protected string typename;
		protected bool customusable;
		protected bool forargument;
		protected ArgumentInfo arginfo;
		protected TypeHandlerAttribute attribute;
		
		#endregion

		#region ================== Properties

		public int Index { get { return index; } }
		public string TypeName { get { return typename; } }
		public bool IsCustomUsable { get { return customusable; } }
		public bool IsForArgument { get { return forargument; } }
		public TypeHandlerAttribute Attribute { get { return attribute; } }

		public virtual bool IsBrowseable { get { return false; } }
		public virtual bool IsEnumerable { get { return false; } }
		public virtual bool IsLimitedToEnums { get { return false; } }
		
		public virtual Image BrowseImage { get { return null; } }
		public virtual bool DynamicImage { get { return false; } } //mxd. When set to true, the button image will be re-applied after value changes

		#endregion

		#region ================== Constructor

		// This sets up the handler for arguments
		public virtual void SetupArgument(TypeHandlerAttribute attr, ArgumentInfo arginfo)
		{
			// Setup
			this.forargument = true;
			this.arginfo = arginfo;
			if(attr != null)
			{
				// Set attributes
				this.attribute = attr;
				this.index = attr.Index;
				this.typename = attr.Name;
				this.customusable = attr.IsCustomUsable;
			}
			else
			{
				// Indexless
				this.attribute = null;
				this.index = -1;
				this.typename = "Unknown";
				this.customusable = false;
			}
		}

		// This sets up the handler for arguments
		public virtual void SetupField(TypeHandlerAttribute attr, UniversalFieldInfo fieldinfo)
		{
			// Setup
			this.forargument = false;
			if(attr != null)
			{
				// Set attributes
				this.attribute = attr;
				this.index = attr.Index;
				this.typename = attr.Name;
				this.customusable = attr.IsCustomUsable;
			}
			else
			{
				// Indexless
				this.attribute = null;
				this.index = -1;
				this.typename = "Unknown";
				this.customusable = false;
			}
		}
		
		#endregion

		#region ================== Methods

		// This must set the value
		// How the value is actually validated and stored is up to the implementation
		public abstract void SetValue(object value);

		//mxd. This should replace current value with the default one
		public virtual void ApplyDefaultValue() { }

		// This must return the value as one of the primitive data types
		// supported by UDMF: int, string, float or bool
		public abstract object GetValue();

		//mxd. This should return the default value
		public abstract object GetDefaultValue();
		
		// This must return the value as integer (for arguments)
		public virtual int GetIntValue()
		{
			throw new NotImplementedException("Override this method to support it as integer for arguments");
		}

		// This must return the value as a string for displaying
		public abstract string GetStringValue();

		// This is called when the user presses the browse button
		public virtual void Browse(IWin32Window parent) { }
		
		// This must returns an enum list when IsEnumerable is true
		public virtual EnumList GetEnumList()
		{
			return new EnumList(); //mxd
		}
		
		// String representation
		public override string ToString()
		{
			return this.GetStringValue();
		}

		// This returns the type to display for fixed fields
		// Must be a custom usable type
		public virtual TypeHandlerAttribute GetDisplayType()
		{
			return this.attribute;
		}
		
		#endregion
	}
}
