using System.Net;

namespace SimpleApp.BaseModel
{
    public class Response<T>
    {
        public bool Success
        {
            get { return !Errors.Any(); }
        }

        public string Message { get; set; }
        public IReadOnlyDictionary<string, string[]> ValidationErrors { get; set; }
        public T Data { get; set; }
        public IList<Error> Errors { get; set; } = new List<Error>();
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string HelpUrl { get; set; }
        public string FAQUrl { get; set; }
        public bool CheckStatus { get; set; }
        public string ClientName { get; set; }
    }

    public class Error
    {
        public string Message { get; set; }
        public string ErrorType { get; set; }
    }

}
