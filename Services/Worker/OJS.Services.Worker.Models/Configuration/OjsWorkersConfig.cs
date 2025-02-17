namespace OJS.Services.Worker.Models.Configuration;

using OJS.Services.Infrastructure.Configurations;
using System.ComponentModel.DataAnnotations;

public class OjsWorkersConfig : BaseConfig
{
    public override string SectionName => "OjsWorkersConfig";

    [Required]
    public string CSharpDotNet3CoreCompilerPath { get; set; } = string.Empty;

    [Required]
    public string CSharpDotNetCore5CompilerPath { get; set; } = string.Empty;

    [Required]
    public string CSharpDotNetCore6CompilerPath { get; set; } = string.Empty;

    [Required]
    public string DotNetCompilerPath { get; set; } = string.Empty;

    [Required]
    public string DotNetCore3RuntimeVersion { get; set; } = string.Empty;

    [Required]
    public string DotNetCore5RuntimeVersion { get; set; } = string.Empty;

    [Required]
    public string DotNetCore6RuntimeVersion { get; set; } = string.Empty;

    [Required]
    public string DotNetCore3SharedAssembliesPath { get; set; } = string.Empty;

    [Required]
    public string DotNetCore5SharedAssembliesPath { get; set; } = string.Empty;

    [Required]
    public string DotNetCore6SharedAssembliesPath { get; set; } = string.Empty;

    [Required]
    public string MavenPath { get; set; } = string.Empty;

    [Required]
    public string CPlusPlusGccCompilerPath { get; set; } = string.Empty;

    [Required]
    public string GolangCompilerPath { get; set; } = string.Empty;

    [Required]
    public string JavaCompilerPath { get; set; } = string.Empty;

    [Required]
    public string JavaExecutablePath { get; set; } = string.Empty;

    [Required]
    public string JavaLibsPath { get; set; } = string.Empty;

    [Required]
    public string Java21CompilerPath { get; set; } = string.Empty;

    [Required]
    public string Java21LibsPath { get; set; } = string.Empty;

    [Required]
    public string Java21ExecutablePath { get; set; } = string.Empty;

    [Required]
    public string JavaSpringAndHibernateStrategyPomFilePath { get; set; } = string.Empty;

    [Required]
    public string Java21SpringAndHibernateStrategyPomFilePath { get; set; } = string.Empty;

    [Required]
    public string NodeJsExecutablePath { get; set; } = string.Empty;

    [Required]
    public string NodeJs20ExecutablePath { get; set; } = string.Empty;

    [Required]
    public string TypeScriptExecutablePath { get; set; } = string.Empty;

    [Required]
    public string NodeResourcesPathPlaceholder { get; set; } = string.Empty;

    [Required]
    public string NodeResourcesPath { get; set; } = string.Empty;

    [Required]
    public string Node20ResourcesPath { get; set; } = string.Empty;

    [Required]
    public string JsProjNodeModules { get; set; } = string.Empty;

    [Required]
    public string MochaModulePath { get; set; } = string.Empty;

    [Required]
    public string ChaiModulePath { get; set; } = string.Empty;

    [Required]
    public string PlaywrightChromiumModulePath { get; set; } = string.Empty;

    [Required]
    public string JsDomModulePath { get; set; } = string.Empty;

    [Required]
    public string JQueryModulePath { get; set; } = string.Empty;

    [Required]
    public string HandlebarsModulePath { get; set; } = string.Empty;

    [Required]
    public string SinonModulePath { get; set; } = string.Empty;

    [Required]
    public string SinonJsDomModulePath { get; set; } = string.Empty;

    [Required]
    public string SinonChaiModulePath { get; set; } = string.Empty;

    [Required]
    public string UnderscoreModulePath { get; set; } = string.Empty;

    [Required]
    public string BabelCoreModulePath { get; set; } = string.Empty;

    [Required]
    public string ReactJsxPluginPath { get; set; } = string.Empty;

