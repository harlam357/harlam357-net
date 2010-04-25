﻿namespace harlam357.Windows.Forms
{
   partial class ExceptionDialog
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.exceptionTextBox = new System.Windows.Forms.TextBox();
         this.btnExit = new System.Windows.Forms.Button();
         this.btnContinue = new System.Windows.Forms.Button();
         this.btnReport = new System.Windows.Forms.Button();
         this.copyErrorCheckBox = new System.Windows.Forms.CheckBox();
         this.label1 = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // exceptionTextBox
         // 
         this.exceptionTextBox.Location = new System.Drawing.Point(12, 43);
         this.exceptionTextBox.Multiline = true;
         this.exceptionTextBox.Name = "exceptionTextBox";
         this.exceptionTextBox.ReadOnly = true;
         this.exceptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
         this.exceptionTextBox.Size = new System.Drawing.Size(460, 184);
         this.exceptionTextBox.TabIndex = 2;
         this.exceptionTextBox.Text = "textBoxExceptionText";
         // 
         // btnExit
         // 
         this.btnExit.Location = new System.Drawing.Point(171, 256);
         this.btnExit.Name = "btnExit";
         this.btnExit.Size = new System.Drawing.Size(141, 23);
         this.btnExit.TabIndex = 9;
         this.btnExit.Text = "Exit Application";
         this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
         // 
         // btnContinue
         // 
         this.btnContinue.Location = new System.Drawing.Point(331, 256);
         this.btnContinue.Name = "btnContinue";
         this.btnContinue.Size = new System.Drawing.Size(141, 23);
         this.btnContinue.TabIndex = 10;
         this.btnContinue.Text = "Continue";
         this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
         // 
         // btnReport
         // 
         this.btnReport.Location = new System.Drawing.Point(12, 256);
         this.btnReport.Name = "btnReport";
         this.btnReport.Size = new System.Drawing.Size(141, 23);
         this.btnReport.TabIndex = 8;
         this.btnReport.Text = "Report Exception";
         this.btnReport.Click += new System.EventHandler(this.btnReport_Click);
         // 
         // copyErrorCheckBox
         // 
         this.copyErrorCheckBox.AutoSize = true;
         this.copyErrorCheckBox.Checked = true;
         this.copyErrorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
         this.copyErrorCheckBox.Location = new System.Drawing.Point(12, 233);
         this.copyErrorCheckBox.Name = "copyErrorCheckBox";
         this.copyErrorCheckBox.Size = new System.Drawing.Size(158, 17);
         this.copyErrorCheckBox.TabIndex = 7;
         this.copyErrorCheckBox.Text = "Copy Error Text to Clipboard";
         // 
         // label1
         // 
         this.label1.Location = new System.Drawing.Point(12, 9);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(460, 28);
         this.label1.TabIndex = 11;
         this.label1.Text = "An Unhandled Exception has occurred.  The exception text and any other messages a" +
             "re found below.  Please choose how you would like to handle the exception.";
         // 
         // ExceptionDialog
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(485, 289);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.btnExit);
         this.Controls.Add(this.btnContinue);
         this.Controls.Add(this.btnReport);
         this.Controls.Add(this.copyErrorCheckBox);
         this.Controls.Add(this.exceptionTextBox);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "ExceptionDialog";
         this.ShowIcon = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "Unhandled Exception";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox exceptionTextBox;
      private System.Windows.Forms.Button btnExit;
      private System.Windows.Forms.Button btnContinue;
      private System.Windows.Forms.Button btnReport;
      private System.Windows.Forms.CheckBox copyErrorCheckBox;
      private System.Windows.Forms.Label label1;
   }
}