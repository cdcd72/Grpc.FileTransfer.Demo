using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcFileClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrpcFileClient
{
    public class FileTransfer
    {
        private readonly ILogger<FileTransfer> _logger;
        private readonly IConfiguration _config;

        public FileTransfer(ILogger<FileTransfer> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<TransferResult<List<string>>> FileUpload(
            List<string> filePaths,
            string mark,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new TransferResult<List<string>> { Message = "沒有檔案需要上傳。" };

            // No file to upload.
            if (filePaths.Count == 0)
                return await Task.Run(() => result);

            result.Message = "未能連線到伺服器。";

            var successFilePaths = new List<string>();

            FileStream fs = null;
            var startTime = DateTime.Now;
            // file chunk equal 1 megabytes.
            var chunkSize = 1024 * 1024;
            var buffer = new byte[chunkSize];
            var channel = GrpcChannel.ForAddress(_config["Url:GrpcFileServer"]);
            var client = new GrpcFileClient.File.FileClient(channel);

            try
            {
                using var call = client.Upload();

                foreach (var filePath in filePaths)
                {
                    // Initiative cancel.
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (!System.IO.File.Exists(filePath))
                    {
                        _logger.LogInformation($"檔案不存在：{filePath}。");
                        continue;
                    }

                    var reply = new UploadRequest()
                    {
                        Filename = Path.GetFileName(filePath),
                        Mark = mark
                    };

                    fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, chunkSize, useAsync: true);

                    var readTimes = 0;

                    while (true)
                    {
                        // Initiative cancel.
                        if (cancellationToken.IsCancellationRequested)
                        {
                            reply.Block = -1; // -1 means file transfer canceled.
                            reply.Content = Google.Protobuf.ByteString.Empty;
                            await call.RequestStream.WriteAsync(reply);
                            break;
                        }

                        var readSize = fs.Read(buffer, 0, buffer.Length);

                        // Transfer file chunk to server.
                        if (readSize > 0)
                        {
                            reply.Block = ++readTimes;
                            reply.Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, readSize);
                            await call.RequestStream.WriteAsync(reply);
                        }
                        // Transfer is completed.
                        else
                        {
                            _logger.LogInformation($"完成檔案【{filePath}】的上傳。");

                            reply.Block = 0;
                            reply.Content = Google.Protobuf.ByteString.Empty;
                            await call.RequestStream.WriteAsync(reply);

                            // Waiting server response.
                            await call.ResponseStream.MoveNext(cancellationToken);

                            // Record success file path.
                            if (call.ResponseStream.Current != null && call.ResponseStream.Current.Mark == mark)
                                successFilePaths.Add(filePath);

                            // Transfer next file.
                            break;
                        }
                    }

                    fs?.Close();
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    result.IsSuccess = true;
                    result.Message = $"完成檔案上傳。共計【{successFilePaths.Count}/{filePaths.Count}】，耗時：{DateTime.Now - startTime}。";

                    await call.RequestStream.WriteAsync(new UploadRequest
                    {
                        Block = -2, // -2 means all file chunk transfer completed.
                        Mark = mark
                    });
                }
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    fs?.Close();
                    result.IsSuccess = false;
                    result.Message = $"使用者取消了上傳檔案。已完成【{successFilePaths.Count}/{filePaths.Count}】，耗時：{DateTime.Now - startTime}。";
                }
                else
                {
                    result.Message = $"檔案上傳發生異常({ex.GetType()})：{ex.Message}。";
                }
            }
            finally
            {
                fs?.Dispose();
            }

            _logger.LogInformation(result.Message);

            result.Record = successFilePaths;

            // Shutdown the channel and return result.
            return await channel?.ShutdownAsync().ContinueWith(t => result);
        }

        public async Task<TransferResult<List<string>>> FileDownload(
            List<string> fileNames,
            string mark,
            string saveDirectoryPath,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new TransferResult<List<string>>() { Message = $"檔案儲存路徑不正確：{saveDirectoryPath}。" };

            if (!Directory.Exists(saveDirectoryPath))
                return await Task.Run(() => result);

            // No file to download.
            if (fileNames.Count == 0)
            {
                result.Message = "未包含任何檔案。";
                return await Task.Run(() => result);
            }

            result.Message = "未能連線到伺服器。";

            var request = new DownloadRequest() { Mark = mark };
            request.Filenames.AddRange(fileNames);

            var successFileNames = new List<string>();

            FileStream fs = null;
            var startTime = DateTime.Now;
            var savePath = string.Empty;
            var channel = GrpcChannel.ForAddress(_config["Url:GrpcFileServer"]);
            var client = new GrpcFileClient.File.FileClient(channel);

            try
            {
                using var call = client.Download(request);

                var fileContents = new List<DownloadResponse>();
                var reaponseStream = call.ResponseStream;

                while (await reaponseStream.MoveNext(cancellationToken))
                {
                    // Initiative cancel.
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // All file transfer completed. (Block = -2)
                    if (reaponseStream.Current.Block == -2)
                    {
                        result.Message = $"完成下載任務【{successFileNames.Count}/{fileNames.Count}】，耗時：{DateTime.Now - startTime}。";
                        result.IsSuccess = true;
                        break;
                    }
                    // file transfer canceled or error happened. (Block = -1)
                    else if (reaponseStream.Current.Block == -1)
                    {
                        _logger.LogInformation($"檔案【{reaponseStream.Current.Filename}】傳輸失敗！");

                        #region Clean file and reset variable

                        fileContents.Clear();
                        fs?.Close();

                        if (!string.IsNullOrEmpty(savePath) && System.IO.File.Exists(savePath))
                            System.IO.File.Delete(savePath);

                        savePath = string.Empty;

                        #endregion
                    }
                    // file transfer completed. (Block = 0)
                    else if (reaponseStream.Current.Block == 0)
                    {
                        #region Write file and reset variable

                        if (fileContents.Any())
                        {
                            fileContents.OrderBy(c => c.Block).ToList().ForEach(c => c.Content.WriteTo(fs));
                            fileContents.Clear();
                        }

                        fs?.Close();

                        successFileNames.Add(reaponseStream.Current.Filename);

                        savePath = string.Empty;

                        #endregion
                    }
                    else
                    {
                        // save path is empty means file probably coming.
                        if (string.IsNullOrEmpty(savePath))
                        {
                            savePath = Path.Combine(saveDirectoryPath, reaponseStream.Current.Filename);
                            fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite);
                        }

                        // Add current file content to list.
                        fileContents.Add(reaponseStream.Current);

                        // Collect 20 file content, then write into file stream. (current file content = 1M, but this size decide by server code...)
                        if (fileContents.Count >= 20)
                        {
                            fileContents.OrderBy(c => c.Block).ToList().ForEach(c => c.Content.WriteTo(fs));
                            fileContents.Clear();
                        }
                    }
                }

                fs?.Close();

                // if download failed, then clean exists file.
                if (!result.IsSuccess && !string.IsNullOrEmpty(savePath) && System.IO.File.Exists(savePath))
                    System.IO.File.Delete(savePath);
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    fs?.Close();
                    result.IsSuccess = false;
                    result.Message = $"使用者取消下載。已完成下載【{successFileNames.Count}/{fileNames.Count}】，耗時：{DateTime.Now - startTime}。";
                }
                else
                {
                    result.Message = $"檔案傳輸發生異常：{ex.Message}。";
                }
            }
            finally
            {
                fs?.Dispose();
            }

            result.Record = fileNames.Except(successFileNames).ToList();

            // Shutdown the channel and return result.
            return await channel?.ShutdownAsync().ContinueWith(t => result);
        }
    }
}
