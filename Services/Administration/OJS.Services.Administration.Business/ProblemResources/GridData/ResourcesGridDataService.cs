namespace OJS.Services.Administration.Business.ProblemResources.GridData;

using OJS.Data.Models.Resources;
using OJS.Services.Administration.Data.Excel;
using OJS.Services.Administration.Data.Implementations;
using OJS.Services.Common.Data;
using OJS.Services.Common.Data.Pagination;

public class ResourcesGridDataService : GridDataService<Resource>, IResourcesGridDataService
{
    public ResourcesGridDataService(IDataService<Resource> dataService, ISortingService sortingService, IFilteringService filteringService, IExcelService excelService)
        : base(dataService, sortingService, filteringService, excelService)
    {
    }
}