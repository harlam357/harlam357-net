/*
 * harlam357.Net - Application Update Presenter Tests
 * Copyright (C) 2010 Ryan Harlamert (harlam357)
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
      
      [Test]
      public void VerifyDownloadTest()
      {
         UpdatePresenter.VerifyDownload("..\\..\\TestFile.txt", _update.UpdateFiles[0]);
      }
   }
}
