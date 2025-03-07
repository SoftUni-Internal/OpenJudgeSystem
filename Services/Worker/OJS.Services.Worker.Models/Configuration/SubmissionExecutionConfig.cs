namespace OJS.Services.Worker.Models.Configuration
{
    using OJS.Services.Infrastructure.Configurations;

    public class SubmissionExecutionConfig : BaseConfig
    {
        public override string SectionName => "SubmissionExecution";

        public int OutputResultMaxLength { get; set; } = 1000;

        public int SubmissionVerboseLogFileMaxBytes { get; set; } = 10 * 1024 * 1024; // 10 MB

        public TimeConfig TimeConfig { get; set; } = new TimeConfig();

        public MemoryConfig MemoryConfig { get; set; } = new MemoryConfig();
    }
}
