
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
         var state = ProgressChangedEventState.None;
         int progressPercentage = 0;
         string message = null;
         object userState = null;
         Exception exception = null;

         Action<ProgressChangedEventArgs> action = args =>
         {
            state = args.State;
            progressPercentage = args.ProgressPercentage;
            message = args.Message;
            userState = args.UserState;
            exception = args.Exception;
         };
         IProgress<ProgressChangedEventArgs> progress = new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler(), action);
         var progressState = new object();
         var progressException = new Exception();
         progress.Report(new ProgressChangedEventArgs(ProgressChangedEventState.InProgress, 50, "Message", progressState, progressException));

         Assert.AreEqual(ProgressChangedEventState.InProgress, state);
         Assert.AreEqual(50, progressPercentage);
         Assert.AreEqual("Message", message);
         Assert.AreSame(progressState, userState);
         Assert.AreSame(progressException, exception);
      }

      [Test]
      public void Progress_Report_Test2()
      {
         var state = ProgressChangedEventState.None;
         int progressPercentage = 0;
         string message = null;
         object userState = null;
         Exception exception = null;

         var progress = new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler());
         ((IProgress<ProgressChangedEventArgs>)progress).Report(new ProgressChangedEventArgs(ProgressChangedEventState.InProgress, 50, "Message", new object(), new Exception()));

         Assert.AreEqual(ProgressChangedEventState.None, state);
         Assert.AreEqual(0, progressPercentage);
         Assert.IsNull(message);
         Assert.IsNull(userState);
         Assert.IsNull(exception);
      }

      [Test]
      public void Progress_Report_Test3()
      {
         var state = ProgressChangedEventState.None;
         int progressPercentage = 0;
         string message = null;
         object userState = null;
         Exception exception = null;

         EventHandler<ProgressChangedEventArgs> handler = (sender, args) =>
         {
            state = args.State;
            progressPercentage = args.ProgressPercentage;
            message = args.Message;
            userState = args.UserState;
            exception = args.Exception;
         };
         var progress = new Progress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler());
         progress.ProgressChanged += handler;
         var progressState = new object();
         var progressException = new Exception();
         ((IProgress<ProgressChangedEventArgs>)progress).Report(new ProgressChangedEventArgs(ProgressChangedEventState.InProgress, 50, "Message", progressState, progressException));

         Assert.AreEqual(ProgressChangedEventState.InProgress, state);
         Assert.AreEqual(50, progressPercentage);
         Assert.AreEqual("Message", message);
         Assert.AreSame(progressState, userState);
         Assert.AreSame(progressException, exception);
      }
   }
}
