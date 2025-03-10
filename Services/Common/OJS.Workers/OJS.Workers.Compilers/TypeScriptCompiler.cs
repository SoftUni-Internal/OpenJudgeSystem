namespace OJS.Workers.Compilers;

public class TypeScriptCompiler(int processExitTimeOutMultiplier) : Compiler(processExitTimeOutMultiplier)
{
    public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        => $"{inputFile} --strict --target es2017 --module commonjs --lib es2017,dom {additionalArguments}";

    public override string RenameInputFile(string inputFile) => inputFile + ".ts";

    public override string GetOutputFileName(string inputFileName) => inputFileName.Replace(".ts", ".js");
}