    [Required]
    public string ReactModulePath { get; set; } = string.Empty;

    [Required]
    public string ReactDomModulePath { get; set; } = string.Empty;

    [Required]
    public string NodeFetchModulePath { get; set; } = string.Empty;

    [Required]
    public string BootstrapModulePath { get; set; } = string.Empty;

    [Required]
    public string BootstrapCssPath { get; set; } = string.Empty;

    [Required]
    public string PythonExecutablePath { get; set; } = string.Empty;

    [Required]
    public string SqlServerLocalDbMasterDbConnectionString { get; set; } = string.Empty;

    [Required]
    public string SqlServerLocalDbRestrictedUserId { get; set; } = string.Empty;

    [Required]
    public string SqlServerLocalDbRestrictedUserPassword { get; set; } = string.Empty;

    [Required]
    public string MySqlSysDbConnectionString { get; set; } = string.Empty;

    [Required]
    public string MySqlRestrictedUserId { get; set; } = string.Empty;

    [Required]
    public string MySqlRestrictedUserPassword { get; set; } = string.Empty;

    [Required]
    public string PythonExecutablePathV311 { get; set; } = string.Empty;

    [Required]
    public string PipExecutablePathV311 { get; set; } = string.Empty;

    [Required]
    public string PostgreSqlMasterDbConnectionString { get; set; } = string.Empty;

    [Required]
    public string PostgreSqlRestrictedUserId { get; set; } = string.Empty;

    [Required]
    public string PostgreSqlRestrictedUserPassword { get; set; } = string.Empty;

    public int NodeJsBaseTimeUsedInMilliseconds { get; set; }

    public int NodeJsBaseMemoryUsedInBytes { get; set; }

    public int MsBuildBaseTimeUsedInMilliseconds { get; set; }

    public int MsBuildBaseMemoryUsedInBytes { get; set; }

    public int DotNetCscBaseTimeUsedInMilliseconds { get; set; }

    public int DotNetCscBaseMemoryUsedInBytes { get; set; }

    public int DotNetCliBaseTimeUsedInMilliseconds { get; set; }

    public int DotNetCliBaseMemoryUsedInBytes { get; set; }

    public int GolangBaseTimeUsedInMilliseconds { get; set; }

    public int GolangBaseMemoryUsedInBytes { get; set; }

    public int JavaBaseTimeUsedInMilliseconds { get; set; }

    public int JavaBaseMemoryUsedInBytes { get; set; }

    public int JavaBaseUpdateTimeOffsetInMilliseconds { get; set; }

    public int GPlusPlusBaseTimeUsedInMilliseconds { get; set; }

    public int GPlusPlusBaseMemoryUsedInBytes { get; set; }

    public int PythonBaseTimeUsedInMilliseconds { get; set; }

    public int PythonBaseMemoryUsedInBytes { get; set; }

    public int PythonV311BaseTimeUsedInMilliseconds { get; set; }

    public int PythonV311InstallPackagesTimeUsedInMilliseconds { get; set; }

    public int PythonV311BaseMemoryUsedInBytes { get; set; }

    // Compiler time out multipliers
    public int CPlusPlusCompilerProcessExitTimeOutMultiplier { get; set; } = 1;

    public int CPlusPlusZipCompilerProcessExitTimeOutMultiplier { get; set; } = 1;

    public int CSharpDotNetCoreCompilerProcessExitTimeOutMultiplier { get; set; } = 1;

    public int DotNetCompilerProcessExitTimeOutMultiplier { get; set; } = 1;

    public int GolangCompilerProcessExitTimeOutMultiplier { get; set; } = 1;

    public int JavaCompilerProcessExitTimeOutMultiplier { get; set; } = 1;

    public int JavaInPlaceCompilerProcessExitTimeOutMultiplier { get; set; } = 1;

    public int JavaZipCompilerProcessExitTimeOutMultiplier { get; set; } = 1;
}