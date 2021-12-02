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
            var successFileNames = new List<string>();

            foreach (var item in FilePathListBox.Items)
            {
                filePaths.Add($"{item}");
            }

            var result =
                await _fileService.FileUpload(filePaths, (progressInfo) =>
                {
                    UploadMessage.Text = progressInfo.Message;

                    if (progressInfo.IsCompleted)
                        successFileNames.Add(progressInfo.Result);

                }, uploadTokenSource.Token);

            UploadMessage.Text = $"{result.Message} Complete count:【{successFileNames.Count}/{filePaths.Count}】.";

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

            if (!_physicalFileAccess.DirectoryExists(downloadToPath))
                _physicalFileAccess.CreateDirectory(downloadToPath);

            var result =
                await _fileService.FileDownload(fileNames, (progressInfo) => DownloadMessage.Text = progressInfo.Message, downloadTokenSource.Token);

            var downloadToSubPath = string.Empty;
            var fileName = string.Empty;

            foreach (var file in result.Record)
            {
                // file.Key value may be:
                // 1. 123.txt
                // 2. Data\\123.txt
                downloadToSubPath = Path.Combine(downloadToPath, Path.GetDirectoryName(file.Key));
                fileName = Path.GetFileName(file.Key);

                if (!_physicalFileAccess.DirectoryExists(downloadToSubPath))
                    _physicalFileAccess.CreateDirectory(downloadToSubPath);

                await _physicalFileAccess.SaveFileAsync(Path.Combine(downloadToSubPath, fileName), file.Value);
            }

            DownloadMessage.Text = $"{result.Message} Complete count:【{result.Record.Count}/{fileNames.Count}】.";

            downloadTokenSource = null;
        }

        private void CancelDownloadButton_Click(object sender, EventArgs e) => downloadTokenSource?.Cancel();
    }
}
