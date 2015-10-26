
using System;

namespace harlam357.Core
{
   public interface IProgressWithEvent<T> : IProgress<T>
   {
#if NET40
      event ProgressHandler<T> ProgressChanged;
#else
      event EventHandler<T> ProgressChanged;
#endif
   }


#if NET45
   public static class ProgressExtensions
   {
      /// <summary>
      /// Creates a wrapper around a Progress instance that implements IProgressWithEvent. 
      /// </summary>
      /// <typeparam name="T">Specifies the type of the progress report value.</typeparam>
      /// <param name="progress">The source Progress instance to wrap.</param>
      /// <returns>A wrapper around a Progress instance that implements IProgressWithEvent. </returns>
      public static IProgressWithEvent<T> WithEvent<T>(this Progress<T> progress)
      {
         return new ProgressWithEvent<T>(progress);
      }

      private sealed class ProgressWithEvent<T> : IProgressWithEvent<T>
      {
         private readonly Progress<T> _progress;

         public ProgressWithEvent(Progress<T> progress)
         {
            _progress = progress;
            _progress.ProgressChanged += (s, e) => OnProgressChanged(s, e);
         }

         public event EventHandler<T> ProgressChanged;

         private void OnProgressChanged(object sender, T args)
         {
            var handler = ProgressChanged;
            if (handler != null) handler(sender, args);
         }

         void IProgress<T>.Report(T value)
         {
            ((IProgress<T>)_progress).Report(value);
         }
      }
   }
#endif
}