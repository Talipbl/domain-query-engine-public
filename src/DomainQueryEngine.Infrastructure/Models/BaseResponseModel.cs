namespace DomainQueryEngine.Infrastructure.Models
{
    public class BaseResponseModel<T>
    {
        public List<T> Result { get; set; }
    }
}
