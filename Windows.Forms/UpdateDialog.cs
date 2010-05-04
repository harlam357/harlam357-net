﻿/*
 * harlam357.Net - Application Update Dialog
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

using System;
using System.Globalization;
using System.Windows.Forms;

namespace harlam357.Windows.Forms
{
   public partial class UpdateDialog : Form, IUpdateView
   {
      private UpdatePresenter _presenter;
   
      #region Properties

      protected ApplicationUpdate UpdateData { get; private set; }

      protected string ApplicationName { get; private set; }

      protected string ApplicationVersion { get; private set; }

      #endregion
   
      public UpdateDialog(ApplicationUpdate update, string applicationName, string applicationVersion)
      {
         InitializeComponent();

         UpdateData = update;
         ApplicationName = applicationName;
         ApplicationVersion = applicationVersion;
      }
      
      private void UpdateDialog_Load(object sender, EventArgs e)
      {
         LayoutUpdateData();
      }

      protected virtual void LayoutUpdateData()
      {
         lblFirstLine.Text = String.Format(CultureInfo.CurrentCulture, lblFirstLine.Text, ApplicationName);

         lblYourVersion.Text = lblYourVersion.Text + ApplicationVersion;
         lblCurrentVersion.Text = lblCurrentVersion.Text + UpdateData.Version;

         cboUpdateFiles.DisplayMember = "Description";
         cboUpdateFiles.ValueMember = "Description";
         cboUpdateFiles.DataSource = UpdateData.UpdateFiles;
      }
      
      private void btnDownload_Click(object sender, EventArgs e)
      {
        _presenter.DownloadClick(cboUpdateFiles.SelectedIndex);
      }
      
      private void btnCancel_Click(object sender, EventArgs e)
      {
         _presenter.CancelClick();
      }

      #region IUpdateView Methods

      public void AttachPresenter(UpdatePresenter presenter)
      {
         _presenter = presenter;
      }

      public void ShowView()
      {
         ShowDialog();
      }

      public void ShowView(IWin32Window owner)
      {
         ShowDialog(owner);
      }

      public void CloseView()
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(CloseView));
            return;
         }

         Close();
      }
      
      public void SetSelectDownloadLabelText(string value)
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(() => SetSelectDownloadLabelText(value)));
            return;
         }

         lblSelectDownload.Text = value;
      }

      public void SetUpdateComboBoxVisible(bool visible)
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(() => SetUpdateComboBoxVisible(visible)));
            return;
         }

         cboUpdateFiles.Visible = visible;
      }

      public void SetDownloadButtonEnabled(bool enabled)
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(() => SetDownloadButtonEnabled(enabled)));
            return;
         }

         btnDownload.Enabled = enabled;
      }
      
      public void SetDownloadProgressVisisble(bool visible)
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(() => SetDownloadProgressVisisble(visible)));
            return;
         }
      
         progressDownload.Visible = visible;
      }
      
      public void SetDownloadProgressValue(int value)
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(() => SetDownloadProgressValue(value)));
            return;
         }
         
         progressDownload.Value = value;
      }
      
      #endregion
   }
}
