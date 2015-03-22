
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using harlam357.Core.ComponentModel;
using harlam357.Core.Threading.Tasks;

namespace harlam357.Core
{
   [TestFixture]
   public class ProgressTests
   {
      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void Progress_InvalidOperationException_Test1()
      {
         // ReSharper disable once ObjectCreationAsStatement
         new Progress<ProgressChangedEventArgs>();
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void Progress_InvalidOperationException_Test2()
      {
         // ReSharper disable once ObjectCreationAsStatement
         new Progress<ProgressChangedEventArgs>((Action<ProgressChangedEventArgs>)null);
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void Progress_InvalidOperationException_Test3()
      {
         // ReSharper disable once ObjectCreationAsStatement
         new Progress<ProgressChangedEventArgs>((TaskScheduler)null);
      }

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Progress_ArgumentNullException_Test()
      {
         // ReSharper disable once ObjectCreationAsStatement
         new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler(), null);
      }

      [Test]
      public void Progress_Report_Test1()
      {
         int progressPercentage = 0;
         string message = null;
         object userState = null;

         Action<ProgressChangedEventArgs> action = args =>
         {
            progressPercentage = args.ProgressPercentage;
            message = args.Message;
            userState = args.UserState;
         };
         IProgress<ProgressChangedEventArgs> progress = new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler(), action);
         var progressState = new object();
         progress.Report(new ProgressChangedEventArgs(50, "Message", progressState));

         Assert.AreEqual(50, progressPercentage);
         Assert.AreEqual("Message", message);
         Assert.AreSame(progressState, userState);
      }

      [Test]
      public void Progress_Report_Test2()
      {
         int progressPercentage = 0;
         string message = null;
         object userState = null;

         var progress = new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler());
         ((IProgress<ProgressChangedEventArgs>)progress).Report(new ProgressChangedEventArgs(50, "Message", new object()));

         Assert.AreEqual(0, progressPercentage);
         Assert.IsNull(message);
         Assert.IsNull(userState);
      }

      [Test]
      public void Progress_Report_Test3()
      {
         int progressPercentage = 0;
         string message = null;
         object userState = null;

         EventHandler<ProgressChangedEventArgs> handler = (sender, args) =>
         {
            progressPercentage = args.ProgressPercentage;
            message = args.Message;
            userState = args.UserState;
         };
         var progress = new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler());
         progress.ProgressChanged += handler;
         var progressState = new object();
         ((IProgress<ProgressChangedEventArgs>)progress).Report(new ProgressChangedEventArgs(50, "Message", progressState));

         Assert.AreEqual(50, progressPercentage);
         Assert.AreEqual("Message", message);
         Assert.AreSame(progressState, userState);
      }
   }
}
