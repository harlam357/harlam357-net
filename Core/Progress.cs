
using System;
using System.Threading;
using System.Threading.Tasks;

namespace harlam357.Core
{
   // Implementation for .NET 4.0 - remove if this library is upgraded to 4.5.
   public interface IProgress<in T>
   {
      void Report(T value);
   }

   // Implementation of IProgress<T> based on the .NET 4.5 implementation.
   public class Progress<T> : IProgress<T> where T : EventArgs
   {
      private readonly TaskScheduler _taskScheduler;
      private readonly Action<T> _handler;
      private readonly Action<object> _invokeHandlers;

      public event EventHandler<T> ProgressChanged;

      public Progress()
         : this(null, null)
      {
         
      }

      public Progress(Action<T> handler)
         : this(null, handler)
      {
         
      }

      internal Progress(TaskScheduler taskScheduler, Action<T> handler)
      {
         _taskScheduler = taskScheduler ?? TaskScheduler.FromCurrentSynchronizationContext();
         if (_taskScheduler == null)
         {
            throw new InvalidOperationException("No task scheduler.");
         }
         _invokeHandlers = InvokeHandlers;

         if (handler == null) throw new ArgumentNullException("handler");
         _handler = handler;
      }

      protected virtual void OnReport(T value)
      {
         if (_handler == null && ProgressChanged == null)
         {
            return;
         }
         Task.Factory.StartNew(_invokeHandlers, value, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
      }

      void IProgress<T>.Report(T value)
      {
         OnReport(value);
      }

      private void InvokeHandlers(object state)
      {
         var e = (T)state;
         Action<T> action = _handler;
         if (action != null)
         {
            action(e);
         }
         EventHandler<T> eventHandler = ProgressChanged;
         if (eventHandler == null)
         {
            return;
         }
         eventHandler(this, e);
      }
   }
}
