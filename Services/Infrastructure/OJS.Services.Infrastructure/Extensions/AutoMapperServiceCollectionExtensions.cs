namespace OJS.Services.Infrastructure.Extensions;

using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static OJS.Common.GlobalConstants.Assemblies;

public static class AutoMapperServiceCollectionExtensions
{
    private static readonly Type MapFromType = typeof(IMapFrom<>);
    private static readonly Type MapToType = typeof(IMapTo<>);
    private static readonly Type ExplicitMapType = typeof(IMapExplicitly);

    public static IServiceCollection AddAutoMapperConfigurations<TProgram>(this IServiceCollection services)
        => services.AddAutoMapperConfigurations(typeof(TProgram));

    private static IServiceCollection AddAutoMapperConfigurations(this IServiceCollection services, Type programType)
    {
        var assemblyPrefix = programType.GetAssemblyPrefix();
        var modelsRegexPattern = string.Format(ModelsRegexPatternTemplate, assemblyPrefix);
        var infrastructureServicesRegexPattern = string.Format(InfrastructureServicesRegexPatternTemplate, assemblyPrefix);

        var mappingAssemblies = programType.Assembly
            .GetAllReferencedAssembliesWhereFullNameMatchesPatterns(modelsRegexPattern, infrastructureServicesRegexPattern)
            .Concat([programType.Assembly])
            .ToArray();

        var configuration = new MapperConfiguration(config =>
        {
            config.RegisterMappingsFrom(mappingAssemblies);
        });

        configuration.AssertConfigurationIsValid();

        var mapper = configuration.CreateMapper();
        services.AddSingleton(mapper);

        return services;
    }

    private static void RegisterMappingsFrom(
        this IMapperConfigurationExpression mapper,
        params Assembly[] assemblies)
        => assemblies
            .SelectMany(a => a.ExportedTypes)
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Type = t,
                AllMapFrom = GetMappingModels(t, MapFromType),
                AllMapTo = GetMappingModels(t, MapToType),
                ExplicitMap = t
                    .GetInterfaces()
                    .Where(i => ExplicitMapType.IsAssignableFrom(i))
                    .Select(_ => (IMapExplicitly?)Activator.CreateInstance(t))
                    .FirstOrDefault(),
            })
            .ToList()
            .ForEach(t =>
            {
                t.AllMapFrom
                    .ToList()
                    .ForEach(mapFrom => CreateMap(mapper, mapFrom, t.Type));

                t.AllMapTo
                    .ToList()
                    .ForEach(mapTo => CreateMap(mapper, t.Type, mapTo));

                t.ExplicitMap?.RegisterMappings(mapper);
            });

    private static void CreateMap(IMapperConfigurationExpression mapper, Type mapFrom, Type mapTo)
    {
        if (mapFrom.IsOpenGenericType() && mapTo.IsOpenGenericType())
        {
            // Handle generic type mappings.
            // mapper.CreateMap(typeof(MapFrom<>), typeof(MapTo<>))
            mapFrom = mapFrom.GetGenericTypeDefinition();
            mapTo = mapTo.GetGenericTypeDefinition();
        }

        mapper.CreateMap(mapFrom, mapTo);
    }

    private static IEnumerable<Type> GetMappingModels(Type source, Type mappingType)
        => source
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == mappingType)
            .Select(i => i.GetGenericArguments().First());
}