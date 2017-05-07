
using System;
using System.Threading;
using System.Threading.Tasks;

namespace harlam357.Core.ComponentModel
{
   public interface IAsyncProcessor
   {
      Task ExecuteAsync(IProgress<ProgressInfo> progress);

      Exception Exception { get; }

      bool IsCompleted { get; }

      bool IsFaulted { get; }
   }

   public interface IAsyncProcessorWithCancellation : IAsyncProcessor
   {
      Task ExecuteAsync(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);

      bool IsCanceled { get; }
   }

#if NET45
   public abstract class AsyncProcessorBase : IAsyncProcessor
   {
      public async Task ExecuteAsync(IProgress<ProgressInfo> progress)
      {
         try
         {
            await OnExecuteAsync(progress).ConfigureAwait(false);
            IsCompleted = true;
         }
         catch (Exception ex)
         {
            Exception = ex;
            IsFaulted = true;
         }
      }

      public Exception Exception { get; private set; }

      public bool IsCompleted { get; private set; }

      public bool IsFaulted { get; private set; }

      protected abstract Task OnExecuteAsync(IProgress<ProgressInfo> progress);
   }

   public abstract class AsyncProcessorWithCancellationBase : IAsyncProcessorWithCancellation
   {
      public async Task ExecuteAsync(IProgress<ProgressInfo> progress)
      {
         await ExecuteAsync(CancellationToken.None, progress).ConfigureAwait(false);
      }

      public async Task ExecuteAsync(CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
      {
         try
         {
            await OnExecuteAsync(cancellationToken, progress).ConfigureAwait(false);
            IsCompleted = true;
         }
         catch (OperationCanceledException)
         {
            IsCanceled = true;
         }
         catch (Exception ex)
         {
            Exception = ex;
            IsFaulted = true;
         }
      }

      public Exception Exception { get; private set; }

      public bool IsCanceled { get; private set; }

      public bool IsCompleted { get; private set; }

      public bool IsFaulted { get; private set; }

      protected abstract Task OnExecuteAsync(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);
   }
#endif
}
