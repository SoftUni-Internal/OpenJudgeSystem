﻿namespace OJS.Services.Administration.Business.Participants.GridData;

using OJS.Data.Models.Participants;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Data.Excel;
using OJS.Services.Administration.Data.Implementations;
using OJS.Services.Common.Data.Pagination;

public class ParticipantsGridDataService : GridDataService<Participant>, IParticipantsGridDataService
{
    public ParticipantsGridDataService(IParticipantsDataService dataService, ISortingService sortingService, IFilteringService filteringService, IExcelService excelService)
        : base(dataService, sortingService, filteringService, excelService)
    {
    }
}