using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface IRecruitmentDriveRepository
    {
        Task<RecruitmentDrive?> GetByIdAsync(int driveId);

        Task<List<RecruitmentDrive>> GetAllAsync(
            int? companyId = null,
            int? collegeId = null,
            int? jobId = null,
            RecruitmentDriveStatus? status = null);

        Task<List<RecruitmentDrive>> GetDrivesForCollegeAsync(int collegeId, RecruitmentDriveStatus? status = null);

        Task<List<RecruitmentDrive>> GetDrivesForCompanyAsync(int companyId, RecruitmentDriveStatus? status = null);

        Task<List<RecruitmentDrive>> GetDrivesForJobAsync(int jobId, RecruitmentDriveStatus? status = null);

        Task AddAsync(RecruitmentDrive drive);

        void Update(RecruitmentDrive drive);

        void Delete(RecruitmentDrive drive);

        Task SaveChangesAsync();
    }
}
