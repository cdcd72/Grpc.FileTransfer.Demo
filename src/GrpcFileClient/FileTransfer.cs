using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcFileClient.Models;
using GrpcFileServer;
using Microsoft.Extensions.Logging;

namespace GrpcFileClient
{
    public class FileTransfer
    {
        private readonly ILogger<FileTransfer> _logger;

        public FileTransfer(ILogger<FileTransfer> logger)
        {
            _logger = logger;
        }

        public async Task<TransferResult<List<string>>> FileUpload(List<string> filesPath, string mark, System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken())
        {
            var result = new TransferResult<List<string>> { Message = "沒有檔案需要下載" };
            if (filesPath.Count == 0)
            {
                return await Task.Run(() => result);//沒有檔案需要下載
            }
            result.Message = "未能連線到伺服器。";
            var lstSuccFiles = new List<string>();//傳輸成功的檔案
            int chunkSize = 1024 * 1024;
            byte[] buffer = new byte[chunkSize];//每次傳送的大小
            FileStream fs = null;//檔案流
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new GrpcFileServer.File.FileClient(channel);
            DateTime startTime = DateTime.Now;
            try
            {
                using (var stream = client.Upload())//連線上傳檔案的客戶端
                {
                    //reply.Block數字的含義是伺服器和客戶端約定的
                    foreach (var filePath in filesPath)//遍歷集合
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;//取消了傳輸
                        var reply = new UploadRequest()
                        {
                            Filename = Path.GetFileName(filePath),
                            Mark = mark
                        };
                        if (!System.IO.File.Exists(filePath))//檔案不存在，繼續下一輪的傳送
                        {
                            _logger.LogInformation($"檔案不存在：{filePath}");//寫入日誌
                            continue;
                        }
                        fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, chunkSize, useAsync: true);
                        int readTimes = 0;
                        while (true)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                reply.Block = -1;//取消了傳輸
                                reply.Content = Google.Protobuf.ByteString.Empty;
                                await stream.RequestStream.WriteAsync(reply);//傳送取消傳輸的命令
                                break;//取消了傳輸
                            }
                            int readSize = fs.Read(buffer, 0, buffer.Length);//讀取資料
                            if (readSize > 0)
                            {
                                reply.Block = ++readTimes;//更新標記，傳送資料
                                reply.Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, readSize);
                                await stream.RequestStream.WriteAsync(reply);
                            }
                            else
                            {
                                _logger.LogInformation($"完成檔案【{filePath}】的上傳。");
                                reply.Block = 0;//傳送本次檔案傳送結束的標記
                                reply.Content = Google.Protobuf.ByteString.Empty;
                                await stream.RequestStream.WriteAsync(reply);//傳送結束標記
                                                                             //等待伺服器回傳
                                await stream.ResponseStream.MoveNext(cancellationToken);
                                if (stream.ResponseStream.Current != null && stream.ResponseStream.Current.Mark == mark)
                                {
                                    lstSuccFiles.Add(filePath);//記錄成功的檔案
                                }
                                break;//傳送下一個檔案
                            }
                        }
                        fs?.Close();
                    }
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        result.IsSuccessful = true;
                        result.Message = $"完成檔案上傳。共計【{lstSuccFiles.Count}/{filesPath.Count}】，耗時:{DateTime.Now - startTime}";

                        await stream.RequestStream.WriteAsync(new UploadRequest
                        {
                            Block = -2,//傳輸結束
                            Mark = mark
                        });//傳送結束標記
                    }
                }
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    fs?.Close();//釋放檔案流
                    result.IsSuccessful = false;
                    result.Message = $"使用者取消了上傳檔案。已完成【{lstSuccFiles.Count}/{filesPath.Count}】，耗時:{DateTime.Now - startTime}";
                }
                else
                {
                    result.Message = $"檔案上傳發生異常({ex.GetType()})：{ex.Message}";
                }
            }
            finally
            {
                fs?.Dispose();
            }
            _logger.LogInformation(result.Message);
            result.Tag = lstSuccFiles;
            //關閉通訊、並返回結果
            return await channel?.ShutdownAsync().ContinueWith(t => result);
        }

        public async Task<TransferResult<List<string>>> FileDownload(List<string> fileNames, string mark, string saveDirectoryPath, System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken())
        {
            var result = new TransferResult<List<string>>() { Message = $"檔案儲存路徑不正確：{saveDirectoryPath}" };
            if (!System.IO.Directory.Exists(saveDirectoryPath))
            {
                return await Task.Run(() => result);//檔案路徑不存在
            }
            if (fileNames.Count == 0)
            {
                result.Message = "未包含任何檔案";
                return await Task.Run(() => result);//檔案路徑不存在
            }
            result.Message = "未能連線到伺服器";
            var request = new DownloadRequest() { Mark = mark };//請求資料
            request.Filenames.AddRange(fileNames);//將需要下載的檔名賦值
            var lstSuccFiles = new List<string>();//傳輸成功的檔案
            string savePath = string.Empty;//儲存路徑
            System.IO.FileStream fs = null;
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new GrpcFileServer.File.FileClient(channel);
            DateTime startTime = DateTime.Now;
            try
            {
                using (var call = client.Download(request))
                {
                    var lstContents = new List<DownloadResponse>();//存放接收的資料
                    var reaponseStream = call.ResponseStream;
                    //reaponseStream.Current.Block數字的含義是伺服器和客戶端約定的
                    while (await reaponseStream.MoveNext(cancellationToken))//開始接收資料
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        if (reaponseStream.Current.Block == -2)//說明檔案已經傳輸完成了
                        {
                            result.Message = $"完成下載任務【{lstSuccFiles.Count}/{fileNames.Count}】，耗時：{DateTime.Now - startTime}";
                            result.IsSuccessful = true;
                            break;
                        }
                        else if (reaponseStream.Current.Block == -1)//當前檔案傳輸錯誤
                        {
                            _logger.LogInformation($"檔案【{reaponseStream.Current.Filename}】傳輸失敗！");//寫入日誌
                            lstContents.Clear();
                            fs?.Close();//釋放檔案流
                            if (!string.IsNullOrEmpty(savePath) && System.IO.File.Exists(savePath))//如果傳輸不成功，刪除該檔案
                            {
                                System.IO.File.Delete(savePath);
                            }
                            savePath = string.Empty;
                        }
                        else if (reaponseStream.Current.Block == 0)//當前檔案傳輸完成
                        {
                            if (lstContents.Any())//如果還有資料，就寫入檔案
                            {
                                lstContents.OrderBy(c => c.Block).ToList().ForEach(c => c.Content.WriteTo(fs));
                                lstContents.Clear();
                            }
                            lstSuccFiles.Add(reaponseStream.Current.Filename);//傳輸成功的檔案
                            fs?.Close();//釋放檔案流
                            savePath = string.Empty;
                        }
                        else//有檔案資料過來
                        {
                            if (string.IsNullOrEmpty(savePath))//如果位元組流為空，則說明時新的檔案資料來了
                            {
                                savePath = Path.Combine(saveDirectoryPath, reaponseStream.Current.Filename);
                                fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite);
                            }
                            lstContents.Add(reaponseStream.Current);//加入連結串列
                            if (lstContents.Count() >= 20)//每個包1M，20M為一個集合，一起寫入資料。
                            {
                                lstContents.OrderBy(c => c.Block).ToList().ForEach(c => c.Content.WriteTo(fs));
                                lstContents.Clear();
                            }
                        }
                    }
                }
                fs?.Close();//釋放檔案流
                if (!result.IsSuccessful && !string.IsNullOrEmpty(savePath) && System.IO.File.Exists(savePath))//如果傳輸不成功，那麼久刪除該檔案
                {
                    System.IO.File.Delete(savePath);
                }
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    fs?.Close();//釋放檔案流
                    result.IsSuccessful = false;
                    result.Message = $"使用者取消下載。已完成下載【{lstSuccFiles.Count}/{fileNames.Count}】，耗時：{DateTime.Now - startTime}";
                }
                else
                {
                    result.Message = $"檔案傳輸發生異常：{ex.Message}";
                }
            }
            finally
            {
                fs?.Dispose();
            }
            result.Tag = fileNames.Except(lstSuccFiles).ToList();//獲取失敗檔案集合
                                                                 //關閉通訊、並返回結果
            return await channel?.ShutdownAsync().ContinueWith(t => result);
        }
    }
}
