/*
 * harlam357.Net - Message Box View
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
   public interface IMessageBoxView
   {
      void ShowError(string text, string caption);
      void ShowError(Form owner, string text, string caption);
      void ShowInformation(string text, string caption);
      void ShowInformation(Form owner, string text, string caption);
   }

   public class MessageBoxView : IMessageBoxView
   {
      public void ShowError(string text, string caption)
      {
         MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
   
      public void ShowError(Form owner, string text, string caption)
      {
         if (owner.InvokeRequired)
         {
            owner.Invoke(new MethodInvoker(() => ShowError(owner, text, caption)));
            return;
         }
      
         MessageBox.Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      
      public void ShowInformation(string text, string caption)
      {
         MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      public void ShowInformation(Form owner, string text, string caption)
      {
         if (owner.InvokeRequired)
         {
            owner.Invoke(new MethodInvoker(() => ShowInformation(owner, text, caption)));
            return;
         }

         MessageBox.Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
   }
}
