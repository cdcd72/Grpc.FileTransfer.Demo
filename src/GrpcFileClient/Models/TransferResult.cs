namespace GrpcFileClient.Models
{
    public class TransferResult<T>
    {
        /// <summary>
        /// 傳輸是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 記錄
        /// </summary>
        public T Record { get; set; }
    }
}
