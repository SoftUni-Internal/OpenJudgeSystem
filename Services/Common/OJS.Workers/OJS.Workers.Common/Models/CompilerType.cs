namespace OJS.Workers.Common.Models
{
    public enum CompilerType
    {
        // Commented out compilers are deprecated, but left here to preserve order,
        // as modifying values will require database migration.
        None = 0,
        // CSharp = 1,
        // MsBuild = 2,
        CPlusPlusGcc = 3,
        Java = 4,
        JavaZip = 5,
        // MsBuildLibrary = 6,
        CPlusPlusZip = 7,
        JavaInPlaceCompiler = 8,
        DotNetCompiler = 9,
        CSharpDotNetCore = 10,
        // SolidityCompiler = 11,
        GolangCompiler = 12,
        TypeScriptCompiler = 13,
    }
}