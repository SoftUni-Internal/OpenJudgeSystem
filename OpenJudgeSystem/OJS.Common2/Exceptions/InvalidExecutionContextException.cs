namespace OJS.Workers.Common.Exceptions
{
    using System;

    public class InvalidExecutionContextException<TInput, TResult> : Exception
        where TResult : ISingleCodeRunResult, new()
    {
        private const string DefaultMessage =
            "Execution context input and/or Execution result output types are invalid for the execution type.";

        public InvalidExecutionContextException(
            IExecutionContext<TInput> executionContext,
            IExecutionResult<TResult> executionResult,
            string message = null)
            : base(ConstructMessage(executionContext, executionResult, message))
        {
        }

        private static string GenericArgumentTypeName(Type type) => type.GetGenericArguments()[0]?.Name;

        private static string ConstructMessage(
            IExecutionContext<TInput> executionContext,
            IExecutionResult<TResult> executionResult,
            string message = null)
            => (message ?? DefaultMessage) +
                Environment.NewLine +
                $"Provided input type: {GenericArgumentTypeName(executionContext.GetType())}" +
                Environment.NewLine +
                $"Provided output type: {GenericArgumentTypeName(executionResult.GetType())}";
    }
}
