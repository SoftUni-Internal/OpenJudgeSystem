#nullable disable
namespace OJS.Workers.Compilers
{
    using System.IO;
    using System.Text;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;

    public class DotNetCompiler : Compiler
    {
        public DotNetCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

        public override bool ShouldDeleteSourceFile => false;

        public override string ChangeOutputFileAfterCompilation(string outputFile)
        {
            var compiledFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(outputFile));
            var rootDir = Path.GetDirectoryName(outputFile);

            if (rootDir is null)
            {
                throw new FileNotFoundException("The compiled submission file was not found!");
            }

            var compiledFile = FileHelpers.FindFileMatchingPattern(
                    rootDir,
                    $"{compiledFileName}{Constants.ClassLibraryFileExtension}");

            return compiledFile;
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();
            arguments.Append("build ");
            arguments.Append($"-o {this.CompilationDirectory} ");
            arguments.Append($"\"{inputFile}\" ");
            arguments.Append(additionalArguments);
            return arguments.ToString().Trim();
        }
    }
}