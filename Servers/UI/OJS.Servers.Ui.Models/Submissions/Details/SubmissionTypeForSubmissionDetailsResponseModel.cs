﻿namespace OJS.Servers.Ui.Models.Submissions.Details;

using OJS.Services.Ui.Models.Submissions;
using SoftUni.AutoMapper.Infrastructure.Models;

public class
    SubmissionTypeForSubmissionDetailsResponseModel : IMapFrom<SubmissionTypeForSubmissionDetailsServiceModel>
{
    public bool AllowBinaryFilesUpload { get; set; }

    public string Name { get; set; } = string.Empty;
}