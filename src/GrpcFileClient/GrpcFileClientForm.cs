using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrpcFileClient
{
    public partial class GrpcFileClientForm : Form
    {
        private readonly FileTransfer _fileTransfer;

        public GrpcFileClientForm(FileTransfer fileTransfer)
        {
            InitializeComponent();

            _fileTransfer = fileTransfer;
        }

        private void OpenUploadButton_Click(object sender, EventArgs e)
        {
            lblUploadPath.Text = GetFilePath();
        }

        private string GetFilePath()
        {
            // Create OpenFileDialog 
            var dlg = openFileDialog1;

            // Set filter for file extension and default file extension 
            dlg.Title = "選擇檔案";
            dlg.Filter = "所有檔案(*.*)|*.*";
            dlg.FileName = "選擇資料夾.";
            dlg.FilterIndex = 1;
            dlg.ValidateNames = false;
            dlg.CheckFileExists = false;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;//允許同時選擇多個檔案 

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == DialogResult.OK)
            {
                // Open document 
                return dlg.FileName;
            }

            return string.Empty;
        }

        CancellationTokenSource uploadTokenSource;

        private async void UploadButton_Click(object sender, EventArgs e)
        {
            lblMessage.Text = string.Empty;

            uploadTokenSource = new CancellationTokenSource();
            var fileNames = new List<string>();
            fileNames.Add(lblUploadPath.Text);
            var result = await _fileTransfer.FileUpload(fileNames, "123", uploadTokenSource.Token);

            lblMessage.Text = result.Message;

            uploadTokenSource = null;
        }

        private void CancelUploadButton_Click(object sender, EventArgs e)
        {
            uploadTokenSource?.Cancel();
        }

        CancellationTokenSource downloadTokenSource;

        private async void DownloadButton_Click(object sender, EventArgs e)
        {
            lblMessage1.Text = string.Empty;

            downloadTokenSource = new CancellationTokenSource();
            var fileNames = new List<string>();
            fileNames.Add(textBox1.Text);
            var result = await _fileTransfer.FileDownload(fileNames, "123", @"D:\Output\File\Download", downloadTokenSource.Token);

            lblMessage1.Text = result.Message;

            downloadTokenSource = null;
        }

        private void CancelDownloadButton_Click(object sender, EventArgs e)
        {
            downloadTokenSource?.Cancel();
        }
    }
}
