using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GrpcFileClient.Resolvers;
using GrpcFileClient.Services;
using GrpcFileClient.Types;
using Infra.Core.FileAccess.Abstractions;
using Microsoft.Extensions.Configuration;

namespace GrpcFileClient
{
    public partial class GrpcFileClientForm : Form
    {
        private readonly IConfiguration _config;
        private readonly FileService _fileService;
        private readonly IFileAccess _physicalFileAccess;

        public GrpcFileClientForm(IConfiguration config, FileService fileService, FileAccessResolver fileAccessResolver)
        {
            InitializeComponent();

            _config = config;
            _fileService = fileService;
            _physicalFileAccess = fileAccessResolver(FileAccessType.Physical);
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
            UploadMessage.Text = "Currently upload file...";

            uploadTokenSource = new CancellationTokenSource();

            var filePaths = new List<string>();
            var successFilePaths = new List<string>();

            foreach (var item in FilePathListBox.Items)
            {
                filePaths.Add($"{item}");
            }

            var result =
                await _fileService.FileUpload(filePaths, (progressInfo) =>
                {
                    UploadMessage.Text = progressInfo.Message;

                    if (progressInfo.IsCompleted)
                        successFilePaths.Add(progressInfo.FilePath);

                }, uploadTokenSource.Token);

            UploadMessage.Text = $"{result.Message} Complete count:【{successFilePaths.Count}/{filePaths.Count}】.";

            uploadTokenSource = null;
        }

        private void CancelUploadButton_Click(object sender, EventArgs e) => uploadTokenSource?.Cancel();

        private CancellationTokenSource downloadTokenSource;

        private async void DownloadButton_Click(object sender, EventArgs e)
        {
            DownloadMessage.Text = "Currently download file...";

            downloadTokenSource = new CancellationTokenSource();

            var fileNames = FileNamesTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .ToList();

            var downloadToPath = Path.Combine(_config["FileAccessSettings:Root"], _config["FileAccessSettings:Directory:Download"]);

            var result =
                await _fileService.FileDownloadTest(fileNames, (progressInfo) => DownloadMessage.Text = progressInfo.Message, downloadTokenSource.Token);

            foreach (var file in result.Record)
            {
                await _physicalFileAccess.SaveFileAsync(Path.Combine(downloadToPath, file.Key), file.Value);
            }

            DownloadMessage.Text = $"{result.Message} Complete count:【{result.Record.Count}/{fileNames.Count}】.";

            downloadTokenSource = null;
        }

        private void CancelDownloadButton_Click(object sender, EventArgs e) => downloadTokenSource?.Cancel();
    }
}
