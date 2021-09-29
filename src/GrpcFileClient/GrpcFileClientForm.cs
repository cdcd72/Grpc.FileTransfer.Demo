using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace GrpcFileClient
{
    public partial class GrpcFileClientForm : Form
    {
        private readonly IConfiguration _config;
        private readonly FileTransfer _fileTransfer;

        public GrpcFileClientForm(IConfiguration config, FileTransfer fileTransfer)
        {
            InitializeComponent();

            _config = config;
            _fileTransfer = fileTransfer;
        }

        private void OpenUploadButton_Click(object sender, EventArgs e)
        {
            FilePathListBox.Items.Clear();
            FilePathListBox.Items.AddRange(GetFilePaths());
        }

        private string[] GetFilePaths()
        {
            openFileDialog.Multiselect = true;

            var result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
                return openFileDialog.FileNames;

            return Array.Empty<string>();
        }

        CancellationTokenSource uploadTokenSource;

        private async void UploadButton_Click(object sender, EventArgs e)
        {
            lblMessage.Text = string.Empty;

            uploadTokenSource = new CancellationTokenSource();

            var filePaths = new List<string>();

            foreach (var item in FilePathListBox.Items)
            {
                filePaths.Add($"{item}");
            }

            var result =
                await _fileTransfer.FileUpload(filePaths, $"{Guid.NewGuid()}", uploadTokenSource.Token);

            lblMessage.Text = result.Message;

            uploadTokenSource = null;
        }

        private void CancelUploadButton_Click(object sender, EventArgs e) => uploadTokenSource?.Cancel();

        CancellationTokenSource downloadTokenSource;

        private async void DownloadButton_Click(object sender, EventArgs e)
        {
            lblMessage1.Text = string.Empty;

            downloadTokenSource = new CancellationTokenSource();

            var fileNames = FileNamesTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .ToList();

            var downloadToPath = Path.Combine(_config["FileAccessSettings:Root"], _config["FileAccessSettings:Directory:Download"]);

            var result =
                await _fileTransfer.FileDownload(fileNames, $"{Guid.NewGuid()}", downloadToPath, downloadTokenSource.Token);

            lblMessage1.Text = result.Message;

            downloadTokenSource = null;
        }

        private void CancelDownloadButton_Click(object sender, EventArgs e) => downloadTokenSource?.Cancel();
    }
}
