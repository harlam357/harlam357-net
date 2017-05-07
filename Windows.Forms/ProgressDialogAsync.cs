/*
 * harlam357.Windows.Forms - Progress Dialog Async
 * Copyright (C) 2010-2017 Ryan Harlamert (harlam357)
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
using System.Threading.Tasks;
using System.Windows.Forms;

using harlam357.Core;
using harlam357.Core.ComponentModel;

namespace harlam357.Windows.Forms
{
#if NET45
   /// <summary>
   /// Represents a view interface for a modal dialog that reports the progress of an asynchronous task.
   /// </summary>
   public interface IProgressDialogAsyncView : IWin32Window, IDisposable
   {
      /// <summary>
      /// Gets or sets the asynchronous processor object.
      /// </summary>
      IAsyncProcessor AsyncProcessor { get; set; }

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
   public sealed partial class ProgressDialogAsync : Form, IProgressDialogAsyncView
   {
      private IAsyncProcessor _asyncProcessor;

      /// <summary>
      /// Gets or sets the asynchronous processor object.
      /// </summary>
      public IAsyncProcessor AsyncProcessor
      {
         get { return _asyncProcessor; }
         set
         {
            _asyncProcessor = value;
            SetCancellationControls(SupportsCancellation);
         }
      }

      private bool SupportsCancellation
      {
         get { return AsyncProcessor is IAsyncProcessorWithCancellation; }
      }

      /// <summary>
      /// Initializes a new instance of the ProgressDialogAsync class.
      /// </summary>
      public ProgressDialogAsync()
      {
         InitializeComponent();
         _baseSize = Size;
         SetCancellationControls(SupportsCancellation);
      }

      private void ProcessCancelButtonClick(object sender, EventArgs e)
      {
         Debug.Assert(SupportsCancellation);

         if (_taskInProgress)
         {
            Debug.Assert(_cancellationTokenSource != null);
            _cancellationTokenSource.Cancel();
         }
      }

      private CancellationTokenSource _cancellationTokenSource;
      private bool _taskInProgress;

      protected override async void OnShown(EventArgs e)
      {
         var progress = new Progress<ProgressInfo>();
         progress.ProgressChanged += (s, progressInfo) =>
         {
            progressBar.Value = progressInfo.ProgressPercentage;
            messageLabel.Text = progressInfo.Message;
         };

         var processorWithCancellation = AsyncProcessor as IAsyncProcessorWithCancellation;
         if (processorWithCancellation != null)
         {
            await RunAsyncProcessorWithCancellation(processorWithCancellation, progress);
         }
         else
         {
            await RunAsyncProcessor(AsyncProcessor, progress);
         }
         Close();
      }

      private async Task RunAsyncProcessorWithCancellation(IAsyncProcessorWithCancellation processor, IProgress<ProgressInfo> progress)
      {
         _cancellationTokenSource = new CancellationTokenSource();
         _taskInProgress = true;
         try
         {
            await processor.ExecuteAsync(_cancellationTokenSource.Token, progress).ConfigureAwait(false);
         }
         catch (OperationCanceledException)
         {
            // handle the cancellation
         }
         catch (Exception ex)
         {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         finally
         {
            _taskInProgress = false;
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
         }
      }

      private async Task RunAsyncProcessor(IAsyncProcessor processor, IProgress<ProgressInfo> progress)
      {
         _taskInProgress = true;
         try
         {
            await processor.ExecuteAsync(progress).ConfigureAwait(false);
         }
         catch (Exception ex)
         {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         finally
         {
            _taskInProgress = false;
         }
      }

      protected override void OnFormClosing(FormClosingEventArgs e)
      {
         if (_taskInProgress)
         {
            if (SupportsCancellation)
            {
               Debug.Assert(_cancellationTokenSource != null);
               _cancellationTokenSource.Cancel();
            }
            e.Cancel = true;
            return;
         }
         base.OnFormClosing(e);
      }

      private readonly Size _baseSize;

      private void SetCancellationControls(bool enabled)
      {
         ProcessCancelButton.Visible = enabled;
         Size = enabled ? _baseSize : new Size(_baseSize.Width, _baseSize.Height - 30);
      }
   }
#endif
}
