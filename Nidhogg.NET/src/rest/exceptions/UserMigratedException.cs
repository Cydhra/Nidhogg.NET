using System;

namespace Nidhogg.rest.exceptions
{
    public class UserMigratedException : Exception
    {
        internal UserMigratedException(string message) : base(message)
        {
            ;
        }
    }
}