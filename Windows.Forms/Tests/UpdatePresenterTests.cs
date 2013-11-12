/*
 * harlam357.Net - Application Update Presenter Tests
 * Copyright (C) 2010-2013 Ryan Harlamert (harlam357)
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;
using Rhino.Mocks;

using harlam357.Core.Net;

namespace harlam357.Windows.Forms.Tests
{
   [TestFixture]
   public class UpdatePresenterTests
   {
      private readonly ApplicationUpdate _update;
      
      public UpdatePresenterTests()
      {
         var checker = new UpdateChecker();
         _update = checker.CheckForUpdate("uniqueId", Path.Combine(Environment.CurrentDirectory, "ApplicationUpdateLocal.xml"));
      }

      [Test]
      public void DownloadClick_Test()
      {
         var updateView = MockRepository.GenerateMock<IUpdateView>();
         var saveFileView = MockRepository.GenerateMock<ISaveFileDialogView>();

         string localFilePath = Path.Combine(Environment.CurrentDirectory, "TestFileDownloaded.txt");
         saveFileView.Expect(x => x.ShowDialog()).Return(DialogResult.OK);
         saveFileView.Stub(x => x.FileName).Return(localFilePath);

         updateView.Expect(x => x.SetSelectDownloadLabelText(String.Empty)).IgnoreArguments();
         updateView.Expect(x => x.SetDownloadButtonEnabled(false));
         updateView.Expect(x => x.SetUpdateComboBoxVisible(false));
         updateView.Expect(x => x.SetDownloadProgressValue(0));
         updateView.Expect(x => x.SetDownloadProgressVisisble(true));
         updateView.Expect(x => x.CloseView());

         // fixup the address to look in the running folder
         _update.UpdateFiles[0].HttpAddress = Path.Combine(Environment.CurrentDirectory, _update.UpdateFiles[0].HttpAddress);
         var presenter = new UpdatePresenter(null, _update, null, updateView, saveFileView, null);

         Assert.IsNull(presenter.SelectedUpdate);
         Assert.IsNull(presenter.LocalFilePath);
         Assert.AreEqual(false, presenter.UpdateReady);
         
         var are = new AutoResetEvent(false);
         presenter.DownloadFinished += delegate { are.Set(); };
         presenter.DownloadClick(0);

         // Wait until the event handler is invoked
         if (!(are.WaitOne(5000, false)))
         {
            Assert.Fail("Test timed out.");
         }  
         
         Assert.AreSame(_update.UpdateFiles[0], presenter.SelectedUpdate);
         Assert.AreEqual(localFilePath, presenter.LocalFilePath);
         Assert.AreEqual(true, presenter.UpdateReady);
         
         updateView.VerifyAllExpectations();
         saveFileView.VerifyAllExpectations();
      }
      
      [Test]
      public void DownloadClick_SaveFileDialogCanceled_Test()
      {
         var updateView = MockRepository.GenerateStub<IUpdateView>();
         var saveFileView = MockRepository.GenerateMock<ISaveFileDialogView>();

         saveFileView.Expect(x => x.ShowDialog()).Return(DialogResult.Cancel);

         var presenter = new UpdatePresenter(null, _update, null, updateView, saveFileView, null);

         Assert.IsNull(presenter.SelectedUpdate);
         Assert.IsNull(presenter.LocalFilePath);
         Assert.AreEqual(false, presenter.UpdateReady);                                          
         
         presenter.DownloadClick(0);

         Assert.IsNull(presenter.SelectedUpdate);
         Assert.IsNull(presenter.LocalFilePath);
         Assert.AreEqual(false, presenter.UpdateReady);       
         
         saveFileView.VerifyAllExpectations();
      }

      [Test]
      public void DownloadClick_CancelDownload_Test()
      {
         var updateView = MockRepository.GenerateMock<IUpdateView>();
         var saveFileView = MockRepository.GenerateMock<ISaveFileDialogView>();

         saveFileView.Expect(x => x.ShowDialog()).Return(DialogResult.OK);
      
         updateView.Expect(x => x.SetSelectDownloadLabelText(String.Empty)).IgnoreArguments();
         updateView.Expect(x => x.SetDownloadButtonEnabled(false));
         updateView.Expect(x => x.SetUpdateComboBoxVisible(false));
         updateView.Expect(x => x.SetDownloadProgressValue(0));
         updateView.Expect(x => x.SetDownloadProgressVisisble(true));

         var webOperation = MockRepository.GenerateMock<IWebOperation>();
         webOperation.Expect(x => x.Download(String.Empty)).IgnoreArguments().Do(new Action<string>(DownloadSleep));
         webOperation.Expect(x => x.State).Return(WebOperationState.InProgress);
         webOperation.Expect(x => x.Cancel());
         webOperation.Expect(x => x.Result).Return(WebOperationResult.Canceled);

         updateView.Expect(x => x.SetSelectDownloadLabelTextDefault());
         updateView.Expect(x => x.SetDownloadButtonEnabled(true));
         updateView.Expect(x => x.SetUpdateComboBoxVisible(true));
         updateView.Expect(x => x.SetDownloadProgressVisisble(false));

         var presenter = new UpdatePresenter(null, _update, null, updateView, saveFileView, webOperation);

         var are = new AutoResetEvent(false);
         presenter.DownloadFinished += delegate { are.Set(); };
         presenter.DownloadClick(0);
         presenter.CancelClick();

         // Wait until the event handler is invoked
         if (!(are.WaitOne(5000, false)))
         {
            Assert.Fail("Test timed out.");
         }

         updateView.VerifyAllExpectations();
         saveFileView.VerifyAllExpectations();
         webOperation.VerifyAllExpectations();
      }
      
      private static void DownloadSleep(string localFilePath)
      {
         Thread.Sleep(1000);
      }

      [Test]
      public void DownloadClick_DownloadException_Test()
      {
         var updateView = MockRepository.GenerateMock<IUpdateView>();
         var saveFileView = MockRepository.GenerateMock<ISaveFileDialogView>();

         saveFileView.Expect(x => x.ShowDialog()).Return(DialogResult.OK);

         updateView.Expect(x => x.SetSelectDownloadLabelText(String.Empty)).IgnoreArguments();
         updateView.Expect(x => x.SetDownloadButtonEnabled(false));
         updateView.Expect(x => x.SetUpdateComboBoxVisible(false));
         updateView.Expect(x => x.SetDownloadProgressValue(0));
         updateView.Expect(x => x.SetDownloadProgressVisisble(true));

         var webOperation = MockRepository.GenerateMock<IWebOperation>();
         webOperation.Expect(x => x.Download(String.Empty)).IgnoreArguments().Do(new Action<string>(DownloadException));

         updateView.Expect(x => x.ShowErrorMessage(String.Empty)).IgnoreArguments();
         updateView.Expect(x => x.SetSelectDownloadLabelTextDefault());
         updateView.Expect(x => x.SetDownloadButtonEnabled(true));
         updateView.Expect(x => x.SetUpdateComboBoxVisible(true));
         updateView.Expect(x => x.SetDownloadProgressVisisble(false));

         var presenter = new UpdatePresenter(null, _update, null, updateView, saveFileView, webOperation);

         var are = new AutoResetEvent(false);
         presenter.DownloadFinished += delegate { are.Set(); };
         presenter.DownloadClick(0);

         // Wait until the event handler is invoked
         if (!(are.WaitOne(5000, false)))
         {
            Assert.Fail("Test timed out.");
         }

         updateView.VerifyAllExpectations();
         saveFileView.VerifyAllExpectations();
         webOperation.VerifyAllExpectations();
      }

      private static void DownloadException(string localFilePath)
      {
         throw new WebException("Failed to download " + localFilePath);
      }

      [Test]
      public void Show_Test()
      {
         var updateView = MockRepository.GenerateMock<IUpdateView>();
         var saveFileView = MockRepository.GenerateStub<ISaveFileDialogView>();

         updateView.Expect(x => x.ShowView(null));

         var presenter = new UpdatePresenter(null, _update, null, updateView, saveFileView, null);
         presenter.Show(null);

         updateView.VerifyAllExpectations();
      }

      [Test]
      public void Cancel_Test()
      {
         var updateView = MockRepository.GenerateMock<IUpdateView>();
         var saveFileView = MockRepository.GenerateStub<ISaveFileDialogView>();

         updateView.Expect(x => x.CloseView());

         var presenter = new UpdatePresenter(null, _update, null, updateView, saveFileView, null);
         presenter.CancelClick();

         updateView.VerifyAllExpectations();
      }     

      [Test]
      public void VerifyDownload_Test()
      {
         UpdatePresenter.VerifyDownload("..\\..\\TestFile.txt", _update.UpdateFiles[0]);
      }
   }
}
