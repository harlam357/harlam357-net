/*
 * harlam357.Windows.Forms - Progress Dialog Async
 * Copyright (C) 2010-2015 Ryan Harlamert (harlam357)
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using harlam357.Core;
using harlam357.Core.ComponentModel;

namespace harlam357.Windows.Forms
{
   /// <summary>
   /// Progress Process Dialog Class
   /// </summary>
   public partial class ProgressDialogAsync : Form
   {
      private readonly Size _baseSize;

      /// <summary>
      /// Gets or sets the window that owns this view.
      /// </summary>
      public IWin32Window OwnerWindow { get; set; }

      private Progress<ProgressChangedEventArgs> _progress;

      public Progress<ProgressChangedEventArgs> Progress
      {
         get { return _progress; }
         set
         {
            if (_progress != null)
            {
               _progress.ProgressChanged -= OnProgressChanged;
            }
            _progress = value;
            if (_progress != null)
            {
               _progress.ProgressChanged += OnProgressChanged;
            }
         }
      }

      private CancellationTokenSource _cancellationTokenSource;

      public CancellationTokenSource CancellationTokenSource
      {
         get { return _cancellationTokenSource; }
         set
         {
            _cancellationTokenSource = value;
            SetCancellationControls(SupportsCancellation);
         }
      }

      private bool SupportsCancellation
      {
         get { return _cancellationTokenSource != null; }
      }

      /// <summary>
      /// Initializes a new instance of the ProgressDialog class.
      /// </summary>
      public ProgressDialogAsync()
      {
         InitializeComponent();
         _baseSize = Size;
         SetCancellationControls(SupportsCancellation);
      }

      /// <summary>
      /// Updates the progress bar value.
      /// </summary>
      public void UpdateProgress(int progress)
      {
         if (InvokeRequired)
         {
            BeginInvoke(new Action<int>(UpdateProgress), progress);
            return;
         }

         progressBar.Value = progress;
      }

      /// <summary>
      /// Updates the text message value.
      /// </summary>
      public void UpdateMessage(string message)
      {
         if (InvokeRequired)
         {
            BeginInvoke(new Action<string>(UpdateMessage), message);
            return;
         }

         messageLabel.Text = message;
      }

      protected ProgressChangedEventState LastProgressChangedEventState { get; set; }

      protected virtual void OnProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         LastProgressChangedEventState = e.State;
         
         UpdateProgress(e.ProgressPercentage);
         UpdateMessage(e.Message);
         if (e.State == ProgressChangedEventState.Started)
         {
            //Show(OwnerWindow);
            ShowDialog(OwnerWindow);
         }
         else if (e.State == ProgressChangedEventState.Finished)
         {
            if (e.Exception != null)
            {
               MessageBox.Show(OwnerWindow, e.Exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Close();
         }
      }

      private void ProcessCancelButtonClick(object sender, EventArgs e)
      {
         Debug.Assert(SupportsCancellation);

         if (TaskInProgress)
         {
            _cancellationTokenSource.Cancel();
         }
      }

      protected bool TaskInProgress
      {
         get
         {
            return LastProgressChangedEventState == ProgressChangedEventState.Started ||
                   LastProgressChangedEventState == ProgressChangedEventState.InProgress;
         }
      }

      /// <summary>
      /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
      /// </summary>
      /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
      /// <remarks>
      /// If the task is processing and task does not support cancellation then this event will be cancelled.
      /// If the task supports cancellation then the task will be cancelled and the dialog will be closed when the task is finished.
      /// </remarks>
      protected override void OnFormClosing(FormClosingEventArgs e)
      {
         if (TaskInProgress)
         {
            if (SupportsCancellation)
            {
               _cancellationTokenSource.Cancel();
            }
            e.Cancel = true;
            return;
         }
         base.OnFormClosing(e);
      }

      /// <summary>
      /// Closes the dialog.
      /// </summary>
      public new void Close()
      {
         if (InvokeRequired)
         {
            BeginInvoke(new MethodInvoker(Close));
            return;
         }

         base.Close();
      }
      
      private void SetCancellationControls(bool enabled)
      {
         ProcessCancelButton.Visible = enabled;
         Size = enabled ? _baseSize : new Size(_baseSize.Width, _baseSize.Height - 30);
      }
   }
}
