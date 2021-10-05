namespace GrpcFileClient
{
    partial class GrpcFileClientForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.FindFilesButton = new System.Windows.Forms.Button();
            this.UploadButton = new System.Windows.Forms.Button();
            this.UploadMessage = new System.Windows.Forms.Label();
            this.CancelUploadButton = new System.Windows.Forms.Button();
            this.FileNamesTextBox = new System.Windows.Forms.TextBox();
            this.CancelDownloadButton = new System.Windows.Forms.Button();
            this.DownloadButton = new System.Windows.Forms.Button();
            this.DownloadMessage = new System.Windows.Forms.Label();
            this.FilePathListBox = new System.Windows.Forms.ListBox();
            this.SelectedFilesLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // FindFilesButton
            // 
            this.FindFilesButton.Location = new System.Drawing.Point(12, 164);
            this.FindFilesButton.Name = "FindFilesButton";
            this.FindFilesButton.Size = new System.Drawing.Size(110, 29);
            this.FindFilesButton.TabIndex = 1;
            this.FindFilesButton.Text = "Find Files";
            this.FindFilesButton.UseVisualStyleBackColor = true;
            this.FindFilesButton.Click += new System.EventHandler(this.OpenUploadButton_Click);
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(128, 164);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(110, 29);
            this.UploadButton.TabIndex = 2;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // UploadMessage
            // 
            this.UploadMessage.Location = new System.Drawing.Point(12, 202);
            this.UploadMessage.Name = "UploadMessage";
            this.UploadMessage.Size = new System.Drawing.Size(776, 42);
            this.UploadMessage.TabIndex = 3;
            this.UploadMessage.Text = "...";
            // 
            // CancelUploadButton
            // 
            this.CancelUploadButton.Location = new System.Drawing.Point(244, 164);
            this.CancelUploadButton.Name = "CancelUploadButton";
            this.CancelUploadButton.Size = new System.Drawing.Size(127, 29);
            this.CancelUploadButton.TabIndex = 4;
            this.CancelUploadButton.Text = "Cancel Upload";
            this.CancelUploadButton.UseVisualStyleBackColor = true;
            this.CancelUploadButton.Click += new System.EventHandler(this.CancelUploadButton_Click);
            // 
            // FileNamesTextBox
            // 
            this.FileNamesTextBox.Location = new System.Drawing.Point(11, 273);
            this.FileNamesTextBox.Multiline = true;
            this.FileNamesTextBox.Name = "FileNamesTextBox";
            this.FileNamesTextBox.Size = new System.Drawing.Size(777, 71);
            this.FileNamesTextBox.TabIndex = 5;
            // 
            // CancelDownloadButton
            // 
            this.CancelDownloadButton.Location = new System.Drawing.Point(128, 356);
            this.CancelDownloadButton.Name = "CancelDownloadButton";
            this.CancelDownloadButton.Size = new System.Drawing.Size(142, 29);
            this.CancelDownloadButton.TabIndex = 7;
            this.CancelDownloadButton.Text = "Cancel Download";
            this.CancelDownloadButton.UseVisualStyleBackColor = true;
            this.CancelDownloadButton.Click += new System.EventHandler(this.CancelDownloadButton_Click);
            // 
            // DownloadButton
            // 
            this.DownloadButton.Location = new System.Drawing.Point(12, 356);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(110, 29);
            this.DownloadButton.TabIndex = 6;
            this.DownloadButton.Text = "Download";
            this.DownloadButton.UseVisualStyleBackColor = true;
            this.DownloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // DownloadMessage
            // 
            this.DownloadMessage.Location = new System.Drawing.Point(12, 396);
            this.DownloadMessage.Name = "DownloadMessage";
            this.DownloadMessage.Size = new System.Drawing.Size(776, 46);
            this.DownloadMessage.TabIndex = 8;
            this.DownloadMessage.Text = "...";
            // 
            // FilePathListBox
            // 
            this.FilePathListBox.FormattingEnabled = true;
            this.FilePathListBox.HorizontalScrollbar = true;
            this.FilePathListBox.ItemHeight = 19;
            this.FilePathListBox.Location = new System.Drawing.Point(12, 34);
            this.FilePathListBox.Name = "FilePathListBox";
            this.FilePathListBox.Size = new System.Drawing.Size(776, 118);
            this.FilePathListBox.TabIndex = 9;
            // 
            // SelectedFilesLabel
            // 
            this.SelectedFilesLabel.AutoSize = true;
            this.SelectedFilesLabel.Location = new System.Drawing.Point(12, 9);
            this.SelectedFilesLabel.Name = "SelectedFilesLabel";
            this.SelectedFilesLabel.Size = new System.Drawing.Size(118, 19);
            this.SelectedFilesLabel.TabIndex = 10;
            this.SelectedFilesLabel.Text = "Selected Files：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 244);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(295, 19);
            this.label1.TabIndex = 11;
            this.label1.Text = "Typed FileNames（Split with comma）：";
            // 
            // GrpcFileClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 454);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SelectedFilesLabel);
            this.Controls.Add(this.FilePathListBox);
            this.Controls.Add(this.DownloadMessage);
            this.Controls.Add(this.CancelDownloadButton);
            this.Controls.Add(this.DownloadButton);
            this.Controls.Add(this.FileNamesTextBox);
            this.Controls.Add(this.CancelUploadButton);
            this.Controls.Add(this.UploadMessage);
            this.Controls.Add(this.UploadButton);
            this.Controls.Add(this.FindFilesButton);
            this.Name = "GrpcFileClientForm";
            this.Text = "GrpcFileClient";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button FindFilesButton;
        private System.Windows.Forms.Button UploadButton;
        private System.Windows.Forms.Label UploadMessage;
        private System.Windows.Forms.Button CancelUploadButton;
        private System.Windows.Forms.TextBox FileNamesTextBox;
        private System.Windows.Forms.Button CancelDownloadButton;
        private System.Windows.Forms.Button DownloadButton;
        private System.Windows.Forms.Label DownloadMessage;
        private System.Windows.Forms.ListBox FilePathListBox;
        private System.Windows.Forms.Label SelectedFilesLabel;
        private System.Windows.Forms.Label label1;
    }
}

