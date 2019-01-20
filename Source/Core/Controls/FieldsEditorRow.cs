
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
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal enum FieldsEditorRowType //mxd
	{
		// This is a fixed field defined in the game configuration
		// The field cannot be deleted (delete will result in a reset)
		// and cannot change type.
		FIXED,

		// This is an abstartct variable field enetered by user
		// The field can be deleted and can change type.
		DYNAMIC,

		// This is a user variable field defined in actor's DECORATE
		// The field cannot be deleted (delete will result in a reset)
		// but can change type.
		USERVAR,
	}
	
	internal class FieldsEditorRow : DataGridViewRow
	{
		#region ================== Constants

		#endregion

		#region ================== Variables
		
		//mxd. Row type
		private readonly FieldsEditorRowType rowtype;

		// Field information (only for fixed fields)
		private readonly UniversalFieldInfo fieldinfo;

		// This is true when the field is defined. Cannot be false when this field
		// is not fixed, because non-fixed fields are deleted from the list when undefined.
		private bool isdefined;

		// Type
		private TypeHandler fieldtype;
		
		#endregion

		#region ================== Properties

		public FieldsEditorRowType RowType { get { return rowtype; } } //mxd
		public bool IsDefined { get { return isdefined; } }
		public bool IsEmpty { get { return (this.Cells[2].Value == null) || (this.Cells[2].Value.ToString().Length == 0); } }
		public string Name { get { return this.Cells[0].Value.ToString(); } }
		public TypeHandler TypeHandler { get { return fieldtype; } }
		public UniversalFieldInfo Info { get { return fieldinfo; } }

		#endregion

		#region ================== Constructor

		// Constructor for a fixed, undefined field
		public FieldsEditorRow(DataGridView view, UniversalFieldInfo fixedfield)
		{
			// Undefined
			this.DefaultCellStyle.ForeColor = SystemColors.GrayText;
			isdefined = false;
			
			// Fixed
			this.fieldinfo = fixedfield;
			this.rowtype = FieldsEditorRowType.FIXED; //mxd
			
			// Type
			this.fieldtype = General.Types.GetFieldHandler(fixedfield);
			
			// Make all cells
			base.CreateCells(view);
			
			// Setup property cell
			this.Cells[0].Value = fixedfield.Name;
			this.Cells[0].ReadOnly = true;

			// Setup type cell
			this.Cells[1].Value = fieldtype.GetDisplayType();
			this.Cells[1].ReadOnly = true;

			// Setup value cell
			this.Cells[2].Value = fieldtype.GetStringValue();
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Constructor for a non-fixed, defined field
		//mxd. Also for a user variable field.
		public FieldsEditorRow(DataGridView view, string name, int type, object value, bool isuservar)
		{
			//mxd. Row type
			this.rowtype = (isuservar ? FieldsEditorRowType.USERVAR : FieldsEditorRowType.DYNAMIC);

			// Type
			this.fieldtype = General.Types.GetFieldHandler(type, value);

			// Make all cells
			base.CreateCells(view);

			//mxd. Our path splits here...
			if(isuservar)
			{
				// Not defined
				this.DefaultCellStyle.ForeColor = SystemColors.GrayText;
				isdefined = false;
				//fieldtype.ApplyDefaultValue(); // [ZZ] don't do this. this is only done for int, and not a very good place to do it...

				// Setup property cell
				this.Cells[0].Value = name;
				this.Cells[0].ReadOnly = true;

				// Setup type cell
				this.Cells[1].Value = fieldtype.GetDisplayType();
				this.Cells[1].ReadOnly = true;

				// Setup value cell
				this.Cells[2].Value = fieldtype.GetStringValue();
			}
			else
			{
				// Defined
				this.DefaultCellStyle.ForeColor = SystemColors.WindowText;
				isdefined = true;

				// Setup property cell
				this.Cells[0].Value = name;
				this.Cells[0].ReadOnly = true;

				// Setup type cell
				this.Cells[1].Value = fieldtype.GetDisplayType();
				this.Cells[1].ReadOnly = false;

				// Setup value cell
				this.Cells[2].Value = fieldtype.GetStringValue();
			}

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods
		
		// Browse for value
		public void Browse(IWin32Window parent)
		{
			if(fieldtype != null)
			{
				// Browse for value
				fieldtype.Browse(parent);

				// This is a fixed field?
				if(rowtype == FieldsEditorRowType.FIXED)
				{
					// Does this match the default setting?
					if(fieldtype.GetValue().Equals(fieldinfo.Default))
					{
						// Undefine this field!
						Undefine();
					}
					else
					{
						// Define
						Define(fieldtype.GetValue());
					}
				}
				else
				{
					// Define
					Define(fieldtype.GetValue());
				}
			}
		}
		
		// This is called when a cell is edited
		public void CellChanged()
		{
			// This gdmn grid thing returns the chosen value as string instead
			// of the object type I added to the combobox...
			if(this.Cells[1].Value is string)
			{
				// Find the TypeHandlerAttribute with this name
				TypeHandlerAttribute attrib = General.Types.GetNamedAttribute(this.Cells[1].Value.ToString());

				// Different?
				if(attrib.Index != fieldtype.Index)
				{
					// Change field type!
					this.ChangeType(attrib.Index);
				}
			}
			
			// Anything in the box?
			if((this.Cells[2].Value != null) && (this.Cells[2].Value.ToString().Length > 0))
			{
				// Validate value
				fieldtype.SetValue(this.Cells[2].Value);
				this.Cells[2].Value = fieldtype.GetStringValue();

				// This is a fixed field?
				if(rowtype == FieldsEditorRowType.FIXED)
				{
					// Does this match the default setting?
					if(fieldtype.GetValue().Equals(fieldinfo.Default))
					{
						// Undefine this field!
						Undefine();
					}
				}
			}
		}
		
		// This undefines the field
		// ONLY VALID FOR FIXED AND USERVAR FIELDS
		// You should just delete non-fixed fields
		public void Undefine()
		{
			// Must be fixed!
			if(rowtype != FieldsEditorRowType.FIXED && rowtype != FieldsEditorRowType.USERVAR) throw new InvalidOperationException();
			
			// Now undefined
			if(rowtype == FieldsEditorRowType.USERVAR)
				fieldtype.ApplyDefaultValue();
			else
				fieldtype.SetValue(fieldinfo.Default);

			this.Cells[2].Value = fieldtype.GetStringValue();
			this.DefaultCellStyle.ForeColor = SystemColors.GrayText;
			isdefined = false;
		}

		// This defines the field
		public void Define(object value)
		{
			//mxd. Don't count as defined when default value is passed
			if(value.ToString() == fieldtype.GetDefaultValue().ToString()) return;
			
			// Now defined
			fieldtype.SetValue(value);
			this.Cells[2].Value = fieldtype.GetStringValue();
			this.DefaultCellStyle.ForeColor = (rowtype == FieldsEditorRowType.USERVAR ? SystemColors.HotTrack : SystemColors.WindowText);
			isdefined = true;
		}

		// This changes the type
		public void ChangeType(int typeindex)
		{
			// Can't do this for a fixed field!
			if(rowtype == FieldsEditorRowType.FIXED) throw new InvalidOperationException();
			
			// Different?
			if(typeindex != fieldtype.Index)
			{
				// Change field type!
				fieldtype = General.Types.GetFieldHandler(typeindex, this.Cells[2].Value);
				this.Cells[1].Value = fieldtype.GetDisplayType();
			}
		}
		
		// This clears the field
		public void Clear()
		{
			this.Cells[2].Value = "";
		}
		
		// This returns the result
		public object GetResult(object value)
		{
			// Anything in the box?
			if((this.Cells[2].Value != null) && (this.Cells[2].Value.ToString().Length > 0))
			{
				// Return validated value
				fieldtype.SetValue(this.Cells[2].Value);
				return fieldtype.GetValue();
			}
			else
			{
				// Return old value
				return value;
			}
		}
		
		#endregion
	}
}
