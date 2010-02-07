/*
 * harlam357.Net - Web Operation Progress EventArgs Class
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

namespace harlam357.Net
{
   public class WebOperationProgressEventArgs : EventArgs
   {
      private readonly long _Length;
      public long Length
      {
         get { return _Length; }
      }

      private readonly long _TotalLength;
      public long TotalLength
      {
         get { return _TotalLength; }
      }

      private readonly WebOperationState _State;
      public WebOperationState State
      {
         get { return _State; }
      }
   
      public WebOperationProgressEventArgs(long length, long totalLength, WebOperationState state)
      {
         _Length = length;
         _TotalLength = totalLength;
         _State = state;
      }
   }
}