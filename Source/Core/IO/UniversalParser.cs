
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
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	public sealed class UniversalParser
	{
		#region ================== Constants
		
		// Path seperator
		//public const string DEFAULT_SEPERATOR = ".";
		
		// Allowed characters in a key
		private const string KEY_CHARACTERS = "abcdefghijklmnopqrstuvwxyz0123456789_";
		
		// Parse mode constants
		private const int PM_NOTHING = 0;
		private const int PM_ASSIGNMENT = 1;
		private const int PM_NUMBER = 2;
		private const int PM_STRING = 3;
		private const int PM_KEYWORD = 4;
		
		// Error strings
		private const string ERROR_KEYMISSING = "Missing key name in assignment or scope.";
		private const string ERROR_KEYCHARACTERS = "Invalid characters in key name.";
		//private const string ERROR_ASSIGNINVALID = "Invalid assignment. Missing a previous terminator symbol?";
		private const string ERROR_VALUEINVALID = "Invalid value in assignment. Missing a previous terminator symbol?";
		private const string ERROR_VALUETOOBIG = "Value too big.";
		private const string ERROR_KEYWITHOUTVALUE = "Key has no value assigned.";
		private const string ERROR_KEYWORDUNKNOWN = "Unknown keyword in assignment. Missing a previous terminator symbol?";
		
		#endregion
		
		#region ================== Variables
		
		// Error result
		private int cpErrorResult;
		private string cpErrorDescription = "";
		private int cpErrorLine;
		
		// Configuration root
		private UniversalCollection root;

		private const string newline = "\n";
		private StringBuilder key;  //mxd
		private StringBuilder val;  //mxd
		private Dictionary<string, UniversalEntry> matches; //mxd

		// Settings
		private bool strictchecking = true;
		
		#endregion
		
		#region ================== Properties
		
		// Properties
		public int ErrorResult { get { return cpErrorResult; } }
		public string ErrorDescription { get { return cpErrorDescription; } }
		public int ErrorLine { get { return cpErrorLine; } }
		public UniversalCollection Root { get { return root; } }
		public bool StrictChecking { get { return strictchecking; } set { strictchecking = value; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public UniversalParser()
		{
			// Standard new configuration
			NewConfiguration();
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		// Constructor to load a file immediately
		public UniversalParser(string filename)
		{
			// Load configuration from file
			LoadConfiguration(filename);

			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		#endregion
		
		#region ================== Private Methods

		// This returns a string added with escape characters
		private static string EscapedString(string str)
		{
			// Replace the \ with \\ first!
			str = str.Replace("\\", "\\\\");
			str = str.Replace(newline, "\\n");
			str = str.Replace("\r", "\\r");
			str = str.Replace("\t", "\\t");
			str = str.Replace("\"", "\\\"");
			
			// Return result
			return str;
		}
		
		
		// This raises an error
		private void RaiseError(int line, string description)
		{
			// Raise error
			cpErrorResult = 1;
			cpErrorDescription = description;
			cpErrorLine = line + 1; //mxd
		}
		
		
		// This validates a given key and sets
		// error properties if key is invalid and errorline > -1
		private bool ValidateKey(string key, int errorline)
		{
			// Check if key is an empty string
			if(key.Length == 0)
			{
				// ERROR: Missing key name in statement
				if(errorline > -1) RaiseError(errorline, ERROR_KEYMISSING);
				return false;
			}

			//Only when strict checking
			if(strictchecking) 
			{
				// Check if all characters are valid
				string keylc = key.ToLowerInvariant(); //mxd. UDMF key names are case-insensitive
				foreach(char c in keylc) 
				{
					if(KEY_CHARACTERS.IndexOf(c) == -1) 
					{
						// ERROR: Invalid characters in key name
						if(errorline > -1) RaiseError(errorline, ERROR_KEYCHARACTERS);
						return false;
					}
				}
			}
			
			// Key is valid
			return true;
		}
		
		
		// This parses a structure in the given data starting
		// from the given pos and line and updates pos and line.
		private UniversalCollection InputStructure(ref string[] data, ref int pos, ref int line, bool topLevel)
		{
			int pm = PM_NOTHING;			// current parse mode
			key.Remove(0, key.Length);
			val.Remove(0, val.Length);
			bool escape = false;			// escape sequence?
			bool endofstruct = false;		// true as soon as this level struct ends
			UniversalCollection cs = new UniversalCollection();
			
			// Go through all of the data until
			// the end or until the struct closes
			// or when an arror occurred
			while((cpErrorResult == 0) && (endofstruct == false))
			{
				// Get current character
				if(line == data.Length - 1) break;
				if(pos > data[line].Length - 1) 
				{
					pos = 0;
					line++;
					if(string.IsNullOrEmpty(data[line])) continue; //mxd. Skip empty lines here so correct line number is displayed on errors
				}

				char c = data[line][pos]; // current data character
				
				// ================ What parse mode are we at?
				if(pm == PM_NOTHING)
				{
					// Now check what character this is
					switch(c)
					{
						case '{': // Begin of new struct
							// Validate key
							string s = key.ToString().Trim();
							if(ValidateKey(s, line))
							{
								// Next character
								pos++;
								
								// Parse this struct and add it
								cs.Add(new UniversalEntry(s.ToLowerInvariant(), InputStructure(ref data, ref pos, ref line, false)));
								
								// Check the last character
								pos--;
								
								// Reset the key
								key.Remove(0, key.Length);
							}
							
							// Leave switch
							break;
							
						case '}': // End of this struct
							// Stop parsing in this struct
							endofstruct = true;
							
							// Leave the loop
							break;
							
						case '=': // Assignment
							// Validate key
							if(ValidateKey(key.ToString().Trim(), line))
							{
								// Now parsing assignment
								pm = PM_ASSIGNMENT;
							}
							
							// Leave switch
							break;
							
						case ';': // Terminator
							
							// Validate key
							if(ValidateKey(key.ToString().Trim(), line))
							{
								// Error: No value
								RaiseError(line, ERROR_KEYWITHOUTVALUE);
							}
							
							// Leave switch
							break;
							
						case '\n': // New line
							// Count the line
							line++;
							pos = -1;
							
							// Add this to the key as a space.
							// Spaces are not allowed, but it will be trimmed
							// when its the first or last character.
							key.Append(" ");
							
							// Leave switch
							break;
							
						case '\\': // Possible comment
						case '/':
							// Check for the line comment //
							if(data[line].Substring(pos, 2) == "//")
							{
								// Skip everything on this line
								pos = -1;
								
								// Have next line?
								if(line < data.Length - 1) line++;
							}
							// Check for the block comment /* */
							else if(data[line].Substring(pos, 2) == "/*")
							{
								// Block comment closes on the same line?.. (mxd)
								int np = data[line].IndexOf("*/", pos);
								if(np > -1)
								{
									pos = np + 1;
								}
								else
								{
									// Find the next closing block comment
									line++;
									while((np = data[line].IndexOf("*/", 0)) == -1) 
									{
										if(line == data.Length - 1) break;
										line++;
									}

									// Closing block comment found?
									if(np > -1) 
									{
										// Skip everything in this block
										pos = np + 1;
									}
								}
							}
							
							// Leave switch
							break;
							
						default: // Everything else
							if(!topLevel && pos == 0) 
							{
								while(matches.ContainsKey(data[line])) 
								{
									cs.Add(matches[data[line]].Key, matches[data[line]].Value);
									line++;
									pos = -1;
								}
							}

							// Add character to key
							if(pos != -1) key.Append(c);

							// Leave switch
							break;
					}
				}
				// ================ Parsing an assignment
				else if(pm == PM_ASSIGNMENT)
				{
					// Check for string opening
					if(c == '\"')
					{
						// Now parsing string
						pm = PM_STRING; //numbers
					}
					// Check for numeric character numbers
					else if(Configuration.NUMBERS2.IndexOf(c) > -1)
					{
						// Now parsing number
						pm = PM_NUMBER;
						
						// Go one byte back, because this
						// byte is part of the number!
						pos--;
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
						pm = PM_NOTHING;
						
						// Remove this if it causes problems
						key.Remove(0, key.Length);
						val.Remove(0, val.Length);
					}
					// Otherwise (if not whitespace) it will be a keyword
					else if((c != ' ') && (c != '\t'))
					{
						// Now parsing a keyword
						pm = PM_KEYWORD;
						
						// Go one byte back, because this
						// byte is part of the keyword!
						pos--;
					}
				}
				// ================ Parsing a number
				else if(pm == PM_NUMBER)
				{
					// Check if number ends here
					if(c == ';')
					{
						// Hexadecimal?
						string s = val.ToString();
						if((s.Length > 2) && s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
						{
							// Convert to int
							try
							{
								// Convert to value
								int ival = Convert.ToInt32(s.Substring(2).Trim(), 16);

								// Add it to struct
								UniversalEntry entry = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), ival);
								cs.Add(entry);
								if(!matches.ContainsKey(data[line])) matches.Add(data[line], entry);
							}
							catch(OverflowException)
							{
								// Too large for Int32, try Int64
								try
								{
									// Convert to value
									long lval = Convert.ToInt64(s.Substring(2).Trim(), 16);

									// Add it to struct
									UniversalEntry entry = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), lval);
									cs.Add(entry);
									if(!matches.ContainsKey(data[line])) matches.Add(data[line], entry);
								}
								catch(OverflowException)
								{
									// Too large for Int64, return error
									RaiseError(line, ERROR_VALUETOOBIG);
								}
								catch(FormatException)
								{
									// ERROR: Invalid value in assignment
									RaiseError(line, ERROR_VALUEINVALID + "\n\nUnrecognized token: \"" + s.Trim() + "\"");
								}
							}
							catch(FormatException)
							{
								// ERROR: Invalid value in assignment
								RaiseError(line, ERROR_VALUEINVALID + "\n\nUnrecognized token: \"" + s.Trim() + "\"");
							}
						}
						// Floating point?
						//mxd. Can be in scientific notation (like "1E-06")
						else if(s.IndexOf('.') > -1 || s.ToLowerInvariant().Contains("e-"))
						{
							float fval = 0;
							
							// Convert to float (remove the f first)
							try { fval = Convert.ToSingle(s.Trim(), CultureInfo.InvariantCulture); }
							catch(FormatException)
							{ 
								// ERROR: Invalid value in assignment
								RaiseError(line, ERROR_VALUEINVALID + "\n\nUnrecognized token: \"" + s.Trim() + "\"");
							}
							
							// Add it to struct
							UniversalEntry entry = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), fval);
							cs.Add(entry);
							if(!matches.ContainsKey(data[line])) matches.Add(data[line], entry);
						}
						else
						{
							// Convert to int
							try
							{
								// Convert to value
								int ival = Convert.ToInt32(s.Trim(), CultureInfo.InvariantCulture);
								
								// Add it to struct
								UniversalEntry entry = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), ival);
								cs.Add(entry);
								if(!matches.ContainsKey(data[line])) matches.Add(data[line], entry);
							}
							catch(OverflowException)
							{
								// Too large for Int32, try Int64
								try
								{
									// Convert to value
									long lval = Convert.ToInt64(s.Trim(), CultureInfo.InvariantCulture);
									
									// Add it to struct
									UniversalEntry entry = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), lval);
									cs.Add(entry);
									if(!matches.ContainsKey(data[line])) matches.Add(data[line], entry);
								}
								catch(OverflowException)
								{
									// Too large for Int64, return error
									RaiseError(line, ERROR_VALUETOOBIG);
								}
								catch(FormatException)
								{ 
									// ERROR: Invalid value in assignment
									RaiseError(line, ERROR_VALUEINVALID + "\n\nUnrecognized token: \"" + s.Trim() + "\"");
								}
							}
							catch(FormatException)
							{ 
								// ERROR: Invalid value in assignment
								RaiseError(line, ERROR_VALUEINVALID + "\n\nUnrecognized token: \"" + s.Trim() + "\"");
							}
						}
						
						// Reset key and value
						key.Remove(0, key.Length);
						val.Remove(0, val.Length);
						
						// End of assignment
						pm = PM_NOTHING;
					}
					// Check for new line
					else if(c == '\n')
					{
						// Count the new line
						line++;
						pos = -1;
					}
					// Everything else is part of the value
					else
					{
						val.Append(c);
					}
				}
				// ================ Parsing a string
				else if(pm == PM_STRING)
				{
					// Check if in an escape sequence
					if(escape)
					{
						// What character?
						switch(c)
						{
							case '\\': val.Append('\\'); break;
							case 'n': val.Append(newline); break;
							case '\"': val.Append('\"'); break;
							case 'r': val.Append('\r'); break;
							case 't': val.Append('\t'); break;
							default:
								// Is it a number?
								if(Configuration.NUMBERS.IndexOf(c) > -1)
								{
									int vv = 0;
									char vc = '0';
									
									// Convert the next 3 characters to a number
									string v = data[line].Substring(pos, 3);
									try { vv = Convert.ToInt32(v.Trim(), CultureInfo.InvariantCulture); }
									catch(FormatException)
									{ 
										// ERROR: Invalid value in assignment
										RaiseError(line, ERROR_VALUEINVALID + "\n\nUnrecognized token: \"" + v.Trim() + "\"");
									}
									
									// Convert the number to a char
									try { vc = Convert.ToChar(vv, CultureInfo.InvariantCulture); }
									catch(FormatException)
									{ 
										// ERROR: Invalid value in assignment
										RaiseError(line, ERROR_VALUEINVALID + "\n\nUnrecognized token: \"" + v.Trim() + "\"");
									}
									
									// Add the char
									val.Append(vc);
								}
								else
								{
									// Add the character as it is
									val.Append(c);
								}
								
								// Leave switch
								break;
						}
						
						// End of escape sequence
						escape = false;
					}
					else
					{
						// Check for sequence start
						if(c == '\\')
						{
							// Next character is of escape sequence
							escape = true;
						}
						// Check if string ends
						else if(c == '\"')
						{
							// Add string to struct
							UniversalEntry entry = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), val.ToString());
							cs.Add(entry);
							if(!matches.ContainsKey(data[line])) matches.Add(data[line], entry);
							
							// End of assignment
							pm = PM_ASSIGNMENT;
							
							// Reset key and value
							key.Remove(0, key.Length);
							val.Remove(0, val.Length);
						}
						// Check for new line
						else if(c == '\n')
						{
							// Count the new line
							line++;
							pos = -1;
						}
						// Everything else is just part of string
						else
						{
							// Add to value
							val.Append(c);
						}
					}
				}
				// ================ Parsing a keyword
				else if(pm == PM_KEYWORD)
				{
					// Check if keyword ends
					if(c == ';')
					{
						// Add to the struct depending on the keyword
						switch(val.ToString().Trim().ToLowerInvariant())
						{
							case "true":
								// Add boolean true
								UniversalEntry t = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), true);
								cs.Add(t);
								if(!matches.ContainsKey(data[line])) matches.Add(data[line], t);
								break;
								
							case "false":
								// Add boolean false
								UniversalEntry f = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), false);
								cs.Add(f);
								if(!matches.ContainsKey(data[line])) matches.Add(data[line], f);
								break;

                            case "nan":
                                // Add float value
                                UniversalEntry nan = new UniversalEntry(key.ToString().Trim().ToLowerInvariant(), float.NaN);
                                cs.Add(nan);
                                if (!matches.ContainsKey(data[line])) matches.Add(data[line], nan);
                                break;

                            default:
								// Unknown keyword
								RaiseError(line, ERROR_KEYWORDUNKNOWN + "\n\nUnrecognized token: \"" + val.ToString().Trim() + "\"");
								break;
						}
						
						// End of assignment
						pm = PM_NOTHING;
						
						// Reset key and value
						key.Remove(0, key.Length);
						val.Remove(0, val.Length);
					}
					// Check for new line
					else if(c == '\n')
					{
						// Count the new line
						line++;
						pos = -1;
					}
					// Everything else is just part of keyword
					else
					{
						// Add to value
						val.Append(c);
					}
				}
				
				// Next character
				pos++;
			}
			
			// Return the parsed result
			return cs;
		}
		
		
		// This will create a data structure from the given object
		private string OutputStructure(UniversalCollection cs, int level, string newline, bool whitespace)
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
				
				// Go for each item
				for(int i = 0; i < cs.Count; i++)
				{
					// Check if the value if of collection type
					if(cs[i].Value is UniversalCollection)
					{
						UniversalCollection c = (UniversalCollection)cs[i].Value;
						
						// Output recursive structure
						if(whitespace) { db.Append(leveltabs); db.Append(newline); }
						db.Append(leveltabs); db.Append(cs[i].Key);
						if(!string.IsNullOrEmpty(c.Comment))
						{
							if(whitespace) db.Append("\t");
							db.Append("// " + c.Comment);
						}
						db.Append(newline);
						db.Append(leveltabs); db.Append("{"); db.Append(newline);
						db.Append(OutputStructure(c, level + 1, newline, whitespace));
						db.Append(leveltabs); db.Append("}"); db.Append(newline);
						//if(whitespace) { db.Append(leveltabs); db.Append(newline); } //mxd. Let's save a few Kbs by using single line breaks...
					}
					// Check if the value is of boolean type
					else if(cs[i].Value is bool)
					{
						db.Append(leveltabs); db.Append(cs[i].Key); db.Append(spacing); db.Append("="); db.Append(spacing);
						db.Append((bool)cs[i].Value ? "true;" : "false;"); db.Append(newline);
					}
					// Check if value is of float type
					else if(cs[i].Value is float)
					{
						// Output the value as float (3 decimals)
						float f = (float)cs[i].Value;
						db.Append(leveltabs); db.Append(cs[i].Key); db.Append(spacing); db.Append("=");
						db.Append(spacing); db.Append(f.ToString("0.000", CultureInfo.InvariantCulture)); db.Append(";"); db.Append(newline);
					}
					//mxd. Check if value is of double type
					else if(cs[i].Value is double)
					{
						// Output the value as double (7 decimals)
						double d = (double)cs[i].Value;
						db.Append(leveltabs); db.Append(cs[i].Key); db.Append(spacing); db.Append("=");
						db.Append(spacing); db.Append(d.ToString("0.0000000", CultureInfo.InvariantCulture)); db.Append(";"); db.Append(newline);
					}
					// Check if value is of other numeric type
					else if(cs[i].Value.GetType().IsPrimitive)
					{
						// Output the value unquoted
						db.Append(leveltabs); db.Append(cs[i].Key); db.Append(spacing); db.Append("=");
						db.Append(spacing); db.Append(String.Format(CultureInfo.InvariantCulture, "{0}", cs[i].Value)); db.Append(";"); db.Append(newline);
					}
					else
					{
						// Output the value with quotes and escape characters
						db.Append(leveltabs); db.Append(cs[i].Key); db.Append(spacing); db.Append("=");
						db.Append(spacing); db.Append("\""); db.Append(EscapedString(cs[i].Value.ToString())); db.Append("\";"); db.Append(newline);
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
			cpErrorResult = 0;
			cpErrorDescription = "";
			cpErrorLine = 0;
		}
		
		
		// This creates a new configuration
		public void NewConfiguration()
		{
			// Create new configuration
			root = new UniversalCollection();
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
			byte[] baData= Encoding.ASCII.GetBytes(data);
			fstream.Write(baData, 0, baData.Length);
			fstream.Flush();
			fstream.Close();
			
			// Return true when done, false when errors occurred
			return cpErrorResult == 0;
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
		public bool LoadConfiguration(string filename)
		{
			// Check if the file is missing
			if(!File.Exists(filename))
			{
				throw(new FileNotFoundException("File not found \"" + filename + "\"", filename));
			}
			else
			{
				// Load the file contents
				List<string> data = new List<string>(100);
				using(FileStream stream = File.OpenRead(filename)) 
				{
					StreamReader reader = new StreamReader(stream, Encoding.ASCII);
					
					while(!reader.EndOfStream) 
					{
						string line = reader.ReadLine();
						if(string.IsNullOrEmpty(line)) continue;
						data.Add(line);
					}
				}

				// Load the configuration from this data
				return InputConfiguration(data.ToArray());
			}
		}
		
		
		// This will load a configuration from string
		public bool InputConfiguration(string[] data)
		{
			// Clear errors
			ClearError();
			
			// Parse the data to the root structure
			int pos = 0;
			int line = 0; //mxd
			matches = new Dictionary<string, UniversalEntry>(StringComparer.Ordinal); //mxd
			key = new StringBuilder(16); //mxd
			val = new StringBuilder(16); //mxd
			root = InputStructure(ref data, ref pos, ref line, true);
			
			// Return true when done, false when errors occurred
			return (cpErrorResult == 0);
		}
		
		#endregion
	}
}
