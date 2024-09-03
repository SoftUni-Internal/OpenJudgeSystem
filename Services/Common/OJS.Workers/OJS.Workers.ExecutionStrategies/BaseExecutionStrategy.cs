﻿namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Exceptions;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;

    public abstract class BaseExecutionStrategy<TSettings> : IExecutionStrategy
        where TSettings : BaseExecutionStrategySettings
    {
        private readonly ILogger<BaseExecutionStrategy<TSettings>> logger;

        protected BaseExecutionStrategy(
            IOjsSubmission submission,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
        {
            this.logger = logger;

            this.Type = submission.ExecutionStrategyType;
            this.Settings = settingsProvider.GetSettings<TSettings>(submission)
                ?? throw new ArgumentException(
                    $"Cannot get settings of type {typeof(TSettings).Name} for execution strategy {this.Type}.",
                    nameof(settingsProvider));
        }

        protected ExecutionStrategyType Type { get; }

        protected TSettings Settings { get; }

        protected string WorkingDirectory { get; set; } = string.Empty;

        public async Task<IExecutionResult<TResult>> SafeExecute<TInput, TResult>(
            IExecutionContext<TInput> executionContext,
            int submissionId)
            where TResult : ISingleCodeRunResult, new()
        {
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectoryForExecutionStrategy();
            this.logger.LogExecutionStrategyCreatedWorkingDirectory(this.Type.ToString(), this.WorkingDirectory, submissionId);

            try
            {
                executionContext.Code = this.PreprocessCode(executionContext);
                return await this.InternalExecute(executionContext, new ExecutionResult<TResult>());
                // Catch logic is handled by the caller
            }
            finally
            {
                // Use another thread for deletion of the working directory,
                // because we don't want the execution flow to wait for the cleanup
                await Task.Run(() =>
                {
                    try
                    {
                        DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
                    }
                    catch (Exception ex)
                    {
                        // Sometimes deletion of the working directory cannot be done,
                        // because the process has not released it yet, which is nondeterministic.
                        // Problems in the deletion of leftover files should not break the execution flow,
                        // because the execution is already completed and results are generated.
                        // Only log the exception and continue.
                        this.logger.LogSafeDeleteDirectoryException(ex);
                    }
                });
            }
        }

        protected virtual Task<IExecutionResult<OutputResult>> ExecuteAgainstSimpleInput(
            IExecutionContext<SimpleInputModel> executionContext,
            IExecutionResult<OutputResult> result)
            => throw new DerivedImplementationNotFoundException();

        protected virtual Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
            => throw new DerivedImplementationNotFoundException();

        protected virtual async Task<IExecutionResult<TResult>> InternalExecute<TInput, TResult>(
            IExecutionContext<TInput> executionContext,
            IExecutionResult<TResult> result)
            where TResult : ISingleCodeRunResult, new()
        {
            if (executionContext is IExecutionContext<SimpleInputModel> stringInputExecutionContext &&
                result is IExecutionResult<OutputResult> outputResult)
            {
                return (IExecutionResult<TResult>)await this.ExecuteAgainstSimpleInput(
                    stringInputExecutionContext,
                    outputResult);
            }

            if (executionContext is IExecutionContext<TestsInputModel> testsExecutionContext &&
                result is IExecutionResult<TestResult> testsResult)
            {
                return (IExecutionResult<TResult>)await this.ExecuteAgainstTestsInput(
                    testsExecutionContext,
                    testsResult);
            }

            throw new InvalidExecutionContextException<TInput, TResult>(executionContext, result);
        }

        protected virtual string PreprocessCode<TInput>(
            IExecutionContext<TInput> executionContext)
            => executionContext.Code;
    }

    public abstract record BaseExecutionStrategySettings : IExecutionStrategySettings
    {
    }
}
