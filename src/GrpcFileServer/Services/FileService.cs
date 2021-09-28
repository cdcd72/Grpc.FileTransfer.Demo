using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcFileServer.Services
{
    public class FileService : File.FileBase
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger) => _logger = logger;

        public override async Task Upload(IAsyncStreamReader<UploadRequest> requestStream, IServerStreamWriter<UploadResponse> responseStream, ServerCallContext context)
        {
            var lstFilesName = new List<string>(); // 檔名
            var lstContents = new List<UploadRequest>(); // 資料集合

            FileStream fs = null;
            var startTime = DateTime.Now; // 開始時間
            var mark = string.Empty;
            var savePath = string.Empty;

            try
            {
                // reply.Block 數字的含義是伺服器和客戶端約束的
                while (await requestStream.MoveNext()) // 讀取資料
                {
                    var reply = requestStream.Current;

                    mark = reply.Mark;

                    if (reply.Block == -2) // 傳輸完成
                    {
                        _logger.LogInformation($"{mark}，完成上傳檔案。共計【{lstFilesName.Count}】個，耗時：{DateTime.Now - startTime}");
                        break;
                    }
                    else if (reply.Block == -1) // 取消了傳輸
                    {
                        _logger.LogInformation($"檔案【{reply.Filename}】取消傳輸！");
                        lstContents.Clear();
                        fs?.Close(); // 釋放檔案流
                        if (!string.IsNullOrEmpty(savePath) && System.IO.File.Exists(savePath)) // 如果傳輸不成功，刪除該檔案
                        {
                            System.IO.File.Delete(savePath);
                        }
                        savePath = string.Empty;
                        break;
                    }
                    else if (reply.Block == 0) // 檔案傳輸完成
                    {
                        if (lstContents.Any()) // 如果還有資料，就寫入檔案
                        {
                            lstContents.OrderBy(c => c.Block).ToList().ForEach(c => c.Content.WriteTo(fs));
                            lstContents.Clear();
                        }
                        lstFilesName.Add(savePath); // 傳輸成功的檔案
                        fs?.Close(); // 釋放檔案流
                        savePath = string.Empty;

                        // 告知客戶端，已經完成傳輸
                        await responseStream.WriteAsync(new UploadResponse
                        {
                            Filename = reply.Filename,
                            Mark = mark
                        });
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(savePath)) // 有新檔案來了
                        {
                            savePath = Path.Combine(@"D:\Output\File", reply.Filename); // 檔案路徑
                            fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite);
                            _logger.LogInformation($"{mark}，上傳檔案：{savePath}，{DateTime.UtcNow:HH:mm:ss:ffff}");
                        }
                        lstContents.Add(reply); // 加入資料集合串列
                        if (lstContents.Count >= 20) // 每個包 1M，20M 為一個集合，一起寫入資料。
                        {
                            lstContents.OrderBy(c => c.Block).ToList().ForEach(c => c.Content.WriteTo(fs));
                            lstContents.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{mark}，發生異常({ex.GetType()})：{ex.Message}");
            }
            finally
            {
                fs?.Dispose();
            }
        }

        public override async Task Download(DownloadRequest request, IServerStreamWriter<DownloadResponse> responseStream, ServerCallContext context)
        {
            var lstSuccFiles = new List<string>(); // 傳輸成功的檔案
            var startTime = DateTime.Now; // 傳輸檔案的起始時間
            var chunkSize = 1024 * 1024; // 每次讀取的資料
            var buffer = new byte[chunkSize]; // 資料緩衝區
            FileStream fs = null; // 檔案流

            try
            {
                // reply.Block 數字的含義是伺服器和客戶端約束的
                for (var i = 0; i < request.Filenames.Count; i++)
                {
                    var fileName = request.Filenames[i]; // 檔名
                    var filePath = Path.Combine(@"D:\Output\File", fileName); // 檔案路徑
                    var reply = new DownloadResponse
                    {
                        Filename = fileName,
                        Mark = request.Mark
                    }; // 應答資料
                    _logger.LogInformation($"{request.Mark}，下載檔案：{filePath}");
                    if (System.IO.File.Exists(filePath))
                    {
                        fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, chunkSize, useAsync: true);

                        // fs.Length 可以告訴客戶端所傳檔案大小
                        var readTimes = 0; // 讀取次數
                        while (true)
                        {
                            var readSise = fs.Read(buffer, 0, buffer.Length); // 讀取資料
                            if (readSise > 0) // 讀取到了資料，有資料需要傳送
                            {
                                reply.Block = ++readTimes;
                                reply.Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, readSise);
                                await responseStream.WriteAsync(reply);
                            }
                            else // 沒有資料了，就告訴對方，讀取完了
                            {
                                reply.Block = 0;
                                reply.Content = Google.Protobuf.ByteString.Empty;
                                await responseStream.WriteAsync(reply);
                                lstSuccFiles.Add(fileName);
                                _logger.LogInformation($"{request.Mark}，完成傳送檔案：{filePath}");
                                break;
                            }
                        }
                        fs?.Close();
                    }
                    else
                    {
                        _logger.LogInformation($"檔案【{filePath}】不存在。");
                        reply.Block = -1; // -1 的標記為檔案不存在
                        await responseStream.WriteAsync(reply); // 告訴客戶端，檔案狀態
                    }
                }
                // 告訴客戶端，檔案傳輸完成
                await responseStream.WriteAsync(new DownloadResponse
                {
                    Filename = string.Empty,
                    Block = -2, // 告訴客戶端，檔案已經傳輸完成
                    Content = Google.Protobuf.ByteString.Empty,
                    Mark = request.Mark
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{request.Mark}，發生異常({ex.GetType()})：{ex.Message}");
            }
            finally
            {
                fs?.Dispose();
            }
            _logger.LogInformation($"{request.Mark}，檔案傳輸完成。共計【{lstSuccFiles.Count / request.Filenames.Count}】，耗時：{DateTime.Now - startTime}");
        }
    }
}
