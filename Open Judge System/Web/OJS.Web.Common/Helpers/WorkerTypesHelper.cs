using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OJS.Workers.Common.Models;

namespace OJS.Web.Common.Helpers
{
    public static class WorkerTypesHelper
    {
        public static IEnumerable<SelectListItem> GetWorkerTypes()
        {
            return Enum.GetValues(typeof(WorkerType)).Cast<WorkerType>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                }).ToList();
        }
    }
}