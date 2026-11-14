using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(int applicationId);

        Task<JobApplication?> GetByStudentAndJobAsync(int studentId, int jobId);

        Task<List<JobApplication>> GetByStudentIdAsync(int studentId);

        Task<List<JobApplication>> GetByJobIdAsync(int jobId, ApplicationStatus? status = null);

        Task<List<JobApplication>> GetByCompanyIdAsync(int companyId, ApplicationStatus? status = null);

        Task<List<JobApplication>> GetByCollegeIdAsync(int collegeId, ApplicationStatus? status = null);

        Task<List<JobApplication>> GetAllAsync(ApplicationStatus? status = null);

        Task AddAsync(JobApplication application);

        void Update(JobApplication application);

        Task AddStatusHistoryAsync(ApplicationStatusHistory history);

        Task<List<ApplicationStatusHistory>> GetStatusHistoryByApplicationIdAsync(int applicationId);

        Task SaveChangesAsync();
    }
}
