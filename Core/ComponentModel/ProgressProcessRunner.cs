
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
   /// Represents an object that runs a process and reports progress.
   /// </summary>
   public abstract class ProgressProcessRunnerBase : IProgressProcessRunner
   {
      /// <summary>
      /// Occurs when the runner's progress has changed.
      /// </summary>
      public event EventHandler<ProgressEventArgs> ProgressChanged;

      protected virtual void OnProgressChanged(ProgressEventArgs e)
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

   /// <summary>
   /// Provides data for the event that is raised when the progress process runner's progress value has changed.
   /// </summary>
   public class ProgressEventArgs : EventArgs
   {
      /// <summary>
      /// Gets a percentage value indicating the task progress.
      /// </summary>
      /// <returns>A percentage value indicating the task progress.</returns>
      public int Progress { get; private set; }
      
      /// <summary>
      /// Gets a message value indicating the task progress.
      /// </summary>
      /// <returns>A System.String message value indicating the task progress.</returns>
      public string Message { get; private set; }

      /// <summary>
      /// Gets a unique user state.
      /// </summary>
      /// <returns>A unique System.Object indicating the user state.</returns>
      public object UserState { get; private set; }

      /// <summary>
      /// Initializes a new instance of the ProgressEventArgs class with progress percentage and message values.
      /// </summary>
      /// <param name="progress">The progress value.</param>
      /// <param name="message">The text message value.</param>
      public ProgressEventArgs(int progress, string message)
      {
         Progress = progress;
         Message = message ?? String.Empty;
      }

      /// <summary>
      /// Initializes a new instance of the ProgressEventArgs class with progress percentage and message values.
      /// </summary>
      /// <param name="progress">A percentage value indicating the task progress.</param>
      /// <param name="message">A message value indicating the task progress.</param>
      /// <param name="userState">A unique user state.</param>
      public ProgressEventArgs(int progress, string message, object userState)
      {
         Progress = progress;
         Message = message ?? String.Empty;
         UserState = userState;
      }
   }
}
