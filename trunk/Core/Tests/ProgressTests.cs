
using System;
using System.ComponentModel;

using NUnit.Framework;

using harlam357.Core.Threading.Tasks;

namespace harlam357.Core
{
   [TestFixture]
   public class ProgressTests
   {
      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void Progress_InvalidOperationException_Test()
      {
         // ReSharper disable once ObjectCreationAsStatement
         new Progress<ProgressChangedEventArgs>();
      }

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Progress_Constructor_Test()
      {
         // ReSharper disable once ObjectCreationAsStatement
         new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler(), null);
      }

      [Test]
      public void Progress_Report_Test()
      {
         int progressPercentage = 0;
         object userState = null;

         Action<ProgressChangedEventArgs> action = args =>
         {
            progressPercentage = args.ProgressPercentage;
            userState = args.UserState;
         };
         IProgress<ProgressChangedEventArgs> progress = new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler(), action);
         var progressState = new object();;
         progress.Report(new ProgressChangedEventArgs(50, progressState));

         Assert.AreEqual(50, progressPercentage);
         Assert.AreSame(progressState, userState);
      }
   }
}
