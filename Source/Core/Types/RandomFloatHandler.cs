#region ================== Namespaces

using System;
using System.Globalization;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.RandomFloat, "Decimal (Random)", true)]
	internal class RandomFloatHandler : TypeHandler
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private float value;
		private bool randomvalue;
		private float min;
		private float max;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Methods

		public override void SetupArgument(TypeHandlerAttribute attr, ArgumentInfo arginfo) 
		{
			base.SetupArgument(attr, arginfo);

			//mxd. We don't want to store this type
			index = (int)UniversalType.Float;
		}

		public override void SetupField(TypeHandlerAttribute attr, UniversalFieldInfo fieldinfo) 
		{
			base.SetupField(attr, fieldinfo);

			//mxd. We don't want to store this type
			index = (int)UniversalType.Float;
		}

		public override void SetValue(object value) 
		{
			// Null?
			if(value == null) 
			{
				this.value = 0.0f;
			}
			// Compatible type?
			else if((value is int) || (value is float) || (value is bool)) 
			{
				// Set directly
				this.value = Convert.ToSingle(value);
			} 
			else
			{
				// Try parsing as string
				float result;
				if(float.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.CurrentCulture, out result)) 
				{
					this.value = result;
				} 
				else 
				{
					//mxd. Try to parse value as random range
					string[] parts = value.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

					if(parts.Length == 2) 
					{
						if(float.TryParse(parts[0], NumberStyles.Float, CultureInfo.CurrentCulture, out min) &&
						   float.TryParse(parts[1], NumberStyles.Float, CultureInfo.CurrentCulture, out max)) 
						{
							randomvalue = (min != max);

							if(min == max) this.value = min;
							else if(min > max) General.Swap(ref min, ref max);
						}
					}

					this.value = 0.0f;
				}
			}
		}

		public override object GetValue() 
		{
			if(randomvalue)	return General.Random(min, max); //mxd
			return this.value;
		}

		public override int GetIntValue() 
		{
			if(randomvalue)	return (int)General.Random(min, max); //mxd
			return (int)this.value;
		}

		public override string GetStringValue() 
		{
			if(randomvalue) return General.Random(min, max).ToString(CultureInfo.InvariantCulture); //mxd
			return this.value.ToString(CultureInfo.InvariantCulture);
		}

		public override object GetDefaultValue()
		{
			return 0f;
		}

		#endregion
	}
}
