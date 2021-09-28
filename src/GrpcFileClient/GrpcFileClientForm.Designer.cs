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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.lblUploadPath = new System.Windows.Forms.Label();
            this.OpenUploadButton = new System.Windows.Forms.Button();
            this.UploadButton = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.CancelUploadButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.CancelDownloadButton = new System.Windows.Forms.Button();
            this.DownloadButton = new System.Windows.Forms.Button();
            this.lblMessage1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // lblUploadPath
            // 
            this.lblUploadPath.AutoSize = true;
            this.lblUploadPath.Location = new System.Drawing.Point(30, 27);
            this.lblUploadPath.Name = "lblUploadPath";
            this.lblUploadPath.Size = new System.Drawing.Size(51, 19);
            this.lblUploadPath.TabIndex = 0;
            this.lblUploadPath.Text = "label1";
            // 
            // OpenUploadButton
            // 
            this.OpenUploadButton.Location = new System.Drawing.Point(31, 60);
            this.OpenUploadButton.Name = "OpenUploadButton";
            this.OpenUploadButton.Size = new System.Drawing.Size(110, 29);
            this.OpenUploadButton.TabIndex = 1;
            this.OpenUploadButton.Text = "FindFile";
            this.OpenUploadButton.UseVisualStyleBackColor = true;
            this.OpenUploadButton.Click += new System.EventHandler(this.OpenUploadButton_Click);
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(147, 60);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(110, 29);
            this.UploadButton.TabIndex = 2;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(31, 104);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(51, 19);
            this.lblMessage.TabIndex = 3;
            this.lblMessage.Text = "label1";
            // 
            // CancelUploadButton
            // 
            this.CancelUploadButton.Location = new System.Drawing.Point(263, 60);
            this.CancelUploadButton.Name = "CancelUploadButton";
            this.CancelUploadButton.Size = new System.Drawing.Size(127, 29);
            this.CancelUploadButton.TabIndex = 4;
            this.CancelUploadButton.Text = "CancelUpload";
            this.CancelUploadButton.UseVisualStyleBackColor = true;
            this.CancelUploadButton.Click += new System.EventHandler(this.CancelUploadButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(30, 256);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(360, 27);
            this.textBox1.TabIndex = 5;
            // 
            // CancelDownloadButton
            // 
            this.CancelDownloadButton.Location = new System.Drawing.Point(147, 289);
            this.CancelDownloadButton.Name = "CancelDownloadButton";
            this.CancelDownloadButton.Size = new System.Drawing.Size(127, 29);
            this.CancelDownloadButton.TabIndex = 7;
            this.CancelDownloadButton.Text = "CancelUpload";
            this.CancelDownloadButton.UseVisualStyleBackColor = true;
            this.CancelDownloadButton.Click += new System.EventHandler(this.CancelDownloadButton_Click);
            // 
            // DownloadButton
            // 
            this.DownloadButton.Location = new System.Drawing.Point(31, 289);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(110, 29);
            this.DownloadButton.TabIndex = 6;
            this.DownloadButton.Text = "Download";
            this.DownloadButton.UseVisualStyleBackColor = true;
            this.DownloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // lblMessage1
            // 
            this.lblMessage1.AutoSize = true;
            this.lblMessage1.Location = new System.Drawing.Point(31, 334);
            this.lblMessage1.Name = "lblMessage1";
            this.lblMessage1.Size = new System.Drawing.Size(51, 19);
            this.lblMessage1.TabIndex = 8;
            this.lblMessage1.Text = "label2";
            // 
            // GrpcFileClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblMessage1);
            this.Controls.Add(this.CancelDownloadButton);
            this.Controls.Add(this.DownloadButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.CancelUploadButton);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.UploadButton);
            this.Controls.Add(this.OpenUploadButton);
            this.Controls.Add(this.lblUploadPath);
            this.Name = "GrpcFileClientForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label lblUploadPath;
        private System.Windows.Forms.Button OpenUploadButton;
        private System.Windows.Forms.Button UploadButton;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button CancelUploadButton;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button CancelDownloadButton;
        private System.Windows.Forms.Button DownloadButton;
        private System.Windows.Forms.Label lblMessage1;
    }
}

