﻿namespace ProniaMVC.Utilities.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message):base(message)
        {
            
        }
        public NotFoundException():base("404 NOT FOUND")
        {
            
        }

    }
}
