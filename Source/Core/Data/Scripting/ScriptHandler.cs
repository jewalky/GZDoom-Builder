#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.IO;
using ScintillaNET;

#endregion

namespace CodeImp.DoomBuilder.Data.Scripting
{
	[ScriptHandler(ScriptType.UNKNOWN)]
	internal class ScriptHandler
	{
		#region ================== Constants

		protected const int MAX_BACKTRACK_LENGTH = 200;

		#endregion

		#region ================== Variables

		private ScriptEditorControl scriptcontrol;
		protected Scintilla scriptedit;
		protected ScriptConfiguration scriptconfig;

		// List of keywords and constants
		private List<string> autocompletelist;

		// Current position information
		private string curfunctionname = "";
		private int curargumentindex;
		private int curfunctionstartpos;

		#endregion

		#region ================== Methods

		public virtual void Initialize(ScriptEditorControl scriptcontrol, ScriptConfiguration scriptconfig)
		{
			this.scriptcontrol = scriptcontrol;
			this.scriptedit = scriptcontrol.Scintilla;
			this.scriptconfig = scriptconfig;

			// Bind events
			this.scriptedit.KeyUp += scriptedit_KeyUp;
		}

		//TODO: Remove ScriptDocumentTab from here
		public virtual List<CompilerError> UpdateFunctionBarItems(ScriptDocumentTab tab, MemoryStream stream, ComboBox target)
		{
			// Unsupported script type. Just clear the items
			target.Items.Clear();
			return new List<CompilerError>();
		}

		//mxd. Autocompletion handling (https://github.com/jacobslusser/ScintillaNET/wiki/Basic-Autocompletion)
		public virtual bool ShowAutoCompletionList()
		{
			int currentpos = scriptedit.CurrentPosition;
			int wordstartpos = scriptedit.WordStartPosition(currentpos, true);

			if(wordstartpos >= currentpos)
			{
				// Hide the list
				scriptedit.AutoCCancel();
				return false;
			}

			// Get entered text
			string start = scriptedit.GetTextRange(wordstartpos, currentpos - wordstartpos);
			if(string.IsNullOrEmpty(start))
			{
				// Hide the list
				scriptedit.AutoCCancel();
				return false;
			}

			// Don't show Auto-completion list when editing comment, include or string
			switch(scriptcontrol.GetScriptStyle(scriptedit.GetStyleAt(currentpos)))
			{
				case ScriptStyleType.Comment:
				case ScriptStyleType.String:
					// Hide the list
					scriptedit.AutoCCancel();
					return false;

				case ScriptStyleType.Include:
					// Hide the list unless current word is a keyword
					if(!start.StartsWith("#"))
					{
						scriptedit.AutoCCancel();
						return false;
					}
					break;
			}

			// Filter the list
			List<string> filtered = new List<string>();
			foreach(string s in autocompletelist)
				if(s.IndexOf(start, StringComparison.OrdinalIgnoreCase) != -1) filtered.Add(s);

			// Any matches?
			if(filtered.Count > 0)
			{
				// Show the list
				scriptedit.AutoCShow(currentpos - wordstartpos, string.Join(" ", filtered.ToArray()));
				return true;
			}

			// Hide the list
			scriptedit.AutoCCancel();
			return false;
		}

