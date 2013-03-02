/*
 * harlam357.Windows.Forms - Progress Dialog
 * Copyright (C) 2010-2013 Ryan Harlamert (harlam357)
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
using System.Windows.Forms;

namespace harlam357.Windows.Forms
{
   /// <summary>
   /// Represents an object that runs a process and reports progress.
   /// </summary>
   public interface IProgressProcessRunner
   {
      /// <summary>
      /// Occurs when the runner's progress has changed.
      /// </summary>
      event EventHandler<ProgressEventArgs> ProgressChanged;
      /// <summary>
      /// Occurs when the runner's process has finished.
      /// </summary>
      event EventHandler ProcessFinished;

      /// <summary>
      /// Gets the exception that resulted from executing the runner or null if no exception occurred.
      /// </summary>
      Exception Exception { get; }
      
      /// <summary>
      /// Gets a value that defines if this runner supports being cancelled.
      /// </summary>
      bool SupportsCancellation { get; }
      
      /// <summary>
      /// Gets a value that reports if the runner is currently processing.
      /// </summary>
      bool Processing { get; }
   
      /// <summary>
      /// Executes the runner.
      /// </summary>
      void Process();
      
      /// <summary>
      /// Cancels the runner.
      /// </summary>
      void Cancel();
   }
   
   /// <summary>
   /// Represents a view interface for a modal dialog that runs a process asynchronously and reports progress.
   /// </summary>
   public interface IProgressDialogView : IWin32Window
   {
      /// <summary>
      /// Gets or sets the window that owns this view.
      /// </summary>
      IWin32Window OwnerWindow { get; set; }

      /// <summary>
      /// Gets or sets the icon for the form.
      /// </summary>
      Icon Icon { get; set; }

      /// <summary>
      /// Gets or sets the starting position of the form at run time.
      /// </summary>
      FormStartPosition StartPosition { get; set; }
      
      /// <summary>
      /// Gets or sets the progress process runner that executes when the Process method is called.
      /// </summary>
      IProgressProcessRunner ProcessRunner { get; set; }
      
      /// <summary>
      /// Gets or sets the text associated with this control.
      /// </summary>
      string Text { get; set; }
      
      /// <summary>
      /// Updates the progress bar value.
      /// </summary>
      void UpdateProgress(int progress);
      
      /// <summary>
      /// Updates the text message value.
      /// </summary>
      void UpdateMessage(string message);

      /// <summary>
      /// Shows the modal dialog and executes the IProgressProcessRunner instance assigned to the ProcessRunner property.
      /// </summary>
      void Process();
   }

   /// <summary>
   /// Progress Process Dialog Class
   /// </summary>
   public partial class ProgressDialog : Form, IProgressDialogView
   {
      private readonly Size _baseSize;

      /// <summary>
      /// Gets or sets the window that owns this view.
      /// </summary>
      public IWin32Window OwnerWindow { get; set; }

      private IProgressProcessRunner _processRunner;
      /// <summary>
      /// Gets or sets the progress process runner that executes when the Process method is called.
      /// </summary>
      public IProgressProcessRunner ProcessRunner
      {
         get { return _processRunner; }
         set 
         { 
            _processRunner = value;
            SetCancellationControls(_processRunner.SupportsCancellation);
         }
      }

      /// <summary>
      /// Initializes a new instance of the ProgressDialog class.
      /// </summary>
      public ProgressDialog()
      {
         InitializeComponent();
         _baseSize = Size;
      }

      /// <summary>
      /// Updates the progress bar value.
      /// </summary>
      public void UpdateProgress(int progress)
      {
         if (InvokeRequired)
         {
            Invoke(new Action<int>(UpdateProgress), progress);
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
            Invoke(new Action<string>(UpdateMessage), message);
            return;
         }

         messageLabel.Text = message;
      }

      /// <summary>
      /// Shows the modal dialog and executes the IProgressProcessRunner instance assigned to the ProcessRunner property.
      /// </summary>
      /// <exception cref="T:System.InvalidOperationException">The ProcessRunner property is null.</exception>
      public void Process()
      {
         if (ProcessRunner == null) throw new InvalidOperationException("ProcessRunner property must not be null.");

         ProcessRunner.ProgressChanged += ProcessRunnerProgressChanged;
         ProcessRunner.ProcessFinished += ProcessRunnerProcessFinished;
         var process = new Action(ProcessRunner.Process);
         process.BeginInvoke(EndProcess, process);
         ShowDialog(OwnerWindow);
      }

      private void EndProcess(IAsyncResult result)
      {
         try
         {
            var process = (Action)result.AsyncState;
            process.EndInvoke(result);
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
         finally
         {
            ProcessRunner.ProgressChanged -= ProcessRunnerProgressChanged;
            ProcessRunner.ProcessFinished -= ProcessRunnerProcessFinished;

            Close();
         }
      }

      /// <summary>
      /// Handles the runner's ProgressChanged event.
      /// </summary>
      protected virtual void ProcessRunnerProgressChanged(object sender, ProgressEventArgs e)
      {
         UpdateProgress(e.Progress);
         UpdateMessage(e.Message);
      }

      /// <summary>
      /// Handles the runner's ProcessFinished event.
      /// </summary>
      protected virtual void ProcessRunnerProcessFinished(object sender, EventArgs e)
      {
         
      }

      private void ProcessCancelButtonClick(object sender, EventArgs e)
      {
         Debug.Assert(_processRunner.SupportsCancellation);

         if (_processRunner.Processing)
         {
            _processRunner.Cancel();
         }
      }

      /// <summary>
      /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
      /// </summary>
      /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
      /// <remarks>
      /// If the runner is processing and runner does not support cancellation then this event will be cancelled.
      /// If the runner supports cancellation then the Cancel method will be called and the dialog will be closed when the runner is finished.
      /// </remarks>
      protected override void OnFormClosing(FormClosingEventArgs e)
      {
         if (_processRunner.Processing)
         {
            if (_processRunner.SupportsCancellation)
            {
               _processRunner.Cancel();
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
            Invoke(new MethodInvoker(Close));
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

   /// <summary>
   /// Provides data for the event that is raised when the progress process runner's progress value has changed.
   /// </summary>
   public class ProgressEventArgs : EventArgs
   {
      /// <summary>
      /// Gets the progress value.
      /// </summary>
      public int Progress { get; private set; }
      /// <summary>
      /// Gets the text message value.
      /// </summary>
      public string Message { get; private set; }

      /// <summary>
      /// Initializes a new instance of the ProgressEventArgs class with progress and text message values.
      /// </summary>
      /// <param name="progress">The progress value.</param>
      /// <param name="message">The text message value.</param>
      public ProgressEventArgs(int progress, string message)
      {
         Progress = progress;
         Message = message ?? String.Empty;
      }
   }
}
