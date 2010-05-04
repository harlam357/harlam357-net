﻿/*
 * harlam357.Net - Application Update Presenter
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
using System.Net;
using System.Windows.Forms;

using harlam357.Net;

namespace harlam357.Windows.Forms
{
   public class UpdatePresenter
   {
      #region Fields

      private IWebOperation _webOperation;
      private readonly Form _owner;
      private readonly Action<Exception> _exceptionLogger;
      private readonly ApplicationUpdate _updateData;
      private readonly IWebProxy _proxy;
      private readonly IUpdateView _updateView;
      private readonly ISaveFileView _saveFileView;
      private readonly IMessageBoxView _messageBoxView;

      #endregion
      
      #region Properties

      public ApplicationUpdateFile SelectedUpdate { get; private set; }

      public string LocalFilePath { get; private set; }
      
      public WebOperationResult Result
      {
         get
         {
            return _webOperation != null ? _webOperation.Result : WebOperationResult.Unknown;
         }
      }
      
      #endregion
      
      #region Constructors
      
      public UpdatePresenter(Form owner, Action<Exception> exceptionLogger, ApplicationUpdate updateData, 
                             IWebProxy proxy, string applicationName, string applicationVersion)
      {
         _owner = owner;
         _exceptionLogger = exceptionLogger;
         _updateData = updateData;
         _proxy = proxy;
         _updateView = new UpdateDialog(updateData, applicationName, applicationVersion);
         _saveFileView = new SaveFileView();
         _messageBoxView = new MessageBoxView();
         _updateView.AttachPresenter(this);
      }

      public UpdatePresenter(Form owner, Action<Exception> exceptionLogger, ApplicationUpdate updateData, 
                             IWebProxy proxy, IUpdateView updateView, ISaveFileView saveFileView,
                             IMessageBoxView messageBoxView)
      {
         _owner = owner;
         _exceptionLogger = exceptionLogger;
         _updateData = updateData;
         _proxy = proxy;
         _updateView = updateView;
         _saveFileView = saveFileView;
         _messageBoxView = messageBoxView;
         _updateView.AttachPresenter(this);
      }
      
      #endregion
      
      #region Download
      
      public void DownloadClick(int index)
      {
         if (ShowSaveFileView(index))
         {
            Action<string> performDownloadAction = PerformDownload;
            performDownloadAction.BeginInvoke(
               _updateData.UpdateFiles[index].HttpAddress, PerformDownloadCallback, performDownloadAction);
         }
      }
      
      public void DownloadClick(int index, Action<string> performDownloadAction)
      {
         if (ShowSaveFileView(index))
         {
            PerformDownloadCallback(performDownloadAction.BeginInvoke(
               _updateData.UpdateFiles[index].HttpAddress, null, performDownloadAction));
         }
      }
      
      private bool ShowSaveFileView(int index)
      {
         SelectedUpdate = _updateData.UpdateFiles[index];
         _saveFileView.FileName = GetFileNameFromUrl(_updateData.UpdateFiles[index].HttpAddress);
         if (_saveFileView.ShowView())
         {
            LocalFilePath = _saveFileView.FileName;
            return true;
         }

         return false;
      }

      public void PerformDownload(string url)
      {
         PerformDownload(url, LocalFilePath);
      }

      public void PerformDownload(string url, string filePath)
      {
         // Preform Download
         _webOperation = WebOperation.Create(url);
         if (_proxy != null) _webOperation.OperationProxy = _proxy;
         _webOperation.WebOperationProgress += WebOperationProgress;

         _updateView.SetSelectDownloadLabelText(String.Format(CultureInfo.CurrentCulture, 
            "Downloading {0}...", GetFileNameFromUrl(url)));
         _updateView.SetDownloadButtonEnabled(false);
         _updateView.SetUpdateComboBoxVisible(false);
         _updateView.SetDownloadProgressVisisble(true);

         _webOperation.Download(filePath);
      }

      private void PerformDownloadCallback(IAsyncResult result)
      {
         Action<string> action = (Action<string>)result.AsyncState;
         try
         {
            action.EndInvoke(result);
         }
         catch (Exception ex)
         {
            LogException(ex);
            string message = String.Format(CultureInfo.CurrentCulture,
                                           "Download failed with the following error:{0}{0}{1}",
                                           Environment.NewLine, ex.Message);
            _messageBoxView.ShowError(_owner, message, _owner.Text);
         }
         CloseView();
      }

      private void WebOperationProgress(object sender, WebOperationProgressEventArgs e)
      {
         int percent = (int)(((double)e.Length / e.TotalLength) * 100);
         _updateView.SetDownloadProgressValue(percent);
      }

      public static string GetFileNameFromUrl(string url)
      {
         return url.Substring(url.LastIndexOf('/') + 1).Replace("%20", " ");
      }
      
      #endregion
      
      #region Cancel
      
      public void CancelClick()
      {
         if (_webOperation != null)
         {
            _webOperation.WebOperationProgress -= WebOperationProgress;
            _webOperation.CancelOperation();
         }
         CloseView();
      }
      
      #endregion
      
      #region Show and Close

      public void ShowView()
      {
         _updateView.ShowView(_owner);
      }

      private void CloseView()
      {
         _updateView.CloseView();
      }
      
      #endregion
      
      private void LogException(Exception ex)
      {
         if (_exceptionLogger != null)
         {
            _exceptionLogger(ex);
         }  
      }
   }
}