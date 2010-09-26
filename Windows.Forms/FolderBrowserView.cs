/*
 * harlam357.Net - Folder Browser Dialog View
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

using System.Windows.Forms;

namespace harlam357.Windows.Forms
{
   public interface IFolderBrowserView
   {
      string SelectedPath { get; set; }

      DialogResult ShowDialog();

      DialogResult ShowDialog(IWin32Window owner);
   }

   public class FolderBrowserView : IFolderBrowserView
   {
      private readonly FolderBrowserDialog _dialog;
      
      public string SelectedPath
      {
         get { return _dialog.SelectedPath; }
         set { _dialog.SelectedPath = value; }
      }
      
      public FolderBrowserView()
      {
         _dialog = new FolderBrowserDialog();
      }

      public DialogResult ShowDialog()
      {
         return _dialog.ShowDialog();
      }

      public DialogResult ShowDialog(IWin32Window owner)
      {
         return _dialog.ShowDialog(owner);
      }
   }
}
