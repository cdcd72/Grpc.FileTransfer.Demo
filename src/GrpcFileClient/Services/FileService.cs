using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GrpcFileClient.Models;
using GrpcFileClient.Resolvers;
using GrpcFileClient.Types;
using Infra.Core.Extensions;
using Infra.Core.FileAccess.Abstractions;
using Infra.Core.FileAccess.Models;
using Microsoft.Extensions.Logging;

namespace GrpcFileClient.Services;

public class FileService(ILogger<FileService> logger, FileAccessResolver fileAccessResolver)
{
    private readonly IFileAccess physicalFileAccess = fileAccessResolver(FileAccessType.Physical);
    private readonly IFileAccess grpcFileAccess = fileAccessResolver(FileAccessType.Grpc);

    public async Task<TransferResult<object>> FileUpload(
        List<string> filePaths,
        Action<ProgressInfo> progressCallBack = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var result = new TransferResult<object>();

        if (filePaths.Count == 0)
        {
            result.Message = "No file to upload.";
            return result;
        }

        foreach (var filePath in filePaths)
        {
            // Initiative cancel.
            if (cancellationToken.IsCancellationRequested)
            {
                result.Message = $"File upload canceled.";
                break;
            }

            if (!physicalFileAccess.FileExists(filePath))
            {
                logger.Information($"File【{filePath}】not exists.");
                continue;
            }

            try
            {
                var fileBytes = await physicalFileAccess.ReadFileAsync(filePath, progressCallBack, cancellationToken);

                await grpcFileAccess.SaveFileAsync(Path.GetFileName(filePath), fileBytes, progressCallBack, cancellationToken);
            }
            catch (Exception)
            {
                result.Message = "Connect grpc server failed or other unexpected exception happened. You can see the error log know more.";
            }
        }

        result.Message = "File upload completed.";

        return result;
    }

    public async Task<TransferResult<Dictionary<string, byte[]>>> FileDownload(
        List<string> fileNames,
        Action<ProgressInfo> progressCallBack = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var result = new TransferResult<Dictionary<string, byte[]>>();
        var downloadedFiles = new Dictionary<string, byte[]>();

        if (fileNames.Count == 0)
        {
            result.Message = "No file to download.";
            result.Record = downloadedFiles;
            return result;
        }

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
                if (await grpcFileAccess.FileExistsAsync(fileName, progressCallBack, cancellationToken))
                {
                    var fileBytes = await grpcFileAccess.ReadFileAsync(fileName, progressCallBack, cancellationToken);

                    if (fileBytes != null)
                        downloadedFiles.Add(fileName, fileBytes);
                }
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
