
using System;
using System.Threading;
using System.Threading.Tasks;

namespace harlam357.Core.ComponentModel
{
   public interface IAsyncProcessor
   {
      Task ExecuteAsync(IProgress<ProgressInfo> progress);
   }

   public interface IAsyncProcessorWithCancellation : IAsyncProcessor
   {
      Task ExecuteAsync(CancellationToken cancellationToken, IProgress<ProgressInfo> progress);
   }
}
