/*
 * harlam357.Core
 * Copyright (C) 2010-2017 Ryan Harlamert (harlam357)
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

namespace harlam357.Core
{
   /// <summary>
   /// Provides progress information data.
   /// </summary>
   public class ProgressInfo
   {
      /// <summary>
      /// Gets the task progress percentage.
      /// </summary>
      /// <returns>A percentage value indicating the task progress.</returns>
      public int ProgressPercentage { get; private set; }

      /// <summary>
      /// Gets a message value indicating the task progress.
      /// </summary>
      /// <returns>A System.String message value indicating the task progress.</returns>
      public string Message { get; private set; }

      /// <summary>
      /// Gets a unique user state.
      /// </summary>
      /// <returns>A unique System.Object indicating the user state.</returns>
      public object UserState { get; private set; }

      /// <summary>
      /// Initializes a new instance of the ProgressInfo class with progress percentage and message values.
      /// </summary>
      /// <param name="progressPercentage">The progress value.</param>
      /// <param name="message">The text message value.</param>
      public ProgressInfo(int progressPercentage, string message)
      {
         ProgressPercentage = progressPercentage;
         Message = message ?? String.Empty;
      }

      /// <summary>
      /// Initializes a new instance of the ProgressInfo class with progress percentage and message values.
      /// </summary>
      /// <param name="progressPercentage">A percentage value indicating the task progress.</param>
      /// <param name="message">A message value indicating the task progress.</param>
      /// <param name="userState">A unique user state.</param>
      public ProgressInfo(int progressPercentage, string message, object userState)
      {
         ProgressPercentage = progressPercentage;
         Message = message ?? String.Empty;
         UserState = userState;
      }
   }
}
