/*
 * Unhandled Exception Dialog
 * Copyright (C) 2002-2010 by AlphaSierraPapa, Christoph Wille
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
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace harlam357.Windows.Forms
{
   public partial class ExceptionDialog : Form
   {
      public delegate void LogException(Exception ex);

      private static string _applicationId;
      private static LogException _exceptionLogger;

      public static void RegisterForUnhandledExceptions(string applicationId)
      {
         RegisterForUnhandledExceptions(applicationId, null);
      }

      public static void RegisterForUnhandledExceptions(string applicationId, LogException exceptionLogger)
      {
         _applicationId = applicationId;
         _exceptionLogger = exceptionLogger;
         Application.ThreadException += ShowErrorDialog;
      }

      private static void ShowErrorDialog(object sender, ThreadExceptionEventArgs e)
      {
         ShowErrorDialog(e.Exception, null);
      }

      public static void ShowErrorDialog(Exception exception, string message)
      {
         ShowErrorDialog(exception, message, false);
      }
      
      public static void ShowErrorDialog(Exception exception, string message, bool mustTerminate)
      {
         ShowErrorDialog(exception, message, null, mustTerminate);
      }
      
      public static void ShowErrorDialog(Exception exception, string message, string reportUrl)
      {
         ShowErrorDialog(exception, message, reportUrl, false);
      }

      public static void ShowErrorDialog(Exception exception, string message, string reportUrl, bool mustTerminate)
      {
         ShowErrorDialog(exception, message, reportUrl, _applicationId, mustTerminate);
      }
      
      public static void ShowErrorDialog(Exception exception, string message, string reportUrl, string messageHeader)
      {
         ShowErrorDialog(exception, message, reportUrl, messageHeader, false);
      }

      public static void ShowErrorDialog(Exception exception, string message, string reportUrl, string messageHeader, bool mustTerminate)
      {
         if (_exceptionLogger != null) _exceptionLogger(exception);
         try
         {
            using (ExceptionDialog box = new ExceptionDialog(exception, message, reportUrl, messageHeader, mustTerminate))
            {
               box.ShowDialog();
            }
         }
         catch (Exception ex)
         {
            if (_exceptionLogger != null) _exceptionLogger(exception);
            MessageBox.Show(ex.ToString(), message, MessageBoxButtons.OK, MessageBoxIcon.Error,
               MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
         }
      }

      private readonly Exception _exceptionThrown;
      private readonly string _message;
      private readonly string _reportUrl;
      private readonly string _messageHeader;

      public ExceptionDialog()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Creates a new ExceptionDialog instance.
      /// </summary>
      /// <param name="exception">The exception to display</param>
      /// <param name="message">An additional message to display</param>
      /// <param name="mustTerminate">If <paramref name="mustTerminate"/> is true, the
      /// continue button is not available.</param>
      public ExceptionDialog(Exception exception, string message, bool mustTerminate)
         : this(exception, message, null, mustTerminate)
      {

      }
      
      /// <summary>
      /// Creates a new ExceptionDialog instance.
      /// </summary>
      /// <param name="exception">The exception to display</param>
      /// <param name="message">An additional message to display</param>
      /// <param name="reportUrl">Override configured target URL for report button</param>
      /// <param name="mustTerminate">If <paramref name="mustTerminate"/> is true, the
      /// continue button is not available.</param>
      public ExceptionDialog(Exception exception, string message, string reportUrl, bool mustTerminate)
         : this(exception, message, reportUrl, null, mustTerminate)
      {
      
      }

      /// <summary>
      /// Creates a new ExceptionDialog instance.
      /// </summary>
      /// <param name="exception">The exception to display</param>
      /// <param name="message">An additional message to display</param>
      /// <param name="reportUrl">Override configured target URL for report button</param>
      /// <param name="messageHeader">A message header to display</param>
      /// <param name="mustTerminate">If <paramref name="mustTerminate"/> is true, the
      /// continue button is not available.</param>
      public ExceptionDialog(Exception exception, string message, string reportUrl, string messageHeader, bool mustTerminate)
      {
         _exceptionThrown = exception;
         _message = message;
         _reportUrl = reportUrl;
         _messageHeader = messageHeader;

         InitializeComponent();

         if (mustTerminate)
         {
            btnExit.Visible = false;
            btnContinue.Text = btnExit.Text;
            btnContinue.Left -= btnExit.Width - btnContinue.Width;
            btnContinue.Width = btnExit.Width;
         }

         exceptionTextBox.Text = GetClipboardString();
      }

      string GetClipboardString()
      {
         StringBuilder sb = new StringBuilder();
         if (_messageHeader != null)
         {
            sb.AppendLine(_messageHeader);
            sb.AppendLine();
         }
         if (_message != null)
         {
            sb.AppendLine(_message);
            sb.AppendLine();
         }
         sb.AppendLine("Exception Thrown:");
         sb.AppendLine(_exceptionThrown.ToString());
         return sb.ToString();
      }

      private void btnReport_Click(object sender, EventArgs e)
      {
         CopyInfoToClipboard();
         string reportUrl = String.IsNullOrEmpty(_reportUrl) ? Properties.Settings.Default.ReportUrl : _reportUrl;
         if (String.IsNullOrEmpty(reportUrl) == false)
         {
            StartUrl(reportUrl);
         }
      }

      private void CopyInfoToClipboard()
      {
         if (copyErrorCheckBox.Checked)
         {
            string exceptionText = exceptionTextBox.Text;
            if (Application.OleRequired() == ApartmentState.STA)
            {
               ClipboardWrapper.SetText(exceptionText);
            }
            else
            {
               Thread th = new Thread((ThreadStart)delegate
               {
                  ClipboardWrapper.SetText(exceptionText);
               });
               th.Name = "CopyInfoToClipboard";
               th.SetApartmentState(ApartmentState.STA);
               th.Start();
            }
         }
      }

      private void StartUrl(string url)
      {
         try
         {
            Process.Start(url);
         }
         catch (Exception ex)
         {
            if (_exceptionLogger != null) _exceptionLogger(ex);
            MessageBox.Show(ex.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Error,
               MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
         }
      }

      private void btnContinue_Click(object sender, EventArgs e)
      {
         DialogResult = DialogResult.Ignore;
         Close();
      }

      private void btnExit_Click(object sender, EventArgs e)
      {
         if (MessageBox.Show("Are you sure you want to exit the application?.", Text,
             MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2,
             MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
         {
            Application.Exit();
         }
      }
   }
}
