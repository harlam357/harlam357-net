
using System;

using NUnit.Framework;

using harlam357.Core.ComponentModel;

namespace harlam357.Core.Threading.Tasks
{
   [TestFixture]
   public class TaskSchedulerProgressTests
   {
      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void TaskSchedulerProgress_ArgumentNullException_Test()
      {
         // ReSharper disable once ObjectCreationAsStatement
         new TaskSchedulerProgress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler(), null);
      }

      [Test]
      public void TaskSchedulerProgress_Report_Test1()
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
         IProgress<ProgressChangedEventArgs> progress = new TaskSchedulerProgress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler(), action);
         var progressState = new object();
         progress.Report(new ProgressChangedEventArgs(50, "Message", progressState));

         Assert.AreEqual(50, progressPercentage);
         Assert.AreEqual("Message", message);
         Assert.AreSame(progressState, userState);
      }

      [Test]
      public void TaskSchedulerProgress_Report_Test2()
      {
         int progressPercentage = 0;
         string message = null;
         object userState = null;

         var progress = new TaskSchedulerProgress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler());
         ((IProgress<ProgressChangedEventArgs>)progress).Report(new ProgressChangedEventArgs(50, "Message", new object()));

         Assert.AreEqual(0, progressPercentage);
         Assert.IsNull(message);
         Assert.IsNull(userState);
      }

      [Test]
      public void TaskSchedulerProgress_Report_Test3()
      {
         int progressPercentage = 0;
         string message = null;
         object userState = null;

#if NET40
         ProgressHandler<ProgressChangedEventArgs> handler = (sender, args) =>
#else
         EventHandler<ProgressChangedEventArgs> handler = (sender, args) =>
#endif
         {
            progressPercentage = args.ProgressPercentage;
            message = args.Message;
            userState = args.UserState;
         };
         var progress = new TaskSchedulerProgress<ProgressChangedEventArgs>(new CurrentThreadTaskScheduler());
         progress.ProgressChanged += handler;
         var progressState = new object();
         ((IProgress<ProgressChangedEventArgs>)progress).Report(new ProgressChangedEventArgs(50, "Message", progressState));

         Assert.AreEqual(50, progressPercentage);
         Assert.AreEqual("Message", message);
         Assert.AreSame(progressState, userState);
      }
   }
}
