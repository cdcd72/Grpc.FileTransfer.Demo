using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrpcFileServer.Services
{
    public class FileService : File.FileBase
    {
        private readonly ILogger<FileService> _logger;
        private readonly IConfiguration _config;

        public FileService(ILogger<FileService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public override async Task Upload(
            IAsyncStreamReader<UploadRequest> requestStream,
            IServerStreamWriter<UploadResponse> responseStream,
            ServerCallContext context)
        {
            var filePaths = new List<string>();
            var fileContents = new List<UploadRequest>();

            FileStream fs = null;
            var startTime = DateTime.Now;
            var mark = string.Empty;
            var savePath = string.Empty;

            try
            {
                while (await requestStream.MoveNext())
                {
                    var reply = requestStream.Current;

                    mark = reply.Mark;

                    // All file transfer completed. (Block = -2)
                    if (reply.Block == -2)
                    {
                        _logger.LogInformation($"{mark}，完成上傳檔案。共計【{filePaths.Count}】個，耗時：{DateTime.Now - startTime}。");
                        break;
                    }
                    // file transfer canceled. (Block = -1)
                    else if (reply.Block == -1)
                    {
                        _logger.LogInformation($"檔案【{reply.Filename}】取消傳輸！");

                        #region Clean file and reset variable

                        fileContents.Clear();
                        fs?.Close();

                        if (!string.IsNullOrEmpty(savePath) && System.IO.File.Exists(savePath))
                            System.IO.File.Delete(savePath);

                        savePath = string.Empty;

                        #endregion

                        break;
                    }
                    // file transfer completed. (Block = 0)
                    else if (reply.Block == 0)
                    {
                        #region Write file and reset variable

                        if (fileContents.Any())
                        {
                            fileContents.OrderBy(c => c.Block).ToList().ForEach(c => c.Content.WriteTo(fs));
                            fileContents.Clear();
                        }

                        fs?.Close();

                        filePaths.Add(savePath);

                        savePath = string.Empty;

                        #endregion

                        // Tell client file transfer completed.
                        await responseStream.WriteAsync(new UploadResponse
                        {
                            Filename = reply.Filename,
                            Mark = mark
                        });
                    }
                    else
                    {
                        // save path is empty means file probably coming.
                        if (string.IsNullOrEmpty(savePath))
                        {
                            savePath = Path.Combine(_config["FileAccessSettings:Root"], reply.Filename);
                            fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite);
                            _logger.LogInformation($"{mark}，上傳檔案：{savePath}，{DateTime.UtcNow:HH:mm:ss:ffff}。");
                        }

                        // Add current file content to list.
                        fileContents.Add(reply);

                        // Collect 20 file content, then write into file stream. (current file content = 1M, but this size decide by client code...)
                        if (fileContents.Count >= 20)
                        {
                            fileContents.OrderBy(c => c.Block).ToList().ForEach(c => c.Content.WriteTo(fs));
                            fileContents.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{mark}，發生異常({ex.GetType()})：{ex.Message}。");
            }
            finally
            {
                fs?.Dispose();
            }
        }

        public override async Task Download(
            DownloadRequest request,
            IServerStreamWriter<DownloadResponse> responseStream,
            ServerCallContext context)
        {
            var successFileNames = new List<string>();

            FileStream fs = null;
            var startTime = DateTime.Now;
            var mark = request.Mark;
            // file chunk equal 1 megabytes.
            var chunkSize = 1024 * 1024;
            var buffer = new byte[chunkSize];

            try
            {
                for (var i = 0; i < request.Filenames.Count; i++)
                {
                    var fileName = request.Filenames[i];
                    var filePath = Path.Combine(_config["FileAccessSettings:Root"], fileName);
                    var reply = new DownloadResponse
                    {
                        Filename = fileName,
                        Mark = mark
                    };

                    _logger.LogInformation($"{mark}，下載檔案：{filePath}。");

                    if (System.IO.File.Exists(filePath))
                    {
                        fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, chunkSize, useAsync: true);

                        var readTimes = 0;

                        while (true)
                        {
                            var readSise = fs.Read(buffer, 0, buffer.Length);

                            // Transfer file chunk to client.
                            if (readSise > 0)
                            {
                                reply.Block = ++readTimes;
                                reply.Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, readSise);
                                await responseStream.WriteAsync(reply);
                            }
                            // Transfer is completed.
                            else
                            {
                                reply.Block = 0;
                                reply.Content = Google.Protobuf.ByteString.Empty;
                                await responseStream.WriteAsync(reply);
                                successFileNames.Add(fileName);
                                _logger.LogInformation($"{mark}，完成傳送檔案：{filePath}。");
                                break;
                            }
                        }

                        fs?.Close();
                    }
                    else
                    {
                        _logger.LogInformation($"檔案【{filePath}】不存在！");
                        reply.Block = -1; // -1 means file not exists, like file transfer canceled situation.
                        await responseStream.WriteAsync(reply);
                    }
                }

                // Tell client file transfer completed.
                await responseStream.WriteAsync(new DownloadResponse
                {
                    Filename = string.Empty,
                    Block = -2, // -2 means all file chunk transfer completed.
                    Content = Google.Protobuf.ByteString.Empty,
                    Mark = mark
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{mark}，發生異常({ex.GetType()})：{ex.Message}。");
            }
            finally
            {
                fs?.Dispose();
            }

            _logger.LogInformation($"{mark}，檔案傳輸完成。共計【{successFileNames.Count / request.Filenames.Count}】，耗時：{DateTime.Now - startTime}。");
        }
    }
}
