#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

#if PROFILE
using JetBrains.Profiler.Core.Api;
#endif

#endregion

namespace CodeImp.DoomBuilder
{
	#region ================== Enums

	[Flags]
	public enum DebugMessageType
	{
		LOG = 1,
		INFO = 2,
		WARNING = 4,
		ERROR = 8,
		SPECIAL = 16,
	}

	#endregion

	public partial class DebugConsole : UserControl
	{
		#region ================== Variables

		private const int MAX_MESSAGES = 1024;
		private static readonly List<KeyValuePair<DebugMessageType, string>> messages = new List<KeyValuePair<DebugMessageType, string>>(MAX_MESSAGES);

		//Colors
		private readonly Dictionary<DebugMessageType, Color> textcolors;
		private readonly Dictionary<DebugMessageType, string> textheaders;

		private DebugMessageType filters;
		private static long starttime = -1;
		private static long storedtime;
		private static int counter;
		private static string storedtext = string.Empty;
		private static DebugConsole me;

		#endregion

		#region ================== Properties

		public bool AlwaysOnTop { get { return alwaysontop.Checked; } }
		public static int Counter { get { return counter; } }

		#endregion

		#region ================== Constructor

		public DebugConsole() 
		{
			InitializeComponent();
			me = this;

			// Setup filters
			foreach(ToolStripMenuItem item in filterselector.DropDownItems)
			{
				UpdateFilters(item);
			}

			// Setup colors
			textcolors = new Dictionary<DebugMessageType, Color> {
				             { DebugMessageType.LOG, SystemColors.WindowText }, 
							 { DebugMessageType.INFO, Color.DarkGreen }, 
							 { DebugMessageType.WARNING, Color.DarkOrange }, 
							 { DebugMessageType.ERROR, Color.DarkRed }, 
							 { DebugMessageType.SPECIAL, Color.DarkMagenta }
			             };

			// Setup headers
			textheaders = new Dictionary<DebugMessageType, string> {
				              { DebugMessageType.LOG, string.Empty}, 
							  { DebugMessageType.INFO, string.Empty}, 
							  { DebugMessageType.WARNING, "Warning: "}, 
							  { DebugMessageType.ERROR, "ERROR: "}, 
							  { DebugMessageType.SPECIAL, string.Empty}
			              };

			// Word wrap?
			wordwrap.Checked = console.WordWrap;

			// Pending messages?
			if(messages.Count > 0) UpdateMessages();
		}

		#endregion

		#region ================== Methods

		public static void StoreText(string text) { storedtext += text + Environment.NewLine; }
		public static void SetStoredText() { Write(DebugMessageType.INFO, storedtext, false); storedtext = string.Empty; }
		public static void SetText(string text) { Write(DebugMessageType.INFO, text, false); } // Useful to display frequently updated text without flickering
		public static void WriteLine(string text) { Write(DebugMessageType.INFO, text + Environment.NewLine, true); }
		public static void WriteLine(DebugMessageType type, string text) { Write(type, text + Environment.NewLine, true); }
		public static void Write(string text) { Write(DebugMessageType.INFO, text, true); }
		public static void Write(DebugMessageType type, string text) { Write(type, text, true); }
		public static void Write(DebugMessageType type, string text, bool append)
		{
			if(me != null && me.InvokeRequired) 
			{
				me.Invoke(new Action<DebugMessageType, string, bool>(Write), new object[] { type, text, append });
			} 
			else 
			{
				if(messages.Count + 1 > MAX_MESSAGES) lock (messages) { messages.RemoveAt(0); }
				messages.Add(new KeyValuePair<DebugMessageType, string>(type, text));
				if(me != null && (me.filters & type) == type) 
				{
					me.AddMessage(type, text, true, append);
				}
			}
		}

		public static void Clear()
		{
			if(me != null && me.InvokeRequired)
			{
				me.Invoke(new Action(Clear));
			}
			else
			{
				if(me != null) me.console.Clear();
				messages.Clear();
			}
		}

		public static void StartTimer() 
		{
			starttime = SlimDX.Configuration.Timer.ElapsedMilliseconds;
		}

		public static void PauseTimer()
		{
			if(starttime == -1) throw new InvalidOperationException("DebugConsole.StartTimer() must be called before DebugConsole.PauseTimer()!");
			
			storedtime += SlimDX.Configuration.Timer.ElapsedMilliseconds - starttime;
		}

