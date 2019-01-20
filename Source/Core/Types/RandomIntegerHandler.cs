#region ================== Namespaces

using System;
using System.Globalization;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.RandomInteger, "Integer (Random)", true)]
	internal class RandomIntegerHandler : TypeHandler
	{
		#region ================== Variables

		private int value;
		private int defaultvalue; 
		private bool randomvalue; 
		private int min;
		private int max;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Methods

		public override void SetupArgument(TypeHandlerAttribute attr, ArgumentInfo arginfo) 
		{
			defaultvalue = (int)arginfo.DefaultValue;
			base.SetupArgument(attr, arginfo);

			//mxd. We don't want to store this type
			index = (int)UniversalType.Integer;
		}

		public override void SetupField(TypeHandlerAttribute attr, UniversalFieldInfo fieldinfo) 
		{
			defaultvalue = (fieldinfo != null ? (int)fieldinfo.Default : 0); //mxd
			base.SetupField(attr, fieldinfo);

			//mxd. We don't want to store this type
			index = (int)UniversalType.Integer;
		}

		public override void SetValue(object value) 
		{
			// Null?
			if(value == null) 
			{
				this.value = 0;
			}
			// Compatible type?
			else if((value is int) || (value is float) || (value is bool)) 
			{
				// Set directly
				this.value = Convert.ToInt32(value);
			} 
			else
			{
				// Try parsing as string
				int result;
				if(int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out result)) 
				{
					this.value = result;
				} 
				else 
				{
					//mxd. Try to parse value as random range
					string[] parts = value.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if(parts.Length == 2) 
					{
						if(int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.CurrentCulture, out min) &&
						   int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.CurrentCulture, out max)) 
						{
							randomvalue = (min != max);
							if(min == max) this.value = min;
							else if(min > max) General.Swap(ref min, ref max);
						}
					}

					this.value = 0;
				}
			}

			if(forargument)
				this.value = General.Clamp(this.value, General.Map.FormatInterface.MinArgument, General.Map.FormatInterface.MaxArgument);
		}

		//mxd
		public override void ApplyDefaultValue() 
		{
			value = defaultvalue;
		}

		public override object GetValue() 
		{
			if(randomvalue)	return General.Random(min, max); //mxd
			return this.value;
		}

		public override int GetIntValue() 
		{
			if(randomvalue)	return General.Random(min, max); //mxd
			return this.value;
		}

		public override string GetStringValue() 
		{
			if(randomvalue)	return General.Random(min, max).ToString(CultureInfo.InvariantCulture); //mxd
			return this.value.ToString(CultureInfo.InvariantCulture);
		}

		public override object GetDefaultValue()
		{
			return defaultvalue;
		}

		#endregion
	}
}
