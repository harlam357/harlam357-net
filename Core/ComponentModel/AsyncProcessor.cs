
using System;
using System.Threading;
using System.Threading.Tasks;

namespace harlam357.Core.ComponentModel
{
   public interface IAsyncProcessor
   {
      Task ExecuteAsync(IProgress<ProgressInfo> progress);

      Exception Exception { get; }
   }

   public interface IAsyncProcessorWithCancellation : IAsyncProcessor
   {
      Task ExecuteAsync(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);
   }

#if NET45
   public abstract class AsyncProcessorBase : IAsyncProcessor
   {
      public async Task ExecuteAsync(IProgress<ProgressInfo> progress)
      {
         try
         {
            await OnExecuteAsync(progress).ConfigureAwait(false);
         }
         catch (Exception ex)
         {
            Exception = ex;
         }
      }

      public Exception Exception { get; private set; }

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
         }
         catch (OperationCanceledException)
         {
            // handle the cancellation
         }
         catch (Exception ex)
         {
            Exception = ex;
         }
      }

      public Exception Exception { get; private set; }

      protected abstract Task OnExecuteAsync(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);
   }
#endif
}
