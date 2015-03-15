
using System;

namespace harlam357.Core.ComponentModel
{
   /// <summary>
   /// Specifies the state of the task.
   /// </summary>
   public enum ProgressChangedEventState
   {
      /// <summary>
      /// Specifies that the task has no determined state.
      /// </summary>
      None,
      /// <summary>
      /// Specifies that the task has started.
      /// </summary>
      Started,
      /// <summary>
      /// Specifies that the task is in progress.
      /// </summary>
      InProgress,
      /// <summary>
      /// Specifies that the task has finished.
      /// </summary>
      Finished
   }

   /// <summary>
   /// Provides data for the a ProgressChanged event.
   /// </summary>
   public class ProgressChangedEventArgs : EventArgs
   {
      /// <summary>
      /// Gets the task state.
      /// </summary>
      /// <returns>A value indicating the state of the task.</returns>
      public ProgressChangedEventState State { get; private set; }

      /// <summary>
      /// Gets the task progress percentage.
      /// </summary>
      /// <returns>A percentage value indicating the task progress.</returns>
      public int ProgressPercentage { get; private set; }

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
      /// Gets the exception that resulted in the task ending prematurely.
      /// </summary>
      /// <returns>A System.Exception indicating why the task ending prematurely or null if the task finished with no errors.</returns>
      public Exception Exception { get; private set; }

      /// <summary>
      /// Initializes a new instance of the ProgressEventArgs class with progress percentage and message values.
      /// </summary>
      /// <param name="state">A value indicating the state of the task.</param>
      /// <param name="progressPercentage">The progress value.</param>
      /// <param name="message">The text message value.</param>
      public ProgressChangedEventArgs(ProgressChangedEventState state, int progressPercentage, string message)
      {
         State = state;
         ProgressPercentage = progressPercentage;
         Message = message ?? String.Empty;
      }

      /// <summary>
      /// Initializes a new instance of the ProgressEventArgs class with progress percentage and message values.
      /// </summary>
      /// <param name="state">A value indicating the state of the task.</param>
      /// <param name="progressPercentage">A percentage value indicating the task progress.</param>
      /// <param name="message">A message value indicating the task progress.</param>
      /// <param name="userState">A unique user state.</param>
      /// <param name="exception">An exception indicating why the task ending prematurely or null if the task finished with no errors.</param>
      public ProgressChangedEventArgs(ProgressChangedEventState state, int progressPercentage, string message, object userState, Exception exception)
      {
         State = state;
         ProgressPercentage = progressPercentage;
         Message = message ?? String.Empty;
         UserState = userState;
         Exception = exception;
      }
   }
}
