namespace OJS.Services.Common.Models
{
    public class ServiceResult<T> : ServiceResult
    {
        public ServiceResult(string? error)
            : base(error)
        {
        }

        public T? Data { get; private set; }

        public new static ServiceResult<T> Success(T data)
            => new(null)
            {
                Data = data,
            };
    }
}