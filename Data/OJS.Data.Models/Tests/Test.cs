namespace OJS.Data.Models.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using AutoMapper;
    using OJS.Data.Models.Problems;
    using OJS.Workers.Common.Models;
    using SoftUni.AutoMapper.Infrastructure.Models;
    using OJS.Data.Models.Common;
    using OJS.Workers.Common.Extensions;

    public class Test : Entity<int>, IOrderableEntity, IMapExplicitly
    {
        public int ProblemId { get; set; }

        public virtual Problem Problem { get; set; } = null!;

        /// <remarks>
        /// Using byte[] (compressed with zip) to save database space.
        /// </remarks>
        public byte[] InputData { get; set; } = Array.Empty<byte>();

        [NotMapped]
        public string InputDataAsString
        {
            get => this.InputData.Decompress();

            set => this.InputData = value.Compress();
        }

        /// <remarks>
        /// Using byte[] (compressed with zip) to save database space.
        /// </remarks>
        public byte[] OutputData { get; set; } = Array.Empty<byte>();

        [NotMapped]
        public string OutputDataAsString
        {
            get => this.OutputData.Decompress();

            set => this.OutputData = value.Compress();
        }

        public bool IsTrialTest { get; set; }

        public bool IsOpenTest { get; set; }

        public bool HideInput { get; set; }

        public double OrderBy { get; set; }

        public virtual ICollection<TestRun> TestRuns { get; set; } = new HashSet<TestRun>();

        public void RegisterMappings(IProfileExpression configuration)
            => configuration.CreateMap<Test, TestContext>()
                .ForMember(
                    d => d.Input,
                    opt => opt.MapFrom(d => d.InputDataAsString))
                .ForMember(
                    d => d.Output,
                    opt => opt.MapFrom(d => d.OutputDataAsString))
                .ForMember(
                    d => d.OrderBy,
                    opt => opt.MapFrom(d => (int)d.OrderBy));
    }
}