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

namespace harlam357.Windows.Forms
{
   /// <summary>
   /// Represents a view interface for a modal dialog that reports the progress of an asynchronous task.
   /// </summary>
   public interface IProgressDialogAsyncView : IWin32Window, IDisposable
   {
      /// <summary>
      /// Gets or sets the progress reporting object responsible for notifying the dialog of task progress.
      /// </summary>
      Core.Progress<Core.ComponentModel.ProgressChangedEventArgs> Progress { get; set; }

      /// <summary>
      /// Gets or sets the object that facilitates task cancellation.
      /// </summary>
      CancellationTokenSource CancellationTokenSource { get; set; }

      /// <summary>
      /// Updates the progress bar value.
      /// </summary>
      void UpdateProgress(int progress);

      /// <summary>
      /// Updates the text message value.
      /// </summary>
      void UpdateMessage(string message);

      /// <summary>
      /// Occurs whenever the form is first displayed.
      /// </summary>
      event EventHandler Shown;

      /// <summary>
      /// Gets or sets the icon for the form.
      /// </summary>
      Icon Icon { get; set; }

      /// <summary>
      /// Gets or sets the starting position of the form at run time.
      /// </summary>
      FormStartPosition StartPosition { get; set; }

      /// <summary>
      /// Gets or sets the text associated with this form.
      /// </summary>
      string Text { get; set; }

      /// <summary>
      /// Shows the form as a modal dialog box with the specified owner.
      /// </summary>
      /// <param name="owner">Any object that implements System.Windows.Forms.IWin32Window that represents the top-level window that will own the modal dialog box.</param>
      /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
      DialogResult ShowDialog(IWin32Window owner);

      /// <summary>
      /// Closes the dialog.
      /// </summary>
      void Close();
   }

   /// <summary>
   /// Progress Process Dialog Class
   /// </summary>
   public partial class ProgressDialogAsync : Form, IProgressDialogAsyncView
   {
      private Core.Progress<Core.ComponentModel.ProgressChangedEventArgs> _progress;

      /// <summary>
      /// Gets or sets the progress reporting object responsible for notifying the dialog of task progress.
      /// </summary>
      public Core.Progress<Core.ComponentModel.ProgressChangedEventArgs> Progress
      {
         get { return _progress; }
         set
         {
            if (_progress != null)
            {
               _progress.ProgressChanged -= OnProgressChanged;
               TaskInProgress = false;
            }
            _progress = value;
            if (_progress != null)
            {
               _progress.ProgressChanged += OnProgressChanged;
               TaskInProgress = true;
            }
         }
      }

      private CancellationTokenSource _cancellationTokenSource;

      /// <summary>
      /// Gets or sets the object that facilitates task cancellation.
      /// </summary>
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

      private readonly Size _baseSize;

      /// <summary>
      /// Initializes a new instance of the ProgressDialogAsync class.
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

      protected virtual void OnProgressChanged(object sender, Core.ComponentModel.ProgressChangedEventArgs e)
      {
         UpdateProgress(e.ProgressPercentage);
         UpdateMessage(e.Message);
      }

      private void ProcessCancelButtonClick(object sender, EventArgs e)
      {
         Debug.Assert(SupportsCancellation);

         if (TaskInProgress)
         {
            _cancellationTokenSource.Cancel();
         }
      }

      protected bool TaskInProgress { get; private set; }

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

         TaskInProgress = false;
         base.Close();
      }

      private void SetCancellationControls(bool enabled)
      {
         ProcessCancelButton.Visible = enabled;
         Size = enabled ? _baseSize : new Size(_baseSize.Width, _baseSize.Height - 30);
      }
   }
}
