namespace FacefusionBE.Response
{
    namespace DorelAppBackend.Models.Responses
    {
        public class Maybe<T>
        {
            public T Data { get; set; }

            public bool IsSuccess { get; set; }

            public bool IsException { get; set; }

            public string ExceptionMessage { get; set; }

            public void SetSuccess(T data)
            {
                this.IsException = false;
                this.IsSuccess = true;
                this.Data = data;
                this.ExceptionMessage = null;
            }
            public void SetException(string exceptionMessage)
            {
                this.IsException = true;
                this.IsSuccess = false;
                this.ExceptionMessage = exceptionMessage;
            }

        }
    }

}
