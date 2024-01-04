﻿namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    using static OJS.Workers.Common.Constants;

    public class BaseInterpretedCodeExecutionStrategy : BaseCodeExecutionStrategy
    {
        protected BaseInterpretedCodeExecutionStrategy(
            IProcessExecutorFactory processExecutorFactory,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(processExecutorFactory, baseTimeUsed, baseMemoryUsed)
        {
        }

        protected static string PrepareTestInput(string testInput)
            => string.Join(
                Environment.NewLine,
                testInput.Split(new[] { NewLineUnix, NewLineWin }, StringSplitOptions.None));

        protected override Task<IExecutionResult<TResult>> InternalExecute<TInput, TResult>(
            IExecutionContext<TInput> executionContext,
            IExecutionResult<TResult> result)
        {
            result.IsCompiledSuccessfully = true;

            return base.InternalExecute(executionContext, result);
        }
    }
}
