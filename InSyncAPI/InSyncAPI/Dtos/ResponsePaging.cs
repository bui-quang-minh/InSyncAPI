namespace InSyncAPI.Dtos
{
    public class ResponsePaging<T>
    {
        public T data { get; set; }
        public int totalOfData { get; set; }
    }
}
