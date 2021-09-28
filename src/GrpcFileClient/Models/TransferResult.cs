namespace GrpcFileClient.Models
{
    public class TransferResult<T>
    {
        /// <summary>
        /// 傳輸是否成功
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 標記型別
        /// </summary>
        public T Tag { get; set; }
    }
}
