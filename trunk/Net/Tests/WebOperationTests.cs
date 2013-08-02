/*
 * harlam357.Net - Web Operation Class Tests
 * Copyright (C) 2009-2013 Ryan Harlamert (harlam357)
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

using NUnit.Framework;

namespace harlam357.Net.Tests
{
   [TestFixture]
   public class WebOperationTests
   {
      private static string TestFilesFolder
      {
         get { return Path.GetFullPath(String.Format(CultureInfo.InvariantCulture, "..{0}..{0}TestFiles", Path.DirectorySeparatorChar)); }
      }

      private static string TestFilesWorkFolder
      {
         get { return Path.GetFullPath(String.Format(CultureInfo.InvariantCulture, "..{0}..{0}TestFiles{0}Work", Path.DirectorySeparatorChar)); }
      }

      [SetUp]
      public static void Init()
      {
         var di = new DirectoryInfo(TestFilesWorkFolder);
         di.Create();
      }

      [Test]
      public void WebOperation_WebRequest_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         Assert.IsNotNull(web.WebRequest);
         //var webRequest = MockRepository.GenerateMock<IWebRequest>();
         //web.WebRequest = webRequest;
         //Assert.AreSame(webRequest, web.WebRequest);
      }

      [Test]
      public void WebOperation_State_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         Assert.AreEqual(WebOperationState.Idle, web.State);
      }

      [Test]
      public void WebOperation_Result_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         Assert.AreEqual(WebOperationResult.None, web.Result);
      }

      [Test]
      public void WebOperation_AutoSizeBuffer_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         Assert.AreEqual(true, web.AutoSizeBuffer);
         web.AutoSizeBuffer = false;
         Assert.AreEqual(false, web.AutoSizeBuffer);
      }

      [Test]
      public void WebOperation_Buffer_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         Assert.AreEqual(1024, web.Buffer);
         web.Buffer = 2048;
         Assert.AreEqual(2048, web.Buffer);
      }

      [Test]
      public void WebOperation_Download_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.None, web.Result);

         string fileName = Path.Combine(TestFilesWorkFolder, "downloadtest.html");
         int count = 0;
         web.ProgressChanged += (s, e) =>
                                {
                                   if (count == 0)
                                   {
                                      // try and execute another download while the first is in progress
                                      // the call will simply return performing no additional action
                                      web.Download(fileName);
                                   }
                                   if (count < 2)
                                   {
                                      Assert.AreEqual(139, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.InProgress, e.State);
                                      Assert.AreEqual(WebOperationResult.None, e.Result);
                                      Assert.AreEqual(WebOperationState.InProgress, web.State);
                                      Assert.AreEqual(WebOperationResult.None, web.Result);
                                   }
                                   if (count == 2)
                                   {
                                      Assert.AreEqual(139, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.Idle, e.State);
                                      Assert.AreEqual(WebOperationResult.Completed, e.Result);
                                      Assert.AreEqual(WebOperationState.Idle, web.State);
                                      Assert.AreEqual(WebOperationResult.Completed, web.Result);
                                   }
                                   count++;
                                };

         // Act - execute the first download
         web.Download(fileName);
         // Assert
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.Completed, web.Result);
         // file should be on disk
         Assert.IsTrue(File.Exists(fileName));
      }

      [Test]
      public void WebOperation_Download_Cancel_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.None, web.Result);

         string fileName = Path.Combine(TestFilesWorkFolder, "downloadtest.html");
         int count = 0;
         web.ProgressChanged += (s, e) =>
                                {
                                   if (count == 0)
                                   {
                                      Assert.AreEqual(139, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.InProgress, e.State);
                                      Assert.AreEqual(WebOperationResult.None, e.Result);
                                      Assert.AreEqual(WebOperationState.InProgress, web.State);
                                      Assert.AreEqual(WebOperationResult.None, web.Result);
                                      // cancel the download while in progress
                                      web.Cancel();
                                   }
                                   if (count == 1)
                                   {
                                      Assert.AreEqual(139, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.Idle, e.State);
                                      Assert.AreEqual(WebOperationResult.Canceled, e.Result);
                                      Assert.AreEqual(WebOperationState.Idle, web.State);
                                      Assert.AreEqual(WebOperationResult.Canceled, web.Result);
                                   }
                                   count++;
                                };

         // Act
         web.Download(fileName);
         // Assert
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.Canceled, web.Result);
         // file should NOT be on disk
         Assert.IsFalse(File.Exists(fileName));
      }

      //[Test]
      //public void WebOperation_Download_Expectations_Test()
      //{
      //   var request = MockRepository.GenerateMock<IWebRequest>();
      //   var response = MockRepository.GenerateMock<IWebResponse>();
      //   request.Expect(x => x.GetResponse()).Return(response);
      //
      //   var web = WebOperation.Create("NetTest.html");
      //   web.WebRequest = request;
      //   using (var stream = new FileStream("NetTest.html", FileMode.Open, FileAccess.Read))
      //   {
      //      response.Expect(x => x.GetResponseStream()).Return(stream);
      //      web.Download(Path.Combine(TestFilesWorkFolder, "downloadtest.html"));
      //   }
      //
      //   Assert.IsTrue(File.Exists(Path.Combine(TestFilesWorkFolder, "downloadtest.html")));
      //
      //   request.VerifyAllExpectations();
      //   response.VerifyAllExpectations();
      //}

      [Test]
      public void WebOperation_GetDownloadLength_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.None, web.Result);

         int count = 0;
         web.ProgressChanged += (s, e) =>
                                {
                                   if (count == 0)
                                   {
                                      Assert.AreEqual(0, e.Length);
                                      Assert.AreEqual(0, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.InProgress, e.State);
                                      Assert.AreEqual(WebOperationResult.None, e.Result);
                                      Assert.AreEqual(WebOperationState.InProgress, web.State);
                                      Assert.AreEqual(WebOperationResult.None, web.Result);
                                      // try and execute another download length get while the first is in progress
                                      // the call will simply return performing no additional action
                                      Assert.AreEqual(0, web.GetDownloadLength());
                                   }
                                   if (count == 1)
                                   {
                                      Assert.AreEqual(0, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.Idle, e.State);
                                      Assert.AreEqual(WebOperationResult.Completed, e.Result);
                                      Assert.AreEqual(WebOperationState.Idle, web.State);
                                      Assert.AreEqual(WebOperationResult.Completed, web.Result);
                                   }
                                   count++;
                                };

         // Act - execute the first download length get
         Assert.AreEqual(139, web.GetDownloadLength());
         // Assert
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.Completed, web.Result);
      }

      [Test]
      public void WebOperation_Upload_Test()
      {
         string requestUriString = Path.Combine(TestFilesWorkFolder, "upload.html");
         WebOperation web = WebOperation.Create(requestUriString);
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.None, web.Result);

         string fileName = Path.Combine(TestFilesFolder, "NetTest.html");
         int count = 0;
         web.ProgressChanged += (s, e) =>
                                {
                                   if (count == 0)
                                   {
                                      // try and execute another upload while the first is in progress
                                      // the call will simply return performing no additional action
                                      web.Upload(fileName);
                                   }
                                   if (count < 2)
                                   {
                                      Assert.AreEqual(139, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.InProgress, e.State);
                                      Assert.AreEqual(WebOperationResult.None, e.Result);
                                      Assert.AreEqual(WebOperationState.InProgress, web.State);
                                      Assert.AreEqual(WebOperationResult.None, web.Result);
                                   }
                                   if (count == 2)
                                   {
                                      Assert.AreEqual(139, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.Idle, e.State);
                                      Assert.AreEqual(WebOperationResult.Completed, e.Result);
                                      Assert.AreEqual(WebOperationState.Idle, web.State);
                                      Assert.AreEqual(WebOperationResult.Completed, web.Result);
                                   }
                                   count++;
                                };

         // Act - execute the first upload
         web.Upload(fileName);
         // Assert
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.Completed, web.Result);
         // file should be at the target location
         Assert.IsTrue(File.Exists(requestUriString));
      }

      [Test]
      public void WebOperation_Upload_Cancel_Test()
      {
         string requestUriString = Path.Combine(TestFilesWorkFolder, "upload.html");
         WebOperation web = WebOperation.Create(requestUriString);
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.None, web.Result);

         string fileName = Path.Combine(TestFilesFolder, "NetTest.html");
         int count = 0;
         web.ProgressChanged += (s, e) =>
                                {
                                   if (count == 0)
                                   {
                                      Assert.AreEqual(139, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.InProgress, e.State);
                                      Assert.AreEqual(WebOperationResult.None, e.Result);
                                      Assert.AreEqual(WebOperationState.InProgress, web.State);
                                      Assert.AreEqual(WebOperationResult.None, web.Result);
                                      // cancel the upload while in progress
                                      web.Cancel();
                                   }
                                   if (count == 1)
                                   {
                                      Assert.AreEqual(139, e.Length);
                                      Assert.AreEqual(139, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.Idle, e.State);
                                      Assert.AreEqual(WebOperationResult.Canceled, e.Result);
                                      Assert.AreEqual(WebOperationState.Idle, web.State);
                                      Assert.AreEqual(WebOperationResult.Canceled, web.Result);
                                   }
                                   count++;
                                };

         // Act
         web.Upload(fileName);
         // Assert
         Assert.AreEqual(WebOperationState.Idle, web.State);
         Assert.AreEqual(WebOperationResult.Canceled, web.Result);
         // file should NOT be at the target location
         Assert.IsFalse(File.Exists(requestUriString));
      }

      [Test]
      public void WebOperation_Upload_MaximumLength_Test1()
      {
         string requestUriString = Path.Combine(TestFilesWorkFolder, "upload.txt");
         WebOperation web = WebOperation.Create(requestUriString);
         string fileName = Path.Combine(TestFilesFolder, "NetTest.txt");
         // Act - LESS than max file size
         web.Upload(fileName, 2048);
         // Assert
         // file should be at the target location
         Assert.IsTrue(File.Exists(requestUriString));
         var fi = new FileInfo(requestUriString);
         Assert.AreEqual(2048, fi.Length);
      }

      [Test]
      public void WebOperation_Upload_MaximumLength_Test2()
      {
         string requestUriString = Path.Combine(TestFilesWorkFolder, "upload.txt");
         WebOperation web = WebOperation.Create(requestUriString);
         string fileName = Path.Combine(TestFilesFolder, "NetTest.txt");
         // Act - GREATER than max file size
         web.Upload(fileName, 8192);
         // Assert
         // file should be at the target location
         Assert.IsTrue(File.Exists(requestUriString));
         var fi = new FileInfo(requestUriString);
         Assert.AreEqual(4096, fi.Length);
      }

      //[Test]
      //public void WebOperation_Upload_Expectations_Test()
      //{
      //   var request = MockRepository.GenerateMock<IWebRequest>();
      //   var response = MockRepository.GenerateMock<IWebResponse>();
      //   request.Expect(x => x.GetResponse()).Return(response);
      //
      //   var web = WebOperation.Create(Path.Combine(TestFilesWorkFolder, "upload.html"));
      //   web.WebRequest = request;
      //   using (var stream = new FileStream(Path.Combine(TestFilesWorkFolder, "upload.html"), FileMode.Create, FileAccess.Write))
      //   {
      //      request.Expect(x => x.GetRequestStream()).Return(stream);
      //      web.Upload("NetTest.html");
      //   }
      //
      //   Assert.IsTrue(File.Exists(Path.Combine(TestFilesWorkFolder, "upload.html")));
      //
      //   request.VerifyAllExpectations();
      //   response.VerifyAllExpectations();
      //}

      [Test]
      public void WebOperation_CheckConnection_File_Test()
      {
         WebOperation web = WebOperation.Create(Path.Combine(TestFilesFolder, "NetTest.html"));
         int count = 0;
         web.ProgressChanged += (s, e) =>
                                {
                                   if (count == 0)
                                   {
                                      Assert.AreEqual(0, e.Length);
                                      Assert.AreEqual(0, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.InProgress, e.State);
                                      Assert.AreEqual(WebOperationResult.None, e.Result);
                                      Assert.AreEqual(WebOperationState.InProgress, web.State);
                                      Assert.AreEqual(WebOperationResult.None, web.Result);
                                      // try and execute another connection check while the first is in progress
                                      // the call will simply return performing no additional action
                                      web.CheckConnection();
                                   }
                                   if (count == 1)
                                   {
                                      Assert.AreEqual(0, e.Length);
                                      Assert.AreEqual(0, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.Idle, e.State);
                                      Assert.AreEqual(WebOperationResult.Completed, e.Result);
                                      Assert.AreEqual(WebOperationState.Idle, web.State);
                                      Assert.AreEqual(WebOperationResult.Completed, web.Result);
                                   }
                                   count++;
                                };
         // Act - execute the connection check
         web.CheckConnection();
      }

      [Test]
      [ExpectedException(typeof(WebException))]
      public void WebOperation_CheckConnection_File_NotExist_Test()
      {
         WebOperation web = WebOperation.Create(Path.GetFullPath("not_found.html"));
         int count = 0;
         web.ProgressChanged += (s, e) =>
                                {
                                   if (count == 0)
                                   {
                                      Assert.AreEqual(0, e.Length);
                                      Assert.AreEqual(0, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.InProgress, e.State);
                                      Assert.AreEqual(WebOperationResult.None, e.Result);
                                      Assert.AreEqual(WebOperationState.InProgress, web.State);
                                      Assert.AreEqual(WebOperationResult.None, web.Result);
                                   }
                                   if (count == 1)
                                   {
                                      Assert.AreEqual(0, e.Length);
                                      Assert.AreEqual(0, e.TotalLength);
                                      Assert.AreEqual(WebOperationState.Idle, e.State);
                                      Assert.AreEqual(WebOperationResult.None, e.Result);
                                      Assert.AreEqual(WebOperationState.Idle, web.State);
                                      Assert.AreEqual(WebOperationResult.None, web.Result);
                                   }
                                   count++;
                                };
         // Act - execute the connection check
         web.CheckConnection();
      }

      [Test]
      [ExpectedException(typeof(WebException))]
      public void WebOperation_CheckConnection_Ftp_NotExist_Test()
      {
         WebOperation web = WebOperation.Create("ftp://a1g.gfdasfsafasdfasfsdafasdfasdf.c1321");
         web.CheckConnection();
      }

      // ignore because some ISPs return a web page when an invalid web address is given
      [Test, Ignore]
      [ExpectedException(typeof(WebException))]
      public void WebOperation_CheckConnection_Http_NotExist_Test()
      {
         WebOperation web = WebOperation.Create("http://a1g.gfdasfsafasdfasfsdafasdfasdf.c1321");
         web.CheckConnection();
      }
   }
}
