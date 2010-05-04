/*
 * harlam357.Net - Save File View
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
   public interface ISaveFileView
   {
      string FileName { get; set; }
      
      bool ShowView();
   }

   public class SaveFileView : ISaveFileView
   {
      private readonly SaveFileDialog _saveFileDialog;
      
      public SaveFileView()
      {
         _saveFileDialog = new SaveFileDialog();
      }
      
      public string FileName
      {
         get { return _saveFileDialog.FileName; }
         set { _saveFileDialog.FileName = value; }
      }
      
      public bool ShowView()
      {
         return _saveFileDialog.ShowDialog().Equals(DialogResult.OK) ? true : false;
      }
   }
}
