namespace OJS.Workers.ExecutionStrategies.Helpers
{
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
        public static void RemoveReferencesFromCsProj(string csProjPath, string[] packageNames, bool removeProjectReferences = false)
        {
            var csProjDoc = XDocument.Load(csProjPath);

            var ns = csProjDoc.Root?.Name.Namespace ?? XNamespace.None;

            var toRemove = csProjDoc
                .Descendants(ns + "PackageReference")
                .Where(ShouldRemoveInclude)
                .ToList();

            toRemove.AddRange(csProjDoc
                .Descendants(ns + "Using")
                .Where(ShouldRemoveInclude));

            if (removeProjectReferences)
            {
                toRemove.AddRange(csProjDoc.Descendants(ns + "ProjectReference"));
            }

            toRemove.ForEach(x => x.Remove());

            csProjDoc.Save(csProjPath);

            return;

            bool ShouldRemoveInclude(XElement element)
            {
                var include = element.Attribute("Include")?.Value;
                return
                    include != null &&
                    packageNames.Any(pn =>
                       include.Equals(pn, StringComparison.OrdinalIgnoreCase) ||
                       include.StartsWith($"{pn}.", StringComparison.InvariantCultureIgnoreCase) ||
                       include.StartsWith($"{pn}:", StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}