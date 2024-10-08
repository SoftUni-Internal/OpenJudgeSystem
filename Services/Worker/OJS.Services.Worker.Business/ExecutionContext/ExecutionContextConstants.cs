﻿namespace OJS.Services.Worker.Business.ExecutionContext;

using static OJS.Common.GlobalConstants.FileExtensions;

public static class ExecutionContextConstants
{
    public const string DefaultAllowedFileExtension = Zip;

    public static class TemplatePlaceholders
    {
        public const string CodePlaceholder = "##code##";
    }
}