		internal void SetKeywords(Configuration lexercfg, string lexername)
		{
			Dictionary<string, string> autocompletedict = new Dictionary<string, string>(StringComparer.Ordinal);
			
			// Create the keywords list and apply it
			string imageindex = ((int)ScriptEditorControl.ImageIndex.ScriptKeyword).ToString(CultureInfo.InvariantCulture);
			int keywordsindex = lexercfg.ReadSetting(lexername + ".keywordsindex", -1);
			if(keywordsindex > -1)
			{
				StringBuilder keywordslist = new StringBuilder();
				foreach(string k in scriptconfig.Keywords)
				{
					if(keywordslist.Length > 0) keywordslist.Append(" ");
					keywordslist.Append(k);

					//mxd. Skip adding the keyword if we have a snippet with the same name
					if(!scriptconfig.Snippets.Contains(k)) autocompletedict.Add(k, k + "?" + imageindex);
				}
				string words = keywordslist.ToString();
				scriptedit.SetKeywords(keywordsindex, (scriptconfig.CaseSensitive ? words : words.ToLowerInvariant()));
			}

			//mxd. Create the properties list and apply it
			imageindex = ((int)ScriptEditorControl.ImageIndex.ScriptProperty).ToString(CultureInfo.InvariantCulture);
			int propertiesindex = lexercfg.ReadSetting(lexername + ".propertiesindex", -1);
			if(propertiesindex > -1)
			{
				StringBuilder propertieslist = new StringBuilder();
				HashSet<string> addedprops = new HashSet<string>();
				char[] dot = { '.' };
				foreach(string p in scriptconfig.Properties)
				{
					if(propertieslist.Length > 0) propertieslist.Append(" ");

					// Scintilla doesn't highlight keywords with '.' or ':', so get rid of those 
					if(scriptconfig.ScriptType == ScriptType.DECORATE)
					{
						string prop = p;
						if(prop.Contains(":")) prop = prop.Replace(":", string.Empty);
						if(prop.Contains("."))
						{
							// Split dotted properties into separate entries
							string[] parts = prop.Split(dot, StringSplitOptions.RemoveEmptyEntries);
							List<string> result = new List<string>();
							foreach(string part in parts)
							{
								if(!addedprops.Contains(part))
								{
									result.Add(part);
									addedprops.Add(part);
								}
							}

							if(result.Count > 0) propertieslist.Append(string.Join(" ", result.ToArray()));
						}
						else
						{
							addedprops.Add(prop);
							propertieslist.Append(prop);
						}
					}
					else
					{
						propertieslist.Append(p);
					}

					// Autocomplete doesn't mind '.' or ':'
					// Skip adding the keyword if we have a snippet with the same name
					if(!scriptconfig.Snippets.Contains(p))
					{
						if(autocompletedict.ContainsKey(p))
							General.ErrorLogger.Add(ErrorType.Warning, "Property \"" + p + "\" is double defined in \"" + scriptconfig.Description + "\" script configuration.");
						else
							autocompletedict.Add(p, p + "?" + imageindex);
					}
				}
				string words = propertieslist.ToString();
				scriptedit.SetKeywords(propertiesindex, (scriptconfig.CaseSensitive ? words : words.ToLowerInvariant()));
			}

			// Create the constants list and apply it
			imageindex = ((int)ScriptEditorControl.ImageIndex.ScriptConstant).ToString(CultureInfo.InvariantCulture);
			int constantsindex = lexercfg.ReadSetting(lexername + ".constantsindex", -1);
			if(constantsindex > -1)
			{
				StringBuilder constantslist = new StringBuilder();
				foreach(string c in scriptconfig.Constants)
				{
					if(autocompletedict.ContainsKey(c))
						continue; //mxd. This happens when there's a keyword and a constant with the same name...

					if(constantslist.Length > 0) constantslist.Append(" ");
					constantslist.Append(c);

					//mxd. Skip adding the constant if we have a snippet with the same name
					if(!scriptconfig.Snippets.Contains(c)) autocompletedict.Add(c, c + "?" + imageindex);
				}
				string words = constantslist.ToString();
				scriptedit.SetKeywords(constantsindex, (scriptconfig.CaseSensitive ? words : words.ToLowerInvariant()));
			}

			//mxd. Create the snippets list and apply it
			imageindex = ((int)ScriptEditorControl.ImageIndex.ScriptSnippet).ToString(CultureInfo.InvariantCulture);
			int snippetindex = lexercfg.ReadSetting(lexername + ".snippetindex", -1);
			if(snippetindex > -1 && scriptconfig.Snippets.Count > 0)
			{
				StringBuilder snippetslist = new StringBuilder();
				foreach(string s in scriptconfig.Snippets)
				{
					if(snippetslist.Length > 0) snippetslist.Append(" ");
					snippetslist.Append(s);
					autocompletedict.Add(s, s + "?" + imageindex);
				}
				string words = snippetslist.ToString();
				scriptedit.SetKeywords(snippetindex, (scriptconfig.CaseSensitive ? words : words.ToLowerInvariant()));
			}

			// Make autocomplete list
			autocompletelist = new List<string>(autocompletedict.Values);
		}

