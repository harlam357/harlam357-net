/*
 * harlam357.Net - Open File Dialog View
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
   public interface IOpenFileDialogView
   {
      string DefaultExt { get; set; }
   
      string FileName { get; set; }
      
      string InitialDirectory { get; set; }
      
      string Filter { get; set; }

      DialogResult ShowDialog();

      DialogResult ShowDialog(IWin32Window owner);
   }

   public class OpenFileDialogView : IOpenFileDialogView
   {
      private readonly OpenFileDialog _dialog;

      public string DefaultExt
      {
         get { return _dialog.DefaultExt; }
         set { _dialog.DefaultExt = value; }
      }
   
      public string FileName
      {
         get { return _dialog.FileName; }
         set { _dialog.FileName = value; }
      }
      
      public string InitialDirectory
      {
         get { return _dialog.InitialDirectory; }
         set { _dialog.InitialDirectory = value; }
      }

      public string Filter
      {
         get { return _dialog.Filter; }
         set { _dialog.Filter = value; }
      }
   
      public OpenFileDialogView()
      {
         _dialog = new OpenFileDialog();
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
