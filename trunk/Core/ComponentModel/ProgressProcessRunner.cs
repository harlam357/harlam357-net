/*
 * harlam357.Core.ComponentModel - Progress Process Runner
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

namespace harlam357.Core.ComponentModel
{
   /// <summary>
   /// Represents an object that runs a process and reports progress.
   /// </summary>
   public interface IProgressProcessRunner
   {
      /// <summary>
      /// Occurs when the runner's progress has changed.
      /// </summary>
      event EventHandler<ProgressChangedEventArgs> ProgressChanged;
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
   /// Represents an object that runs a process and reports progress.
   /// </summary>
   public abstract class ProgressProcessRunnerBase : IProgressProcessRunner
   {
      /// <summary>
      /// Occurs when the runner's progress has changed.
      /// </summary>
      public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

      protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
      {
         var handler = ProgressChanged;
         if (handler != null)
         {
            handler(this, e);
         }
      }

      /// <summary>
      /// Occurs when the runner's process has finished.
      /// </summary>
      public event EventHandler ProcessFinished;

      protected virtual void OnProcessFinished(EventArgs e)
      {
         var handler = ProcessFinished;
         if (handler != null)
         {
            handler(this, e);
         }
      }

      private Exception _exception;
      /// <summary>
      /// Gets the exception that resulted from executing the runner or null if no exception occurred.
      /// </summary>
      public Exception Exception
      {
         get { return _exception; }
      }

      /// <summary>
      /// Gets a value that defines if this runner supports being cancelled.
      /// </summary>
      public bool SupportsCancellation
      {
         get { return SupportsCancellationInternal; }
      }

      protected abstract bool SupportsCancellationInternal { get; }

      private bool _processing;
      /// <summary>
      /// Gets a value that reports if the runner is currently processing.
      /// </summary>
      public bool Processing
      {
         get { return _processing; }
         private set
         {
            CancelToken = false;
            _processing = value;
         }
      }

      /// <summary>
      /// Executes the runner.
      /// </summary>
      public void Process()
      {
         Processing = true;
         try
         {
            ProcessInternal();
         }
         catch (Exception ex)
         {
            _exception = ex;
         }
         finally
         {
            Processing = false;
            OnProcessFinished(EventArgs.Empty);
         }
      }

      protected abstract void ProcessInternal();

      protected bool CancelToken { get; set; }

      /// <summary>
      /// Cancels the runner.
      /// </summary>
      public void Cancel()
      {
         CancelToken = true;
      }
   }
}
