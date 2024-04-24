namespace OJS.Services.Administration.Models.Tests;

using AutoMapper;
using OJS.Data.Models.Tests;
using OJS.Services.Common.Models;
using SoftUni.AutoMapper.Infrastructure.Models;

public class TestAdministrationModel : BaseAdministrationModel<int>, IMapExplicitly
{
    public bool IsTrialTest { get; set; }

    public bool IsOpenTest { get; set; }

    public bool HideInput { get; set; }

    public string? Input { get; set; }

    public string? Output { get; set; }

    public string? Type { get; set; }

    public double OrderBy { get; set; }

    public int ProblemId { get; set; }

    public string? ProblemName { get; set; }

    public bool RetestProblem { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
    {
        configuration.CreateMap<Test, TestAdministrationModel>()
            .ForMember(
                tam => tam.Input,
                opt => opt.MapFrom(d => d.InputDataAsString))
            .ForMember(
                tam => tam.Output,
                opt => opt.MapFrom(d => d.OutputDataAsString))
            .ForMember(
                tam => tam.Type,
                opt => opt.MapFrom(t
                    => MapTestType(t.IsTrialTest, t.IsOpenTest)))
            .ForMember(t => t.OperationType, opt
                => opt.Ignore())
            .ForMember(tam => tam.RetestProblem, op => op.Ignore());

        configuration.CreateMap<TestAdministrationModel, Test>()
            .ForMember(
                t => t.InputDataAsString,
                opt => opt.MapFrom(tam => tam.Input))
            .ForMember(
                t => t.OutputDataAsString,
                opt => opt.MapFrom(tam => tam.Output))
            .ForMember(
                t => t.IsTrialTest,
                opt => opt.MapFrom(tam => tam.Type == "Practice"))
            .ForMember(
                t => t.IsOpenTest,
                opt => opt.MapFrom(tam => tam.Type == "Compete"))
            .ForMember(
                t => t.InputData, opt => opt.Ignore())
            .ForMember(
                t => t.Problem, opt => opt.Ignore())
            .ForMember(
                t => t.OutputData, opt => opt.Ignore())
            .ForMember(
                t => t.TestRuns, opt => opt.Ignore());
    }

    private static string? MapTestType(bool isTrialTest, bool isOpenTest) =>
        isOpenTest
            ? TestTypeEnum.Open.ToString()
            : isTrialTest
                ? TestTypeEnum.Trial.ToString()
                : TestTypeEnum.Standard.ToString();
}
