using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GrpcFileClient.Services;
using Microsoft.Extensions.Configuration;

namespace GrpcFileClient
{
    public partial class GrpcFileClientForm : Form
    {
        private readonly IConfiguration _config;
        private readonly FileService _fileService;

        public GrpcFileClientForm(IConfiguration config, FileService fileService)
        {
            InitializeComponent();

            _config = config;
            _fileService = fileService;
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

        private CancellationTokenSource uploadTokenSource;

        private async void UploadButton_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "正在上傳檔案中...";

            uploadTokenSource = new CancellationTokenSource();

            var filePaths = new List<string>();

            foreach (var item in FilePathListBox.Items)
            {
                filePaths.Add($"{item}");
            }

            var result =
                await _fileService.FileUpload(filePaths, $"{Guid.NewGuid()}", uploadTokenSource.Token);

            lblMessage.Text = result.Message;

            uploadTokenSource = null;
        }

        private void CancelUploadButton_Click(object sender, EventArgs e) => uploadTokenSource?.Cancel();

        private CancellationTokenSource downloadTokenSource;

        private async void DownloadButton_Click(object sender, EventArgs e)
        {
            lblMessage1.Text = "正在下載檔案中...";

            downloadTokenSource = new CancellationTokenSource();

            var fileNames = FileNamesTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .ToList();

            var downloadToPath = Path.Combine(_config["FileAccessSettings:Root"], _config["FileAccessSettings:Directory:Download"]);

            var result =
                await _fileService.FileDownload(fileNames, $"{Guid.NewGuid()}", downloadToPath, downloadTokenSource.Token);

            lblMessage1.Text = result.Message;

            downloadTokenSource = null;
        }

        private void CancelDownloadButton_Click(object sender, EventArgs e) => downloadTokenSource?.Cancel();
    }
}
