
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

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal struct NotifyHeader
	{
		// hwndFrom is really an environment specifc window handle or pointer
		// but most clients of Scintilla.h do not have this type visible.
		//WindowID hwndFrom;
		public IntPtr hwndFrom;
		public uint idFrom;
		public uint code;
	};

	internal struct SCNotification
	{
		public NotifyHeader nmhdr;
		public int position;	// SCN_STYLENEEDED, SCN_MODIFIED, SCN_DWELLSTART, SCN_DWELLEND
		public int ch;		// SCN_CHARADDED, SCN_KEY
		public int modifiers;	// SCN_KEY
		public int modificationType;	// SCN_MODIFIED
		public IntPtr text;	// SCN_MODIFIED
		public int length;		// SCN_MODIFIED
		public int linesAdded;	// SCN_MODIFIED
		public int message;	// SCN_MACRORECORD
		public IntPtr wParam;	// SCN_MACRORECORD
		public IntPtr lParam;	// SCN_MACRORECORD
		public int line;		// SCN_MODIFIED
		public int foldLevelNow;	// SCN_MODIFIED
		public int foldLevelPrev;	// SCN_MODIFIED
		public int margin;		// SCN_MARGINCLICK
		public int listType;	// SCN_USERLISTSELECTION
		public int x;			// SCN_DWELLSTART, SCN_DWELLEND
		public int y;		// SCN_DWELLSTART, SCN_DWELLEND
	};
	
	internal enum ScriptWhiteSpace
	{
		Invisible = 0,
		VisibleAlways = 1,
		VisibleAfterIndent = 2
	}

	internal enum ScriptEndOfLine
	{
		CRLF = 0,
		CR = 1,
		LF = 2
	}

	internal enum ScriptMarkerSymbol
	{
		Circle = 0,
		Roundrect = 1,
		Arrow = 2,
		SmallRect = 3,
		ShortArrow = 4,
		Empty = 5,
		ArrowDown = 6,
		Minus = 7,
		Plus = 8,
		VLine = 9,
		LCorner = 10,
		TCorner = 11,
		BoxPlus = 12,
		BoxPlusConnected = 13,
		BoxMinus = 14,
		BoxMinusConnected = 15,
		LCornerCurve = 16,
		TCornerCurve = 17,
		CirclePlus = 18,
		CirclePlusConnected = 19,
		CircleMinus = 20,
		CircleMinusConnected = 21,
		Background = 22,
		DotDotDot = 23,
		Arrows = 24,
		Pixmap = 25,
		Character = 10000
	}

	internal enum ScriptMarkerOutline
	{
		FolderEnd = 25,
		FolderOpenMid = 26,
		FolderMidTail = 27,
		FolderTail = 28,
		FolderSub = 29,
		Folder = 30,
		FolderOpen = 31
	}

	internal enum ScriptMarginType
	{
		Symbol = 0,
		Number = 1
	}

	internal enum ScriptStylesCommon
	{
		Default = 32,
		LineNumber = 33,
		BraceLight = 34,
		BraceBad = 35,
		ControlChar = 36,
		IndentGuide = 37,
		LastPredefined = 39,
		Max = 127
	}

	internal enum ScriptCharacterSet
	{
		Ansi = 0,
		Default = 1,
		Baltic = 186,
		ChineseBig5 = 136,
		EastEurope = 238,
		GB2312 = 134,
		Greek = 161,
		Hangul = 129,
		Mac = 77,
		Oem = 255,
		Russian = 204,
		Shiftjis = 128,
		Symbol = 2,
		Turkish = 162,
		Johab = 130,
		Hebrew = 177,
		Arabic = 178,
		Vietnamese = 163,
		Thai = 222
	}

	internal enum ScriptCaseVisible
	{
		Mixed = 0,
		Upper = 1,
		Lower = 2
	}

	internal enum ScriptIndicatorStyle
	{
		Max = 7,
		Plain = 0,
		Squiggle = 1,
		TT = 2,
		Diagonal = 3,
		Strike = 4,
		Hidden = 5,
		Box = 6
	}

	internal enum ScriptPrintOption
	{
		Normal = 0,
		InvertLight = 1,
		BlackOnWhite = 2,
		ColourOnWhite = 3,
		ColourOnWhiteDefaultBG = 4
	}

	internal enum ScriptFindOption
	{
		WholeWord = 2,
		MatchCase = 4,
		WordStart = 0x00100000,
		RegExp = 0x00200000,
		Posix = 0x00400000
	}

	internal enum ScriptFoldLevel
	{
		Base = 0x400,
		WhiteFlag = 0x1000,
		HeaderFlag = 0x2000,
		BoxHeaderFlag = 0x4000,
		BoxFooterFlag = 0x8000,
		Contracted = 0x10000,
		Unindent = 0x20000,
		NumberMask = 0x0FFF
	}

	internal enum ScriptFoldFlag
	{
		LineBefore_Expanded = 0x0002,
		LineBefore_Contracted = 0x0004,
		LineAfter_Expanded = 0x0008,
		LineAfter_Contracted = 0x0010,
		LevelNumbers = 0x0040,
		Box = 0x0001
	}

	internal enum ScriptWrap
	{
		None = 0,
		Word = 1
	}

	internal enum ScriptWrapVisualFlag
	{
		None = 0x0000,
		End = 0x0001,
		Start = 0x0002
	}

	internal enum ScriptWrapVisualLocation
	{
		Default = 0x0000,
		EndByText = 0x0001,
		StartByText = 0x0002
	}

	internal enum ScriptLineCache
	{
		None = 0,
		Caret = 1,
		Page = 2,
		Document = 3
	}

	internal enum ScriptIdentGuides
	{
		None = 0,
		Real = 1,
		LookForward = 2,
		LookBoth = 3
	}

	internal enum ScriptEdgeVisualStyle
	{
		None = 0,
		Line = 1,
		Background = 2
	}

	internal enum ScriptCursorShape
	{
		Normal = -1,
		Wait = 4
	}

	internal enum ScriptCaretPolicy
	{
		Slop = 0x01,
		Strict = 0x04,
		Jumps = 0x10,
		Even = 0x08
	}

	internal enum ScriptSelectionMode
	{
		Stream = 0,
		Rectangle = 1,
		Lines = 2
	}

	internal enum ScriptModificationFlags
	{
		InsertText = 0x1,
		DeleteText = 0x2,
		ChangeStyle = 0x4,
		ChangeFold = 0x8,
		User = 0x10,
		Undo = 0x20,
		Redo = 0x40,
		StepInUndoRedo = 0x100,
		ChangeMarker = 0x200,
		BeforeInsert = 0x400,
		BeforeDelete = 0x800
	}

	internal enum ScriptKeys
	{
		Down = 300,
		Up = 301,
		Left = 302,
		Right = 303,
		Home = 304,
		End = 305,
		Prior = 306,
		Next = 307,
		Delete = 308,
		Insert = 309,
		Escape = 7,
		Back = 8,
		Tab = 9,
		Return = 13,
		Add = 310,
		Subtract = 311,
		Divide = 312
	}

	internal enum ScriptKeyMod
	{
		Shift = 1,
		Ctrl = 2,
		Alt = 4
	}

	internal enum ScriptLexer
	{
		Container = 0,
		Null = 1,
		Python = 2,
		Cpp = 3,
		Html = 4,
		Xml = 5,
		Perl = 6,
		Sql = 7,
		Vb = 8,
		Properties = 9,
		Errorlist = 10,
		Makefile = 11,
		Batch = 12,
		Xcode = 13,
		Latex = 14,
		Lua = 15,
		Diff = 16,
		Conf = 17,
		Pascal = 18,
		Ave = 19,
		Ada = 20,
		Lisp = 21,
		Ruby = 22,
		Eiffel = 23,
		Eiffelkw = 24,
		Tcl = 25,
		NncronTab = 26,
		Bullant = 27,
		VBScript = 28,
		Asp = 29,
		Php = 30,
		Baan = 31,
		Matlab = 32,
		Scriptol = 33,
		Asm = 34,
		CppNoCase = 35,
		Fortran = 36,
		F77 = 37,
		Css = 38,
		Pov = 39,
		Lout = 40,
		Escript = 41,
		Ps = 42,
		Nsis = 43,
		Mmixal = 44,
		Clw = 45,
		Clwnocase = 46,
		Lot = 47,
		Yaml = 48,
		Tex = 49,
		Metapost = 50,
		Powerbasic = 51,
		Forth = 52,
		Erlang = 53,
		Octave = 54,
		Mssql = 55,
		Verilog = 56,
		Kix = 57,
		Gui4cli = 58,
		Specman = 59,
		Au3 = 60,
		Apdl = 61,
		Bash = 62,
		Automatic = 1000
	}
	
	internal enum ScintillaEvents
	{
		StyleNeeded = 2000,
		CharAdded = 2001,
		SavePointReached = 2002,
		SavePointLeft = 2003,
		ModifyAttemptRO = 2004,
		Key = 2005,
		DoubleClick = 2006,
		UpdateUI = 2007,
		Modified = 2008,
		MacroRecord = 2009,
		MarginClick = 2010,
		NeedShown = 2011,
		Painted = 2013,
		UserlistSelection = 2014,
		UriDropped = 2015,
		DwellStart = 2016,
		DwellEnd = 2017,
		Zoom = 2018,
		HotspotClick = 2019,
		HotspotDoubleClick = 2020,
		CallTipClick = 2021
	}
	
	internal enum ScriptStyleType
	{
		PlainText = 0,
		Keyword = 1,
		Constant = 2,
		Comment = 3,
		Literal = 4,
		LineNumber = 5
	}
}

