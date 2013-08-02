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

using harlam357.Net;

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

      private MockRepository _mocks;
      private IUpdateView _updateView;
      private ISaveFileDialogView _saveFileView;
      
      [SetUp]
      public void Init()
      {
         _mocks = new MockRepository();
         _updateView = _mocks.DynamicMock<IUpdateView>();
         _saveFileView = _mocks.DynamicMock<ISaveFileDialogView>();
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
         Expect.Call(_saveFileView.ShowDialog()).Return(DialogResult.Cancel);

         _mocks.ReplayAll();

         var presenter = new UpdatePresenter(null, _update, null, _updateView, _saveFileView, null);

         Assert.IsNull(presenter.SelectedUpdate);
         Assert.IsNull(presenter.LocalFilePath);
         Assert.AreEqual(false, presenter.UpdateReady);                                          
         
         presenter.DownloadClick(0);

         Assert.IsNull(presenter.SelectedUpdate);
         Assert.IsNull(presenter.LocalFilePath);
         Assert.AreEqual(false, presenter.UpdateReady);       
         
         _mocks.VerifyAll(); 
      }

      [Test]
      public void DownloadClick_CancelDownload_Test()
      {
         Expect.Call(_saveFileView.ShowDialog()).Return(DialogResult.OK);
      
         Expect.Call(() => _updateView.SetSelectDownloadLabelText(String.Empty)).IgnoreArguments();
         Expect.Call(() => _updateView.SetDownloadButtonEnabled(false));
         Expect.Call(() => _updateView.SetUpdateComboBoxVisible(false));
         Expect.Call(() => _updateView.SetDownloadProgressValue(0));
         Expect.Call(() => _updateView.SetDownloadProgressVisisble(true));

         var webOperation = _mocks.DynamicMock<IWebOperation>();
         Expect.Call(() => webOperation.Download(String.Empty)).IgnoreArguments().Do(new Action<string>(DownloadSleep));
         Expect.Call(webOperation.State).Return(WebOperationState.InProgress);
         Expect.Call(webOperation.Cancel);
         Expect.Call(webOperation.Result).Return(WebOperationResult.Canceled);

         Expect.Call(() => _updateView.SetSelectDownloadLabelTextDefault());
         Expect.Call(() => _updateView.SetDownloadButtonEnabled(true));
         Expect.Call(() => _updateView.SetUpdateComboBoxVisible(true));
         Expect.Call(() => _updateView.SetDownloadProgressVisisble(false));

         _mocks.ReplayAll();

         var presenter = new UpdatePresenter(null, _update, null, _updateView, _saveFileView, webOperation);

         var are = new AutoResetEvent(false);
         presenter.DownloadFinished += delegate { are.Set(); };
         presenter.DownloadClick(0);
         presenter.CancelClick();

         // Wait until the event handler is invoked
         if (!(are.WaitOne(5000, false)))
         {
            Assert.Fail("Test timed out.");
         }

         _mocks.VerifyAll();
      }
      
      private static void DownloadSleep(string localFilePath)
      {
         Thread.Sleep(1000);
      }

      [Test]
      public void DownloadClick_DownloadException_Test()
      {
         Expect.Call(_saveFileView.ShowDialog()).Return(DialogResult.OK);

         Expect.Call(() => _updateView.SetSelectDownloadLabelText(String.Empty)).IgnoreArguments();
         Expect.Call(() => _updateView.SetDownloadButtonEnabled(false));
         Expect.Call(() => _updateView.SetUpdateComboBoxVisible(false));
         Expect.Call(() => _updateView.SetDownloadProgressValue(0));
         Expect.Call(() => _updateView.SetDownloadProgressVisisble(true));

         var webOperation = _mocks.DynamicMock<IWebOperation>();
         Expect.Call(() => webOperation.Download(String.Empty)).IgnoreArguments().Do(new Action<string>(DownloadException));

         Expect.Call(() => _updateView.ShowErrorMessage(String.Empty)).IgnoreArguments();
         Expect.Call(() => _updateView.SetSelectDownloadLabelTextDefault());
         Expect.Call(() => _updateView.SetDownloadButtonEnabled(true));
         Expect.Call(() => _updateView.SetUpdateComboBoxVisible(true));
         Expect.Call(() => _updateView.SetDownloadProgressVisisble(false));

         _mocks.ReplayAll();

         var presenter = new UpdatePresenter(null, _update, null, _updateView, _saveFileView, webOperation);

         var are = new AutoResetEvent(false);
         presenter.DownloadFinished += delegate { are.Set(); };
         presenter.DownloadClick(0);

         // Wait until the event handler is invoked
         if (!(are.WaitOne(5000, false)))
         {
            Assert.Fail("Test timed out.");
         }

         _mocks.VerifyAll();
      }

      private static void DownloadException(string localFilePath)
      {
         throw new WebException("Failed to download " + localFilePath);
      }

      [Test]
      public void Show_Test()
      {
         Expect.Call(() => _updateView.ShowView(null));

         _mocks.ReplayAll();

         var presenter = new UpdatePresenter(null, _update, null, _updateView, _saveFileView, null);
         presenter.Show(null);

         _mocks.VerifyAll();
      }

      [Test]
      public void Cancel_Test()
      {
         Expect.Call(() => _updateView.CloseView());

         _mocks.ReplayAll();

         var presenter = new UpdatePresenter(null, _update, null, _updateView, _saveFileView, null);
         presenter.CancelClick();

         _mocks.VerifyAll();
      }     

      [Test]
      public void VerifyDownload_Test()
      {
         UpdatePresenter.VerifyDownload("..\\..\\TestFile.txt", _update.UpdateFiles[0]);
      }
   }
}
