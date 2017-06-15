namespace Library.Exceptions
{
    using System;

    internal class InstantiationException : Exception
    {
        public InstantiationException(string message) : base(message)
        {
        }
    }
}