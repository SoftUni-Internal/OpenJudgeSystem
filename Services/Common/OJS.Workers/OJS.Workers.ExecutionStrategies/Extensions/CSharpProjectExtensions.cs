﻿#nullable disable
namespace OJS.Workers.ExecutionStrategies.Extensions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Microsoft.Build.Evaluation;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Exceptions;

    internal static class CSharpProjectExtensions
    {
        private const string MicrosoftCsProjXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private const string VsToolsImportProjectPath = @"$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets";
        private const string EnsureNuGetPackageBuildImportsTargetName = "EnsureNuGetPackageBuildImports";

        private static readonly string NuGetXmlNodeXPath = $@"//msns:Target[@Name='{EnsureNuGetPackageBuildImportsTargetName}']";
        private static readonly string VsToolsXmlNodeXPath = $@"//msns:Import[@Project='{VsToolsImportProjectPath}']";

        public static void RemoveNuGetPackageImportsTarget(this Project project)
        {
            if (project.Targets.ContainsKey(EnsureNuGetPackageBuildImportsTargetName))
            {
                RemoveXmlNodeFromCsProj(project.FullPath, NuGetXmlNodeXPath);
            }
        }

        public static void RemoveItemByName(this Project project, string itemName)
        {
            var item = project.Items.FirstOrDefault(pi =>
                pi.EvaluatedInclude.Contains(itemName));

            if (item != null)
            {
                project.RemoveItem(item);
            }
        }

        public static void AddReferences(this Project project, params string[] references)
        {
            RemoveExistingReferences(project, references);

            var referenceMetaData = new Dictionary<string, string>
            {
                { "SpecificVersion", "False" },
                { "Private", "True" },
            };

            foreach (var reference in references)
            {
                project.AddItem("Reference", reference, referenceMetaData);
            }
        }

        public static void AddCompileItems(this Project project, IEnumerable<string> itemNames)
        {
            foreach (var itemName in itemNames)
            {
                project.AddItem("Compile", $"{itemName}{Constants.cSharpFileExtension}");
            }
        }

        public static void EnsureAssemblyNameIsCorrect(this Project project)
        {
            var assemblyNameProperty = project.AllEvaluatedProperties.FirstOrDefault(x => x.Name == "AssemblyName");
            if (assemblyNameProperty == null)
            {
                throw new SolutionException("The .csproj file is missing or it does not contain the 'Assembly Name' property!");
            }

            var csProjFullpath = project.FullPath;
            var projectName = Path.GetFileNameWithoutExtension(csProjFullpath);
            project.SetProperty("AssemblyName", projectName);
        }

        private static void RemoveExistingReferences(Project project, IEnumerable<string> references)
        {
            foreach (var reference in references)
            {
                var referenceName = reference[..reference.IndexOf(',')];
                var existingReference = project.Items.FirstOrDefault(x => x.EvaluatedInclude.Contains(referenceName));
                if (existingReference != null)
                {
                    project.RemoveItem(existingReference);
                }
            }
        }

        private static void RemoveXmlNodeFromCsProj(string csprojPath, string xpathExpression)
        {
            var csprojXml = new XmlDocument();
            csprojXml.Load(csprojPath);
            var namespaceManager = new XmlNamespaceManager(csprojXml.NameTable);
            namespaceManager.AddNamespace("msns", MicrosoftCsProjXmlNamespace);

            var rootNode = csprojXml.DocumentElement;

            if (rootNode is null)
            {
                throw new SolutionException("The .csproj file is empty or its structure is incorrect / corrupted.");
            }

            var targetNode = rootNode.SelectSingleNode(xpathExpression, namespaceManager);
            rootNode.RemoveChild(targetNode);
            csprojXml.Save(csprojPath);
        }
    }
}
