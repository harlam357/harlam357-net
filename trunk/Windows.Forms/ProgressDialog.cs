/*
 * harlam357.Net - Progress Dialog
 * Copyright (C) 2010 Ryan Harlamert (harlam357)
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
   /// Defines the Progress Process Class Interface
   /// </summary>
   public interface IProgressProcessRunner
   {
      /// <summary>
      /// Raised on Progress Process Changed
      /// </summary>
      event EventHandler<ProgressEventArgs> ProgressChanged;
      /// <summary>
      /// Raised on Progress Process Finished
      /// </summary>
      event EventHandler ProcessFinished;

      /// <summary>
      /// Exception returned from Progress Process
      /// </summary>
      Exception Exception { get; }
      
      /// <summary>
      /// Defines if this Progress Process supports being cancelled
      /// </summary>
      bool SupportsCancellation { get; }
      
      /// <summary>
      /// Flag denoting if the Progress Process is in progress
      /// </summary>
      bool Processing { get; }
   
      /// <summary>
      /// Execute the Progress Process
      /// </summary>
      void Process();
      
      /// <summary>
      /// Cancel the Progress Process
      /// </summary>
      void Cancel();
   }
   
   /// <summary>
   /// Defines the Progress Dialog View Interface
   /// </summary>
   public interface IProgressDialogView : IWin32Window
   {
      /// <summary>
      /// Window that owns this dialog
      /// </summary>
      IWin32Window OwnerWindow { set; }
      
      /// <summary>
      /// Progress Process Class to execute
      /// </summary>
      IProgressProcessRunner ProcessRunner { get; set; }
      
      /// <summary>
      /// Update Progress Bar Value
      /// </summary>
      void UpdateProgress(int progress);
      
      /// <summary>
      /// Update Text Message Value
      /// </summary>
      void UpdateMessage(string message);

      /// <summary>
      /// Show the Dialog and execute the Progress Process
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
      /// Window that owns this dialog
      /// </summary>
      public IWin32Window OwnerWindow { get; set; }

      private IProgressProcessRunner _processRunner;
      /// <summary>
      /// Progress Process Class to execute
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

      public ProgressDialog()
      {
         InitializeComponent();
         _baseSize = Size;
      }

      /// <summary>
      /// Update Progress Bar Value (safe to be called from a worker thread)
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
      /// Update Text Message Value (safe to be called from a worker thread)
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
      /// Show the Dialog and execute the Progress Process asynchronously
      /// </summary>
      public virtual void Process()
      {
         if (OwnerWindow == null) throw new InvalidOperationException("OwnerWindow property must not be null.");
         if (ProcessRunner == null) throw new InvalidOperationException("ProcessRunner property must not be null.");

         ProcessRunner.ProgressChanged += ProcessRunnerProgressChanged;
         ProcessRunner.ProcessFinished += ProcessRunnerProcessFinished;
         var process = new MethodInvoker(ProcessRunner.Process);
         process.BeginInvoke(null, null);
         ShowDialog(OwnerWindow);
      }

      /// <summary>
      /// Handles the given ProcessRunner's Progress Changed Event
      /// </summary>
      protected virtual void ProcessRunnerProgressChanged(object sender, ProgressEventArgs e)
      {
         UpdateProgress(e.Progress);
         UpdateMessage(e.Message);
      }

      /// <summary>
      /// Handles the given ProcessRunner's Process Finished Event
      /// </summary>
      protected virtual void ProcessRunnerProcessFinished(object sender, EventArgs e)
      {
         ProcessRunner.ProgressChanged -= ProcessRunnerProgressChanged;
         ProcessRunner.ProcessFinished -= ProcessRunnerProcessFinished;
      
         Close();
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
      /// Handles the Dialog's FormClosing Event
      /// </summary>
      /// <remarks>If the ProcessRunner is Processing this event will be cancelled.
      /// If the ProcessRunner supports cancellation then the Cancel() method will be 
      /// called and the dialog will be closed when the ProcessRunner is finished.</remarks>
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
      /// Close the Dialog (safe to be called from a worker thread)
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
   /// Progress Process Event Arguments
   /// </summary>
   public class ProgressEventArgs : EventArgs
   {
      /// <summary>
      /// Progress Bar Value
      /// </summary>
      public int Progress { get; private set; }
      /// <summary>
      /// Text Message Value
      /// </summary>
      public string Message { get; private set; }

      public ProgressEventArgs(int progress, string message)
      {
         Progress = progress;
         Message = message ?? String.Empty;
      }
   }
}
