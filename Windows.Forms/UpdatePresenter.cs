﻿/*
 * harlam357.Net - Application Update Presenter
 * Copyright (C) 2010-2013 Ryan Harlamert (harlam357)
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
using System.Windows.Forms;

using harlam357.Core.Net;
using harlam357.Core.Security;

namespace harlam357.Windows.Forms
{
   public class UpdatePresenter
   {
      #region Fields

      private IWebOperation _webOperation;
      
      private readonly Action<Exception> _exceptionLogger;
      private readonly ApplicationUpdate _updateData;
      private readonly IWebProxy _proxy;
      private readonly IUpdateView _updateView;
      private readonly ISaveFileDialogView _saveFileView;

      #endregion
      
      #region Properties

      /// <summary>
      /// Update Data Selected by the User
      /// </summary>
      public ApplicationUpdateFile SelectedUpdate { get; private set; }

      /// <summary>
      /// Local File Path for File to be Downloaded
      /// </summary>
      public string LocalFilePath { get; private set; }

      /// <summary>
      /// Indicates the Update was Downloaded Completely and was Verified
      /// </summary>
      public bool UpdateReady { get; private set; }
      
      #endregion

      /// <summary>
      /// Raises when the download process is finished, regardless of whether is was successful, errored, or canceled.
      /// </summary>
      public event EventHandler DownloadFinished;
      
      private void OnDownloadFinished(EventArgs e)
      {
         if (DownloadFinished != null)
         {
            DownloadFinished(this, e);
         }
      }
      
      #region Constructors

      public UpdatePresenter(Action<Exception> exceptionLogger, ApplicationUpdate updateData, 
                             IWebProxy proxy, string applicationName, string applicationVersion)
      {
         _exceptionLogger = exceptionLogger;
         _updateData = updateData;
         _proxy = proxy;
         _updateView = new UpdateDialog(updateData, applicationName, applicationVersion);
         _saveFileView = new SaveFileDialogView();
         _updateView.AttachPresenter(this);
      }

      public UpdatePresenter(Action<Exception> exceptionLogger, ApplicationUpdate updateData, 
                             IWebProxy proxy, IUpdateView updateView, ISaveFileDialogView saveFileView,
                             IWebOperation webOperation)
      {
         _exceptionLogger = exceptionLogger;
         _updateData = updateData;
         _proxy = proxy;
         _updateView = updateView;
         _saveFileView = saveFileView;
         _webOperation = webOperation;
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
      
      private bool ShowSaveFileView(int index)
      {
         _saveFileView.FileName = GetFileNameFromUrl(_updateData.UpdateFiles[index].HttpAddress);
         if (_saveFileView.ShowDialog().Equals(DialogResult.OK))
         {
            SelectedUpdate = _updateData.UpdateFiles[index];
            LocalFilePath = _saveFileView.FileName;
            return true;
         }

         return false;
      }
      
      private void PerformDownload(string url)
      {
         if (_webOperation == null)
         {
            // create
            _webOperation = WebOperation.Create(url);
         }
         // set proxy (if applicable)
         if (_proxy != null) _webOperation.WebRequest.Proxy = _proxy;
         // listen for progress messages
         _webOperation.ProgressChanged += WebOperationProgressChanged;
         // set the view for download
         SetViewControlsForDownload(url);
         // execute the download
         _webOperation.Download(LocalFilePath);
      }

      private void PerformDownloadCallback(IAsyncResult result)
      {
         var action = (Action<string>)result.AsyncState;
         try
         {
            action.EndInvoke(result);
            if (_webOperation.Result.Equals(WebOperationResult.Completed))
            {
               // verify, throws exception on error
               VerifyDownload();
               // no errors, set ready flag
               UpdateReady = true;
               // success, close the view
               _updateView.CloseView();
            }
            // not completed, let the user try again if they wish
            else
            {
               SetViewControlsForUpdateSelection();
            }
         }
         catch (Exception ex)
         {
            LogException(ex);
            string message = String.Format(CultureInfo.CurrentCulture,
                                           "Download failed with the following error:{0}{0}{1}",
                                           Environment.NewLine, ex.Message);
            _updateView.ShowErrorMessage(message);
            // download error, let the user try again if they wish
            SetViewControlsForUpdateSelection();
         }
         finally
         {
            // clean up
            _webOperation.ProgressChanged -= WebOperationProgressChanged;
            _webOperation = null;
            // signal listeners
            OnDownloadFinished(EventArgs.Empty);
         }
      }
      
      private void SetViewControlsForDownload(string url)
      {
         _updateView.SetSelectDownloadLabelText(String.Format(CultureInfo.CurrentCulture,
            "Downloading {0}...", GetFileNameFromUrl(url)));
         _updateView.SetDownloadButtonEnabled(false);
         _updateView.SetUpdateComboBoxVisible(false);
         _updateView.SetDownloadProgressValue(0);
         _updateView.SetDownloadProgressVisisble(true);
      }

      private void SetViewControlsForUpdateSelection()
      {
         _updateView.SetSelectDownloadLabelTextDefault();
         _updateView.SetDownloadButtonEnabled(true);
         _updateView.SetUpdateComboBoxVisible(true);
         _updateView.SetDownloadProgressVisisble(false);
      }

      private void WebOperationProgressChanged(object sender, WebOperationProgressChangedEventArgs e)
      {
         int percent = (int)(((double)e.Length / e.TotalLength) * 100);
         _updateView.SetDownloadProgressValue(percent);
      }

      private static string GetFileNameFromUrl(string url)
      {
         return url.Substring(url.LastIndexOf('/') + 1).Replace("%20", " ");
      }
      
      #endregion
      
      #region Cancel
      
      public void CancelClick()
      {
         // if download is in progress then the 
         if (_webOperation != null &&
             _webOperation.State.Equals(WebOperationState.InProgress))
         {
            _webOperation.ProgressChanged -= WebOperationProgressChanged;
            _webOperation.Cancel();
         }
         else
         {
            _updateView.CloseView();
         }
      }
      
      #endregion
      
      #region Show

      public void Show(IWin32Window owner)
      {
         _updateView.ShowView(owner);
      }
      
      #endregion
      
      public void VerifyDownload()
      {
         VerifyDownload(LocalFilePath, SelectedUpdate);
      }
      
      public static void VerifyDownload(string localFilePath, ApplicationUpdateFile selectedUpdate)
      {
         Stream stream = null;
         try
         {
            FileInfo fileInfo = new FileInfo(localFilePath);
            if (selectedUpdate.Size != fileInfo.Length)
            {
               throw new IOException(String.Format(CultureInfo.CurrentCulture, 
                  "File length is '{0}', expected length is '{1}'.", fileInfo.Length, selectedUpdate.Size));
            }
         
            stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            if (selectedUpdate.SHA1.Length != 0)
            {
               Hash hash = new Hash(HashProvider.SHA1);
               Data hashData = hash.Calculate(stream);
               if (String.Compare(selectedUpdate.SHA1, hashData.Bytes.ToHex(), StringComparison.OrdinalIgnoreCase) != 0)
               {
                  throw new IOException("SHA1 file hash is not correct.");
               }
            }
            stream.Position = 0;
            if (selectedUpdate.MD5.Length != 0)
            {
               Hash hash = new Hash(HashProvider.MD5);
               Data hashData = hash.Calculate(stream);
               if (String.Compare(selectedUpdate.MD5, hashData.Bytes.ToHex(), StringComparison.OrdinalIgnoreCase) != 0)
               {
                  throw new IOException("MD5 file hash is not correct.");
               }
            }
         }
         catch (Exception)
         {
            TryToDelete(localFilePath);
            throw;
         }
         finally
         {
            if (stream != null)
            {
               stream.Dispose();
            }
         }
      }

      private static void TryToDelete(string filePath)
      {
         try
         {
            File.Delete(filePath);
         }
         catch (Exception)
         { }
      }

      private void LogException(Exception ex)
      {
         if (_exceptionLogger != null)
         {
            _exceptionLogger(ex);
         }  
      }
   }
}
