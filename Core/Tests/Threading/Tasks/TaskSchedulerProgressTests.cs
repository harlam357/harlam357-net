
using System;

using NUnit.Framework;

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
         new TaskSchedulerProgress<ProgressInfo>(new CurrentThreadTaskScheduler(), null);
      }

      [Test]
      public void TaskSchedulerProgress_Report_Test1()
      {
         int progressPercentage = 0;
         string message = null;
         object userState = null;

         Action<ProgressInfo> action = args =>
         {
            progressPercentage = args.ProgressPercentage;
            message = args.Message;
            userState = args.UserState;
         };
         IProgress<ProgressInfo> progress = new TaskSchedulerProgress<ProgressInfo>(new CurrentThreadTaskScheduler(), action);
         var progressState = new object();
         progress.Report(new ProgressInfo(50, "Message", progressState));

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

         var progress = new TaskSchedulerProgress<ProgressInfo>(new CurrentThreadTaskScheduler());
         ((IProgress<ProgressInfo>)progress).Report(new ProgressInfo(50, "Message", new object()));

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
         ProgressHandler<ProgressInfo> handler = (sender, args) =>
#else
         EventHandler<ProgressInfo> handler = (sender, args) =>
#endif
         {
            progressPercentage = args.ProgressPercentage;
            message = args.Message;
            userState = args.UserState;
         };
         var progress = new TaskSchedulerProgress<ProgressInfo>(new CurrentThreadTaskScheduler());
         progress.ProgressChanged += handler;
         var progressState = new object();
         ((IProgress<ProgressInfo>)progress).Report(new ProgressInfo(50, "Message", progressState));

         Assert.AreEqual(50, progressPercentage);
         Assert.AreEqual("Message", message);
         Assert.AreSame(progressState, userState);
      }
   }
}