		public static void StopTimer(string message) 
		{
			if(starttime == -1) throw new InvalidOperationException("DebugConsole.StartTimer() must be called before DebugConsole.StopTimer()!");

			long duration = SlimDX.Configuration.Timer.ElapsedMilliseconds - starttime + storedtime;
			
			if(message.Contains("%"))
				message = message.Replace("%", duration.ToString(CultureInfo.InvariantCulture));
			else
				message = message.TrimEnd() + " " + duration + " ms.";

#if DEBUG
			WriteLine(DebugMessageType.SPECIAL, message);
#else
			General.ShowErrorMessage(message, MessageBoxButtons.OK, false);
#endif

			starttime = -1;
			storedtime = 0;
		}

		public static void IncrementCounter() { IncrementCounter(1); }
		public static void IncrementCounter(int incrementby)
		{
			counter += incrementby;
		}

		public static void ResetCounter() { ResetCounter(string.Empty); }
		public static void ResetCounter(string message)
		{
			if(!string.IsNullOrEmpty(message))
			{
				if(message.Contains("%"))
					message = message.Replace("%", counter.ToString());
				else
					message = message.TrimEnd() + ": " + counter;

				WriteLine(DebugMessageType.SPECIAL, message);
			}

			counter = 0;
		}

		public static void StartProfiler()
		{
#if PROFILE
			if(PerformanceProfiler.IsActive)
			{
				WriteLine(DebugMessageType.SPECIAL, "Starting the Profiler...");
				PerformanceProfiler.Begin();
				PerformanceProfiler.Start();
			}
			else
			{
				WriteLine(DebugMessageType.SPECIAL, "Unable to start the Profiler...");
			}
#else
			WriteLine(DebugMessageType.SPECIAL, "Unable to start the Profiler: incorrect build configuration selected!");
#endif
		}

		public static void StopProfiler() { StopProfiler(true); }
		public static void StopProfiler(bool savesnapshot)
		{
#if PROFILE
			if(PerformanceProfiler.IsActive)
			{
				PerformanceProfiler.Stop();
				if(savesnapshot) PerformanceProfiler.EndSave();
				else PerformanceProfiler.EndDrop();

				WriteLine(DebugMessageType.SPECIAL, "Profiler Stopped...");
			}
			else
			{
				WriteLine(DebugMessageType.SPECIAL, "Unable to stop the Profiler...");
			}
#else
			WriteLine(DebugMessageType.SPECIAL, "Unable to stop the Profiler: incorrect build configuration selected!");
#endif
		}

		private void AddMessage(DebugMessageType type, string text, bool scroll, bool append)
		{
			text = textheaders[type] + text;
			if(append)
			{
				console.SelectionStart = console.TextLength;
				console.SelectionColor = textcolors[type];
				console.AppendText(text);
			}
			else
			{
				console.SuspendLayout();
				console.Text = text;
				console.SelectAll();
				console.SelectionColor = textcolors[type];
				console.Select(0, 0);
				console.ResumeLayout();
			}
			if(scroll && autoscroll.Checked) console.ScrollToCaret();
		}

		private void UpdateFilters(ToolStripMenuItem item)
		{
			DebugMessageType flag = (DebugMessageType)(int)item.Tag;
			if(item.Checked) 
			{
				filters |= flag;
			} 
			else 
			{
				filters &= ~flag;
			}
		}

		private void UpdateMessages()
		{
			console.Clear();

			console.SuspendLayout();
			foreach(KeyValuePair<DebugMessageType, string> pair in messages)
			{
				if((filters & pair.Key) == pair.Key && CheckTextFilter(pair.Value, searchbox.Text))
				{
					AddMessage(pair.Key, pair.Value, false, true);
				}
			}

			console.ResumeLayout();
			console.ScrollToCaret();
		}

		// Should we display this message?
		private static bool CheckTextFilter(string text, string filter) 
		{
			if(string.IsNullOrEmpty(filter) || filter.Length < 3) return true;
			return text.ToUpperInvariant().Contains(filter.ToUpperInvariant());
		}

		#endregion

		#region ================== Events

		private void clearall_Click(object sender, EventArgs e) 
		{
			Clear();
		}

		private void filters_Click(object sender, EventArgs e)
		{
			UpdateFilters(sender as ToolStripMenuItem);
			UpdateMessages();
		}

		private void wordwrap_Click(object sender, EventArgs e) 
		{
			console.WordWrap = wordwrap.Checked;
		}

		#endregion

		#region ================== Search events

		private void searchclear_Click(object sender, EventArgs e) 
		{
			searchbox.Clear();
		}

		private void searchbox_TextChanged(object sender, EventArgs e) 
		{
			if(string.IsNullOrEmpty(searchbox.Text) || searchbox.Text.Length > 2) UpdateMessages();
		}

		#endregion

	}
}
