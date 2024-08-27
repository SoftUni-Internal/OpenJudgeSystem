﻿namespace OJS.Workers.Common
{
    using System;

    public class ProcessExecutionResult
    {
        public string ReceivedOutput { get; set; } = string.Empty;

        public string ErrorOutput { get; set; } = string.Empty;

        public int ExitCode { get; set; }

        public ProcessExecutionResultType Type { get; set; } = ProcessExecutionResultType.Success;

        public TimeSpan TimeWorked { get; set; }

        public long MemoryUsed { get; set; }

        public TimeSpan PrivilegedProcessorTime { get; set; }

        public TimeSpan UserProcessorTime { get; set; }

        public bool ProcessWasKilled { get; set; }

        public TimeSpan TotalProcessorTime => this.PrivilegedProcessorTime + this.UserProcessorTime;

        public void ApplyTimeAndMemoryOffset(int baseTimeUsed, int baseMemoryUsed)
        {
            this.MemoryUsed = this.MemoryUsed > baseMemoryUsed
                ? this.MemoryUsed - baseMemoryUsed
                : this.MemoryUsed;

            // Display the TimeWorked, when the process was killed for being too slow (TotalProcessorTime is still usually under the timeLimit when a process is killed),
            // otherwise display TotalProcessorTime, so that the final result is as close as possible to the actual worker time
            if (this.ProcessWasKilled)
            {
                this.TimeWorked = this.TimeWorked.TotalMilliseconds > baseTimeUsed
                    ? this.TimeWorked - TimeSpan.FromMilliseconds(baseTimeUsed)
                    : this.TimeWorked;
            }
            else
            {
                this.TimeWorked = this.TotalProcessorTime.TotalMilliseconds > baseTimeUsed
                    ? this.TotalProcessorTime - TimeSpan.FromMilliseconds(baseTimeUsed)
                    : this.TotalProcessorTime;
            }
        }
    }
}