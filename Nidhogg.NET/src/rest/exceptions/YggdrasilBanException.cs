using System;

namespace Nidhogg.rest.exceptions
{
    public class YggdrasilBanException : Exception
    {
        internal YggdrasilBanException(string message) : base(message)
        {
            ;
        }
    }
}