		// This gathers information about the current caret position
		protected void UpdatePositionInfo()
		{
			int bracketlevel = 0;			// bracket level counting
			int argindex = 0;				// function argument counting
			int pos = scriptedit.CurrentPosition;

			// Get the text
			string scripttext = scriptedit.Text;

			// Reset position info
			curfunctionname = "";
			curargumentindex = 0;
			curfunctionstartpos = 0;

			// Determine lowest backtrack position
			int limitpos = Math.Max(0, scriptedit.CurrentPosition - MAX_BACKTRACK_LENGTH);

			// We can only do this when we have function syntax information
			if((scriptconfig.ArgumentDelimiter.Length == 0) || (scriptconfig.FunctionClose.Length == 0) ||
			   (scriptconfig.FunctionOpen.Length == 0) || (scriptconfig.Terminator.Length == 0))
				return;

			// Get int versions of the function syntax informantion
			int argumentdelimiter = scriptconfig.ArgumentDelimiter[0];
			int functionclose = scriptconfig.FunctionClose[0];
			int functionopen = scriptconfig.FunctionOpen[0];
			int terminator = scriptconfig.Terminator[0];

			// Continue backtracking until we reached the limitpos
			while(pos >= limitpos)
			{
				// Backtrack 1 character
				pos--;

				// Get the style and character at this position
				ScriptStyleType curstyle = scriptcontrol.GetScriptStyle(scriptedit.GetStyleAt(pos));
				int curchar = scriptedit.GetCharAt(pos);

				// Then meeting ) then increase bracket level
				// When meeting ( then decrease bracket level
				// When bracket level goes -1, then the next word should be the function name
				// Only when at bracket level 0, count the comma's for argument index

				// TODO:
				// Original code checked for scope character here and breaks if found

				// Check if in plain text or keyword
				if((curstyle == ScriptStyleType.PlainText) || (curstyle == ScriptStyleType.Keyword))
				{
					// Closing bracket
					if(curchar == functionclose)
					{
						bracketlevel++;
					}
					// Opening bracket
					else if(curchar == functionopen)
					{
						bracketlevel--;

						// Out of the brackets?
						if(bracketlevel < 0)
						{
							// Skip any whitespace before this bracket
							do
							{
								// Backtrack 1 character
								curchar = scriptedit.GetCharAt(--pos);
							}
							while((pos >= limitpos) && ((curchar == ' ') || (curchar == '\t') ||
														(curchar == '\r') || (curchar == '\n')));

							// NOTE: We may need to set onlyWordCharacters argument in the
							// following calls to false to get any argument delimiter included,
							// but this may also cause a valid keyword to be combined with other
							// surrounding characters that do not belong to the keyword.

							// Find the word before this bracket
							int wordstart = scriptedit.WordStartPosition(pos, true);
							int wordend = scriptedit.WordEndPosition(pos, true);
							string word = scripttext.Substring(wordstart, wordend - wordstart);
							if(word.Length > 0)
							{
								// Check if this is an argument delimiter
								// I can't remember why I did this, but I'll probably stumble
								// upon the problem if this doesn't work right (see note above)
								if(word[0] == argumentdelimiter)
								{
									// We are now in the parent function
									bracketlevel++;
									argindex = 0;
								}
								// Now check if this is a keyword
								else if(scriptconfig.IsKeyword(word))
								{
									// Found it!
									curfunctionname = scriptconfig.GetKeywordCase(word);
									curargumentindex = argindex;
									curfunctionstartpos = wordstart;
									break;
								}
								else
								{
									// Don't know this word
									break;
								}
							}
						}
					}
					// Argument delimiter
					else if(curchar == argumentdelimiter)
					{
						// Only count these at brackt level 0
						if(bracketlevel == 0) argindex++;
					}
					// Terminator
					else if(curchar == terminator)
					{
						// Can't find anything, break now
						break;
					}
				}
			}
		}

		#endregion

		#region ================== Events

		private void scriptedit_KeyUp(object sender, KeyEventArgs e)
		{
			bool showcalltip = false;
			int highlightstart = 0;
			int highlightend = 0;

			UpdatePositionInfo();

			// Call tip shown
			if(scriptedit.CallTipActive)
			{
				// Should we hide the call tip?
				if(curfunctionname.Length == 0)
				{
					// Hide the call tip
					scriptedit.CallTipCancel();
				}
				else
				{
					// Update the call tip
					showcalltip = true;
				}
			}
			// No call tip
			else
			{
				// Should we show a call tip?
				showcalltip = (curfunctionname.Length > 0) && !scriptedit.AutoCActive;
			}

			// Show or update call tip
			if(showcalltip)
			{
				string functiondef = scriptconfig.GetFunctionDefinition(curfunctionname);
				if(functiondef != null)
				{
					// Determine the range to highlight
					int argsopenpos = functiondef.IndexOf(scriptconfig.FunctionOpen, StringComparison.Ordinal);
					int argsclosepos = functiondef.LastIndexOf(scriptconfig.FunctionClose, StringComparison.Ordinal);
					if((argsopenpos > -1) && (argsclosepos > -1))
					{
						string argsstr = functiondef.Substring(argsopenpos + 1, argsclosepos - argsopenpos - 1);
						string[] args = argsstr.Split(scriptconfig.ArgumentDelimiter[0]);
						if((curargumentindex >= 0) && (curargumentindex < args.Length))
						{
							int argoffset = 0;
							for(int i = 0; i < curargumentindex; i++) argoffset += args[i].Length + 1;
							highlightstart = argsopenpos + argoffset + 1;
							highlightend = highlightstart + args[curargumentindex].Length;
						}
					}

					// Determine callip position
					int tippos = curfunctionstartpos;
					int funcline = scriptedit.LineFromPosition(curfunctionstartpos);

					//mxd. If the tip obscures the view, move it down
					if(scriptedit.CurrentLine > funcline)
					{
						int offset = curfunctionstartpos - scriptedit.Lines[funcline].Position;
						tippos = scriptedit.Lines[scriptedit.CurrentLine].Position + offset;
					}

					//mxd. Take line wrapping into account
					if(scriptedit.Lines[scriptedit.CurrentLine].WrapCount > 0)
					{
						int x = scriptedit.PointXFromPosition(tippos);
						int y = scriptedit.PointYFromPosition(scriptedit.CurrentPosition);
						int newtippos = scriptedit.CharPositionFromPointClose(x, y);
						if(newtippos != -1) tippos = newtippos;
					}

					// Show tip
					scriptedit.CallTipShow(tippos, functiondef);
					scriptedit.CallTipSetHlt(highlightstart, highlightend);
				}
			}
		}

		#endregion
	}
}
