
using System;
using System.Threading;
using System.Threading.Tasks;

namespace harlam357.Core.Threading.Tasks
{
   public class TaskSchedulerProgress<T> : IProgress<T>
   {
      private readonly TaskScheduler _taskScheduler;
      private readonly Action<T> _handler;
      private readonly Action<object> _invokeHandlers;

#if NET40
      public event ProgressHandler<T> ProgressChanged;
#else
      public event EventHandler<T> ProgressChanged;
#endif

      public TaskSchedulerProgress()
         : this((TaskScheduler)null)
      {

      }

      public TaskSchedulerProgress(Action<T> handler)
         : this(null, handler)
      {

      }

      public TaskSchedulerProgress(TaskScheduler taskScheduler)
      {
         _taskScheduler = taskScheduler ?? (SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : new CurrentThreadTaskScheduler());
         _invokeHandlers = InvokeHandlers;
      }

      public TaskSchedulerProgress(TaskScheduler taskScheduler, Action<T> handler)
      {
         _taskScheduler = taskScheduler ?? (SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : new CurrentThreadTaskScheduler());
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
#if NET40
         ProgressHandler<T> eventHandler = ProgressChanged;
#else
         EventHandler<T> eventHandler = ProgressChanged;
#endif
         if (eventHandler == null)
         {
            return;
         }
         eventHandler(this, e);
      }
   }
}
