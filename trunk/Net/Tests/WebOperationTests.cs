/*
 * harlam357.Net - Web Operation Class Tests
 * Copyright (C) 2009-2010 Ryan Harlamert (harlam357)
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
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;

using NUnit.Framework;
using Rhino.Mocks;

namespace harlam357.Net.Tests
{
   [TestFixture]
   public class WebOperationTests
   {
      private readonly string TestFilesFolder = String.Format(CultureInfo.InvariantCulture, "..{0}..{0}TestFiles", Path.DirectorySeparatorChar);
      private readonly string TestFilesWorkFolder = String.Format(CultureInfo.InvariantCulture, "..{0}..{0}TestFiles{0}Work", Path.DirectorySeparatorChar);
      
      private MockRepository mocks;

      [SetUp]
      public void Init()
      {
         DirectoryInfo di = new DirectoryInfo(TestFilesWorkFolder);
         if (di.Exists)
         {
            di.Delete(true);
         }

         di.Create();

         mocks = new MockRepository();
      }

      [TearDown]
      public void CleanUp()
      {
         DirectoryInfo di = new DirectoryInfo(TestFilesWorkFolder);
         if (di.Exists)
         {
            di.Delete(true);
         }
      }
   
      [Test]
      public void DownloadTest()
      {
         WebOperation web = WebOperation.Create(Path.GetFullPath(Path.Combine(TestFilesFolder, "test.html")));
         web.AutoSizeBuffer = true;
         web.Buffer = 2048;
         web.OperationRequest.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
         web.OperationRequest.Timeout = 10000;
         
         string tempPath = Path.Combine(Path.GetTempPath(), "downloadtest.html");
         Assert.AreEqual(WebOperationState.Idle, web.State);
         web.Download(tempPath);
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.IsTrue(File.Exists(tempPath));
         
         Assert.AreEqual(true, web.AutoSizeBuffer);
         Assert.AreEqual(1024, web.Buffer);
         Assert.AreEqual(RequestCacheLevel.NoCacheNoStore, web.OperationRequest.CachePolicy.Level);
         Assert.AreEqual(10000, web.OperationRequest.Timeout);
      }

      [Test]
      public void UploadTest()
      {
         string UploadPath = Path.GetFullPath(Path.Combine(TestFilesWorkFolder, "upload.html"));
         WebOperation web = WebOperation.Create(UploadPath);
         string LocalFilePath = Path.Combine(TestFilesFolder, "test.html");
         web.Upload(LocalFilePath);

         Assert.IsTrue(File.Exists(UploadPath));
      }

      [Test]
      public void UploadMaximumLengthTest()
      {
         string UploadPath = Path.GetFullPath(Path.Combine(TestFilesWorkFolder, "upload.txt"));
         WebOperation web = WebOperation.Create(UploadPath);
         string LocalFilePath = Path.Combine(TestFilesFolder, "FAHlog.txt");
         web.Upload(LocalFilePath, 102400);

         Assert.IsTrue(File.Exists(UploadPath));
         FileInfo fi = new FileInfo(UploadPath);
         Assert.AreEqual(102400, fi.Length);
      }

      [Test]
      public void GetDownloadLengthTest()
      {
         WebOperation web = WebOperation.Create(Path.GetFullPath(Path.Combine(TestFilesFolder, "test.html")));
         Assert.AreEqual(139, web.GetDownloadLength());
      }

      [Test]
      public void CheckConnectionTest()
      {
         WebOperation web = WebOperation.Create(Path.GetFullPath(Path.Combine(TestFilesFolder, "test.html")));
         web.CheckConnection();

         web = WebOperation.Create(Path.GetFullPath(Path.Combine(TestFilesFolder, "not_found.html")));

         try
         {
            web.CheckConnection();
            Assert.Fail();
         }
         catch (WebException)
         { }

         web = WebOperation.Create("ftp://a1g.gfdasfsafasdfasfsdafasdfasdf.c1321");

         try
         {
            web.CheckConnection();
            Assert.Fail();
         }
         catch (WebException)
         { }

         web = WebOperation.Create("http://a1g.gfdasfsafasdfasfsdafasdfasdf.c1321");

         try
         {
            web.CheckConnection();
            Assert.Fail();
         }
         catch (WebException)
         { }
      }
      
      [Test]
      public void WebOperationDownload()
      {
         WebRequest Request = mocks.DynamicMock<WebRequest>();
         WebResponse Response = mocks.DynamicMock<WebResponse>();
         FileWebOperation web = new FileWebOperation(new WebOperationRequest(Request));

         Expect.Call(Request.GetResponse()).Return(Response);

         using (FileStream stream = new FileStream(Path.Combine(TestFilesFolder, "test.html"), FileMode.Open))
         {
            Expect.Call(Response.GetResponseStream()).Return(stream);
            mocks.ReplayAll();

            web.Download(Path.Combine(TestFilesWorkFolder, "downloadtest.html"));

            mocks.VerifyAll();
         }

         Assert.IsTrue(File.Exists(Path.Combine(TestFilesWorkFolder, "downloadtest.html")));
      }
      
      [Test]
      public void WebOperationUpload()
      {
         WebRequest Request = mocks.DynamicMock<WebRequest>();
         WebResponse Response = mocks.DynamicMock<WebResponse>();
         FileWebOperation web = new FileWebOperation(new WebOperationRequest(Request));

         Expect.Call(Request.GetResponse()).Return(Response);

         using (FileStream stream = new FileStream(Path.Combine(TestFilesWorkFolder, "upload.html"), FileMode.Create))
         {
            Expect.Call(Request.GetRequestStream()).Return(stream);
            mocks.ReplayAll();

            web.Upload(Path.Combine(TestFilesFolder, "test.html"));

            mocks.VerifyAll();
         }

         Assert.IsTrue(File.Exists(Path.Combine(TestFilesWorkFolder, "upload.html")));
      }
   }
}
