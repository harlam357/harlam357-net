/*
 * harlam357.Net - Web Operation Class
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

namespace harlam357.Net
{
   public enum WebOperationState
   {
      Idle,
      InProgress,
      Finished
   }

   public abstract class WebOperation : IWebOperation
   {
      private const int DefaultBufferSize = 1024;
   
      public event EventHandler<WebOperationProgressEventArgs> WebOperationProgress;
      
      private IWebOperationRequest _OperationRequest;
      public IWebOperationRequest OperationRequest
      {
         get { return _OperationRequest; }
         protected set { _OperationRequest = value; }
      }

      private WebOperationState _State = WebOperationState.Idle;
      public WebOperationState State
      {
         get { return _State; }
         protected set { _State = value; }
      }
      
      private bool _AutoSizeBuffer = true;
      public bool AutoSizeBuffer
      {
         get { return _AutoSizeBuffer; }
         set { _AutoSizeBuffer = value; }
      }

      private int _Buffer = DefaultBufferSize;
      public int Buffer
      {
         get { return _Buffer; }
         set { _Buffer = value; }
      }
      
      public void Download(string LocalFilePath)
      {
         ExecuteDownload(LocalFilePath);
      }
      
      public long GetDownloadLength()
      {
         return ExecuteGetDownloadLength();
      }
      
      protected void ExecuteDownload(string LocalFilePath)
      {
         if (State.Equals(WebOperationState.Idle) == false)
         {
            throw new InvalidOperationException("Web Operation must be Idle.");
         }

         OperationRequest.Request.Method = GetWebDownloadMethod();
         
         long totalBytesRead = 0;
         long totalLength;

         WebResponse response = _OperationRequest.Request.GetResponse();
         using (Stream responseStream = response.GetResponseStream())
         {
            totalLength = response.ContentLength;
            if (AutoSizeBuffer)
            {
               Buffer = CalculateBufferSize(totalLength);
            }

            using (Stream fileStream = File.Create(LocalFilePath))
            {
               byte[] buffer = new byte[Buffer];
               int bytesRead;

               do
               {
                  bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                  fileStream.Write(buffer, 0, bytesRead);
                  
                  totalBytesRead += bytesRead;
                  State = WebOperationState.InProgress;
                  OnWebOperationProgress(new WebOperationProgressEventArgs(totalBytesRead, totalLength, State));
               }
               while (bytesRead > 0);
            }
         }

         if (totalBytesRead == totalLength)
         {
            State = WebOperationState.Finished;
            OnWebOperationProgress(new WebOperationProgressEventArgs(totalBytesRead, totalLength, State));
         }

         // Close the Response Stream
         response.Close();

         State = WebOperationState.Idle;
         OnWebOperationProgress(new WebOperationProgressEventArgs(totalBytesRead, totalLength, State));
      }
      
      protected long ExecuteGetDownloadLength()
      {
         if (State.Equals(WebOperationState.Idle) == false)
         {
            throw new InvalidOperationException("Web Operation must be Idle.");
         }

         OperationRequest.Request.Method = GetWebDownloadMethod();

         WebResponse response = _OperationRequest.Request.GetResponse();
         long length = response.ContentLength;
         response.Close();
         
         return length;
      }

      public void Upload(string LocalFilePath)
      {
         ExecuteUpload(LocalFilePath);
      }

      protected void ExecuteUpload(string LocalFilePath)
      {
         if (State.Equals(WebOperationState.Idle) == false)
         {
            throw new InvalidOperationException("Web Operation must be Idle.");
         }

         OperationRequest.Request.Method = GetWebUploadMethod();

         long totalBytesRead = 0;
         long totalLength;

         using (Stream fileStream = File.OpenRead(LocalFilePath))
         {
            totalLength = fileStream.Length;
            if (AutoSizeBuffer)
            {
               Buffer = CalculateBufferSize(totalLength);
            }

            using (Stream requestStream = OperationRequest.Request.GetRequestStream())
            {
               byte[] buffer = new byte[Buffer];
               int bytesRead;

               do
               {
                  bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                  requestStream.Write(buffer, 0, bytesRead);

                  totalBytesRead += bytesRead;
                  State = WebOperationState.InProgress;
                  OnWebOperationProgress(new WebOperationProgressEventArgs(totalBytesRead, totalLength, State));

               } while (bytesRead > 0);
            }
         }

         if (totalBytesRead == totalLength)
         {
            State = WebOperationState.Finished;
            OnWebOperationProgress(new WebOperationProgressEventArgs(totalBytesRead, totalLength, State));
         }

         // Get the Response Stream and Close
         //if (FtpRequest != null)
         //{
         //   FtpWebResponse response = (FtpWebResponse)FtpRequest.GetResponse();
         //   response.Close();
         //}

         WebResponse response = OperationRequest.Request.GetResponse();
         response.Close();

         State = WebOperationState.Idle;
         OnWebOperationProgress(new WebOperationProgressEventArgs(totalBytesRead, totalLength, State));
      }
      
      public void CheckConnection()
      {
         ExecuteCheckConnection();
      }
      
      protected void ExecuteCheckConnection()
      {
         if (State.Equals(WebOperationState.Idle) == false)
         {
            throw new InvalidOperationException("Web Operation must be Idle.");
         }

         OperationRequest.Request.Method = GetWebCheckConnectionMethod();

         WebResponse response = null;
         try
         {
            State = WebOperationState.InProgress;

            response = OperationRequest.Request.GetResponse();
         }
         finally
         {
            if (response != null)
            {
               response.Close();
            }

            State = WebOperationState.Idle;
         }
      }

      protected static int CalculateBufferSize(long streamLength)
      {
         long autoBuffer = streamLength / 200;
         if (autoBuffer <= Int32.MaxValue && autoBuffer > DefaultBufferSize)
         {
            return (int)autoBuffer;
         }
         
         return DefaultBufferSize;
      }

      protected virtual void OnWebOperationProgress(WebOperationProgressEventArgs e)
      {
         if (WebOperationProgress != null)
         {
            WebOperationProgress(this, e);
         }
      }

      protected virtual string GetWebDownloadMethod()
      {
         throw new NotImplementedException("Method is not implemented.");
      }

      protected virtual string GetWebUploadMethod()
      {
         throw new NotImplementedException("Method is not implemented.");
      }
      
      protected virtual string GetWebCheckConnectionMethod()
      {
         throw new NotImplementedException("Method is not implemented.");
      }

      public static WebOperation Create(string requestUriString)
      {
         return Create(new Uri(requestUriString));
      }
      
      public static WebOperation Create(Uri requestUri)
      {
         WebRequest request = WebRequest.Create(requestUri);

         if (request is FileWebRequest)
         {
            return new FileWebOperation(request);
         }
         else if (request is HttpWebRequest)
         {
            return new HttpWebOperation(request);
         }
         else if (request is FtpWebRequest)
         {
            return new FtpWebOperation((FtpWebRequest)request);
         }

         throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
            "Web Request Type '{0}' is not valid.", request.GetType()));
      }
   }
   
   public class FileWebOperation : WebOperation
   {
      public FileWebOperation(IWebOperationRequest operationRequest)
      {
         OperationRequest = operationRequest;
      }

      internal FileWebOperation(WebRequest webRequest)
      {
         OperationRequest = new WebOperationRequest(webRequest);
      }
      
      protected override string GetWebDownloadMethod()
      {
 	      return WebRequestMethods.File.DownloadFile;
      }

      protected override string GetWebUploadMethod()
      {
         return WebRequestMethods.File.UploadFile;
      }

      protected override string GetWebCheckConnectionMethod()
      {
         return WebRequestMethods.File.DownloadFile;
      }
   }

   public class HttpWebOperation : WebOperation
   {
      public HttpWebOperation(IWebOperationRequest operationRequest)
      {
         OperationRequest = operationRequest;
      }
   
      internal HttpWebOperation(WebRequest webRequest)
      {
         OperationRequest = new WebOperationRequest(webRequest);
      }
      
      protected override string GetWebDownloadMethod()
      {
 	      return WebRequestMethods.Http.Get;
      }

      protected override string GetWebUploadMethod()
      {
         return WebRequestMethods.Http.Post;
      }

      protected override string GetWebCheckConnectionMethod()
      {
         return WebRequestMethods.Http.Head;
      }
   }

   public class FtpWebOperation : WebOperation, IFtpWebOperation
   {
      public IFtpWebOperationRequest FtpOperationRequest
      {
         get { return (IFtpWebOperationRequest)OperationRequest; }
      }

      public FtpWebOperation(IFtpWebOperationRequest operationRequest)
      {
         OperationRequest = operationRequest;
      }
      
      internal FtpWebOperation(FtpWebRequest webRequest)
      {
         OperationRequest = new FtpWebOperationRequest(webRequest);
      }
      
      protected override string GetWebDownloadMethod()
      {
         return WebRequestMethods.Ftp.DownloadFile;
      }

      protected override string GetWebUploadMethod()
      {
         return WebRequestMethods.Ftp.UploadFile;
      }

      protected override string GetWebCheckConnectionMethod()
      {
         return WebRequestMethods.Ftp.ListDirectory;
      }
   }
   
   public class WebOperationRequest : IWebOperationRequest
   {
      public WebOperationRequest(WebRequest request)
      {
         _Request = request;
      }

      private readonly WebRequest _Request;
      public WebRequest Request
      { 
         get { return _Request; }
      }
      
      public RequestCachePolicy CachePolicy 
      { 
         get { return Request.CachePolicy; }
         set { Request.CachePolicy = value; }
      }
      
      public int Timeout
      { 
         get { return Request.Timeout; }
         set { Request.Timeout = value; }
      }
   }
   
   public class FtpWebOperationRequest : WebOperationRequest, IFtpWebOperationRequest
   {
      public FtpWebOperationRequest(FtpWebRequest request)
         : base(request)
      {
      
      }
      
      public FtpWebRequest FtpRequest
      {
         get { return (FtpWebRequest) Request; }
      }

      public bool UsePassive
      { 
         get { return FtpRequest.UsePassive; }
         set { FtpRequest.UsePassive = value; }
      }
      
      public bool KeepAlive
      { 
         get { return FtpRequest.KeepAlive; }
         set { FtpRequest.KeepAlive = value; }
      }
   }
}
