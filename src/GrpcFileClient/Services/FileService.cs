using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GrpcFileClient.Models;
using GrpcFileClient.Resolvers;
using GrpcFileClient.Types;
using Infra.Core.FileAccess.Abstractions;
using Infra.Core.FileAccess.Models;
using Microsoft.Extensions.Logging;

namespace GrpcFileClient.Services
{
    public class FileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IFileAccess _physicalFileAccess;
        private readonly IFileAccess _grpcFileAccess;

        public FileService(ILogger<FileService> logger, FileAccessResolver fileAccessResolver)
        {
            _logger = logger;
            _physicalFileAccess = fileAccessResolver(FileAccessType.Physical);
            _grpcFileAccess = fileAccessResolver(FileAccessType.Grpc);
        }

        public async Task<TransferResult<object>> FileUpload(
            List<string> filePaths,
            Action<ProgressInfo> progressCallBack = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new TransferResult<object>();

            if (filePaths.Count == 0)
            {
                result.Message = "No file to upload.";
                return await Task.Run(() => result);
            }

            foreach (var filePath in filePaths)
            {
                // Initiative cancel.
                if (cancellationToken.IsCancellationRequested)
                {
                    result.Message = $"File upload canceled.";
                    break;
                }

                if (!_physicalFileAccess.FileExists(filePath))
                {
                    _logger.LogInformation($"File【{filePath}】not exists.");
                    continue;
                }

                try
                {
                    await _grpcFileAccess.SaveFileAsync(filePath, progressCallBack, cancellationToken);
                }
                catch (Exception)
                {
                    result.Message = "Connect grpc server failed or other unexpected exception happened. You can see the error log know more.";
                }
            }

            result.Message = "File upload completed.";

            return result;
        }

        public async Task<TransferResult<Dictionary<string, byte[]>>> FileDownloadTest(
            List<string> fileNames,
            Action<ProgressInfo> progressCallBack = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new TransferResult<Dictionary<string, byte[]>>();

            if (fileNames.Count == 0)
            {
                result.Message = "No file to download.";
                return await Task.Run(() => result);
            }

            var downloadedFiles = new Dictionary<string, byte[]>();

            foreach (var fileName in fileNames)
            {
                // Initiative cancel.
                if (cancellationToken.IsCancellationRequested)
                {
                    result.Message = $"File download canceled.";
                    break;
                }

                try
                {
                    var fileBytes = await _grpcFileAccess.ReadFileAsync(fileName, progressCallBack, cancellationToken);

                    if (fileBytes != null)
                        downloadedFiles.Add(fileName, fileBytes);
                }
                catch (Exception)
                {
                    result.Message = "Connect grpc server failed or other unexpected exception happened. You can see the error log know more.";
                }
            }

            result.Message = "File download completed.";
            result.Record = downloadedFiles;

            return result;
        }
    }
}
