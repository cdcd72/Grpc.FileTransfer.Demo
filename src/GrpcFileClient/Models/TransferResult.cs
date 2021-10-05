namespace GrpcFileClient.Models
{
    public class TransferResult<T>
    {
        public string Message { get; set; }

        public T Record { get; set; }
    }
}
