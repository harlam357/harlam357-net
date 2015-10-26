
#if NET40
namespace harlam357.Core
{

   // Implementation for .NET 4.0
   public interface IProgress<in T>
   {
      void Report(T value);
   }

   // Delegate for IProgress<T> implementations to use in .NET 4.0 in lieu of EventHandler<T>
   public delegate void ProgressHandler<T>(object sender, T eventArgs);
}
#endif
