namespace OJS.Services.Ui.Business
{
    using OJS.Services.Common.Models;
    using SoftUni.Services.Infrastructure;
    using System.Threading.Tasks;

    public interface IProblemGroupsBusinessService : IService
    {
        Task<ServiceResult> DeleteById(int id);
    }
}