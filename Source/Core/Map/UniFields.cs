#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.Map
{
	/// <summary>
	/// List of universal fields and their values.
	/// </summary>
	[Serializable]
	public class UniFields : Dictionary<string, UniValue>
	{
		#region ================== Variables

		// Owner of this list
		private MapElement owner;

		#endregion

		#region ================== Properties
		
		public MapElement Owner { get { return owner; } internal set { owner = value; } }

		#endregion

		#region ================== Constructors

		// New constructor
		///<summary></summary>
		public UniFields() : base(2) { }

		// New constructor
		///<summary></summary>
		public UniFields(int capacity) : base(capacity) { }

		// Copy constructor (makes a deep copy)
		///<summary></summary>
		public UniFields(UniFields copyfrom) : base(copyfrom.Count)
		{
			foreach(KeyValuePair<string, UniValue> v in copyfrom)
				this.Add(v.Key, new UniValue(v.Value));
		}
		
		// New constructor
		///<summary></summary>
		public UniFields(MapElement owner) : base(2)
		{
			this.owner = owner;
		}

		// New constructor
		///<summary></summary>
		public UniFields(MapElement owner, int capacity) : base(capacity)
		{
			this.owner = owner;
		}

		// Copy constructor
		///<summary></summary>
		public UniFields(MapElement owner, UniFields copyfrom) : base(copyfrom.Count)
		{
			this.owner = owner;

			foreach(KeyValuePair<string, UniValue> v in copyfrom) //mxd. No-no-no, David Blaine, I don't want to copy these by reference!
				this.Add(v.Key, new UniValue(v.Value));
		}

		#endregion

		#region ================== Mathods

		/// <summary>Call this before making changes to the fields, or they may not be updated correctly with undo/redo!</summary>
		public void BeforeFieldsChange()
		{
			if(owner != null) owner.BeforeFieldsChange();
		}
		
		/// <summary>This returns the value of a field by name, or returns the specified value when no such field exists or the field value fails to convert to the same datatype.</summary>
		public T GetValue<T>(string fieldname, T defaultvalue)
		{
			if(!this.ContainsKey(fieldname)) return defaultvalue;

			try
			{
				T val = (T)this[fieldname].Value;
				return val;
			}
			catch(InvalidCastException)
			{
				return defaultvalue;
			}
		}

		#endregion

		#region ================== mxd. Static methods

		// float
		public static void SetFloat(UniFields fields, string key, float value) { SetFloat(fields, key, value, 0f); }
		public static void SetFloat(UniFields fields, string key, float value, float defaultvalue)
		{
			if(fields == null) return;
			if(value != defaultvalue)
			{
				if(!fields.ContainsKey(key)) fields.Add(key, new UniValue(UniversalType.Float, value));
				else fields[key].Value = value;
			}
			// Don't save default value
			else if(fields.ContainsKey(key)) 
			{
				fields.Remove(key);
			}
		}

		public static float GetFloat(UniFields fields, string key) { return GetFloat(fields, key, 0f); }
		public static float GetFloat(UniFields fields, string key, float defaultvalue)
		{
			if(fields == null) return defaultvalue;
			return fields.GetValue(key, defaultvalue);
		}

		// int
		public static void SetInteger(UniFields fields, string key, int value) { SetInteger(fields, key, value, 0); }
		public static void SetInteger(UniFields fields, string key, int value, int defaultvalue)
		{
			if(fields == null) return;
			if(value != defaultvalue)
			{
				if(!fields.ContainsKey(key)) fields.Add(key, new UniValue(UniversalType.Integer, value));
				else fields[key].Value = value;
			}
			// Don't save default value
			else if(fields.ContainsKey(key)) 
			{
				fields.Remove(key);
			}
		}

		public static int GetInteger(UniFields fields, string key) { return GetInteger(fields, key, 0); }
		public static int GetInteger(UniFields fields, string key, int defaultvalue)
		{
			if(fields == null) return defaultvalue;
			return fields.GetValue(key, defaultvalue);
		}

		// String
		public static void SetString(UniFields fields, string key, string value, string defaultvalue)
		{
			if(fields == null) return;
			if(value != defaultvalue)
			{
				if(!fields.ContainsKey(key)) fields.Add(key, new UniValue(UniversalType.String, value));
				else fields[key].Value = value;
			}
			// Don't save default value
			else if(fields.ContainsKey(key)) 
			{
				fields.Remove(key);
			}
		}

		/// <summary>This removes fields with given names.</summary>
		public static void RemoveFields(UniFields fields, IEnumerable<string> keys)
		{
			if(fields == null) return;
			foreach(string key in keys) 
				if(fields.ContainsKey(key)) fields.Remove(key);
		}

		/// <summary>This removes a field with given name.</summary>
		public static void RemoveField(UniFields fields, string key)
		{
			if(fields == null || !fields.ContainsKey(key)) return;
			fields.Remove(key);
		}

		/// <summary>This compares all fields.</summary>
		public static bool AllFieldsMatch(UniFields fields1, UniFields fields2)
		{
			if(fields1.Keys.Count != fields2.Keys.Count) return false;
			foreach(KeyValuePair<string, UniValue> group in fields1)
			{
				if(!fields2.ContainsKey(group.Key) || !UniValuesMatch(fields1[group.Key], fields2[group.Key]))
					return false;
			}
			return true;
		}

		/// <summary>This compares non-UI fields (e.g. fields, visible in the "Custom" tab).</summary>
		public static bool CustomFieldsMatch(UniFields fields1, UniFields fields2)
		{
			if(fields1.owner == null || fields2.owner == null) throw new NotSupportedException("Cannot compare custom fields without owners!");
			if(fields1.owner.ElementType != fields2.owner.ElementType) return false;
			return CustomFieldsMatch(fields1, fields2, fields1.owner.ElementType);
		}

		private static bool CustomFieldsMatch(UniFields fields1, UniFields fields2, MapElementType type)
		{
			// Collect non-UI fields
			UniFields filtered1 = new UniFields();
			UniFields filtered2 = new UniFields();

			foreach(string key in fields1.Keys)
			{
				if(!General.Map.FormatInterface.UIFields[type].ContainsKey(key))
					filtered1.Add(key, fields1[key]);
			}

			foreach(string key in fields2.Keys)
			{
				if(!General.Map.FormatInterface.UIFields[type].ContainsKey(key))
					filtered2.Add(key, fields2[key]);
			}

			if(filtered1.Keys.Count != filtered2.Keys.Count) return false;
			return AllFieldsMatch(filtered1, filtered2);
		}

		/// <summary>This compares types and values of given UniValues.</summary>
		public static bool UniValuesMatch(UniValue val1, UniValue val2)
		{
			if(val1.Type != val2.Type) return false;
			if(val1.Value is int) return (int)val1.Value == (int)val2.Value;
			if(val1.Value is float) return (float)val1.Value == (float)val2.Value;
			if(val1.Value is bool) return (bool)val1.Value == (bool)val2.Value;
			if(val1.Value is string) return (string)val1.Value == (string)val2.Value;
			throw new NotImplementedException("Unknown Custom Field type: " + val1.Value.GetType());
		}

		/// <summary>This compares types and values of given UniFields by key.</summary>
		public static bool ValuesMatch(string key, UniFields fields1, UniFields fields2)
		{
			bool f1 = fields1.ContainsKey(key);
			bool f2 = fields2.ContainsKey(key);
			if(!f1 && !f2) return true;
			if(f1 != f2) return false;
			return UniValuesMatch(fields1[key], fields2[key]);
		}

		/// <summary>This compares types and values of given UniFields by 2 keys. Returns true when both values match.</summary>
		public static bool ValuesMatch(string key1, string key2, UniFields fields1, UniFields fields2)
		{
			return ValuesMatch(key1, fields1, fields2) && ValuesMatch(key2, fields1, fields2);
		}

		/// <summary>This compares UniFields types and values of given MapElements by key.</summary>
		public static bool ValuesMatch(string key, MapElement e1, MapElement e2)
		{
			if(e1.ElementType != e2.ElementType || (e1.Fields == null && e2.Fields != null) || e2.Fields == null) return false;
			if(e1.Fields == null && e2.Fields == null) return true;
			return ValuesMatch(key, e1.Fields, e2.Fields);
		}

		/// <summary>This compares UniFields types and values of given MapElements by 2 keys. Returns true when both values match.</summary>
		public static bool ValuesMatch(string key1, string key2, MapElement e1, MapElement e2)
		{
			if(e1.ElementType != e2.ElementType || (e1.Fields == null && e2.Fields != null) || e2.Fields == null) return false;
			if(e1.Fields == null && e2.Fields == null) return true;
			return ValuesMatch(key1, key2, e1.Fields, e2.Fields);
		}

		#endregion
	}
}
