
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	// This is only a wrapper for the Scintilla editor control. Most of this code is
	// from ScintillaNET project, I only refactored it a bit. See the BuilderScriptControl
	// for the script editor with Doom Builder features.
	internal class ScintillaControl : Control
	{
		#region ================== API Declarations

		[DllImport(SCINTILLA_FILENAME, EntryPoint = "Scintilla_DirectFunction")]
		private static extern int Perform(int ptr, UInt32 message, UInt32 wparam, UInt32 lparam);
		
		#endregion
		
		#region ================== Constants

		// Class library
		private const string SCINTILLA_FILENAME = "Scintilla.dll";
		private const string SCINTILLA_CLASSNAME = "Scintilla";

		// Styles
		private const uint WS_CHILD = (uint)0x40000000L;
		private const uint WS_VISIBLE = (uint)0x10000000L;
		private const uint WS_TABSTOP = (uint)0x00010000L;
		private const int WM_NOTIFY = 0x004E;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		
		#endregion

		#region ================== Delegates / Events

		public delegate void StyleNeededHandler(ScintillaControl pSender, int position);
		public delegate void CharAddedHandler(ScintillaControl pSender, int ch);
		public delegate void SavePointReachedHandler(ScintillaControl pSender);
		public delegate void SavePointLeftHandler(ScintillaControl pSender);
		public delegate void ModifyAttemptROHandler(ScintillaControl pSender);
		public delegate void KeyHandler(ScintillaControl pSender, int ch, int modifiers);
		public delegate void DoubleClickHandler(ScintillaControl pSender);
		public delegate void UpdateUIHandler(ScintillaControl pSender);
		public delegate void ModifiedHandler(ScintillaControl pSender, int position, int modificationType, string text, int length, int linesAdded, int line, int foldLevelNow, int foldLevelPrev);
		public delegate void MacroRecordHandler(ScintillaControl pSender, int message, IntPtr wParam, IntPtr lParam);
		public delegate void MarginClickHandler(ScintillaControl pSender, int modifiers, int position, int margin);
		public delegate void NeedShownHandler(ScintillaControl pSender, int position, int length);
		public delegate void PaintedHandler(ScintillaControl pSender);
		public delegate void UserListSelectionHandler(ScintillaControl pSender, int listType, string text);
		public delegate void URIDroppedHandler(ScintillaControl pSender, string text);
		public delegate void DwellStartHandler(ScintillaControl pSender, int position);
		public delegate void DwellEndHandler(ScintillaControl pSender, int position);
		public delegate void ZoomHandler(ScintillaControl pSender);
		public delegate void HotSpotClickHandler(ScintillaControl pSender, int modifiers, int position);
		public delegate void HotSpotDoubleClickHandler(ScintillaControl pSender, int modifiers, int position);
		public delegate void CallTipClickHandler(ScintillaControl pSender, int position);
		public delegate void TextInsertedHandler(ScintillaControl pSender, int position, int length, int linesAdded);
		public delegate void TextDeletedHandler(ScintillaControl pSender, int position, int length, int linesAdded);
		public delegate void StyleChangedHandler(ScintillaControl pSender, int position, int length);
		public delegate void FoldChangedHandler(ScintillaControl pSender, int line, int foldLevelNow, int foldLevelPrev);
		public delegate void UserPerformedHandler(ScintillaControl pSender);
		public delegate void UndoPerformedHandler(ScintillaControl pSender);
		public delegate void RedoPerformedHandler(ScintillaControl pSender);
		public delegate void LastStepInUndoRedoHandler(ScintillaControl pSender);
		public delegate void MarkerChangedHandler(ScintillaControl pSender, int line);
		public delegate void BeforeInsertHandler(ScintillaControl pSender, int position, int length);
		public delegate void BeforeDeleteHandler(ScintillaControl pSender, int position, int length);

		public event StyleNeededHandler StyleNeeded;
		public event CharAddedHandler CharAdded;
		public event SavePointReachedHandler SavePointReached;
		public event SavePointLeftHandler SavePointLeft;
		public event ModifyAttemptROHandler ModifyAttemptRO;
		public event KeyHandler Key;
		public new event DoubleClickHandler DoubleClick;
		public event UpdateUIHandler UpdateUI;
		public event ModifiedHandler Modified;
		public event MacroRecordHandler MacroRecord;
		public event MarginClickHandler MarginClick;
		public event NeedShownHandler NeedShown;
		public event PaintedHandler Painted;
		//public event UserListSelectionHandler UserListSelection;
		//public event URIDroppedHandler URIDropped;
		public event DwellStartHandler DwellStart;
		public event DwellEndHandler DwellEnd;
		public event ZoomHandler Zoom;
		public event HotSpotClickHandler HotSpotClick;
		public event HotSpotDoubleClickHandler HotSpotDoubleClick;
		public event CallTipClickHandler CallTipClick;
		public event TextInsertedHandler TextInserted;
		public event TextDeletedHandler TextDeleted;
		public event FoldChangedHandler FoldChanged;
		public event UserPerformedHandler UserPerformed;
		public event UndoPerformedHandler UndoPerformed;
		public event RedoPerformedHandler RedoPerformed;
		public event LastStepInUndoRedoHandler LastStepInUndoRedo;
		public event MarkerChangedHandler MarkerChanged;
		public event BeforeInsertHandler BeforeInsert;
		public event BeforeDeleteHandler BeforeDelete;
		public new event StyleChangedHandler StyleChanged;
		
		#endregion

		#region ================== Variables

		// Main objects
		private IntPtr libraryptr = IntPtr.Zero;
		private IntPtr controlptr = IntPtr.Zero;
		private int directptr;

		// This ignores key combinations so that they are passed
		// on to the other controls on the parent form
		private readonly Dictionary<int, int> ignoredkeys;
		
		#endregion

		#region ================== Properties
		
		/// <summary>
		/// Are white space characters currently visible?
		/// Returns one of SCWS_* constants.
		/// </summary>
		public ScriptWhiteSpace ViewWhitespace
		{
			get { return (ScriptWhiteSpace)ViewWS; }
			set { ViewWS = (int)value; }
		}

		/// <summary>
		/// Retrieve the current end of line mode - one of CRLF, CR, or LF.
		/// </summary>
		public ScriptEndOfLine EndOfLineMode
		{
			get { return (ScriptEndOfLine)EOLMode; }
			set { EOLMode = (int)value; }
		}

		/// <summary>
		/// Are there any redoable actions in the undo history?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN09</remarks>
		public bool CanRedo
		{
			get
			{
				return FastPerform(2016, 0, 0) != 0;
			}
		}

		/// <summary>
		/// Is there an auto-completion list visible?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN09</remarks>
		public bool IsAutoCActive
		{
			get
			{
				return FastPerform(2102, 0, 0) != 0;
			}
		}

		/// <summary>
		/// Retrieve the position of the caret when the auto-completion list was displayed.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN10</remarks>
		public int AutoCPosStart
		{
			get
			{
				return (int)FastPerform(2103, 0, 0);
			}
		}

		/// <summary>
		/// Will a paste succeed?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN09</remarks>
		public bool CanPaste
		{
			get
			{
				return FastPerform(2173, 0, 0) != 0;
			}
		}

		/// <summary>
		/// Are there any undoable actions in the undo history?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN09</remarks>
		public bool CanUndo
		{
			get
			{
				return FastPerform(2174, 0, 0) != 0;
			}
		}

		/// <summary>
		/// Is there an active call tip?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN09</remarks>
		public bool IsCallTipActive
		{
			get
			{
				return FastPerform(2202, 0, 0) != 0;
			}
		}

		/// <summary>
		/// Retrieve the position where the caret was before displaying the call tip.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN10</remarks>
		public int CallTipPosStart
		{
			get
			{
				return (int)FastPerform(2203, 0, 0);
			}
		}

		/// <summary>
		/// Create a new document object.
		/// Starts with reference count of 1 and not selected into editor.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN10</remarks>
		public int CreateDocument
		{
			get
			{
				return (int)FastPerform(2375, 0, 0);
			}
		}

		/// <summary>
		/// Get currently selected item position in the auto-completion list
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN10</remarks>
		public int AutoCGetCurrent
		{
			get
			{
				return (int)FastPerform(2445, 0, 0);
			}
		}

		/// <summary>
		/// Width of the the auto-completion list
		/// </summary>
		public int AutoCMaximumWidth
		{
			get
			{
				return (int)FastPerform(2209, 0, 0);
			}
			
			set
			{
				FastPerform(2208, (uint)value, 0);
			}
		}

		/// <summary>
		/// Height of the the auto-completion list
		/// </summary>
		public int AutoCMaximumHeight
		{
			get
			{
				return (int)FastPerform(2211, 0, 0);
			}

			set
			{
				FastPerform(2210, (uint)value, 0);
			}
		}

		/// <summary>
		/// Spacing above a line
		/// </summary>
		public int ExtraAscent
		{
			get
			{
				return (int)FastPerform(2526, 0, 0);
			}

			set
			{
				FastPerform(2525, (uint)value, 0);
			}
		}

		/// <summary>
		/// Spacing below a line
		/// </summary>
		public int ExtraDescent
		{
			get
			{
				return (int)FastPerform(2528, 0, 0);
			}

			set
			{
				FastPerform(2527, (uint)value, 0);
			}
		}

		/// <summary>
		/// Returns the number of characters in the document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int Length
		{
			get
			{
				return (int)FastPerform(2006, 0, 0);
			}
		}

		/// <summary>
		/// Returns the character byte at the position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN18</remarks>
		public int CharAt(int pos)
		{
			return (int)FastPerform(2007, (uint)pos, 0);
		}

		/// <summary>
		/// Returns the position of the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int CurrentPos
		{
			get
			{
				return (int)FastPerform(2008, 0, 0);
			}
			set
			{
				FastPerform(2141, (uint)value, 0);
			}
		}

		/// <summary>
		/// Returns the position of the opposite end of the selection to the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int AnchorPosition
		{
			get
			{
				return (int)FastPerform(2009, 0, 0);
			}
			set
			{
				FastPerform(2026, (uint)value, 0);
			}
		}

		/// <summary>
		/// Returns the style byte at the position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN18</remarks>
		public int StyleAt(int pos)
		{
			return (int)FastPerform(2010, (uint)pos, 0);
		}

		/// <summary>
		/// Is undo history being collected?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsUndoCollection
		{
			get
			{
				return FastPerform(2019, 0, 0) != 0;
			}
			set
			{
				FastPerform(2012, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Are white space characters currently visible?
		/// Returns one of SCWS_* constants.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int ViewWS
		{
			get
			{
				return (int)FastPerform(2020, 0, 0);
			}
			set
			{
				FastPerform(2021, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the position of the last correctly styled character.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int EndStyled
		{
			get
			{
				return (int)FastPerform(2028, 0, 0);
			}
		}

		/// <summary>
		/// Retrieve the current end of line mode - one of CRLF, CR, or LF.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int EOLMode
		{
			get
			{
				return (int)FastPerform(2030, 0, 0);
			}
			set
			{
				FastPerform(2031, (uint)value, 0);
			}
		}

		/// <summary>
		/// Is drawing done first into a buffer or direct to the screen?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsBufferedDraw
		{
			get
			{
				return FastPerform(2034, 0, 0) != 0;
			}
			set
			{
				FastPerform(2035, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve the visible size of a tab.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int TabWidth
		{
			get
			{
				return (int)FastPerform(2121, 0, 0);
			}
			set
			{
				FastPerform(2036, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the type of a margin.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN29</remarks>
		public int GetMarginTypeN(int margin)
		{
			return (int)FastPerform(2241, (uint)margin, 0);
		}

		/// <summary>
		/// Set a margin to be either numeric or symbolic.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN30</remarks>
		public void SetMarginTypeN(int margin, int marginType)
		{
			FastPerform(2240, (uint)margin, (uint)marginType);
		}

		/// <summary>
		/// Retrieve the width of a margin in pixels.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN29</remarks>
		public int GetMarginWidthN(int margin)
		{
			return (int)FastPerform(2243, (uint)margin, 0);
		}

		/// <summary>
		/// Set the width of a margin to a width expressed in pixels.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN30</remarks>
		public void SetMarginWidthN(int margin, int pixelWidth)
		{
			FastPerform(2242, (uint)margin, (uint)pixelWidth);
		}

		/// <summary>
		/// Retrieve the marker mask of a margin.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN29</remarks>
		public int GetMarginMaskN(int margin)
		{
			return (int)FastPerform(2245, (uint)margin, 0);
		}

		/// <summary>
		/// Set a mask that determines which markers are displayed in a margin.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN30</remarks>
		public void SetMarginMaskN(int margin, int mask)
		{
			FastPerform(2244, (uint)margin, (uint)mask);
		}

		/// <summary>
		/// Retrieve the mouse click sensitivity of a margin.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN31</remarks>
		public bool MarginSensitiveN(int margin)
		{
			return FastPerform(2247, (uint)margin, 0) != 0;
		}

		/// <summary>
		/// Make a margin sensitive or insensitive to mouse clicks.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN32</remarks>
		public void MarginSensitiveN(int margin, bool sensitive)
		{
			FastPerform(2246, (uint)margin, (uint)(sensitive ? 1 : 0));
		}

		/// <summary>
		/// Get the time in milliseconds that the caret is on and off.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int CaretPeriod
		{
			get
			{
				return (int)FastPerform(2075, 0, 0);
			}
			set
			{
				FastPerform(2076, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the style of an indicator.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN29</remarks>
		public int GetIndicStyle(int indic)
		{
			return (int)FastPerform(2081, (uint)indic, 0);
		}

		/// <summary>
		/// Set an indicator to plain, squiggle or TT.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN30</remarks>
		public void SetIndicStyle(int indic, int style)
		{
			FastPerform(2080, (uint)indic, (uint)style);
		}

		/// <summary>
		/// Retrieve the foreground colour of an indicator.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN29</remarks>
		public int GetIndicFore(int indic)
		{
			return (int)FastPerform(2083, (uint)indic, 0);
		}

		/// <summary>
		/// Set the foreground colour of an indicator.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN30</remarks>
		public void SetIndicFore(int indic, int fore)
		{
			FastPerform(2082, (uint)indic, (uint)fore);
		}

		/// <summary>
		/// Retrieve number of bits in style bytes used to hold the lexical state.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int StyleBits
		{
			get
			{
				return (int)FastPerform(2091, 0, 0);
			}
			set
			{
				FastPerform(2090, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the extra styling information for a line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN29</remarks>
		public int GetLineState(int line)
		{
			return (int)FastPerform(2093, (uint)line, 0);
		}

		/// <summary>
		/// Used to hold extra styling information for each line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN30</remarks>
		public void SetLineState(int line, int state)
		{
			FastPerform(2092, (uint)line, (uint)state);
		}

		/// <summary>
		/// Retrieve the last line number that has line state.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int MaxLineState
		{
			get
			{
				return (int)FastPerform(2094, 0, 0);
			}
		}

		/// <summary>
		/// Is the background of the line containing the caret in a different colour?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsCaretLineVisible
		{
			get
			{
				return FastPerform(2095, 0, 0) != 0;
			}
			set
			{
				FastPerform(2096, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Get the colour of the background of the line containing the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int CaretLineBack
		{
			get
			{
				return (int)FastPerform(2097, 0, 0);
			}
			set
			{
				FastPerform(2098, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the auto-completion list separator character.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int AutoCSeparator
		{
			get
			{
				return (int)FastPerform(2107, 0, 0);
			}
			set
			{
				FastPerform(2106, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve whether auto-completion cancelled by backspacing before start.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsAutoCGetCancelAtStart
		{
			get
			{
				return FastPerform(2111, 0, 0) != 0;
			}
			set
			{
				FastPerform(2110, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve whether a single item auto-completion list automatically choose the item.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsAutoCGetChooseSingle
		{
			get
			{
				return FastPerform(2114, 0, 0) != 0;
			}
			set
			{
				FastPerform(2113, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve state of ignore case flag.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsAutoCGetIgnoreCase
		{
			get
			{
				return FastPerform(2116, 0, 0) != 0;
			}
			set
			{
				FastPerform(2115, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve whether or not autocompletion is hidden automatically when nothing matches.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsAutoCGetAutoHide
		{
			get
			{
				return FastPerform(2119, 0, 0) != 0;
			}
			set
			{
				FastPerform(2118, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve whether or not autocompletion deletes any word characters
		/// after the inserted text upon completion.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsAutoCGetDropRestOfWord
		{
			get
			{
				return FastPerform(2271, 0, 0) != 0;
			}
			set
			{
				FastPerform(2270, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve the auto-completion list type-separator character.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int AutoCTypeSeparator
		{
			get
			{
				return (int)FastPerform(2285, 0, 0);
			}
			set
			{
				FastPerform(2286, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve indentation size.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int Indent
		{
			get
			{
				return (int)FastPerform(2123, 0, 0);
			}
			set
			{
				FastPerform(2122, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve whether tabs will be used in indentation.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsUseTabs
		{
			get
			{
				return FastPerform(2125, 0, 0) != 0;
			}
			set
			{
				FastPerform(2124, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve the number of columns that a line is indented.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN29</remarks>
		public int GetLineIndentation(int line)
		{
			return (int)FastPerform(2127, (uint)line, 0);
		}

		/// <summary>
		/// Change the indentation of a line to a number of columns.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN30</remarks>
		public void SetLineIndentation(int line, int indentSize)
		{
			FastPerform(2126, (uint)line, (uint)indentSize);
		}

		/// <summary>
		/// Retrieve the position before the first non indentation character on a line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN18</remarks>
		public int LineIndentPosition(int line)
		{
			return (int)FastPerform(2128, (uint)line, 0);
		}

		/// <summary>
		/// Retrieve the column number of a position, taking tab width into account.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN18</remarks>
		public int Column(int pos)
		{
			return (int)FastPerform(2129, (uint)pos, 0);
		}

		/// <summary>
		/// Is the horizontal scroll bar visible?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsHScrollBar
		{
			get
			{
				return FastPerform(2131, 0, 0) != 0;
			}
			set
			{
				FastPerform(2130, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Are the indentation guides visible?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public int IndentationGuides
		{
			get
			{
				return (int)FastPerform(2133, 0, 0);
			}
			set
			{
				FastPerform(2132, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get the highlighted indentation guide column.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int HighlightGuide
		{
			get
			{
				return (int)FastPerform(2135, 0, 0);
			}
			set
			{
				FastPerform(2134, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get the position after the last visible characters on a line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN18</remarks>
		public int LineEndPosition(int line)
		{
			return (int)FastPerform(2136, (uint)line, 0);
		}

		/// <summary>
		/// Get the code page used to interpret the bytes of the document as characters.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int CodePage
		{
			get
			{
				return (int)FastPerform(2137, 0, 0);
			}
			set
			{
				FastPerform(2037, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get the foreground colour of the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int CaretFore
		{
			get
			{
				return (int)FastPerform(2138, 0, 0);
			}
			set
			{
				FastPerform(2069, (uint)value, 0);
			}
		}

		/// <summary>
		/// In palette mode?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsUsePalette
		{
			get
			{
				return FastPerform(2139, 0, 0) != 0;
			}
			set
			{
				FastPerform(2039, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// In read-only mode?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsReadOnly
		{
			get
			{
				return FastPerform(2140, 0, 0) != 0;
			}
			set
			{
				FastPerform(2171, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Returns the position at the start of the selection.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int SelectionStart
		{
			get
			{
				return (int)FastPerform(2143, 0, 0);
			}
			set
			{
				FastPerform(2142, (uint)value, 0);
			}
		}

		/// <summary>
		/// Returns the position at the end of the selection.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int SelectionEnd
		{
			get
			{
				return (int)FastPerform(2145, 0, 0);
			}
			set
			{
				FastPerform(2144, (uint)value, 0);
			}
		}

		/// <summary>
		/// Returns the print magnification.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int PrintMagnification
		{
			get
			{
				return (int)FastPerform(2147, 0, 0);
			}
			set
			{
				FastPerform(2146, (uint)value, 0);
			}
		}

		/// <summary>
		/// Returns the print colour mode.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int PrintColourMode
		{
			get
			{
				return (int)FastPerform(2149, 0, 0);
			}
			set
			{
				FastPerform(2148, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the display line at the top of the display.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int FirstVisibleLine
		{
			get
			{
				return (int)FastPerform(2152, 0, 0);
			}
		}

		/// <summary>
		/// Returns the number of lines in the document. There is always at least one.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int LineCount
		{
			get
			{
				return (int)FastPerform(2154, 0, 0);
			}
		}

		/// <summary>
		/// Returns the size in pixels of the left margin.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int MarginLeft
		{
			get
			{
				return (int)FastPerform(2156, 0, 0);
			}
			set
			{
				FastPerform(2155, (uint)value, 0);
			}
		}

		/// <summary>
		/// Returns the size in pixels of the right margin.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int MarginRight
		{
			get
			{
				return (int)FastPerform(2158, 0, 0);
			}
			set
			{
				FastPerform(2157, (uint)value, 0);
			}
		}

		/// <summary>
		/// Is the document different from when it was last saved?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN05</remarks>
		public bool IsModify
		{
			get
			{
				return FastPerform(2159, 0, 0) != 0;
			}
		}

		/// <summary>
		/// Retrieve the number of characters in the document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int TextLength
		{
			get
			{
				return (int)FastPerform(2183, 0, 0);
			}
		}

		/// <summary>
		/// Retrieve a pointer to a function that processes messages for this Scintilla.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int DirectFunction
		{
			get
			{
				return (int)FastPerform(2184, 0, 0);
			}
		}

		/// <summary>
		/// Retrieve a pointer value to use as the first argument when calling
		/// the function returned by GetDirectFunction.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int DirectPointer
		{
			get
			{
				return (int)FastPerform(2185, 0, 0);
			}
		}

		/// <summary>
		/// Returns true if overtype mode is active otherwise false is returned.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsOvertype
		{
			get
			{
				return FastPerform(2187, 0, 0) != 0;
			}
			set
			{
				FastPerform(2186, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Returns the width of the insert mode caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int CaretWidth
		{
			get
			{
				return (int)FastPerform(2189, 0, 0);
			}
			set
			{
				FastPerform(2188, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get the position that starts the target.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int TargetStart
		{
			get
			{
				return (int)FastPerform(2191, 0, 0);
			}
			set
			{
				FastPerform(2190, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get the position that ends the target.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int TargetEnd
		{
			get
			{
				return (int)FastPerform(2193, 0, 0);
			}
			set
			{
				FastPerform(2192, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get the search flags used by SearchInTarget.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int SearchFlags
		{
			get
			{
				return (int)FastPerform(2199, 0, 0);
			}
			set
			{
				FastPerform(2198, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the fold level of a line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN29</remarks>
		public int GetFoldLevel(int line)
		{
			return (int)FastPerform(2223, (uint)line, 0);
		}

		/// <summary>
		/// Set the fold level of a line.
		/// This encodes an integer level along with flags indicating whether the
		/// line is a header and whether it is effectively white space.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN30</remarks>
		public void SetFoldLevel(int line, int level)
		{
			FastPerform(2222, (uint)line, (uint)level);
		}

		/// <summary>
		/// Find the last child line of a header line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN19</remarks>
		public int LastChild(int line, int level)
		{
			return (int)FastPerform(2224, (uint)line, (uint)level);
		}

		/// <summary>
		/// Find the last child line of a header line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN18</remarks>
		public int LastChild(int line)
		{
			return (int)FastPerform(2224, (uint)line, 0);
		}

		/// <summary>
		/// Find the parent line of a child line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN18</remarks>
		public int FoldParent(int line)
		{
			return (int)FastPerform(2225, (uint)line, 0);
		}

		/// <summary>
		/// Is a line visible?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN05</remarks>
		public bool IsLineVisible
		{
			get
			{
				return FastPerform(2228, 0, 0) != 0;
			}
		}

		/// <summary>
		/// Is a header line expanded?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN31</remarks>
		public bool FoldExpanded(int line)
		{
			return FastPerform(2230, (uint)line, 0) != 0;
		}

		/// <summary>
		/// Show the children of a header line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN32</remarks>
		public void FoldExpanded(int line, bool expanded)
		{
			FastPerform(2229, (uint)line, (uint)(expanded ? 1 : 0));
		}

		/// <summary>
		/// Does a tab pressed when caret is within indentation indent?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsTabIndents
		{
			get
			{
				return FastPerform(2261, 0, 0) != 0;
			}
			set
			{
				FastPerform(2260, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Does a backspace pressed when caret is within indentation unindent?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsBackSpaceUnIndents
		{
			get
			{
				return FastPerform(2263, 0, 0) != 0;
			}
			set
			{
				FastPerform(2262, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve the time the mouse must sit still to generate a mouse dwell event.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int MouseDwellTime
		{
			get
			{
				return (int)FastPerform(2265, 0, 0);
			}
			set
			{
				FastPerform(2264, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve whether text is word wrapped.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int WrapMode
		{
			get
			{
				return (int)FastPerform(2269, 0, 0);
			}
			set
			{
				FastPerform(2268, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrive the display mode of visual flags for wrapped lines.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int WrapVisualFlags
		{
			get
			{
				return (int)FastPerform(2461, 0, 0);
			}
			set
			{
				FastPerform(2460, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrive the location of visual flags for wrapped lines.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int WrapVisualFlagsLocation
		{
			get
			{
				return (int)FastPerform(2463, 0, 0);
			}
			set
			{
				FastPerform(2462, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrive the start indent for wrapped lines.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int WrapStartIndent
		{
			get
			{
				return (int)FastPerform(2465, 0, 0);
			}
			set
			{
				FastPerform(2464, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the degree of caching of layout information.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int LayoutCache
		{
			get
			{
				return (int)FastPerform(2273, 0, 0);
			}
			set
			{
				FastPerform(2272, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the document width assumed for scrolling.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int ScrollWidth
		{
			get
			{
				return (int)FastPerform(2275, 0, 0);
			}
			set
			{
				FastPerform(2274, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve whether the maximum scroll position has the last
		/// line at the bottom of the view.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int EndAtLastLine
		{
			get
			{
				return (int)FastPerform(2278, 0, 0);
			}
			set
			{
				FastPerform(2277, (uint)value, 0);
			}
		}

		/// <summary>
		/// Is the vertical scroll bar visible?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsVScrollBar
		{
			get
			{
				return FastPerform(2281, 0, 0) != 0;
			}
			set
			{
				FastPerform(2280, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Is drawing done in two phases with backgrounds drawn before faoregrounds?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsTwoPhaseDraw
		{
			get
			{
				return FastPerform(2283, 0, 0) != 0;
			}
			set
			{
				FastPerform(2284, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Are the end of line characters visible?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsViewEOL
		{
			get
			{
				return FastPerform(2355, 0, 0) != 0;
			}
			set
			{
				FastPerform(2356, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Retrieve a pointer to the document object.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int DocPointer
		{
			get
			{
				return (int)FastPerform(2357, 0, 0);
			}
			set
			{
				FastPerform(2358, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the column number which text should be kept within.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int EdgeColumn
		{
			get
			{
				return (int)FastPerform(2360, 0, 0);
			}
			set
			{
				FastPerform(2361, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the edge highlight mode.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int EdgeMode
		{
			get
			{
				return (int)FastPerform(2362, 0, 0);
			}
			set
			{
				FastPerform(2363, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the colour used in edge indication.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int EdgeColour
		{
			get
			{
				return (int)FastPerform(2364, 0, 0);
			}
			set
			{
				FastPerform(2365, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieves the number of lines completely visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN06</remarks>
		public int LinesOnScreen
		{
			get
			{
				return (int)FastPerform(2370, 0, 0);
			}
		}

		/// <summary>
		/// Is the selection rectangular? The alternative is the more common stream selection.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN05</remarks>
		public bool IsSelectionIsRectangle
		{
			get
			{
				return FastPerform(2372, 0, 0) != 0;
			}
		}

		/// <summary>
		/// Set the zoom level. This number of points is added to the size of all fonts.
		/// It may be positive to magnify or negative to reduce.
		/// Retrieve the zoom level.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int ZoomLevel
		{
			get
			{
				return (int)FastPerform(2374, 0, 0);
			}
			set
			{
				FastPerform(2373, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get which document modification events are sent to the container.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int ModEventMask
		{
			get
			{
				return (int)FastPerform(2378, 0, 0);
			}
			set
			{
				FastPerform(2359, (uint)value, 0);
			}
		}

		/// <summary>
		/// Change internal focus flag.
		/// Get internal focus flag.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsFocus
		{
			get
			{
				return FastPerform(2381, 0, 0) != 0;
			}
			set
			{
				FastPerform(2380, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Change error status - 0 = OK.
		/// Get error status.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int Status
		{
			get
			{
				return (int)FastPerform(2383, 0, 0);
			}
			set
			{
				FastPerform(2382, (uint)value, 0);
			}
		}

		/// <summary>
		/// Set whether the mouse is captured when its button is pressed.
		/// Get whether mouse gets captured.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN07</remarks>
		public bool IsMouseDownCaptures
		{
			get
			{
				return FastPerform(2385, 0, 0) != 0;
			}
			set
			{
				FastPerform(2384, (uint)(value ? 1 : 0), 0);
			}
		}

		/// <summary>
		/// Sets the cursor to one of the SC_CURSOR* values.
		/// Get cursor type.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int CursorType
		{
			get
			{
				return (int)FastPerform(2387, 0, 0);
			}
			set
			{
				FastPerform(2386, (uint)value, 0);
			}
		}

		/// <summary>
		/// Change the way control characters are displayed:
		/// If symbol is less than 32, keep the drawn way, else, use the given character.
		/// Get the way control characters are displayed.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int ControlCharSymbol
		{
			get
			{
				return (int)FastPerform(2389, 0, 0);
			}
			set
			{
				FastPerform(2388, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get and Set the xOffset (ie, horizonal scroll position).
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int XOffset
		{
			get
			{
				return (int)FastPerform(2398, 0, 0);
			}
			set
			{
				FastPerform(2397, (uint)value, 0);
			}
		}

		/// <summary>
		/// Is printing line wrapped?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int PrintWrapMode
		{
			get
			{
				return (int)FastPerform(2407, 0, 0);
			}
			set
			{
				FastPerform(2406, (uint)value, 0);
			}
		}

		/// <summary>
		/// Get the mode of the current selection.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int SelectionMode
		{
			get
			{
				return (int)FastPerform(2423, 0, 0);
			}
			set
			{
				FastPerform(2422, (uint)value, 0);
			}
		}

		/// <summary>
		/// Retrieve the lexing language of the document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN08</remarks>
		public int Lexer
		{
			get
			{
				return (int)FastPerform(4002, 0, 0);
			}
			set
			{
				FastPerform(4001, (uint)value, 0);
			}
		}

		/// <summary>
		/// Clear all the styles and make equivalent to the global default style.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN20</remarks>
		public void StyleClearAll()
		{
			FastPerform(2050, 0, 0);
		}

		/// <summary>
		/// Set the foreground colour of a style.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN24</remarks>
		public void StyleSetFore(int style, int fore)
		{
			FastPerform(2051, (uint)style, (uint)fore);
		}

		/// <summary>
		/// Set the background colour of a style.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN24</remarks>
		public void StyleSetBack(int style, int back)
		{
			FastPerform(2052, (uint)style, (uint)back);
		}

		/// <summary>
		/// Set a style to be bold or not.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN25</remarks>
		public void StyleSetBold(int style, bool bold)
		{
			FastPerform(2053, (uint)style, (uint)(bold ? 1 : 0));
		}

		/// <summary>
		/// Set a style to be italic or not.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN25</remarks>
		public void StyleSetItalic(int style, bool italic)
		{
			FastPerform(2054, (uint)style, (uint)(italic ? 1 : 0));
		}

		/// <summary>
		/// Set the size of characters of a style.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN24</remarks>
		public void StyleSetSize(int style, int sizePoints)
		{
			FastPerform(2055, (uint)style, (uint)sizePoints);
		}

		/// <summary>
		/// Set the font of a style.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN27</remarks>
		unsafe public void StyleSetFont(int style, string fontName)
		{
			if(fontName == null || fontName.Equals(""))
				fontName = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(fontName))
				FastPerform(2056, (uint)style, (uint)b);
		}


		/// <summary>
		/// Set a style to have its end of line filled or not.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN25</remarks>
		public void StyleSetEOLFilled(int style, bool filled)
		{
			FastPerform(2057, (uint)style, (uint)(filled ? 1 : 0));
		}

		/// <summary>
		/// Set a style to be underlined or not.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN25</remarks>
		public void StyleSetUnderline(int style, bool underline)
		{
			FastPerform(2059, (uint)style, (uint)(underline ? 1 : 0));
		}

		/// <summary>
		/// Set a style to be mixed case, or to force upper or lower case.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN24</remarks>
		public void StyleSetCase(int style, int caseForce)
		{
			FastPerform(2060, (uint)style, (uint)caseForce);
		}

		/// <summary>
		/// Set the character set of the font in a style.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN24</remarks>
		public void StyleSetCharacterSet(int style, int characterSet)
		{
			FastPerform(2066, (uint)style, (uint)characterSet);
		}

		/// <summary>
		/// Set a style to be a hotspot or not.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN25</remarks>
		public void StyleSetHotSpot(int style, bool hotspot)
		{
			FastPerform(2409, (uint)style, (uint)(hotspot ? 1 : 0));
		}

		/// <summary>
		/// Set a style to be visible or not.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN25</remarks>
		public void StyleSetVisible(int style, bool visible)
		{
			FastPerform(2074, (uint)style, (uint)(visible ? 1 : 0));
		}

		/// <summary>
		/// Set the set of characters making up words for when moving or selecting by word.
		/// First sets deaults like SetCharsDefault.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN28</remarks>
		unsafe public void WordChars(string characters)
		{
			if(characters == null || characters.Equals(""))
				characters = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(characters))
				FastPerform(2077, 0, (uint)b);
		}


		/// <summary>
		/// Set a style to be changeable or not (read only).
		/// Experimental feature, currently buggy.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN25</remarks>
		public void StyleSetChangeable(int style, bool changeable)
		{
			FastPerform(2099, (uint)style, (uint)(changeable ? 1 : 0));
		}

		/// <summary>
		/// Define a set of characters that when typed will cause the autocompletion to
		/// choose the selected item.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN28</remarks>
		unsafe public void AutoCSetFillUps(string characterSet)
		{
			if(characterSet == null || characterSet.Equals(""))
				characterSet = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(characterSet))
				FastPerform(2112, 0, (uint)b);
		}


		/// <summary>
		/// Set a fore colour for active hotspots.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN26</remarks>
		public void HotspotActiveFore(bool useSetting, int fore)
		{
			FastPerform(2410, (uint)(useSetting ? 1 : 0), (uint)fore);
		}

		/// <summary>
		/// Set a back colour for active hotspots.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN26</remarks>
		public void HotspotActiveBack(bool useSetting, int back)
		{
			FastPerform(2411, (uint)(useSetting ? 1 : 0), (uint)back);
		}

		/// <summary>
		/// Set the set of characters making up whitespace for when moving or selecting by word.
		/// Should be called after SetWordChars.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN28</remarks>
		unsafe public void WhitespaceChars(string characters)
		{
			if(characters == null || characters.Equals(""))
				characters = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(characters))
				FastPerform(2443, 0, (uint)b);
		}


		/// <summary>
		/// Set up a value that may be used by a lexer for some optional feature.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN27</remarks>
		unsafe public void Property(string key, string value)
		{
			if(key == null || key.Equals(""))
				key = "\0\0";
			if(value == null || value.Equals(""))
				value = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(value))
			fixed(byte* b2 = System.Text.UTF8Encoding.UTF8.GetBytes(key))
				FastPerform(4004, (uint)b2, (uint)b);
		}


		/// <summary>
		/// Set up the key words used by the lexer.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN27</remarks>
		unsafe public void KeyWords(int keywordSet, string keyWords)
		{
			if(keyWords == null || keyWords.Equals(""))
				keyWords = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(keyWords))
				FastPerform(4005, (uint)keywordSet, (uint)b);
		}


		/// <summary>
		/// Set the lexing language of the document based on string name.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN28</remarks>
		unsafe public void LexerLanguage(string language)
		{
			if(language == null || language.Equals(""))
				language = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(language))
				FastPerform(4006, 0, (uint)b);
		}
		
		#endregion

		#region ================== Contructor / Disposer

		// Constructor
		public ScintillaControl()
		{
			this.BackColor = SystemColors.Window;
			
			// Setup collections
			ignoredkeys = new Dictionary<int, int>();

			if(!this.DesignMode) Initialize();
		}

		// Initializer
		public void Initialize()
		{
			// Initialize control
			libraryptr = General.LoadLibrary(SCINTILLA_FILENAME);
			controlptr = General.CreateWindowEx(0, SCINTILLA_CLASSNAME, "", WS_CHILD | WS_VISIBLE | WS_TABSTOP, 0, 0,
												this.Width, this.Height, this.Handle, 0, new IntPtr(0), null);
			
			// Get a direct pointer
			directptr = (int)SlowPerform(2185, 0, 0);

			// Don't know why this is done here again
			directptr = (int)FastPerform(2185, 0, 0);
		}
		
		// Disposer
		protected override void Dispose(bool disposing)
		{
			// Disposing?
			if(!base.IsDisposed)
			{
				if(!this.DesignMode)
				{
					// Dispose managed resources
					if(disposing)
					{
						
						
					}
					
					// Clean up
					// Why does this crash?
					//ClearRegisteredImages();
					
					// Dispose unmanaged elements
					if(controlptr != IntPtr.Zero) General.DestroyWindow(controlptr);
					if(libraryptr != IntPtr.Zero) General.FreeLibrary(libraryptr);
				}
			}
			
			base.Dispose(disposing);
		}
		
		#endregion

		#region ================== Events
		
		// When resized
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			// Resize control
			//General.SetWindowPos(controlptr, 0, base.Location.X, base.Location.Y, base.Width, base.Height, 0);
			General.SetWindowPos(controlptr, 0, 0, 0, base.Width, base.Height, 0);
		}
		
		// When a windows message is pre-processed
		public override bool PreProcessMessage(ref Message m)
		{
			switch(m.Msg)
			{
				case WM_KEYUP:
				case WM_KEYDOWN:
					// Why do I have to call this for my events work properly?
					// I should be able to call base.PreProcessMessage, but that doesn't raise my events!
					return base.ProcessKeyEventArgs(ref m);
			}
			return false;
		}
		
		#endregion
		
		#region ================== Methods

		// Perform a command (using SendMessage)
		protected UInt32 SlowPerform(UInt32 message, UInt32 wParam, UInt32 lParam)
		{
			if(controlptr != IntPtr.Zero)
			{
				return (UInt32)General.SendMessage(controlptr, message, (int)wParam, (int)lParam);
			}
			else
			{
				return 0;
			}
		}

		// Immediately perform a command (send directly to control)
		protected UInt32 FastPerform(UInt32 message, UInt32 wParam, UInt32 lParam)
		{
			if(controlptr != IntPtr.Zero)
			{
				return (UInt32)Perform(directptr, message, wParam, lParam);
			}
			else
			{
				return 0;
			}
		}

		private void AddIgnoredKey(Shortcut shortcutkey)
		{
			int key = (int)shortcutkey;
			this.ignoredkeys.Add(key, key);
		}

		public void AddIgnoredKey(Keys key, Keys modifier)
		{
			this.ignoredkeys.Add((int)key + (int)modifier, (int)key + (int)modifier);
		}

		private void AddShortcuts(Menu m)
		{
			foreach(MenuItem mi in m.MenuItems)
			{
				if(mi.Shortcut != Shortcut.None)
					AddIgnoredKey(mi.Shortcut);
				if(mi.MenuItems.Count > 0)
					AddShortcuts(mi);
			}
		}

		protected void AddShortcutsFromForm(Form parentForm)
		{
			if((parentForm != null) && (parentForm.Menu != null))
			{
				AddShortcuts(parentForm.Menu);
			}
		}

		/// <summary>
		/// Convert all line endings in the document to one mode.
		/// </summary>
		public void ConvertEOLs(ScriptEndOfLine eolMode)
		{
			ConvertEOLs((int)eolMode);
		}

		/// <summary>
		/// Set the symbol used for a particular marker number.
		/// </summary>
		public void MarkerDefine(int markerNumber, ScriptMarkerSymbol markerSymbol)
		{
			MarkerDefine(markerNumber, (int)markerSymbol);
		}

		/// <summary>
		/// Set the character set of the font in a style.
		/// </summary>
		public void StyleSetCharacterSet(int style, ScriptCharacterSet characterSet)
		{
			StyleSetCharacterSet(style, (int)characterSet);
		}

		/// <summary>
		/// Set a style to be mixed case, or to force upper or lower case.
		/// </summary>
		public void StyleSetCase(int style, ScriptCaseVisible caseForce)
		{
			StyleSetCase(style, (int)caseForce);
		}
		
		#endregion

		#region ================== Message Pump
		
		// This handles messages
		protected override void WndProc(ref Message m)
		{
			// Notify message?
			if(m.Msg == WM_NOTIFY)
			{
				SCNotification scn = (SCNotification)Marshal.PtrToStructure(m.LParam, typeof(SCNotification));

				if(scn.nmhdr.hwndFrom == controlptr)
				{
					switch(scn.nmhdr.code)
					{
						#region "scintilla-event-dispatch"

						case (uint)ScintillaEvents.StyleNeeded:
							if(StyleNeeded != null)
								StyleNeeded(this, scn.position);
							break;

						case (uint)ScintillaEvents.CharAdded:
							if(CharAdded != null)
								CharAdded(this, scn.ch);
							break;

						case (uint)ScintillaEvents.SavePointReached:
							if(SavePointReached != null)
								SavePointReached(this);
							break;

						case (uint)ScintillaEvents.SavePointLeft:
							if(SavePointLeft != null)
								SavePointLeft(this);
							break;

						case (uint)ScintillaEvents.ModifyAttemptRO:
							if(ModifyAttemptRO != null)
								ModifyAttemptRO(this);
							break;

						case (uint)ScintillaEvents.Key:
							if(Key != null)
								Key(this, scn.ch, scn.modifiers);
							break;

						case (uint)ScintillaEvents.DoubleClick:
							if(DoubleClick != null)
								DoubleClick(this);
							break;

						case (uint)ScintillaEvents.UpdateUI:
							if(UpdateUI != null)
								UpdateUI(this);
							break;

						case (uint)ScintillaEvents.MacroRecord:
							if(MacroRecord != null)
								MacroRecord(this, scn.message, scn.wParam, scn.lParam);
							break;

						case (uint)ScintillaEvents.MarginClick:
							if(MarginClick != null)
								MarginClick(this, scn.modifiers, scn.position, scn.margin);
							break;

						case (uint)ScintillaEvents.NeedShown:
							if(NeedShown != null)
								NeedShown(this, scn.position, scn.length);
							break;

						case (uint)ScintillaEvents.Painted:
							if(Painted != null)
								Painted(this);
							break;

						case (uint)ScintillaEvents.UserlistSelection:
							//if(UserListSelection != null)
							//	UserListSelection(this, scn.listType, System.Runtime.InteropServices.Marshal.PtrToStringAuto(scn.text));
							break;

						case (uint)ScintillaEvents.UriDropped:
							//if(URIDropped != null)
							//	URIDropped(this, System.Runtime.InteropServices.Marshal.PtrToStringAuto(scn.text));
							break;

						case (uint)ScintillaEvents.DwellStart:
							if(DwellStart != null)
								DwellStart(this, scn.position);
							break;

						case (uint)ScintillaEvents.DwellEnd:
							if(DwellEnd != null)
								DwellEnd(this, scn.position);
							break;

						case (uint)ScintillaEvents.Zoom:
							if(Zoom != null)
								Zoom(this);
							break;

						case (uint)ScintillaEvents.HotspotClick:
							if(HotSpotClick != null)
								HotSpotClick(this, scn.modifiers, scn.position);
							break;

						case (uint)ScintillaEvents.HotspotDoubleClick:
							if(HotSpotDoubleClick != null)
								HotSpotDoubleClick(this, scn.modifiers, scn.position);
							break;

						case (uint)ScintillaEvents.CallTipClick:
							if(CallTipClick != null)
								CallTipClick(this, scn.position);
							break;
						#endregion

						case (uint)ScintillaEvents.Modified:
							if((scn.modificationType & (uint)ScriptModificationFlags.InsertText) > 0)
								if(TextInserted != null)
									TextInserted(this, scn.position, scn.length, scn.linesAdded);
							if((scn.modificationType & (uint)ScriptModificationFlags.DeleteText) > 0)
								if(TextDeleted != null)
									TextDeleted(this, scn.position, scn.length, scn.linesAdded);
							if((scn.modificationType & (uint)ScriptModificationFlags.ChangeStyle) > 0)
								if(StyleChanged != null)
									StyleChanged(this, scn.position, scn.length);
							if((scn.modificationType & (uint)ScriptModificationFlags.ChangeFold) > 0)
								if(FoldChanged != null)
									FoldChanged(this, scn.line, scn.foldLevelNow, scn.foldLevelPrev);
							if((scn.modificationType & (uint)ScriptModificationFlags.User) > 0)
								if(UserPerformed != null)
									UserPerformed(this);
							if((scn.modificationType & (uint)ScriptModificationFlags.Undo) > 0)
								if(UndoPerformed != null)
									UndoPerformed(this);
							if((scn.modificationType & (uint)ScriptModificationFlags.Redo) > 0)
								if(RedoPerformed != null)
									RedoPerformed(this);
							if((scn.modificationType & (uint)ScriptModificationFlags.StepInUndoRedo) > 0)
								if(LastStepInUndoRedo != null)
									LastStepInUndoRedo(this);
							if((scn.modificationType & (uint)ScriptModificationFlags.ChangeMarker) > 0)
								if(MarkerChanged != null)
									MarkerChanged(this, scn.line);
							if((scn.modificationType & (uint)ScriptModificationFlags.BeforeInsert) > 0)
								if(BeforeInsert != null)
									BeforeInsert(this, scn.position, scn.length);
							if((scn.modificationType & (uint)ScriptModificationFlags.BeforeDelete) > 0)
								if(BeforeDelete != null)
									BeforeDelete(this, scn.position, scn.length);

								if(Modified != null)
								{
									string textstr = null;
									try
									{
										textstr = Marshal.PtrToStringAuto(scn.text);
									}
									catch(IndexOutOfRangeException)
									{
										// I don't know why this is happening, but I don't need the text here anyways
									}
									
									Modified(this, scn.position, scn.modificationType, textstr, scn.length, scn.linesAdded, scn.line, scn.foldLevelNow, scn.foldLevelPrev);
								}
							break;

					}
				}

			}
			else
				base.WndProc(ref m);
		}
		
		#endregion
		
		#region ================== Scintilla Functions

		/// <summary>
		/// Add text to the document at current position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void AddText(int length, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				FastPerform(2001, (uint)length, (uint)b);
		}


		/// <summary>
		/// Insert string at a position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void InsertText(int pos, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				FastPerform(2003, (uint)pos, (uint)b);
		}


		/// <summary>
		/// Delete all text in the document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ClearAll()
		{
			FastPerform(2004, 0, 0);
		}


		/// <summary>
		/// Set all style bytes to 0, remove all folding information.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ClearDocumentStyle()
		{
			FastPerform(2005, 0, 0);
		}


		/// <summary>
		/// Redoes the next action on the undo history.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Redo()
		{
			FastPerform(2011, 0, 0);
		}


		/// <summary>
		/// Select all the text in the document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void SelectAll()
		{
			FastPerform(2013, 0, 0);
		}


		/// <summary>
		/// Remember the current position in the undo history as the position
		/// at which the document was saved.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void SetSavePoint()
		{
			FastPerform(2014, 0, 0);
		}


		/// <summary>
		/// Retrieve the line number at which a particular marker is located.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int MarkerLineFromHandle(int handle)
		{
			return (int)FastPerform(2017, (uint)handle, 0);
		}


		/// <summary>
		/// Delete a marker.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void MarkerDeleteHandle(int handle)
		{
			FastPerform(2018, (uint)handle, 0);
		}


		/// <summary>
		/// Find the position from a point within the window.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public int PositionFromPoint(int x, int y)
		{
			return (int)FastPerform(2022, (uint)x, (uint)y);
		}


		/// <summary>
		/// Find the position from a point within the window but return
		/// INVALID_POSITION if not close to text.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public int PositionFromPointClose(int x, int y)
		{
			return (int)FastPerform(2023, (uint)x, (uint)y);
		}


		/// <summary>
		/// Set caret to start of a line and ensure it is visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void GotoLine(int line)
		{
			FastPerform(2024, (uint)line, 0);
		}


		/// <summary>
		/// Set caret to a position and ensure it is visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void GotoPos(int pos)
		{
			FastPerform(2025, (uint)pos, 0);
		}


		/// <summary>
		/// Retrieve the text of the line containing the caret.
		/// Returns the index of the caret on the line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN14</remarks>
		unsafe public string GetCurLine(int length)
		{
			int sz = (int)FastPerform(2027, (uint)length, 0);

			byte[] buffer = new byte[sz + 1];
			fixed(byte* b = buffer)
				FastPerform(2027, (uint)length + 1, (uint)b);
			return System.Text.UTF8Encoding.UTF8.GetString(buffer, 0, sz);
		}

		/// <summary>
		/// Length Method for : Retrieve the text of the line containing the caret.
		/// Returns the index of the caret on the line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN15</remarks>
		public int CurLineSize
		{
			get
			{
				return (int)FastPerform(2027, 0, 0);
			}
		}

		/// <summary>
		/// Convert all line endings in the document to one mode.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void ConvertEOLs(int eolMode)
		{
			FastPerform(2029, (uint)eolMode, 0);
		}


		/// <summary>
		/// Set the current styling position to pos and the styling mask to mask.
		/// The styling mask can be used to protect some bits in each styling byte from modification.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void StartStyling(int pos, int mask)
		{
			FastPerform(2032, (uint)pos, (uint)mask);
		}


		/// <summary>
		/// Change style from current styling position for length characters to a style
		/// and move the current styling position to after this newly styled segment.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void SetStyling(int length, int style)
		{
			FastPerform(2033, (uint)length, (uint)style);
		}


		/// <summary>
		/// Set the symbol used for a particular marker number.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void MarkerDefine(int markerNumber, int markerSymbol)
		{
			FastPerform(2040, (uint)markerNumber, (uint)markerSymbol);
		}


		/// <summary>
		/// Set the foreground colour used for a particular marker number.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void MarkerSetFore(int markerNumber, int fore)
		{
			FastPerform(2041, (uint)markerNumber, (uint)fore);
		}


		/// <summary>
		/// Set the background colour used for a particular marker number.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void MarkerSetBack(int markerNumber, int back)
		{
			FastPerform(2042, (uint)markerNumber, (uint)back);
		}


		/// <summary>
		/// Add a marker to a line, returning an ID which can be used to find or delete the marker.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public int MarkerAdd(int line, int markerNumber)
		{
			return (int)FastPerform(2043, (uint)line, (uint)markerNumber);
		}


		/// <summary>
		/// Delete a marker from a line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void MarkerDelete(int line, int markerNumber)
		{
			FastPerform(2044, (uint)line, (uint)markerNumber);
		}


		/// <summary>
		/// Delete all markers with a particular number from all lines.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void MarkerDeleteAll(int markerNumber)
		{
			FastPerform(2045, (uint)markerNumber, 0);
		}


		/// <summary>
		/// Get a bit mask of all the markers set on a line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int MarkerGet(int line)
		{
			return (int)FastPerform(2046, (uint)line, 0);
		}


		/// <summary>
		/// Find the next line after lineStart that includes a marker in mask.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public int MarkerNext(int lineStart, int markerMask)
		{
			return (int)FastPerform(2047, (uint)lineStart, (uint)markerMask);
		}


		/// <summary>
		/// Find the previous line before lineStart that includes a marker in mask.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public int MarkerPrevious(int lineStart, int markerMask)
		{
			return (int)FastPerform(2048, (uint)lineStart, (uint)markerMask);
		}


		/// <summary>
		/// Define a marker from a pixmap.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void MarkerDefinePixmap(int markerNumber, string pixmap)
		{
			if(pixmap == null || pixmap.Equals(""))
				pixmap = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(pixmap))
				FastPerform(2049, (uint)markerNumber, (uint)b);
		}


		/// <summary>
		/// Reset the default style to its state at startup
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void StyleResetDefault()
		{
			FastPerform(2058, 0, 0);
		}


		/// <summary>
		/// Set the foreground colour of the selection and whether to use this setting.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN21</remarks>
		public void SetSelFore(bool useSetting, int fore)
		{
			FastPerform(2067, (uint)(useSetting ? 1 : 0), (uint)fore);
		}


		/// <summary>
		/// Set the background colour of the selection and whether to use this setting.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN21</remarks>
		public void SetSelBack(bool useSetting, int back)
		{
			FastPerform(2068, (uint)(useSetting ? 1 : 0), (uint)back);
		}


		/// <summary>
		/// When key+modifier combination km is pressed perform msg.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void AssignCmdKey(int km, int msg)
		{
			FastPerform(2070, (uint)km, (uint)msg);
		}


		/// <summary>
		/// When key+modifier combination km is pressed do nothing.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void ClearCmdKey(int km)
		{
			FastPerform(2071, (uint)km, 0);
		}


		/// <summary>
		/// Drop all key mappings.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ClearAllCmdKeys()
		{
			FastPerform(2072, 0, 0);
		}


		/// <summary>
		/// Set the styles for a segment of the document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void SetStylingEx(int length, string styles)
		{
			if(styles == null || styles.Equals(""))
				styles = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(styles))
				FastPerform(2073, (uint)length, (uint)b);
		}


		/// <summary>
		/// Start a sequence of actions that is undone and redone as a unit.
		/// May be nested.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void BeginUndoAction()
		{
			FastPerform(2078, 0, 0);
		}


		/// <summary>
		/// End a sequence of actions that is undone and redone as a unit.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void EndUndoAction()
		{
			FastPerform(2079, 0, 0);
		}


		/// <summary>
		/// Set the foreground colour of all whitespace and whether to use this setting.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN21</remarks>
		public void SetWhitespaceFore(bool useSetting, int fore)
		{
			FastPerform(2084, (uint)(useSetting ? 1 : 0), (uint)fore);
		}


		/// <summary>
		/// Set the background colour of all whitespace and whether to use this setting.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN21</remarks>
		public void SetWhitespaceBack(bool useSetting, int back)
		{
			FastPerform(2085, (uint)(useSetting ? 1 : 0), (uint)back);
		}


		/// <summary>
		/// Display a auto-completion list.
		/// The lenEntered parameter indicates how many characters before
		/// the caret should be used to provide context.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void AutoCShow(int lenEntered, string itemList)
		{
			if(itemList == null || itemList.Equals(""))
				itemList = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(itemList))
				FastPerform(2100, (uint)lenEntered, (uint)b);
		}


		/// <summary>
		/// Remove the auto-completion list from the screen.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void AutoCCancel()
		{
			FastPerform(2101, 0, 0);
		}


		/// <summary>
		/// User has selected an item so remove the list and insert the selection.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void AutoCComplete()
		{
			FastPerform(2104, 0, 0);
		}


		/// <summary>
		/// Define a set of character that when typed cancel the auto-completion list.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN04</remarks>
		unsafe public void AutoCStops(string characterSet)
		{
			if(characterSet == null || characterSet.Equals(""))
				characterSet = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(characterSet))
				FastPerform(2105, 0, (uint)b);
		}


		/// <summary>
		/// Select the item in the auto-completion list that starts with a string.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN04</remarks>
		unsafe public void AutoCSelect(string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				FastPerform(2108, 0, (uint)b);
		}


		/// <summary>
		/// Display a list of strings and send notification when user chooses one.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void UserListShow(int listType, string itemList)
		{
			if(itemList == null || itemList.Equals(""))
				itemList = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(itemList))
				FastPerform(2117, (uint)listType, (uint)b);
		}


		/// <summary>
		/// Register an XPM image for use in autocompletion lists.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void RegisterImage(int type, string xpmData)
		{
			if(xpmData == null || xpmData.Equals(""))
				xpmData = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(xpmData))
				FastPerform(2405, (uint)type, (uint)b);
		}


		/// <summary>
		/// Clear all the registered XPM images.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ClearRegisteredImages()
		{
			FastPerform(2408, 0, 0);
		}


		/// <summary>
		/// Retrieve the contents of a line.
		/// Returns the length of the line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN14</remarks>
		unsafe public string GetLine(int line)
		{
			int sz = (int)FastPerform(2153, (uint)line, 0);

			byte[] buffer = new byte[sz + 1];
			fixed(byte* b = buffer)
				FastPerform(2153, (uint)line, (uint)b);
			return System.Text.UTF8Encoding.UTF8.GetString(buffer, 0, sz);
		}

		/// <summary>
		/// Length Method for : Retrieve the contents of a line.
		/// Returns the length of the line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN15</remarks>
		public int LineSize
		{
			get
			{
				return (int)FastPerform(2153, 0, 0);
			}
		}

		/// <summary>
		/// Select a range of text.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void SetSel(int start, int end)
		{
			FastPerform(2160, (uint)start, (uint)end);
		}


		/// <summary>
		/// Retrieve the selected text.
		/// Return the length of the text.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN12</remarks>
		unsafe public string SelText
		{
			get
			{
				int sz = (int)FastPerform(2161, 0, 0);

				byte[] buffer = new byte[sz + 1];
				fixed(byte* b = buffer)
					FastPerform(2161, (UInt32)sz + 1, (uint)b);
				return System.Text.UTF8Encoding.UTF8.GetString(buffer, 0, sz);
			}
		}

		/// <summary>
		/// Length Method for : Retrieve the selected text.
		/// Return the length of the text.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN13</remarks>
		public int SelTextSize
		{
			get
			{
				return (int)FastPerform(2161, 0, 0);
			}
		}

		/// <summary>
		/// Draw the selection in normal style or with selection highlighted.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN02</remarks>
		public void HideSelection(bool normal)
		{
			FastPerform(2163, (uint)(normal ? 1 : 0), 0);
		}


		/// <summary>
		/// Retrieve the x value of the point in the window where a position is displayed.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN11</remarks>
		public int PointXFromPosition(int pos)
		{
			return (int)FastPerform(2164, 0, (uint)pos);
		}


		/// <summary>
		/// Retrieve the y value of the point in the window where a position is displayed.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN11</remarks>
		public int PointYFromPosition(int pos)
		{
			return (int)FastPerform(2165, 0, (uint)pos);
		}


		/// <summary>
		/// Retrieve the line containing a position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int LineFromPosition(int pos)
		{
			return (int)FastPerform(2166, (uint)pos, 0);
		}


		/// <summary>
		/// Retrieve the position at the start of a line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int PositionFromLine(int line)
		{
			return (int)FastPerform(2167, (uint)line, 0);
		}


		/// <summary>
		/// Scroll horizontally and vertically.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void LineScroll(int columns, int lines)
		{
			FastPerform(2168, (uint)columns, (uint)lines);
		}


		/// <summary>
		/// Ensure the caret is visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ScrollCaret()
		{
			FastPerform(2169, 0, 0);
		}


		/// <summary>
		/// Replace the selected text with the argument text.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN04</remarks>
		unsafe public void ReplaceSel(string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				FastPerform(2170, 0, (uint)b);
		}


		/// <summary>
		/// Null operation.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Null()
		{
			FastPerform(2172, 0, 0);
		}


		/// <summary>
		/// Delete the undo history.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void EmptyUndoBuffer()
		{
			FastPerform(2175, 0, 0);
		}


		/// <summary>
		/// Undo one action in the undo history.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Undo()
		{
			FastPerform(2176, 0, 0);
		}


		/// <summary>
		/// Cut the selection to the clipboard.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Cut()
		{
			FastPerform(2177, 0, 0);
		}


		/// <summary>
		/// Copy the selection to the clipboard.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Copy()
		{
			FastPerform(2178, 0, 0);
		}


		/// <summary>
		/// Paste the contents of the clipboard into the document replacing the selection.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Paste()
		{
			FastPerform(2179, 0, 0);
		}


		/// <summary>
		/// Clear the selection.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Clear()
		{
			FastPerform(2180, 0, 0);
		}


		/// <summary>
		/// Replace the contents of the document with the argument text.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN04</remarks>
		unsafe public void SetText(byte[] text)
		{
			if(text.Length == 0)
				text = new byte[2] { 0, 0 };

			fixed(byte* b = text)
				FastPerform(2181, 0, (uint)b);
		}


		/// <summary>
		/// Retrieve all the text in the document.
		/// Returns number of characters retrieved.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN14</remarks>
		unsafe public byte[] GetText(int length)
		{
			int sz = (int)FastPerform(2182, (uint)length, 0);
			if(sz > 0)
			{
				byte[] buffer = new byte[sz + 1];
				fixed(byte* b = buffer)
					FastPerform(2182, (uint)length + 1, (uint)b);
				return buffer;
			}
			else
			{
				return new byte[0];
			}
		}

		/// <summary>
		/// Length Method for : Retrieve all the text in the document.
		/// Returns number of characters retrieved.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN15</remarks>
		public int TextSize
		{
			get
			{
				return (int)FastPerform(2182, 0, 0);
			}
		}

		/// <summary>
		/// Replace the target text with the argument text.
		/// Text is counted so it can contain NULs.
		/// Returns the length of the replacement text.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public int ReplaceTarget(int length, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				return (int)FastPerform(2194, (uint)length, (uint)b);
		}


		/// <summary>
		/// Replace the target text with the argument text after \d processing.
		/// Text is counted so it can contain NULs.
		/// Looks for \d where d is between 1 and 9 and replaces these with the strings
		/// matched in the last search operation which were surrounded by \( and \).
		/// Returns the length of the replacement text including any change
		/// caused by processing the \d patterns.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public int ReplaceTargetRE(int length, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				return (int)FastPerform(2195, (uint)length, (uint)b);
		}


		/// <summary>
		/// Search for a counted string in the target and set the target to the found
		/// range. Text is counted so it can contain NULs.
		/// Returns length of range or -1 for failure in which case target is not moved.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public int SearchInTarget(int length, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				return (int)FastPerform(2197, (uint)length, (uint)b);
		}


		/// <summary>
		/// Show a call tip containing a definition near position pos.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void CallTipShow(int pos, string definition)
		{
			if(definition == null || definition.Equals(""))
				definition = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(definition))
				FastPerform(2200, (uint)pos, (uint)b);
		}


		/// <summary>
		/// Remove the call tip from the screen.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void CallTipCancel()
		{
			FastPerform(2201, 0, 0);
		}


		/// <summary>
		/// Highlight a segment of the definition.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void CallTipSetHlt(int start, int end)
		{
			FastPerform(2204, (uint)start, (uint)end);
		}


		/// <summary>
		/// Find the display line of a document line taking hidden lines into account.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int VisibleFromDocLine(int line)
		{
			return (int)FastPerform(2220, (uint)line, 0);
		}


		/// <summary>
		/// Find the document line of a display line taking hidden lines into account.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int DocLineFromVisible(int lineDisplay)
		{
			return (int)FastPerform(2221, (uint)lineDisplay, 0);
		}


		/// <summary>
		/// Make a range of lines visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void ShowLines(int lineStart, int lineEnd)
		{
			FastPerform(2226, (uint)lineStart, (uint)lineEnd);
		}


		/// <summary>
		/// Make a range of lines invisible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void HideLines(int lineStart, int lineEnd)
		{
			FastPerform(2227, (uint)lineStart, (uint)lineEnd);
		}


		/// <summary>
		/// Switch a header line between expanded and contracted.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void ToggleFold(int line)
		{
			FastPerform(2231, (uint)line, 0);
		}


		/// <summary>
		/// Ensure a particular line is visible by expanding any header line hiding it.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void EnsureVisible(int line)
		{
			FastPerform(2232, (uint)line, 0);
		}


		/// <summary>
		/// Set some style options for folding.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void SetFoldFlags(int flags)
		{
			FastPerform(2233, (uint)flags, 0);
		}


		/// <summary>
		/// Ensure a particular line is visible by expanding any header line hiding it.
		/// Use the currently set visibility policy to determine which range to display.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void EnsureVisibleEnforcePolicy(int line)
		{
			FastPerform(2234, (uint)line, 0);
		}


		/// <summary>
		/// Get position of start of word.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN23</remarks>
		public int WordStartPosition(int pos, bool onlyWordCharacters)
		{
			return (int)FastPerform(2266, (uint)pos, (uint)(onlyWordCharacters ? 1 : 0));
		}


		/// <summary>
		/// Get position of end of word.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN23</remarks>
		public int WordEndPosition(int pos, bool onlyWordCharacters)
		{
			return (int)FastPerform(2267, (uint)pos, (uint)(onlyWordCharacters ? 1 : 0));
		}


		/// <summary>
		/// Measure the pixel width of some text in a particular style.
		/// NUL terminated text argument.
		/// Does not handle tab or control characters.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public int TextWidth(int style, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				return (int)FastPerform(2276, (uint)style, (uint)b);
		}


		/// <summary>
		/// Retrieve the height of a particular line of text in pixels.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int TextHeight(int line)
		{
			return (int)FastPerform(2279, (uint)line, 0);
		}


		/// <summary>
		/// Append a string to the end of the document without changing the selection.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void AppendText(int length, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				FastPerform(2282, (uint)length, (uint)b);
		}


		/// <summary>
		/// Make the target range start and end be the same as the selection range start and end.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void TargetFromSelection()
		{
			FastPerform(2287, 0, 0);
		}


		/// <summary>
		/// Join the lines in the target.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LinesJoin()
		{
			FastPerform(2288, 0, 0);
		}


		/// <summary>
		/// Split the lines in the target into lines that are less wide than pixelWidth
		/// where possible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void LinesSplit(int pixelWidth)
		{
			FastPerform(2289, (uint)pixelWidth, 0);
		}


		/// <summary>
		/// Set the colours used as a chequerboard pattern in the fold margin
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN21</remarks>
		public void SetFoldMarginColour(bool useSetting, int back)
		{
			FastPerform(2290, (uint)(useSetting ? 1 : 0), (uint)back);
		}


		/// <summary>
		/// Set the colours used as a chequerboard pattern in the fold margin
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN21</remarks>
		public void SetFoldMarginHiColour(bool useSetting, int fore)
		{
			FastPerform(2291, (uint)(useSetting ? 1 : 0), (uint)fore);
		}


		/// <summary>
		/// Move caret down one line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineDown()
		{
			FastPerform(2300, 0, 0);
		}


		/// <summary>
		/// Move caret down one line extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineDownExtend()
		{
			FastPerform(2301, 0, 0);
		}


		/// <summary>
		/// Move caret up one line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineUp()
		{
			FastPerform(2302, 0, 0);
		}


		/// <summary>
		/// Move caret up one line extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineUpExtend()
		{
			FastPerform(2303, 0, 0);
		}


		/// <summary>
		/// Move caret left one character.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void CharLeft()
		{
			FastPerform(2304, 0, 0);
		}


		/// <summary>
		/// Move caret left one character extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void CharLeftExtend()
		{
			FastPerform(2305, 0, 0);
		}


		/// <summary>
		/// Move caret right one character.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void CharRight()
		{
			FastPerform(2306, 0, 0);
		}


		/// <summary>
		/// Move caret right one character extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void CharRightExtend()
		{
			FastPerform(2307, 0, 0);
		}


		/// <summary>
		/// Move caret left one word.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordLeft()
		{
			FastPerform(2308, 0, 0);
		}


		/// <summary>
		/// Move caret left one word extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordLeftExtend()
		{
			FastPerform(2309, 0, 0);
		}


		/// <summary>
		/// Move caret right one word.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordRight()
		{
			FastPerform(2310, 0, 0);
		}


		/// <summary>
		/// Move caret right one word extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordRightExtend()
		{
			FastPerform(2311, 0, 0);
		}


		/// <summary>
		/// Move caret to first position on line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Home()
		{
			FastPerform(2312, 0, 0);
		}


		/// <summary>
		/// Move caret to first position on line extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void HomeExtend()
		{
			FastPerform(2313, 0, 0);
		}


		/// <summary>
		/// Move caret to last position on line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineEnd()
		{
			FastPerform(2314, 0, 0);
		}


		/// <summary>
		/// Move caret to last position on line extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineEndExtend()
		{
			FastPerform(2315, 0, 0);
		}


		/// <summary>
		/// Move caret to first position in document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DocumentStart()
		{
			FastPerform(2316, 0, 0);
		}


		/// <summary>
		/// Move caret to first position in document extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DocumentStartExtend()
		{
			FastPerform(2317, 0, 0);
		}


		/// <summary>
		/// Move caret to last position in document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DocumentEnd()
		{
			FastPerform(2318, 0, 0);
		}


		/// <summary>
		/// Move caret to last position in document extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DocumentEndExtend()
		{
			FastPerform(2319, 0, 0);
		}


		/// <summary>
		/// Move caret one page up.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void PageUp()
		{
			FastPerform(2320, 0, 0);
		}


		/// <summary>
		/// Move caret one page up extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void PageUpExtend()
		{
			FastPerform(2321, 0, 0);
		}


		/// <summary>
		/// Move caret one page down.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void PageDown()
		{
			FastPerform(2322, 0, 0);
		}


		/// <summary>
		/// Move caret one page down extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void PageDownExtend()
		{
			FastPerform(2323, 0, 0);
		}


		/// <summary>
		/// Switch from insert to overtype mode or the reverse.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void EditToggleOvertype()
		{
			FastPerform(2324, 0, 0);
		}


		/// <summary>
		/// Cancel any modes such as call tip or auto-completion list display.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Cancel()
		{
			FastPerform(2325, 0, 0);
		}


		/// <summary>
		/// Delete the selection or if no selection, the character before the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DeleteBack()
		{
			FastPerform(2326, 0, 0);
		}


		/// <summary>
		/// If selection is empty or all on one line replace the selection with a tab character.
		/// If more than one line selected, indent the lines.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void Tab()
		{
			FastPerform(2327, 0, 0);
		}


		/// <summary>
		/// Dedent the selected lines.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void BackTab()
		{
			FastPerform(2328, 0, 0);
		}


		/// <summary>
		/// Insert a new line, may use a CRLF, CR or LF depending on EOL mode.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void NewLine()
		{
			FastPerform(2329, 0, 0);
		}


		/// <summary>
		/// Insert a Form Feed character.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void FormFeed()
		{
			FastPerform(2330, 0, 0);
		}


		/// <summary>
		/// Move caret to before first visible character on line.
		/// If already there move to first character on line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void VCHome()
		{
			FastPerform(2331, 0, 0);
		}


		/// <summary>
		/// Like VCHome but extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void VCHomeExtend()
		{
			FastPerform(2332, 0, 0);
		}


		/// <summary>
		/// Magnify the displayed text by increasing the sizes by 1 point.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ZoomIn()
		{
			FastPerform(2333, 0, 0);
		}


		/// <summary>
		/// Make the displayed text smaller by decreasing the sizes by 1 point.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ZoomOut()
		{
			FastPerform(2334, 0, 0);
		}


		/// <summary>
		/// Delete the word to the left of the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DelWordLeft()
		{
			FastPerform(2335, 0, 0);
		}


		/// <summary>
		/// Delete the word to the right of the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DelWordRight()
		{
			FastPerform(2336, 0, 0);
		}


		/// <summary>
		/// Cut the line containing the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineCut()
		{
			FastPerform(2337, 0, 0);
		}


		/// <summary>
		/// Delete the line containing the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineDelete()
		{
			FastPerform(2338, 0, 0);
		}


		/// <summary>
		/// Switch the current line with the previous.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineTranspose()
		{
			FastPerform(2339, 0, 0);
		}


		/// <summary>
		/// Duplicate the current line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineDuplicate()
		{
			FastPerform(2404, 0, 0);
		}


		/// <summary>
		/// Transform the selection to lower case.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LowerCase()
		{
			FastPerform(2340, 0, 0);
		}


		/// <summary>
		/// Transform the selection to upper case.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void UpperCase()
		{
			FastPerform(2341, 0, 0);
		}


		/// <summary>
		/// Scroll the document down, keeping the caret visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineScrollDown()
		{
			FastPerform(2342, 0, 0);
		}


		/// <summary>
		/// Scroll the document up, keeping the caret visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineScrollUp()
		{
			FastPerform(2343, 0, 0);
		}


		/// <summary>
		/// Delete the selection or if no selection, the character before the caret.
		/// Will not delete the character before at the start of a line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DeleteBackNotLine()
		{
			FastPerform(2344, 0, 0);
		}


		/// <summary>
		/// Move caret to first position on display line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void HomeDisplay()
		{
			FastPerform(2345, 0, 0);
		}


		/// <summary>
		/// Move caret to first position on display line extending selection to
		/// new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void HomeDisplayExtend()
		{
			FastPerform(2346, 0, 0);
		}


		/// <summary>
		/// Move caret to last position on display line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineEndDisplay()
		{
			FastPerform(2347, 0, 0);
		}


		/// <summary>
		/// Move caret to last position on display line extending selection to new
		/// caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineEndDisplayExtend()
		{
			FastPerform(2348, 0, 0);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void HomeWrap()
		{
			FastPerform(2349, 0, 0);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void HomeWrapExtend()
		{
			FastPerform(2450, 0, 0);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineEndWrap()
		{
			FastPerform(2451, 0, 0);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineEndWrapExtend()
		{
			FastPerform(2452, 0, 0);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void VCHomeWrap()
		{
			FastPerform(2453, 0, 0);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void VCHomeWrapExtend()
		{
			FastPerform(2454, 0, 0);
		}


		/// <summary>
		/// Copy the line containing the caret.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineCopy()
		{
			FastPerform(2455, 0, 0);
		}


		/// <summary>
		/// Move the caret inside current view if it's not there already.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void MoveCaretInsideView()
		{
			FastPerform(2401, 0, 0);
		}


		/// <summary>
		/// How many characters are on a line, not including end of line characters?
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int LineLength(int line)
		{
			return (int)FastPerform(2350, (uint)line, 0);
		}


		/// <summary>
		/// Highlight the characters at two positions.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void BraceHighlight(int pos1, int pos2)
		{
			FastPerform(2351, (uint)pos1, (uint)pos2);
		}


		/// <summary>
		/// Highlight the character at a position indicating there is no matching brace.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void BraceBadLight(int pos)
		{
			FastPerform(2352, (uint)pos, 0);
		}


		/// <summary>
		/// Find the position of a matching brace or INVALID_POSITION if no match.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int BraceMatch(int pos)
		{
			return (int)FastPerform(2353, (uint)pos, 0);
		}


		/// <summary>
		/// Sets the current caret position to be the search anchor.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void SearchAnchor()
		{
			FastPerform(2366, 0, 0);
		}


		/// <summary>
		/// Find some text starting at the search anchor.
		/// Does not ensure the selection is visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public int SearchNext(int flags, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				return (int)FastPerform(2367, (uint)flags, (uint)b);
		}


		/// <summary>
		/// Find some text starting at the search anchor and moving backwards.
		/// Does not ensure the selection is visible.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public int SearchPrev(int flags, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				return (int)FastPerform(2368, (uint)flags, (uint)b);
		}


		/// <summary>
		/// Set whether a pop up menu is displayed automatically when the user presses
		/// the wrong mouse button.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN02</remarks>
		public void UsePopUp(bool allowPopUp)
		{
			FastPerform(2371, (uint)(allowPopUp ? 1 : 0), 0);
		}


		/// <summary>
		/// Create a new document object.
		/// Starts with reference count of 1 and not selected into editor.
		/// Extend life of document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN11</remarks>
		public void AddRefDocument(int doc)
		{
			FastPerform(2376, 0, (uint)doc);
		}


		/// <summary>
		/// Create a new document object.
		/// Starts with reference count of 1 and not selected into editor.
		/// Extend life of document.
		/// Release a reference to the document, deleting document if it fades to black.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN11</remarks>
		public void ReleaseDocument(int doc)
		{
			FastPerform(2377, 0, (uint)doc);
		}


		/// <summary>
		/// Move to the previous change in capitalisation.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordPartLeft()
		{
			FastPerform(2390, 0, 0);
		}


		/// <summary>
		/// Move to the previous change in capitalisation.
		/// Move to the previous change in capitalisation extending selection
		/// to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordPartLeftExtend()
		{
			FastPerform(2391, 0, 0);
		}


		/// <summary>
		/// Move to the previous change in capitalisation.
		/// Move to the previous change in capitalisation extending selection
		/// to new caret position.
		/// Move to the change next in capitalisation.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordPartRight()
		{
			FastPerform(2392, 0, 0);
		}


		/// <summary>
		/// Move to the previous change in capitalisation.
		/// Move to the previous change in capitalisation extending selection
		/// to new caret position.
		/// Move to the change next in capitalisation.
		/// Move to the next change in capitalisation extending selection
		/// to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordPartRightExtend()
		{
			FastPerform(2393, 0, 0);
		}


		/// <summary>
		/// Constants for use with SetVisiblePolicy, similar to SetCaretPolicy.
		/// Set the way the display area is determined when a particular line
		/// is to be moved to by Find, FindNext, GotoLine, etc.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void SetVisiblePolicy(int visiblePolicy, int visibleSlop)
		{
			FastPerform(2394, (uint)visiblePolicy, (uint)visibleSlop);
		}


		/// <summary>
		/// Delete back from the current position to the start of the line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DelLineLeft()
		{
			FastPerform(2395, 0, 0);
		}


		/// <summary>
		/// Delete forwards from the current position to the end of the line.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void DelLineRight()
		{
			FastPerform(2396, 0, 0);
		}


		/// <summary>
		/// Set the last x chosen value to be the caret x position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ChooseCaretX()
		{
			FastPerform(2399, 0, 0);
		}


		/// <summary>
		/// Set the focus to this Scintilla widget.
		/// GTK+ Specific.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void GrabFocus()
		{
			FastPerform(2400, 0, 0);
		}


		/// <summary>
		/// Set the way the caret is kept visible when going sideway.
		/// The exclusion zone is given in pixels.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void SetXCaretPolicy(int caretPolicy, int caretSlop)
		{
			FastPerform(2402, (uint)caretPolicy, (uint)caretSlop);
		}


		/// <summary>
		/// Set the way the line the caret is on is kept visible.
		/// The exclusion zone is given in lines.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void SetYCaretPolicy(int caretPolicy, int caretSlop)
		{
			FastPerform(2403, (uint)caretPolicy, (uint)caretSlop);
		}


		/// <summary>
		/// Move caret between paragraphs (delimited by empty lines).
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ParaDown()
		{
			FastPerform(2413, 0, 0);
		}


		/// <summary>
		/// Move caret between paragraphs (delimited by empty lines).
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ParaDownExtend()
		{
			FastPerform(2414, 0, 0);
		}


		/// <summary>
		/// Move caret between paragraphs (delimited by empty lines).
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ParaUp()
		{
			FastPerform(2415, 0, 0);
		}


		/// <summary>
		/// Move caret between paragraphs (delimited by empty lines).
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void ParaUpExtend()
		{
			FastPerform(2416, 0, 0);
		}


		/// <summary>
		/// Given a valid document position, return the previous position taking code
		/// page into account. Returns 0 if passed 0.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int PositionBefore(int pos)
		{
			return (int)FastPerform(2417, (uint)pos, 0);
		}


		/// <summary>
		/// Given a valid document position, return the next position taking code
		/// page into account. Maximum value returned is the last position in the document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int PositionAfter(int pos)
		{
			return (int)FastPerform(2418, (uint)pos, 0);
		}


		/// <summary>
		/// Copy a range of text to the clipboard. Positions are clipped into the document.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void CopyRange(int start, int end)
		{
			FastPerform(2419, (uint)start, (uint)end);
		}


		/// <summary>
		/// Copy argument text to the clipboard.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN22</remarks>
		unsafe public void CopyText(int length, string text)
		{
			if(text == null || text.Equals(""))
				text = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(text))
				FastPerform(2420, (uint)length, (uint)b);
		}


		/// <summary>
		/// Retrieve the position of the start of the selection at the given line (INVALID_POSITION if no selection on this line).
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int GetLineSelStartPosition(int line)
		{
			return (int)FastPerform(2424, (uint)line, 0);
		}


		/// <summary>
		/// Retrieve the position of the end of the selection at the given line (INVALID_POSITION if no selection on this line).
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public int GetLineSelEndPosition(int line)
		{
			return (int)FastPerform(2425, (uint)line, 0);
		}


		/// <summary>
		/// Move caret down one line, extending rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineDownRectExtend()
		{
			FastPerform(2426, 0, 0);
		}


		/// <summary>
		/// Move caret up one line, extending rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineUpRectExtend()
		{
			FastPerform(2427, 0, 0);
		}


		/// <summary>
		/// Move caret left one character, extending rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void CharLeftRectExtend()
		{
			FastPerform(2428, 0, 0);
		}


		/// <summary>
		/// Move caret right one character, extending rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void CharRightRectExtend()
		{
			FastPerform(2429, 0, 0);
		}


		/// <summary>
		/// Move caret to first position on line, extending rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void HomeRectExtend()
		{
			FastPerform(2430, 0, 0);
		}


		/// <summary>
		/// Move caret to before first visible character on line.
		/// If already there move to first character on line.
		/// In either case, extend rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void VCHomeRectExtend()
		{
			FastPerform(2431, 0, 0);
		}


		/// <summary>
		/// Move caret to last position on line, extending rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void LineEndRectExtend()
		{
			FastPerform(2432, 0, 0);
		}


		/// <summary>
		/// Move caret one page up, extending rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void PageUpRectExtend()
		{
			FastPerform(2433, 0, 0);
		}


		/// <summary>
		/// Move caret one page down, extending rectangular selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void PageDownRectExtend()
		{
			FastPerform(2434, 0, 0);
		}


		/// <summary>
		/// Move caret to top of page, or one page up if already at top of page.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void StutteredPageUp()
		{
			FastPerform(2435, 0, 0);
		}


		/// <summary>
		/// Move caret to top of page, or one page up if already at top of page, extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void StutteredPageUpExtend()
		{
			FastPerform(2436, 0, 0);
		}


		/// <summary>
		/// Move caret to bottom of page, or one page down if already at bottom of page.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void StutteredPageDown()
		{
			FastPerform(2437, 0, 0);
		}


		/// <summary>
		/// Move caret to bottom of page, or one page down if already at bottom of page, extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void StutteredPageDownExtend()
		{
			FastPerform(2438, 0, 0);
		}


		/// <summary>
		/// Move caret left one word, position cursor at end of word.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordLeftEnd()
		{
			FastPerform(2439, 0, 0);
		}


		/// <summary>
		/// Move caret left one word, position cursor at end of word, extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordLeftEndExtend()
		{
			FastPerform(2440, 0, 0);
		}


		/// <summary>
		/// Move caret right one word, position cursor at end of word.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordRightEnd()
		{
			FastPerform(2441, 0, 0);
		}


		/// <summary>
		/// Move caret right one word, position cursor at end of word, extending selection to new caret position.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void WordRightEndExtend()
		{
			FastPerform(2442, 0, 0);
		}


		/// <summary>
		/// Reset the set of characters for whitespace and word characters to the defaults.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void SetCharsDefault()
		{
			FastPerform(2444, 0, 0);
		}


		/// <summary>
		/// Enlarge the document to a particular size of text bytes.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN03</remarks>
		public void Allocate(int bytes)
		{
			FastPerform(2446, (uint)bytes, 0);
		}


		/// <summary>
		/// Start notifying the container of all key presses and commands.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void StartRecord()
		{
			FastPerform(3001, 0, 0);
		}


		/// <summary>
		/// Stop notifying the container of all key presses and commands.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN01</remarks>
		public void StopRecord()
		{
			FastPerform(3002, 0, 0);
		}


		/// <summary>
		/// Colourise a segment of the document using the current lexing language.
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN16</remarks>
		public void Colourise(int start, int end)
		{
			FastPerform(4003, (uint)start, (uint)end);
		}


		/// <summary>
		/// Load a lexer library (dll / so).
		/// 
		/// </summary>
		/// <remarks>Autogenerated: IGEN04</remarks>
		unsafe public void LoadLexerLibrary(string path)
		{
			if(path == null || path.Equals(""))
				path = "\0\0";

			fixed(byte* b = System.Text.UTF8Encoding.UTF8.GetBytes(path))
				FastPerform(4007, 0, (uint)b);
		}

		#endregion
	}
}
