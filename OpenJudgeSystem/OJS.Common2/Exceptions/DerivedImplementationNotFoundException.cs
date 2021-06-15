namespace OJS.Workers.Common.Exceptions
{
    using System;

    public class DerivedImplementationNotFoundException : Exception
    {
        private const string ExceptionMessage = "The method should be implemented in the derived class";

        public DerivedImplementationNotFoundException()
            : base(ExceptionMessage)
        {
        }
    }
}