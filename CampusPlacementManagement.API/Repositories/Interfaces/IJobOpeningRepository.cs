using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface IJobOpeningRepository
    {
        Task<List<JobOpening>> GetAllAsync(int? companyId = null, JobStatus? status = null);

        Task<List<JobOpening>> GetPublishedJobsForCollegeAsync(int collegeId);

        Task<JobOpening?> GetByIdAsync(int jobId);

        Task AddAsync(JobOpening job);

        void Update(JobOpening job);

        void Delete(JobOpening job);

        Task SaveChangesAsync();

        // Related Collections
        Task AddTargetCollegesAsync(IEnumerable<JobTargetCollege> targetColleges);

        Task AddEligibleDepartmentsAsync(IEnumerable<JobEligibleDepartment> departments);

        Task AddEligibleGraduationYearsAsync(IEnumerable<JobEligibleGraduationYear> years);

        Task<List<JobTargetCollege>> GetTargetCollegesByJobIdAsync(int jobId);

        Task<List<JobEligibleDepartment>> GetEligibleDepartmentsByJobIdAsync(int jobId);

        Task<List<JobEligibleGraduationYear>> GetEligibleGraduationYearsByJobIdAsync(int jobId);

        void RemoveTargetColleges(IEnumerable<JobTargetCollege> targetColleges);

        void RemoveEligibleDepartments(IEnumerable<JobEligibleDepartment> departments);

        void RemoveEligibleGraduationYears(IEnumerable<JobEligibleGraduationYear> years);
    }
}
