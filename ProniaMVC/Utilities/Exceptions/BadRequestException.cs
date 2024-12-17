namespace ProniaMVC.Utilities.Exceptions
{
    public class BadRequestException:Exception
    {
        public BadRequestException(string message) : base(message)
        {

        }
        public BadRequestException() : base("BAD REQUEST")
        {

        }
    }
}
