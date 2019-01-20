
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

#region ================== CFG file structure syntax

/*
' ====================================================================================
'    CONFIGURATION FILE STRUCTURE SYNTAX
' ====================================================================================
'
'    Whitepace is always allowed. This includes spaces, tabs
'    linefeeds (10) and carriage returns (13)
'
'    Keys may not have spaces or assignment operator = in them.
'
'    Comments start with // (unless used within strings)
'    and count as comment for the rest of the line. Or use /* and */				/*
'    to mark the beginning and end of a comment.
'
'    Simple setting:
'
'              key = value;
'
'    Example:  speed = 345;
'              cars = 8;
'
'    Strings must be in quotes.
'
'    Example:  nickname = "Gherkin";
'              altnick = "Gherk inn";
'
'    String Escape Sequences:
'        \n    New line (10)
'        \r    Carriage return (13)
'        \t    Tab (9)
'        \"    Double-quotation mark
'        \\    Backslash
'        \000  Any ASCII character (MUST be 3 digits! So for 13 you use \013)
'
'    Decimals ALWAYS use a dot, NEVER comma!
'
'    Example:  pressure = 15.29;
'              acceleration = 1.0023;
'
'    true, false and null are valid keywords.
'    null values can be left out.
'
'    In this example, both items are null.
'    Example:  myitem = null;
'              myotheritem;
'
'    Structures must use brackets.
'
'    Structure Example:
'
'              key
'              {
'                   key = value;
'                   key = value;
'
'                   key
'                   {
'                        key = value;
'                        key = value;
'                        key = value;
'                   }
'
'                   key = value;
'                   key = value;
'                   key = value;
'                   key = value;
'                   key = value;
'              }
'
'    As you can see, structures inside structures are allowed
'    and you may go as deep as you want. Note that only the root structures
'    can be readed from config using ReadSetting. ReadSetting will return a
'    Dictionary object containing everything in that root structure.
'
'    Key names must be unique within their scope.
'
'    This is NOT allowed, it may not have 'father' more
'    than once in the same scope:
'
'              mother = 45;
'              father = 52;
'
'              father
'              {
'                   length = 1.87;
'              }
'
'    This however is allowed, because father
'    now exists in a different scope:
'
'              mother = 45;
'              father = 52;
'
'              parents
'              {
'                   father = 52;
'              }
'
'    This too is allowed, both 'age' are in a different scope:
'
'              mother
'              {
'                   age = 45;
'              }
'
'              father
'              {
'                   age = 52;
'              }
*/

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	public sealed class Configuration
	{
		#region ================== Constants
		
		// Path seperator
		public const string DEFAULT_SEPERATOR = ".";
		
		// Error strings
		private const string ERROR_KEYMISSING = "Missing key name in assignment or scope.";
		private const string ERROR_KEYSPACES = "Spaces not allowed in key names.";
		private const string ERROR_ASSIGNINVALID = "Invalid assignment. Missing a previous terminator symbol?";
		private const string ERROR_VALUEINVALID = "Invalid value in assignment. Missing a previous terminator symbol?";
		private const string ERROR_VALUETOOBIG = "Value too big.";
		//private const string ERROR_KEYNOTUNQIUE = "Key is not unique within scope.";
		private const string ERROR_KEYWORDUNKNOWN = "Unknown keyword in assignment. Missing a previous terminator symbol?";
		private const string ERROR_UNEXPECTED_END = "Unexpected end of data. Missing a previous terminator symbol?";
		private const string ERROR_UNKNOWN_FUNCTION = "Unknown function call.";
		private const string ERROR_INVALID_ARGS = "Invalid function arguments.";
		private const string ERROR_INCLUDE_UNSUPPORTED = "Include function is not supported in data parsed from stream.";

		public const string NUMBERS = "0123456789"; 
		public const string NUMBERS2 = "0123456789-.&"; 

		#endregion
		
		#region ================== Variables
		
		// Error result
		private bool cpErrorResult;
		private string cpErrorDescription = "";
		private int cpErrorLine;
		private string cpErrorFile = "";
		private static readonly char[] space = new[] { ' ' }; //mxd
		private static readonly char[] newline = new[] { '\n' }; //mxd
		
		// Configuration root
		private IDictionary root;

		//mxd. Cache
		private static Dictionary<string, IDictionary> cfgcache = new Dictionary<string, IDictionary>(StringComparer.Ordinal);
		
		#endregion
		
		#region ================== Properties
		
		// Properties
		public bool ErrorResult { get { return cpErrorResult; } }
		public string ErrorDescription { get { return cpErrorDescription; } }
		public int ErrorLine { get { return cpErrorLine; } }
		public string ErrorFile { get { return cpErrorFile; } }
		public IDictionary Root { get { return root; } set { root = value; } }
		public bool Sorted { get { return (root is ListDictionary); } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public Configuration()
		{
			// Standard new configuration
			NewConfiguration();
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		// Constructor
		public Configuration(bool sorted)
		{
			// Standard new configuration
			NewConfiguration(sorted);

			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		// Constructor to load a file immediately
		public Configuration(string filename)
		{
			// Load configuration from file
			LoadConfiguration(filename);

			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		// Constructor to load a file immediately
		public Configuration(string filename, bool sorted)
		{
			// Load configuration from file
			LoadConfiguration(filename, sorted);

			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		#endregion
		
		#region ================== Private Methods
		
		// This merges two structures
		private static IDictionary Combine(IDictionary d1, IDictionary d2, bool sorted)
		{
			// Create new dictionary
			IDictionary result;
			if(sorted) result = new ListDictionary(); else result = new Hashtable();
			
			// Copy all items from d1 to result
			IDictionaryEnumerator d1e = d1.GetEnumerator();
			while(d1e.MoveNext()) result.Add(d1e.Key, d1e.Value);
			
			// Go for all items in d2
			IDictionaryEnumerator d2e = d2.GetEnumerator();
			while(d2e.MoveNext())
			{
				// Check if this is another Hashtable
				if(d2e.Value is IDictionary)
				{
					// Check if already in result
					if(result.Contains(d2e.Key) && (result[d2e.Key] is IDictionary))
					{
						// Modify result
						result[d2e.Key] = Combine((IDictionary)result[d2e.Key], (IDictionary)d2e.Value, sorted);
					}
					else
					{
						// Copy from d2
						if(sorted)
						{
							// Sorted combine
							result[d2e.Key] = Combine(new ListDictionary(), (IDictionary)d2e.Value, true);
						}
						else
						{
							// Unsorted combine
							result[d2e.Key] = Combine(new Hashtable(), (IDictionary)d2e.Value, false);
						}
					}
				}
				else
				{
					// Check if also in d1
					if(result.Contains(d2e.Key))
					{
						// Modify result
						result[d2e.Key] = d2e.Value;
					}
					else
					{
						// Copy
						result.Add(d2e.Key, d2e.Value);
					}
				}
			}
			
			// Return result
			return result;
		}
		
		// This is called by all the ReadSetting overloads to perform the read
		private bool CheckSetting(IDictionary dic, string setting, string pathseperator)
		{
			// Split the path in an array
			string[] keys = setting.Split(pathseperator.ToCharArray());

			// Get the root item
			object item = dic;

			// Go for each item
			foreach(string key in keys)
			{
				// Check if the current item is of ConfigStruct type
				if(item is IDictionary)
				{
					// Check if the key is valid
					if(ValidateKey(key.Trim(), "", -1))
					{
						// Cast to ConfigStruct
						IDictionary cs = (IDictionary)item;
						
						// Check if the requested item exists
						if(cs.Contains(key))
						{
							// Set the item to the next item
							item = cs[key];
						}
						else
						{
							// Key not found
							return false;
						}
					}
					else
					{
						// Invalid key in path
						return false;
					}
				}
				else
				{
					// Unable to go any further
					return false;
				}
			}

			// Return result
			return true;
		}
		
		// This is called by all the ReadSetting overloads to perform the read
		private object ReadAnySetting(string setting, object defaultsetting, string pathseperator) { return ReadAnySetting(root, setting, defaultsetting, pathseperator); }
		private object ReadAnySetting(IDictionary dic, string setting, object defaultsetting, string pathseperator) { return ReadAnySetting(dic, "", -1, setting, defaultsetting, pathseperator); }
		private object ReadAnySetting(IDictionary dic, string file, int line, string setting, object defaultsetting, string pathseperator)
		{
			// Split the path in an array
			string[] keys = setting.Split(pathseperator.ToCharArray());
			
			// Get the root item
			object item = dic;
			
			// Go for each item
			foreach(string key in keys)
			{
				// Check if the current item is of ConfigStruct type
				if(item is IDictionary)
				{
					// Check if the key is valid
					if(ValidateKey(key.Trim(), file, line))
					{
						// Cast to ConfigStruct
						IDictionary cs = (IDictionary)item;
						
						// Check if the requested item exists
						if(cs.Contains(key))
						{
							// Set the item to the next item
							item = cs[key];
						}
						else
						{
							// Key not found
							// return default setting
							return defaultsetting;
						}
					}
					else
					{
						// Invalid key in path
						// return default setting
						return defaultsetting;
					}
				}
				else
				{
					// Unable to go any further
					// return default setting
					return defaultsetting;
				}
			}
			
			// Return the item
			return item;
		}
		
		
		// This returns a string added with escape characters
		private static string EscapedString(string str)
		{
			// Replace the \ with \\ first!
			str = str.Replace("\\", "\\\\");
			str = str.Replace("\n", "\\n");
			str = str.Replace("\r", "\\r");
			str = str.Replace("\t", "\\t");
			str = str.Replace("\"", "\\\"");
			
			// Return result
			return str;
		}
		
		
		// This raises an error
		private void RaiseError(string file, int line, string description)
		{
			// Raise error
			if(!cpErrorResult)
			{
				cpErrorResult = true;
				cpErrorDescription = description;
				cpErrorLine = line;
				cpErrorFile = file;
			}
		}
		
		
		// This validates a given key and sets
		// error properties if key is invalid and errorline > -1
		private bool ValidateKey(string key, string file, int errorline)
		{
			bool validateresult;
			
			// Check if key is an empty string
			if(string.IsNullOrEmpty(key))
			{
				// ERROR: Missing key name in statement
				if(errorline > -1) RaiseError(file, errorline, ERROR_KEYMISSING);
				validateresult = false;
			}
			else
			{
				// Check if there are spaces in the key
				if(key.IndexOfAny(space) > -1)
				{
					// ERROR: Spaces not allowed in key names
					if(errorline > -1) RaiseError(file, errorline, ERROR_KEYSPACES);
					validateresult = false;
				}
				else
				{
					// Check if we can test existance
					//if(container != null)
					//{
						/*
						// Test if the key exists in this container
						if(container.Contains(key) == true)
						{
							// ERROR: Key is not unique within struct
							if(errorline > -1) RaiseError(file, errorline, ERROR_KEYNOTUNQIUE);
							validateresult = false;
						}
						else
						*/
						//{
							// Key OK
							//validateresult = true;
						//}
					//}
					//else
					//{
						// Key OK
						validateresult = true;
					//}
				}
			}
			
			// Return result
			return validateresult;
		}
		
		
		// This validates a given keyword and sets
		// error properties if keyword is invalid and errorline > -1
		private bool ValidateKeyword(string keyword, string file, int errorline)
		{
			bool validateresult;
			
			// Check if key is an empty string
			if(string.IsNullOrEmpty(keyword))
			{
				// ERROR: Missing key name in statement
				if(errorline > -1) RaiseError(file, errorline, ERROR_ASSIGNINVALID);
				validateresult = false;
			}
			else
			{
				// Check if there are spaces in the key
				if(keyword.IndexOfAny(space) > -1)
				{
					// ERROR: Spaces not allowed in key names
					if(errorline > -1) RaiseError(file, errorline, ERROR_ASSIGNINVALID);
					validateresult = false;
				}
				else
				{
					// Key OK
					validateresult = true;
				}
			}
			
			// Return result
			return validateresult;
		}
		
		#endregion
		
		#region ================== Parsing
		
		// This parses an assignment
		private object ParseAssignment(ref string file, ref string data, ref int pos, ref int line)
		{
			object val = null;
			
			while((pos < data.Length) && !cpErrorResult)
			{
				// Get current character
				char c = data[pos++];
				
				// Check for string opening
				if(c == '\"')
				{
					// Now parsing a string
					val = ParseString(ref file, ref data, ref pos, ref line);
					if(cpErrorResult) return null;
				}
				// Check for numeric character
				else if(NUMBERS2.IndexOf(c.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) > -1)
				{
					// Go one byte back, because this
					// byte is part of the number!
					pos--;
					
					// Now parsing a number
					val = ParseNumber(ref file, ref data, ref pos, ref line);
					if(cpErrorResult) return null;
				}
				// Check for new line
				else if(c == '\n')
				{
					// Count the new line
					line++;
				}
				// Check if assignment ends
				else if(c == ';')
				{
					// End of assignment
					return val;
				}
				// Otherwise (if not whitespace) it is a keyword
				else if((c != ' ') && (c != '\t'))
				{
					// Go one byte back, because this
					// byte is part of the keyword!
					pos--;
					
					// Now parsing a keyword
					val = ParseKeyword(ref file, ref data, ref pos, ref line);
					if(cpErrorResult) return null;
				}
			}
			
			RaiseError(file, line, ERROR_UNEXPECTED_END);
			return null;
		}
		
		
		// This parses a string
		private string ParseString(ref string file, ref string data, ref int pos, ref int line)
		{
			string val = "";
			
			// In escape sequence?
			bool escape = false;
			
			while((pos < data.Length) && !cpErrorResult)
			{
				// Get current character
				char c = data[pos++];
				
				// Check if in an escape sequence
				if(escape)
				{
					// What character?
					switch(c)
					{
						case '\\': val += "\\"; break;
						case 'n': val += "\n"; break;
						case '\"': val += "\""; break;
						case 'r': val += "\r"; break;
						case 't': val += "\t"; break;
						default:

							// Is it a number?
							if(NUMBERS.IndexOf(c.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) > -1)
							{
								int vv;
								char vc;

								// Convert the next 3 characters to a number
								string v = data.Substring(pos, 3);
								try { vv = Convert.ToInt32(v.Trim(), CultureInfo.InvariantCulture); }
								catch(FormatException)
								{
									// ERROR: Invalid value in assignment
									RaiseError(file, line, ERROR_VALUEINVALID);
									return null;
								}

								// Convert the number to a char
								try { vc = Convert.ToChar(vv, CultureInfo.InvariantCulture); }
								catch(FormatException)
								{
									// ERROR: Invalid value in assignment
									RaiseError(file, line, ERROR_VALUEINVALID);
									return null;
								}

								// Add the char
								val += vc.ToString(CultureInfo.InvariantCulture);
							}
							else
							{
								// Add the character as it is
								val += c.ToString(CultureInfo.InvariantCulture);
							}

							// Leave switch
							break;
					}

					// End of escape sequence
					escape = false;
				}
				else
				{
					switch(c) //mxd
					{ 
						// Check for sequence start
						case '\\':
							// Next character is of escape sequence
							escape = true;
							break;

						// Check if string ends
						case '\"':
							return val;

						// Check for new line
						case '\n':
							// Count the new line
							line++;
							break;

						// Everything else is just part of string
						default:
							// Add to value
							val += c.ToString(CultureInfo.InvariantCulture);
							break;
					}
				}
			}
			
			RaiseError(file, line, ERROR_UNEXPECTED_END);
			return null;
		}
		
		
		// Parsing a number
		private object ParseNumber(ref string file, ref string data, ref int pos, ref int line)
		{
			string val = "";
			
			while((pos < data.Length) && !cpErrorResult)
			{
				// Get current character
				char c = data[pos++];
				
				// Check if number ends here
				if((c == ';') || (c == ',') || (c == ')'))
				{
					// One byte back, because this
					// is also the end of the assignment
					pos--;
					
					// Floating point?
					if(val.IndexOf("f", StringComparison.Ordinal) > -1)
					{
						float fval;
						
						// Convert to float (remove the f first)
						try { fval = Convert.ToSingle(val.Trim().Replace("f", ""), CultureInfo.InvariantCulture); }
						catch(FormatException)
						{
							// ERROR: Invalid value in assignment
							RaiseError(file, line, ERROR_VALUEINVALID);
							return null;
						}
						return fval;
					}
					else
					{
						// Convert to int
						try
						{
							// Convert to value
							int ival = Convert.ToInt32(val.Trim(), CultureInfo.InvariantCulture);
							return ival;
						}
						catch(OverflowException)
						{
							// Too large for Int32, try Int64
							try
							{
								// Convert to value
								long lval = Convert.ToInt64(val.Trim(), CultureInfo.InvariantCulture);
								return lval;
							}
							catch(OverflowException)
							{
								// Too large for Int64, return error
								RaiseError(file, line, ERROR_VALUETOOBIG);
								return null;
							}
							catch(FormatException)
							{
								// ERROR: Invalid value in assignment
								RaiseError(file, line, ERROR_VALUEINVALID);
								return null;
							}
						}
						catch(FormatException)
						{
							// ERROR: Invalid value in assignment
							RaiseError(file, line, ERROR_VALUEINVALID);
							return null;
						}
					}
				}
				// Check for new line
				else if(c == '\n')
				{
					// Count the new line
					line++;
				}
				// Everything else is part of the value
				else
				{
					val += c.ToString(CultureInfo.InvariantCulture);
				}
			}
			
			RaiseError(file, line, ERROR_UNEXPECTED_END);
			return null;
		}
		
		
		// Parsing a keyword
		private object ParseKeyword(ref string file, ref string data, ref int pos, ref int line)
		{
			string val = "";
			
			while((pos < data.Length) && !cpErrorResult)
			{
				// Get current character
				char c = data[pos++];
				
				// Check if keyword ends
				if((c == ';') || (c == ',') || (c == ')'))
				{
					// One byte back, because this
					// is also the end of the assignment
					pos--;
					
					// Validate the keyword
					if(ValidateKeyword(val.Trim(), file, line))
					{
						// Return result depending on the keyword
						switch(val.Trim().ToLowerInvariant())
						{
							case "true": return true;
							case "false": return false;
							case "null": return null;
							default: RaiseError(file, line, ERROR_KEYWORDUNKNOWN + "\nUnrecognized token: \"" + val.Trim().ToLowerInvariant() + "\"");
								return null;
						}
					}
				}
				// Check for new line
				else if(c == '\n')
				{
					// Count the new line
					line++;
				}
				// Everything else is just part of keyword
				else
				{
					// Add to value
					val += c.ToString(CultureInfo.InvariantCulture);
				}
			}
			
			RaiseError(file, line, ERROR_UNEXPECTED_END);
			return null;
		}
		
		
		// This includes another file
		private void FunctionInclude(IDictionary cs, List<object> args, ref string file, int line)
		{
			string data;
			
			if(string.IsNullOrEmpty(file)) RaiseError(file, line, ERROR_INCLUDE_UNSUPPORTED);
			if(args.Count < 1) RaiseError(file, line, ERROR_INVALID_ARGS);
			if(!(args[0] is string)) RaiseError(file, line, ERROR_INVALID_ARGS + " Expected a string for argument 1.");
			if((args.Count > 1) && !(args[1] is string)) RaiseError(file, line, ERROR_INVALID_ARGS + " Expected a string for argument 2.");
			string filename = Path.GetFileName(file);
			if(string.IsNullOrEmpty(filename)) RaiseError(file, line, "Invalid include statement: file name is missing."); //mxd
			else if(args[0].ToString().ToUpperInvariant() == filename.ToUpperInvariant()) RaiseError(file, line, "A file cannot call include() on itself."); //mxd
			if(cpErrorResult) return;
			
			// Determine the full path of the file to include
			string includefile = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + args[0];

			//mxd. Caching
			if(cfgcache.ContainsKey(includefile)) 
			{
				IDictionary cinc = cfgcache[includefile];
				
				// Check if a path is given
				if((args.Count > 1) && !string.IsNullOrEmpty(args[1].ToString())) 
				{
					IDictionary def;
					if(cs is ListDictionary) def = new ListDictionary(); else def = new Hashtable();
					if(CheckSetting(cinc, args[1].ToString(), DEFAULT_SEPERATOR)) 
					{
						cinc = (IDictionary)ReadAnySetting(cinc, file, line, args[1].ToString(), def, DEFAULT_SEPERATOR);
					} 
					else 
					{
						RaiseError(file, line, "Include missing structure \"" + args[1] + "\" in file \"" + includefile + "\"");
						return;
					}
				}

				// Recursively merge the structures with the current structure
				IDictionary newcs = Combine(cs, cinc, (cs is ListDictionary));
				cs.Clear();
				foreach(DictionaryEntry de in newcs) cs.Add(de.Key, de.Value);
				return;
			}
			
			try
			{
				// Load the file contents
				FileStream fstream = File.OpenRead(includefile);
				byte[] fbuffer = new byte[fstream.Length];
				fstream.Read(fbuffer, 0, fbuffer.Length);
				fstream.Close();
				
				// Convert byte array to string
				data = Encoding.UTF8.GetString(fbuffer);
			}
			catch(Exception e)
			{
				RaiseError(file, line, "Unable to include file \"" + includefile + "\". " + e.GetType().Name + ": " + e.Message);
				return;
			}
			
			// Remove returns and tabs because the
			// parser only uses newline for new lines.
			data = data.Replace("\r", "");
			data = data.Replace("\t", "");
			
			// Parse the data
			IDictionary inc;
			if(cs is ListDictionary) inc = new ListDictionary(); else inc = new Hashtable();
			int npos = 0, nline = 1;
			InputStructure(inc, ref includefile, ref data, ref npos, ref nline);
			if(!cpErrorResult)
			{
				//mxd. Add to cache
				cfgcache.Add(includefile, inc);
				
				// Check if a path is given
				if((args.Count > 1) && !string.IsNullOrEmpty(args[1].ToString()))
				{
					IDictionary def;
					if(cs is ListDictionary) def = new ListDictionary(); else def = new Hashtable();
					if(CheckSetting(inc, args[1].ToString(), DEFAULT_SEPERATOR))
					{
						inc = (IDictionary)ReadAnySetting(inc, file, line, args[1].ToString(), def, DEFAULT_SEPERATOR);
					}
					else
					{
						RaiseError(file, line, "Include missing structure \"" + args[1] + "\" in file \"" + includefile + "\"");
						return;
					}
				}
				
				// Recursively merge the structures with the current structure
				IDictionary newcs = Combine(cs, inc, (cs is ListDictionary));
				cs.Clear();
				foreach(DictionaryEntry de in newcs) cs.Add(de.Key, de.Value);
			}
		}
		
		
		// This parses a function
		private void ParseFunction(IDictionary cs, ref string file, ref string data, ref int pos, ref int line, ref string functionname)
		{
			// We now parse arguments, separated by commas, until we reach the end of the function
			List<object> args = new List<object>();
			while((pos < data.Length) && !cpErrorResult)
			{
				// Get current character
				char c = data[pos++];
				
				// Check for end of function
				if(c == ')')
				{
					// Check what function to run
					switch(functionname.Trim().ToLowerInvariant())
					{
						// Include another file in here
						case "include":
							FunctionInclude(cs, args, ref file, line);
							return;
							
						default:
							RaiseError(file, line, ERROR_UNKNOWN_FUNCTION);
							return;
					}
				}

				// Check for string opening
				if(c == '\"')
				{
					// Now parsing a string
					object val = ParseString(ref file, ref data, ref pos, ref line);
					if(cpErrorResult) return;
					args.Add(val);
				}
				// Check for numeric character
				else if(NUMBERS2.IndexOf(c.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) > -1)
				{
					// Go one byte back, because this
					// byte is part of the number!
					pos--;
					
					// Now parsing a number
					object val = ParseNumber(ref file, ref data, ref pos, ref line);
					if(cpErrorResult) return;
					args.Add(val);
				}
				// Check for new line
				else if(c == '\n')
				{
					// Count the new line
					line++;
				}
				// Check if argument ends
				else if(c == ',')
				{
					// End of argument
				}
				// Otherwise (if not whitespace) it is a keyword
				else if((c != ' ') && (c != '\t'))
				{
					// Go one byte back, because this
					// byte is part of the keyword!
					pos--;
					
					// Now parsing a keyword
					object val = ParseKeyword(ref file, ref data, ref pos, ref line);
					if(cpErrorResult) return;
					args.Add(val);
				}
			}
			
			RaiseError(file, line, ERROR_UNEXPECTED_END);
		}
		
		
		// This parses a structure in the given data starting
		// from the given pos and line and updates pos and line.
		private void InputStructure(IDictionary cs, ref string file, ref string data, ref int pos, ref int line)
		{
			string key = "";
			
			// Go through all of the data until
			// the end or until the struct closes
			// or when an arror occurred
			while((pos < data.Length) && !cpErrorResult)
			{
				// Get current character
				char c = data[pos++];
				
				// Check what character this is
				switch(c)
				{
					case '{': // Begin of new struct
						
						// Validate key
						if(ValidateKey(key.Trim(), file, line))
						{
							// Parse this struct and add it
							IDictionary cs2;
							if(cs is ListDictionary) cs2 = new ListDictionary(); else cs2 = new Hashtable();
							InputStructure(cs2, ref file, ref data, ref pos, ref line);
							if(cs.Contains(key.Trim()) && (cs[key.Trim()] is IDictionary))
								cs[key.Trim()] = Combine((IDictionary)cs[key.Trim()], cs2, (cs is ListDictionary));
							else
								cs[key.Trim()] = cs2;
							key = "";
						}
						break;
						
					case '}': // End of this struct
						
						// Stop parsing in this struct
						return;
						
					case '(': // Function
						ParseFunction(cs, ref file, ref data, ref pos, ref line, ref key);
						key = "";
						break;
						
					case '=': // Assignment
						
						// Validate key
						if(ValidateKey(key.Trim(), file, line))
						{
							// Now parsing assignment
							object val = ParseAssignment(ref file, ref data, ref pos, ref line);
							if(!cpErrorResult)
							{
								cs[key.Trim()] = val;
								key = "";
							}
						}
						break;
						
					case ';': // Terminator
						
						// Validate key
						if(!string.IsNullOrEmpty(key))
						{
							if(ValidateKey(key.Trim(), file, line))
							{
								// Add the key with null as value
								cs[key.Trim()] = null;
								key = "";
							}
						}
						break;
						
					case '\n': // New line
						
						// Count the line
						line++;
						
						// Add this to the key as a space.
						// Spaces are not allowed, but it will be trimmed
						// when its the first or last character.
						key += " ";
						break;
						
					case '\\': // Possible comment
					case '/':
						
						// Backtrack to use previous character also
						pos--;
						
						// Check for the line comment //
						if(data.Substring(pos, 2) == "//")
						{
							// Find the next line
							int np = data.IndexOf("\n", pos, StringComparison.Ordinal);
							
							// Next line found?
							if(np > -1)
							{
								// Count the line
								line++;
								
								// Skip everything on this line
								pos = np + 1;
							}
							else
							{
								// No end of line
								// Skip everything else
								pos = data.Length + 1;
							}
						}
						// Check for the block comment /* */
						else if(data.Substring(pos, 2) == "/*")
						{
							// Find the next closing block comment
							int np = data.IndexOf("*/", pos, StringComparison.Ordinal);
							
							// Closing block comment found?
							if(np > -1)
							{
								// Count the lines in the block comment
								string blockdata = data.Substring(pos, np - pos + 2);
								line += (blockdata.Split(newline).Length - 1);
								
								// Skip everything in this block
								pos = np + 2;
							}
							else
							{
								// No end of line
								// Skip everything else
								pos = data.Length + 1;
							}
						}
						else
						{
							// No whitespace
							pos++;
						}
						break;
						
					default: // Everything else
						
						// Add character to key
						key += c.ToString(CultureInfo.InvariantCulture);
						break;
				}
			}
		}
		
		#endregion

		#region ================== Writing

		// This will create a data structure from the given object
		private static string OutputStructure(IDictionary cs, int level, string newline, bool whitespace)
		{
			string leveltabs = "";
			string spacing = "";
			StringBuilder db = new StringBuilder("");
			
			// Check if this ConfigStruct is not empty
			if(cs.Count > 0)
			{
				// Create whitespace
				if(whitespace)
				{
					for(int i = 0; i < level; i++) leveltabs += "\t";
					spacing = " ";
				}
				
				// Get enumerator
				IDictionaryEnumerator de = cs.GetEnumerator();
				
				// Go for each item
				for(int i = 0; i < cs.Count; i++)
				{
					// Go to next item
					de.MoveNext();
					
					// Check if the value is null
					if(de.Value == null)
					{
						// Output key only
						db.Append(leveltabs); db.Append(de.Key); db.Append(";"); db.Append(newline);
					}
					// Check if the value if of ConfigStruct type
					else if(de.Value is IDictionary)
					{
						// Output recursive structure
						if(whitespace) { db.Append(leveltabs); db.Append(newline); }
						db.Append(leveltabs); db.Append(de.Key); db.Append(newline);
						db.Append(leveltabs); db.Append("{"); db.Append(newline);
						db.Append(OutputStructure((IDictionary)de.Value, level + 1, newline, whitespace));
						db.Append(leveltabs); db.Append("}"); db.Append(newline);
						//if(whitespace) { db.Append(leveltabs); db.Append(newline); }
					}
					// Check if the value is of boolean type
					else if(de.Value is bool)
					{
						// Check value
						if((bool)de.Value)
						{
							// Output the keyword "true"
							db.Append(leveltabs); db.Append(de.Key); db.Append(spacing);
							db.Append("="); db.Append(spacing); db.Append("true;"); db.Append(newline);
						}
						else
						{
							// Output the keyword "false"
							db.Append(leveltabs); db.Append(de.Key); db.Append(spacing);
							db.Append("="); db.Append(spacing); db.Append("false;"); db.Append(newline);
						}
					}
					// Check if value is of float type
					else if(de.Value is float)
					{
						// Output the value with a postfixed f
						db.Append(leveltabs); db.Append(de.Key); db.Append(spacing); db.Append("=");
						db.Append(spacing); db.Append(String.Format(CultureInfo.InvariantCulture, "{0}", de.Value)); db.Append("f;"); db.Append(newline);
					}
					// Check if value is of other numeric type
					else if(de.Value.GetType().IsPrimitive)
					{
						// Output the value unquoted
						db.Append(leveltabs); db.Append(de.Key); db.Append(spacing); db.Append("=");
						db.Append(spacing); db.Append(String.Format(CultureInfo.InvariantCulture, "{0}", de.Value)); db.Append(";"); db.Append(newline);
					}
					else
					{
						// Output the value with quotes and escape characters
						db.Append(leveltabs); db.Append(de.Key); db.Append(spacing); db.Append("=");
						db.Append(spacing); db.Append("\""); db.Append(EscapedString(de.Value.ToString())); db.Append("\";"); db.Append(newline);
					}
				}
			}
			
			// Return the structure
			return db.ToString();
		}
		
		#endregion
		
		#region ================== Public Methods
		
		// This clears the last error
		public void ClearError()
		{
			// Clear error
			cpErrorResult = false;
			cpErrorDescription = "";
			cpErrorLine = 0;
			cpErrorFile = "";
		}
		
		
		// This creates a new configuration
		public void NewConfiguration() { NewConfiguration(false); }
		public void NewConfiguration(bool sorted)
		{
			// Create new configuration
			if(sorted) root = new ListDictionary(); else root = new Hashtable();
		}
		
		// This checks if a given setting exists (disregards type)
		public bool SettingExists(string setting) { return CheckSetting(root, setting, DEFAULT_SEPERATOR); }
		public bool SettingExists(string setting, string pathseperator) { return CheckSetting(root, setting, pathseperator); }
		
		// This can give a value of a key specified in a path form
		// also, this does not error when the setting does not exist,
		// but instead returns the given default value.
		public string ReadSetting(string setting, string defaultsetting) { object r = ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR); if(r != null) return r.ToString(); else return null; }
		public string ReadSetting(string setting, string defaultsetting, string pathseperator) { object r = ReadAnySetting(setting, defaultsetting, pathseperator); if(r != null) return r.ToString(); else return null; }
		public int ReadSetting(string setting, int defaultsetting) { return Convert.ToInt32(ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR), CultureInfo.InvariantCulture); }
		public int ReadSetting(string setting, int defaultsetting, string pathseperator) { return Convert.ToInt32(ReadAnySetting(setting, defaultsetting, pathseperator), CultureInfo.InvariantCulture); }
		public float ReadSetting(string setting, float defaultsetting) { return Convert.ToSingle(ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR), CultureInfo.InvariantCulture); }
		public float ReadSetting(string setting, float defaultsetting, string pathseperator) { return Convert.ToSingle(ReadAnySetting(setting, defaultsetting, pathseperator), CultureInfo.InvariantCulture); }
		public short ReadSetting(string setting, short defaultsetting) { return Convert.ToInt16(ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR), CultureInfo.InvariantCulture); }
		public short ReadSetting(string setting, short defaultsetting, string pathseperator) { return Convert.ToInt16(ReadAnySetting(setting, defaultsetting, pathseperator), CultureInfo.InvariantCulture); }
		public long ReadSetting(string setting, long defaultsetting) { return Convert.ToInt64(ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR), CultureInfo.InvariantCulture); }
		public long ReadSetting(string setting, long defaultsetting, string pathseperator) { return Convert.ToInt64(ReadAnySetting(setting, defaultsetting, pathseperator), CultureInfo.InvariantCulture); }
		public bool ReadSetting(string setting, bool defaultsetting) { return Convert.ToBoolean(ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR), CultureInfo.InvariantCulture); }
		public bool ReadSetting(string setting, bool defaultsetting, string pathseperator) { return Convert.ToBoolean(ReadAnySetting(setting, defaultsetting, pathseperator), CultureInfo.InvariantCulture); }
		public byte ReadSetting(string setting, byte defaultsetting) { return Convert.ToByte(ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR), CultureInfo.InvariantCulture); }
		public byte ReadSetting(string setting, byte defaultsetting, string pathseperator) { return Convert.ToByte(ReadAnySetting(setting, defaultsetting, pathseperator), CultureInfo.InvariantCulture); }
		public IDictionary ReadSetting(string setting, IDictionary defaultsetting) { return (IDictionary)ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR); }
		public IDictionary ReadSetting(string setting, IDictionary defaultsetting, string pathseperator) { return (IDictionary)ReadAnySetting(setting, defaultsetting, pathseperator); }

		public object ReadSettingObject(string setting, object defaultsetting) { return ReadAnySetting(setting, defaultsetting, DEFAULT_SEPERATOR); }
		
		
		// This writes a given setting to the configuration.
		// Wont change existing structs, but will add them as needed.
		// Returns true when written, false when failed.
		public bool WriteSetting(string setting, object settingvalue) { return WriteSetting(setting, settingvalue, DEFAULT_SEPERATOR); }
		public bool WriteSetting(string setting, object settingvalue, string pathseperator)
		{
			IDictionary cs;
			
			// Split the path in an array
			string[] keys = setting.Split(pathseperator.ToCharArray());
			string finalkey = keys[keys.Length - 1];
			
			// Get the root item
			object item = root;
			
			// Go for each path item
			for(int i = 0; i < (keys.Length - 1); i++)
			{
				// Check if the key is valid
				if(ValidateKey(keys[i].Trim(), "", -1))
				{
					// Cast to ConfigStruct
					cs = (IDictionary)item;
					
					// Check if the requested item exists
					if(cs.Contains(keys[i]))
					{
						// Check if the requested item is a ConfigStruct
						if(cs[keys[i]] is IDictionary)
						{
							// Set the item to the next item
							item = cs[keys[i]];
						}
						else
						{
							// Cant proceed with path
							return false;
						}
					}
					else
					{
						// Key not found
						// Create it now
						IDictionary ncs;
						if(root is ListDictionary) ncs = new ListDictionary(); else ncs = new Hashtable();
						cs.Add(keys[i], ncs);
						
						// Set the item to the next item
						item = cs[keys[i]];
					}
				}
				else
				{
					// Invalid key in path
					return false;
				}
			}
			
			// Cast to ConfigStruct
			cs = (IDictionary)item;
			
			// Check if the key already exists
			if(cs.Contains(finalkey))
			{
				// Update the value
				cs[finalkey] = settingvalue;
			}
			else
			{
				// Create the key/value pair
				cs.Add(finalkey, settingvalue);
			}
			
			// Return success
			return true;
		}
		
		
		// This removes a given setting from the configuration.
		public bool DeleteSetting(string setting) { return DeleteSetting(setting, DEFAULT_SEPERATOR); }
		public bool DeleteSetting(string setting, string pathseperator)
		{
			IDictionary cs;
			
			// Split the path in an array
			string[] keys = setting.Split(pathseperator.ToCharArray());
			string finalkey = keys[keys.Length - 1];
			
			// Get the root item
			object item = root;
			
			// Go for each path item
			for(int i = 0; i < (keys.Length - 1); i++)
			{
				// Check if the key is valid
				if(ValidateKey(keys[i].Trim(), "", -1))
				{
					// Cast to ConfigStruct
					cs = (IDictionary)item;
					
					// Check if the requested item exists
					if(cs.Contains(keys[i]))
					{
						// Check if the requested item is a ConfigStruct
						if(cs[keys[i]] is IDictionary)
						{
							// Set the item to the next item
							item = cs[keys[i]];
						}
						else
						{
							// Cant proceed with path
							return false;
						}
					}
					else
					{
						// Key not found
						// Create it now
						IDictionary ncs;
						if(root is ListDictionary) ncs = new ListDictionary(); else ncs = new Hashtable();
						cs.Add(keys[i], ncs);
						
						// Set the item to the next item
						item = cs[keys[i]];
					}
				}
				else
				{
					// Invalid key in path
					return false;
				}
			}
			
			// Cast to ConfigStruct
			cs = (IDictionary)item;
			
			// Arrived at our destination
			// Delete the key if the key exists
			if(cs.Contains(finalkey))
			{
				// Key exists, delete it
				cs.Remove(finalkey);
				
				// Return success
				return true;
			}
			else
			{
				// Key not found, return fail
				return false;
			}
		}
		
		
		// This will save the current configuration to the specified file
		public bool SaveConfiguration(string filename) { return SaveConfiguration(filename, "\r\n", true); }
		public bool SaveConfiguration(string filename, string newline) { return SaveConfiguration(filename, newline, true); }
		public bool SaveConfiguration(string filename, string newline, bool whitespace)
		{
			// Kill the file if it exists
			if(File.Exists(filename)) File.Delete(filename);
			
			// Open file stream for writing
			FileStream fstream = File.OpenWrite(filename);
			
			// Create output structure and write to file
			string data = OutputConfiguration(newline, whitespace);
			byte[] baData= Encoding.UTF8.GetBytes(data);
			fstream.Write(baData, 0, baData.Length);
			fstream.Flush();
			fstream.Close();
			
			// Return true when done, false when errors occurred
			return !cpErrorResult;
		}
		
		
		// This will output the current configuration as a string
		public string OutputConfiguration() { return OutputConfiguration("\r\n", true); }
		public string OutputConfiguration(string newline) { return OutputConfiguration(newline, true); }
		public string OutputConfiguration(string newline, bool whitespace)
		{
			// Simply return the configuration structure as string
			return OutputStructure(root, 0, newline, whitespace);
		}
		
		
		// This will load a configuration from file
		public bool LoadConfiguration(string filename) { return LoadConfiguration(filename, false); }
		public bool LoadConfiguration(string filename, bool sorted)
		{
			// Check if the file is missing
			if(!File.Exists(filename))
			{
				throw(new FileNotFoundException("File not found \"" + filename + "\"", filename));
			}
			else
			{
				// Load the file contents
				FileStream fstream = File.OpenRead(filename);
				byte[] fbuffer = new byte[fstream.Length];
				fstream.Read(fbuffer, 0, fbuffer.Length);
				fstream.Close();
				
				// Convert byte array to string
				string data = Encoding.UTF8.GetString(fbuffer);
				
				// Load the configuration from this data
				return InputConfiguration(filename, data, sorted);
			}
		}
		
		
		// This will load a configuration from string
		public bool InputConfiguration(string data) { return InputConfiguration(data, false); }
		public bool InputConfiguration(string data, bool sorted) { return InputConfiguration("", data, sorted); }
		private bool InputConfiguration(string file, string data, bool sorted)
		{
			// Remove returns and tabs because the
			// parser only uses newline for new lines.
			data = data.Replace("\r", "");
			data = data.Replace("\t", "");
			
			// Clear errors
			ClearError();
			
			// Parse the data to the root structure
			if(sorted) root = new ListDictionary(); else root = new Hashtable();
			int pos = 0, line = 1;
			InputStructure(root, ref file, ref data, ref pos, ref line);
			
			// Return true when done, false when errors occurred
			return !cpErrorResult;
		}
		
		#endregion
	}
}
