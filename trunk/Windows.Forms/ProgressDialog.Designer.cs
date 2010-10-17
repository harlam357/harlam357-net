namespace harlam357.Windows.Forms
{
   partial class ProgressDialog
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
         this.progressBar = new System.Windows.Forms.ProgressBar();
         this.messageLabel = new System.Windows.Forms.Label();
         this.CancelButton = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // progressBar
         // 
         this.progressBar.Location = new System.Drawing.Point(12, 30);
         this.progressBar.Name = "progressBar";
         this.progressBar.Size = new System.Drawing.Size(306, 24);
         this.progressBar.TabIndex = 0;
         // 
         // messageLabel
         // 
         this.messageLabel.Location = new System.Drawing.Point(12, 9);
         this.messageLabel.Name = "messageLabel";
         this.messageLabel.Size = new System.Drawing.Size(306, 13);
         this.messageLabel.TabIndex = 1;
         this.messageLabel.Text = "messageLabel";
         this.messageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
         // 
         // CancelButton
         // 
         this.CancelButton.Location = new System.Drawing.Point(129, 63);
         this.CancelButton.Name = "CancelButton";
         this.CancelButton.Size = new System.Drawing.Size(72, 23);
         this.CancelButton.TabIndex = 2;
         this.CancelButton.Text = "Cancel";
         this.CancelButton.UseVisualStyleBackColor = true;
         // 
         // ProgressDialog
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(330, 95);
         this.Controls.Add(this.CancelButton);
         this.Controls.Add(this.messageLabel);
         this.Controls.Add(this.progressBar);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "ProgressDialog";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "ProgressDialog";
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ProgressBar progressBar;
      private System.Windows.Forms.Label messageLabel;
      private System.Windows.Forms.Button CancelButton;
   }
}