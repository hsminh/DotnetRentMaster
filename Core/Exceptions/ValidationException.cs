namespace RentMaster.Core.Exceptions
{
    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }

        public ValidationException(string field, string message) : base("Validation error")
        {
            Errors = new Dictionary<string, string>
            {
                { field, message }
            };
        }

        public ValidationException(Dictionary<string, string> errors)
            : base("Validation error")
        {
            Errors = errors;
        }
    }
}