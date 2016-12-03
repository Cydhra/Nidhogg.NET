using System;

namespace Nidhogg.rest.exceptions
{
    public class InvalidSessionException : Exception
    {
        internal InvalidSessionException(string message) : base(message)
        {
            ;
        }
    }
}