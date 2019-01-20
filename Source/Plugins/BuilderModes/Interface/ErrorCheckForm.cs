
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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;
using System.Reflection;
using System.Globalization;
using System.Threading;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public partial class ErrorCheckForm : DelayedForm
	{
		#region ================== Constants

		#endregion

		#region ================== Delegates

		private delegate void CallVoidMethodDeletage();
		private delegate void CallIntMethodDelegate(int i);
		private delegate void CallResultMethodDelegate(ErrorResult r);
		
		#endregion
		
		#region ================== Variables
		
		private volatile bool running;
		private Thread checksthread;
		private BlockMap<BlockEntry> blockmap;
		private Size initialsize; //mxd
		private List<ErrorResult> resultslist; //mxd 
		private List<Type> hiddentresulttypes; //mxd 
		private bool bathselectioninprogress; //mxd
		
		#endregion

		#region ================== Properties
		
		public List<ErrorResult> SelectedResults { 
			get 
			{
				List<ErrorResult> selection = new List<ErrorResult>();
				foreach(Object ro in results.SelectedItems)
				{
					ErrorResult result = ro as ErrorResult;
					if(result == null) continue;
					selection.Add(result);
				}
				return selection;
			} 
		}
		public BlockMap<BlockEntry> BlockMap { get { return blockmap; } }
		
		#endregion
		
		#region ================== Constructor / Show

		// Constructor
		public ErrorCheckForm()
		{
			// Initialize
			InitializeComponent();
			
			// Find all error checkers
			Type[] checkertypes = BuilderPlug.Me.FindClasses(typeof(ErrorChecker));
			foreach(Type t in checkertypes)
			{
				object[] attr = t.GetCustomAttributes(typeof(ErrorCheckerAttribute), true);
				if(attr.Length > 0)
				{
					//mxd. Skip this check?..
					ErrorChecker checker;

					try
					{
						// Create instance
						checker = (ErrorChecker)Assembly.GetExecutingAssembly().CreateInstance(t.FullName, false, BindingFlags.Default, null, null, CultureInfo.CurrentCulture, new object[0]);
					}
					catch(TargetInvocationException ex)
					{
						// Error!
						General.ErrorLogger.Add(ErrorType.Error, "Failed to create class instance \"" + t.Name + "\"");
						General.WriteLogLine(ex.InnerException.GetType().Name + ": " + ex.InnerException.Message);
						throw;
					}
					catch(Exception ex)
					{
						// Error!
						General.ErrorLogger.Add(ErrorType.Error, "Failed to create class instance \"" + t.Name + "\"");
						General.WriteLogLine(ex.GetType().Name + ": " + ex.Message);
						throw;
					}

					if(checker.SkipCheck) continue;
					
					ErrorCheckerAttribute checkerattr = (attr[0] as ErrorCheckerAttribute);
					
					// Add the type to the checkbox list
					CheckBox c = checks.Add(checkerattr.DisplayName, t);
					c.Checked = checkerattr.DefaultChecked;
				}
			}
			checks.Sort(); //mxd

			//mxd. Store initial height
			initialsize = this.Size;
			resultslist = new List<ErrorResult>();
			hiddentresulttypes = new List<Type>();
		}
		
		// This shows the window
		public void Show(Form owner)
		{
			// Move controls according to the height of the checkers box
			checks.PerformLayout();
			buttoncheck.Top = checks.Bottom + 14;
			resultspanel.Top = buttoncheck.Bottom + 14;
			this.Text = "Map Analysis"; //mxd

			// Position at left-top of owner
			this.Location = new Point(owner.Location.X + 20, owner.Location.Y + 90);
			
			// Close results part
			resultspanel.Visible = false;
			this.Size = new Size(initialsize.Width, this.Height - this.ClientSize.Height + resultspanel.Top);
			this.MinimumSize = this.Size; //mxd
			this.MaximumSize = this.Size; //mxd
			
			// Show window
			base.Show(owner);
		}
		
		#endregion
		
		#region ================== Thread Calls

		public void SubmitResult(ErrorResult result)
		{
			if(results.InvokeRequired)
			{
				CallResultMethodDelegate d = SubmitResult;
				try { progress.Invoke(d, result); }
				catch(ThreadInterruptedException) { }
			}
			else
			{
				if(!result.IsHidden && !hiddentresulttypes.Contains(result.GetType())) //mxd
				{
					results.Items.Add(result);
				}
				resultslist.Add(result); //mxd
				UpdateTitle();
			}
		}

		private void SetProgressMaximum(int maximum)
		{
			if(progress.InvokeRequired)
			{
				CallIntMethodDelegate d = SetProgressMaximum;
				try { progress.Invoke(d, maximum); }
				catch(ThreadInterruptedException) { }
			}
			else
			{
				progress.Maximum = maximum;
			}
		}

		public void AddProgressValue(int value)
		{
			if(progress.InvokeRequired)
			{
				CallIntMethodDelegate d = AddProgressValue;
				try { progress.Invoke(d, value); }
				catch(ThreadInterruptedException) { }
			}
			else
			{
				progress.Value += value;
			}
		}

		//mxd
		private void UpdateTitle()
		{
			int hiddencount = resultslist.Count - results.Items.Count;
			string title = "Map Analysis [" + resultslist.Count + " results";
			if(hiddencount > 0) title += hiddencount + " hidden";
			title += ", " + results.SelectedItems.Count + " selected";
			this.Text = title + @"]";
		}
		
		// This stops checking (only called from the checking management thread)
		private void StopChecking()
		{
			if(this.InvokeRequired)
			{
				CallVoidMethodDeletage d = StopChecking;
				this.Invoke(d);
			}
			else
			{
				checksthread = null;
				progress.Value = 0;
				buttoncheck.Text = "Start Analysis";
				Cursor.Current = Cursors.Default;
				running = false;
				blockmap.Dispose();
				blockmap = null;
				
				// When no results found, show "no results" and disable the list
				if(resultslist.Count == 0) 
				{
					results.Items.Add(new ResultNoErrors());
					results.Enabled = false;
					UpdateTitle(); //mxd
				} 
				else 
				{ 
					ClearSelectedResult(); //mxd
				}
			}
		}
		
		// This starts checking
		private void StartChecking()
		{
			if(running) return;
			
			Cursor.Current = Cursors.WaitCursor;
			
			// Make blockmap
			RectangleF area = MapSet.CreateArea(General.Map.Map.Vertices);
			area = MapSet.IncreaseArea(area, General.Map.Map.Things);
			blockmap = new BlockMap<BlockEntry>(area);
			blockmap.AddLinedefsSet(General.Map.Map.Linedefs);
			blockmap.AddSectorsSet(General.Map.Map.Sectors);
			blockmap.AddThingsSet(General.Map.Map.Things);
			blockmap.AddVerticesSet(General.Map.Map.Vertices); //mxd
			
			//mxd. Open the results panel
			if(!resultspanel.Visible) 
			{
				this.MinimumSize = new Size();
				this.MaximumSize = new Size();
				this.Size = initialsize;
				resultspanel.Size = new Size(resultspanel.Width, this.ClientSize.Height - resultspanel.Top);
				resultspanel.Visible = true;
			}
			progress.Value = 0;
			results.Items.Clear();
			results.Enabled = true;
			resultslist = new List<ErrorResult>(); //mxd
			ClearSelectedResult();
			buttoncheck.Text = "Abort Analysis";
			General.Interface.RedrawDisplay();
			
			// Start checking
			running = true;
			checksthread = new Thread(RunChecks);
			checksthread.Name = "Error Checking Management";
			checksthread.Priority = ThreadPriority.Normal;
			checksthread.Start();
			
			Cursor.Current = Cursors.Default;
		}

		#endregion

		#region ================== Methods
		
		// This stops the checking
		public void CloseWindow()
		{
			// Currently running?
			if(running)
			{
				Cursor.Current = Cursors.WaitCursor;
				checksthread.Interrupt();
			}

			ClearSelectedResult();
			
			//mxd. Clear results 
			resultslist.Clear();
			results.Items.Clear();

			this.Hide();
		}

		// This clears the selected result
		private void ClearSelectedResult()
		{
			results.SelectedItems.Clear(); //mxd
			if(results.Items.Count == 0 && resultslist.Count > 0) //mxd
				resultinfo.Text = "All results are hidden. Use context menu to show them.";
			else
				resultinfo.Text = "Select a result from the list to see more information.\r\nHold 'Ctrl' to select several results.\r\nHold 'Shift' to select a range of results.\r\nRight-click on a result to show context menu.";
			resultinfo.Enabled = false;
			fix1.Visible = false;
			fix2.Visible = false;
			fix3.Visible = false;

			UpdateTitle(); //mxd
		}
		
		// This runs in a seperate thread to manage the checking threads
		private void RunChecks()
		{
			List<ErrorChecker> checkers = new List<ErrorChecker>();
			List<Thread> threads = new List<Thread>();
			int maxthreads = Environment.ProcessorCount;
			int totalprogress = 0;
			int nextchecker = 0;
			
			// Initiate all checkers
			foreach(CheckBox c in checks.Checkboxes)
			{
				// Include this one?
				if(c.Checked)
				{
					Type t = (c.Tag as Type);
					ErrorChecker checker;
					
					try
					{
						// Create instance
						checker = (ErrorChecker)Assembly.GetExecutingAssembly().CreateInstance(t.FullName, false, BindingFlags.Default, null, null, CultureInfo.CurrentCulture, new object[0]);
					}
					catch(TargetInvocationException ex)
					{
						// Error!
						General.ErrorLogger.Add(ErrorType.Error, "Failed to create class instance \"" + t.Name + "\"");
						General.WriteLogLine(ex.InnerException.GetType().Name + ": " + ex.InnerException.Message);
						throw;
					}
					catch(Exception ex)
					{
						// Error!
						General.ErrorLogger.Add(ErrorType.Error, "Failed to create class instance \"" + t.Name + "\"");
						General.WriteLogLine(ex.GetType().Name + ": " + ex.Message);
						throw;
					}
					
					// Add to list
					if(checker != null)
					{
						checkers.Add(checker);
						totalprogress += checker.TotalProgress;
					}
				}
			}
			
			// Sort the checkers with highest cost first
			// See CompareTo method in ErrorChecker for sorting comparison
			checkers.Sort();
			
			// Setup
			SetProgressMaximum(totalprogress);
			
			// Continue while threads are running or checks are to be done
			while((nextchecker < checkers.Count) || (threads.Count > 0))
			{
				// Start new thread when less than maximum number of
				// threads running and there is more work to be done
				while((threads.Count < maxthreads) && (nextchecker < checkers.Count))
				{
					ErrorChecker c = checkers[nextchecker++];
					Thread t = new Thread(c.Run);
					t.Name = "Error Checker '" + c.GetType().Name + "'";
					t.Priority = ThreadPriority.BelowNormal;
					t.Start();
					threads.Add(t);
				}
				
				// Remove threads that are done
				for(int i = threads.Count - 1; i >= 0; i--)
					if(!threads[i].IsAlive) threads.RemoveAt(i);
				
				// Handle thread interruption
				try { Thread.Sleep(1); }
				catch(ThreadInterruptedException) { break; }
			}
			
			// Stop all running threads
			foreach(Thread t in threads)
			{
				while(t.IsAlive)
				{
					try
					{ 
						t.Interrupt();
						t.Join(1);
					}
					catch(ThreadInterruptedException)
					{
						// We have to continue, we can't just leave the other threads running!
					}
				}
			}
			
			// Done
			StopChecking();
		}

		//mxd
		private Dictionary<Type, bool> GetSelectedTypes()
		{
			Dictionary<Type, bool> selectedtypes = new Dictionary<Type, bool>();
			foreach(var ro in results.SelectedItems)
			{
				ErrorResult r = ro as ErrorResult;
				if(r == null) continue;
				Type t = r.GetType();
				if(!selectedtypes.ContainsKey(t)) selectedtypes.Add(t, false);
			}

			return selectedtypes;
		}
		
		#endregion
		
		#region ================== Events
		
		// Window closing
		private void ErrorCheckForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			//mxd. Clear results 
			resultslist.Clear();
			results.Items.Clear();
			
			// If the user closes the form, then just cancel the mode
			if(e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				General.Interface.Focus();
				General.Editing.CancelMode();
			}
		}
		
		// Start/stop
		private void buttoncheck_Click(object sender, EventArgs e)
		{
			// Currently running?
			if(running)
			{
				Cursor.Current = Cursors.WaitCursor;
				checksthread.Interrupt();
			}
			else
			{
				StartChecking();
			}
		}

		// Close
		private void closebutton_Click(object sender, EventArgs e)
		{
			General.Interface.Focus();
			General.Editing.CancelMode();
		}
		
		// Results selection changed
		private void results_SelectedIndexChanged(object sender, EventArgs e)
		{
			//mxd
			if(bathselectioninprogress) return;
			
			// Anything selected?
			if(results.SelectedItems.Count > 0)
			{
				ErrorResult firstresult = (results.SelectedItems[0] as ErrorResult);
				if(firstresult == null)
				{
					ClearSelectedResult();
				}
				else
				{
					bool sametype = true;
					List<ErrorResult> validresults = new List<ErrorResult>();

					// Selected results have the same fixes?
					foreach(var ri in results.SelectedItems)
					{
						ErrorResult result = ri as ErrorResult;
						if(result == null) continue;
						validresults.Add(result);

						if(result.Buttons != firstresult.Buttons || result.Button1Text != firstresult.Button1Text
							|| result.Button2Text != firstresult.Button2Text || result.Button3Text != firstresult.Button3Text)
						{
							sametype = false;
							break;
						}
					}

					resultinfo.Enabled = true;

					if(sametype)
					{
						resultinfo.Text = firstresult.Description;
						fix1.Text = firstresult.Button1Text;
						fix2.Text = firstresult.Button2Text;
						fix3.Text = firstresult.Button3Text;
						fix1.Visible = (firstresult.Buttons > 0);
						fix2.Visible = (firstresult.Buttons > 1);
						fix3.Visible = (firstresult.Buttons > 2);
					}
					else
					{
						resultinfo.Text = "Several types of map analysis results are selected. To display fixes, make sure that only a single result type is selected.";
						fix1.Visible = false;
						fix2.Visible = false;
						fix3.Visible = false;
					}

					// Zoom to area
					if(validresults.Count > 0)
					{
						RectangleF zoomarea = validresults[0].GetZoomArea();
						foreach(ErrorResult result in validresults)
						{
							zoomarea = RectangleF.Union(zoomarea, result.GetZoomArea());
						}

						ClassicMode editmode = (General.Editing.Mode as ClassicMode);
						editmode.CenterOnArea(zoomarea, 0.6f);
					}
				}

				UpdateTitle(); //mxd
			}
			else
			{
				ClearSelectedResult();
			}
			
			General.Interface.RedrawDisplay();
		}
		
		// First button
		private void fix1_Click(object sender, EventArgs e)
		{
			// Anything selected?
			if(results.SelectedItems.Count > 0)
			{
				if(running)
				{
					General.ShowWarningMessage("You must stop the analysis before you can make changes to your map!", MessageBoxButtons.OK);
				}
				else
				{
					ErrorResult r = (results.SelectedItem as ErrorResult);
					if(r.Button1Click(false)) 
					{
						if(results.SelectedItems.Count > 1) FixSimilarErrors(r.GetType(), 1); //mxd
						StartChecking();
					} 
					else 
					{
						General.Interface.RedrawDisplay();
					}
				}
			}
		}
		
		// Second button
		private void fix2_Click(object sender, EventArgs e)
		{
			// Anything selected?
			if(results.SelectedIndex >= 0)
			{
				if(running)
				{
					General.ShowWarningMessage("You must stop the analysis before you can make changes to your map!", MessageBoxButtons.OK);
				}
				else
				{
					ErrorResult r = (results.SelectedItem as ErrorResult);
					if(r.Button2Click(false)) 
					{
						if(results.SelectedItems.Count > 1) FixSimilarErrors(r.GetType(), 2); //mxd
						StartChecking();
					} 
					else 
					{
						General.Interface.RedrawDisplay();
					}
				}
			}
		}
		
		// Third button
		private void fix3_Click(object sender, EventArgs e)
		{
			// Anything selected?
			if(results.SelectedIndex >= 0)
			{
				if(running)
				{
					General.ShowWarningMessage("You must stop the analysis before you can make changes to your map!", MessageBoxButtons.OK);
				}
				else
				{
					ErrorResult r = (results.SelectedItem as ErrorResult);
					if(r.Button3Click(false)) 
					{
						if(results.SelectedItems.Count > 1) FixSimilarErrors(r.GetType(), 3); //mxd
						StartChecking();
					} 
					else 
					{
						General.Interface.RedrawDisplay();
					}
				}
			}
		}

		//mxd
		private void FixSimilarErrors(Type type, int fixIndex) 
		{
			foreach(Object item in results.SelectedItems) 
			{
				if(item == results.SelectedItem) continue;
				if(item.GetType() != type) continue;

				ErrorResult r = item as ErrorResult;

				if(fixIndex == 1 && !r.Button1Click(true)) break;
				if(fixIndex == 2 && !r.Button2Click(true)) break;
				if(fixIndex == 3 && !r.Button3Click(true)) break;
			}
		}

		//mxd
		private void toggleall_CheckedChanged(object sender, EventArgs e) 
		{
			foreach(CheckBox cb in checks.Checkboxes) cb.Checked = toggleall.Checked;
		}

		private void ErrorCheckForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			General.ShowHelp("e_mapanalysis.html");
		}
		
		#endregion

		#region ================== Results Context Menu (mxd)

		private void resultcontextmenustrip_Opening(object sender, System.ComponentModel.CancelEventArgs e) 
		{
			//disable or enable stuff
			bool haveresult = resultslist.Count > 0 && results.SelectedItems.Count > 0;
			resultshowall.Enabled = (resultslist.Count > 0 && resultslist.Count > results.Items.Count);
			resultselectcurrenttype.Enabled = haveresult;
			resultcopytoclipboard.Enabled = haveresult;
			resulthidecurrent.Enabled = haveresult;
			resulthidecurrenttype.Enabled = haveresult;
			resultshowonlycurrent.Enabled = haveresult;
		}

		private void resultshowall_Click(object sender, EventArgs e) 
		{
			// Reset ignored items
			foreach(ErrorResult result in resultslist) result.Hide(false);
			
			// Restore items
			results.Items.Clear();
			results.Items.AddRange(resultslist.ToArray());
			hiddentresulttypes.Clear();

			// Do the obvious
			ClearSelectedResult();
		}

		private void resulthidecurrent_Click(object sender, EventArgs e) 
		{
			// Collect results to hide
			List<ErrorResult> tohide = new List<ErrorResult>();
			foreach(var ro in results.SelectedItems)
			{
				ErrorResult r = ro as ErrorResult;
				if(r == null) return;
				r.Hide(true);
				tohide.Add(r);
			}
			
			// Remove from the list
			results.BeginUpdate();
			foreach(ErrorResult r in tohide) results.Items.Remove(r);
			results.EndUpdate();

			// Do the obvious
			ClearSelectedResult();
		}

		private void resulthidecurrenttype_Click(object sender, EventArgs e)
		{
			Dictionary<Type, bool> tohide = GetSelectedTypes();
			List<ErrorResult> filtered = new List<ErrorResult>();
			hiddentresulttypes.AddRange(tohide.Keys);

			// Apply filtering
			foreach(ErrorResult result in results.Items)
			{
				if(!tohide.ContainsKey(result.GetType())) filtered.Add(result);
			}

			// Replace items
			results.Items.Clear();
			results.Items.AddRange(filtered.ToArray());

			// Do the obvious
			ClearSelectedResult();
		}

		private void resultshowonlycurrent_Click(object sender, EventArgs e) 
		{
			Dictionary<Type, bool> toshow = GetSelectedTypes();
			List<ErrorResult> filtered = new List<ErrorResult>();
			hiddentresulttypes.Clear();

			// Apply filtering
			foreach(ErrorResult result in results.Items)
			{
				Type curresulttype = result.GetType();
				if(!toshow.ContainsKey(curresulttype))
				{
					hiddentresulttypes.Add(curresulttype);
				}
				else
				{
					filtered.Add(result);
				}
			}

			// Replace items
			results.Items.Clear();
			results.Items.AddRange(filtered.ToArray());

			// Do the obvious
			ClearSelectedResult();
		}

		private void resultcopytoclipboard_Click(object sender, EventArgs e)
		{
			// Get results
			StringBuilder sb = new StringBuilder();
			foreach(ErrorResult result in results.SelectedItems) sb.AppendLine(result.ToString());
			
			try
			{
				//mxd. Set on clipboard
				Clipboard.SetDataObject(sb.ToString(), true, 5, 200);

				// Inform the user
				General.Interface.DisplayStatus(StatusType.Info, "Analysis results copied to clipboard.");
			}
			catch(ExternalException)
			{
				// Inform the user
				General.Interface.DisplayStatus(StatusType.Warning, "Failed to perform a Clipboard operation...");
			}
		}

		private void results_KeyUp(object sender, KeyEventArgs e) 
		{
			// Copy descriptions to clipboard?
			if(e.Control && e.KeyCode == Keys.C)
			{
				resultcopytoclipboard_Click(sender, EventArgs.Empty);
			} 
			// Select all?
			else if(e.Control && e.KeyCode == Keys.A)
			{
				results.SelectedItems.Clear();

				bathselectioninprogress = true; //mxd
				results.BeginUpdate(); //mxd
				for(int i = 0; i < results.Items.Count; i++) results.SelectedItems.Add(results.Items[i]);
				results.EndUpdate(); //mxd
				bathselectioninprogress = false; //mxd

				results_SelectedIndexChanged(this, EventArgs.Empty); //trigger update manually
			}
		}

		private void resultselectcurrenttype_Click(object sender, EventArgs e)
		{
			Dictionary<Type, bool> toselect = GetSelectedTypes();
			results.SelectedItems.Clear();

			bathselectioninprogress = true; //mxd
			results.BeginUpdate(); //mxd

			for(int i = 0; i < results.Items.Count; i++) 
			{
				if(toselect.ContainsKey(results.Items[i].GetType())) results.SelectedItems.Add(results.Items[i]);
			}

			results.EndUpdate(); //mxd
			bathselectioninprogress = false; //mxd

			results_SelectedIndexChanged(this, EventArgs.Empty); //trigger update manually
		}

		#endregion

	}
}