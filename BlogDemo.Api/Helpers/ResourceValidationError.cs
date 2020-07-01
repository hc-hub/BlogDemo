namespace BlogDemo.Api.Helpers
{
    public class ResourceValidationError
    {
        public ResourceValidationError(string message, string validatorkey = "")
        {
            Message = message;
            ValidationKey = validatorkey;
        }

        public string ValidationKey { get; private set; }
        public string Message { get; private set; }
    }
}
