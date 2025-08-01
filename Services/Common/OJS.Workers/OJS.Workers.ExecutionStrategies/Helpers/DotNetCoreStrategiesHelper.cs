namespace OJS.Workers.ExecutionStrategies.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    internal static class DotNetCoreStrategiesHelper
    {
        /// <summary>
        /// Removes all "<PackageReference />" items from the given .csproj path
        /// that Include the given package names.
        /// </summary>
        /// <param name="csProjPath">Path to .csproj file.</param>
        /// <param name="packageNames">The names of the packages that should be removed (case-insensitive).</param>
        /// <param name="removeProjectReferences">Whether to remove "<ProjectReference />" items as well.</param>
        /// <param name="removeGlobalUsings">Whether to remove "<Using />" items as well.</param>
        public static void RemoveReferencesFromCsProj(string csProjPath, IEnumerable<string> packageNames, bool removeProjectReferences = false, bool removeGlobalUsings = false)
        {
            var packageNamesToLower = new HashSet<string>(
                packageNames.Select(p => p.ToLowerInvariant()));

            var csProjDoc = XDocument.Load(csProjPath);

            var ns = csProjDoc.Root?.Name.Namespace ?? XNamespace.None;

            var toRemove = csProjDoc
                .Descendants(ns + "PackageReference")
                .Where(pr =>
                {
                    var include = pr.Attribute("Include")?.Value;
                    return include != null && packageNamesToLower.Contains(include.ToLowerInvariant());
                })
                .ToList();

            if (removeProjectReferences)
            {
                toRemove.AddRange(csProjDoc.Descendants(ns + "ProjectReference"));
            }

            if (removeGlobalUsings)
            {
                toRemove.AddRange(csProjDoc.Descendants(ns + "Using")
                    .Where(u =>
                    {
                        var value = u.Attribute("Include")?.Value;
                        return value != null &&
                               packageNamesToLower.Any(pn =>
                                   value == pn ||
                                   value.StartsWith($"{pn}.", StringComparison.InvariantCultureIgnoreCase) ||
                                   value.StartsWith($"{pn}:", StringComparison.InvariantCultureIgnoreCase));
                    }));
            }

            toRemove.ForEach(x => x.Remove());

            csProjDoc.Save(csProjPath);
        }
    }
}