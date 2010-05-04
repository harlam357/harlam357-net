
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;

namespace harlam357.Windows.Forms.Tests
{
   [TestFixture]
   public class UpdatePresenterTests
   {
      private ApplicationUpdate _update;

      [SetUp]
      public void Init()
      {
         UpdateChecker checker = new UpdateChecker();
         _update = checker.CheckForUpdate("uniqueId", Path.Combine(Environment.CurrentDirectory, "ApplicationUpdateLocal.xml"));
      }
   
      [Test]
      public void DownloadClickTest()
      {
         MockRepository mocks = new MockRepository();
         IUpdateView updateView = mocks.DynamicMock<IUpdateView>();
         ISaveFileView saveFileView = mocks.Stub<ISaveFileView>();
         Expect.Call(saveFileView.ShowView()).Return(true);
         IMessageBoxView messageBoxView = mocks.DynamicMock<IMessageBoxView>();

         mocks.ReplayAll();

         UpdatePresenter presenter = new UpdatePresenter(new Form(), LogException, _update, null, 
                                                         updateView, saveFileView, messageBoxView);
         presenter.DownloadClick(0, PerformDownload);
         
         Assert.AreSame(_update.UpdateFiles[0], presenter.SelectedUpdate);
         Assert.AreEqual("TestFile.txt", presenter.LocalFilePath);
         
         mocks.VerifyAll();
      }

      [Test]
      public void DownloadClickFailedTest()
      {
         MockRepository mocks = new MockRepository();
         IUpdateView updateView = mocks.DynamicMock<IUpdateView>();
         ISaveFileView saveFileView = mocks.Stub<ISaveFileView>();
         Expect.Call(saveFileView.ShowView()).Return(true);
         IMessageBoxView messageBoxView = mocks.DynamicMock<IMessageBoxView>();
         Expect.Call(() => messageBoxView.ShowError(new Form(), "text", "caption")).IgnoreArguments();

         mocks.ReplayAll();

         UpdatePresenter presenter = new UpdatePresenter(new Form(), LogException, _update, null,
                                                         updateView, saveFileView, messageBoxView);
         presenter.DownloadClick(0, PerformDownloadException);

         Assert.AreSame(_update.UpdateFiles[0], presenter.SelectedUpdate);
         Assert.AreEqual("TestFile.txt", presenter.LocalFilePath);

         mocks.VerifyAll();
      }

      [Test]
      public void PerformDownloadTest()
      {
         MockRepository mocks = new MockRepository();
         IUpdateView updateView = mocks.DynamicMock<IUpdateView>();
         ISaveFileView saveFileView = mocks.DynamicMock<ISaveFileView>();
         IMessageBoxView messageBoxView = mocks.DynamicMock<IMessageBoxView>();

         mocks.ReplayAll();

         UpdatePresenter presenter = new UpdatePresenter(new Form(), LogException, _update, null,
                                                         updateView, saveFileView, messageBoxView);
         string localFilePath = Path.Combine(Path.GetTempPath(), UpdatePresenter.GetFileNameFromUrl(_update.UpdateFiles[0].HttpAddress));
         presenter.PerformDownload(Path.GetFullPath(_update.UpdateFiles[0].HttpAddress), localFilePath);
         
         mocks.VerifyAll();
      }

      

      private static void PerformDownload(string url)
      {

      }

      private static void PerformDownloadException(string url)
      {
         throw new WebException("Download failed.");
      }

      private static void LogException(Exception ex)
      {

      }
   }
}
