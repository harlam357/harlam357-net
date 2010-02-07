/*
 * harlam357.Net - Web Operation Interface
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
using System.Net;
using System.Net.Cache;

namespace harlam357.Net
{
   public interface IWebOperation
   {
      event EventHandler<WebOperationProgressEventArgs> WebOperationProgress;

      IWebOperationRequest OperationRequest { get; }

      WebOperationState State { get; }

      bool AutoSizeBuffer { get; set; }

      int Buffer { get; set; }

      void Download(string LocalFilePath);
      
      long GetDownloadLength();
      
      void Upload(string LocalFilePath);
      
      void CheckConnection();
   }

   public interface IWebOperationRequest
   {
      WebRequest Request { get; }

      RequestCachePolicy CachePolicy { get; set; }

      int Timeout { get; set; }
   }
}