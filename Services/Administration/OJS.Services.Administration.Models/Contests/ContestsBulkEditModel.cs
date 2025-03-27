namespace OJS.Services.Administration.Models.Contests;

using System;

public class ContestsBulkEditModel
{
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public string? Type { get; set; }

    public int? LimitBetweenSubmissions { get; set; }

    public int CategoryId { get; set; }
}