using CampusPlacementManagement.API.DTOs.Student;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(
            IStudentRepository studentRepository,
            ICollegeRepository collegeRepository,
            IPlacementOfficerRepository officerRepository,
            UserManager<ApplicationUser> userManager)
        {
            _studentRepository = studentRepository;
            _collegeRepository = collegeRepository;
            _officerRepository = officerRepository;
            _userManager = userManager;
        }

        // Get all students with cross-college security
        public async Task<List<StudentResponseDto>> GetAllAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isPlacementOfficer,
            int? collegeId = null,
            string? department = null,
            int? graduationYear = null)
        {
            int? targetCollegeId = collegeId;

            // Placement officers can ONLY view students belonging to their own college
            if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return new List<StudentResponseDto>();
                }

                targetCollegeId = officer.CollegeId;
            }

            var students = await _studentRepository.GetAllAsync(
                targetCollegeId,
                department,
                graduationYear);

            return students.Select(MapToResponseDto).ToList();
        }

        // Get student by ID with role and resource checks
        public async Task<StudentResponseDto?> GetByIdAsync(
            int studentId,
            string currentUserId,
            bool isSuperAdmin,
            bool isPlacementOfficer,
            bool isStudent)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return null;
            }

            // Student can only view their own record
            if (isStudent && student.UserId != currentUserId)
            {
                return null;
            }

            // Placement officer can only view students belonging to their own college
            if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != student.CollegeId)
                {
                    return null;
                }
            }

            return MapToResponseDto(student);
        }

        // Get student by ApplicationUser ID
        public async Task<StudentResponseDto?> GetByUserIdAsync(string userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);
            return student == null ? null : MapToResponseDto(student);
        }

        // Create new student
        public async Task<string> CreateAsync(
            CreateStudentDto request,
            string currentUserId,
            bool isSuperAdmin)
        {
            int targetCollegeId;
            int targetOfficerId;

            if (isSuperAdmin)
            {
                if (!request.CollegeId.HasValue || !request.PlacementOfficerId.HasValue)
                {
                    return "CollegeId and PlacementOfficerId are required for SuperAdmin.";
                }

                var college = await _collegeRepository.GetByIdAsync(request.CollegeId.Value);
                if (college == null)
                {
                    return "College not found.";
                }

                if (college.Status != CollegeStatus.Approved)
                {
                    return "Cannot register student to an unapproved college.";
                }

                var officer = await _officerRepository.GetByIdAsync(request.PlacementOfficerId.Value);
                if (officer == null)
                {
                    return "Placement officer not found.";
                }

                if (officer.CollegeId != request.CollegeId.Value)
                {
                    return "The specified placement officer does not belong to the selected college.";
                }

                targetCollegeId = request.CollegeId.Value;
                targetOfficerId = request.PlacementOfficerId.Value;
            }
            else
            {
                // Must be PlacementOfficer creating for their own college
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return "Placement officer profile not found.";
                }

                if (officer.Status != AccountStatus.Approved)
                {
                    return "Your placement officer account is not approved.";
                }

                targetCollegeId = officer.CollegeId;
                targetOfficerId = officer.PlacementOfficerId;
            }

            // Check duplicate Register Number within this college
            var existingReg = await _studentRepository.GetByRegisterNumberAsync(
                targetCollegeId,
                request.RegisterNumber);

            if (existingReg != null)
            {
                return "Register number already exists for this college.";
            }

            // Check duplicate email
            var existingUser = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (existingUser != null)
            {
                return "Email is already registered.";
            }

            // Create Identity ApplicationUser account
            var user = new ApplicationUser
            {
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return string.Join("; ", createResult.Errors.Select(e => e.Description));
            }

            // Assign Student role
            var roleResult = await _userManager.AddToRoleAsync(user, "Student");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return string.Join("; ", roleResult.Errors.Select(e => e.Description));
            }

            // Create Student profile record
            var student = new Student
            {
                UserId = user.Id,
                CollegeId = targetCollegeId,
                PlacementOfficerId = targetOfficerId,
                RegisterNumber = request.RegisterNumber.Trim().ToUpperInvariant(),
                Department = request.Department.Trim(),
                Degree = request.Degree.Trim(),
                GraduationYear = request.GraduationYear,
                CGPA = request.CGPA,
                ActiveBacklogs = request.ActiveBacklogs,
                ResumeUrl = request.ResumeUrl?.Trim(),
                Status = AccountStatus.Pending
            };

            await _studentRepository.AddAsync(student);
            await _studentRepository.SaveChangesAsync();

            return "Student registered successfully. Awaiting approval.";
        }

        // Update student details
        public async Task<string> UpdateAsync(
            int studentId,
            UpdateStudentDto request,
            string currentUserId,
            bool isSuperAdmin)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return "Student not found.";
            }

            // Prevent unauthorized cross-college access
            if (!isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != student.CollegeId)
                {
                    return "You are not authorized to update students from another college.";
                }
            }

            // Update user contact details
            if (student.User != null)
            {
                student.User.FullName = request.FullName.Trim();
                student.User.PhoneNumber = request.PhoneNumber.Trim();
                student.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(student.User);
            }

            // Update academic and profile details
            student.Department = request.Department.Trim();
            student.Degree = request.Degree.Trim();
            student.GraduationYear = request.GraduationYear;
            student.CGPA = request.CGPA;
            student.ActiveBacklogs = request.ActiveBacklogs;
            student.ResumeUrl = request.ResumeUrl?.Trim();

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();

            return "Student updated successfully.";
        }

        // Update student resume URL (called by the student themselves)
        public async Task<string> UpdateResumeAsync(string userId, UpdateResumeDto request)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);
            if (student == null)
            {
                return "Student profile not found.";
            }

            student.ResumeUrl = request.ResumeUrl.Trim();

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();

            return "Resume URL updated successfully.";
        }

        // Approve student
        public async Task<string> ApproveAsync(int studentId, string currentUserId, bool isSuperAdmin)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return "Student not found.";
            }

            // Cross-college validation
            if (!isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != student.CollegeId)
                {
                    return "You are not authorized to approve students from another college.";
                }
            }

            if (student.Status == AccountStatus.Approved)
            {
                return "Student is already approved.";
            }

            student.Status = AccountStatus.Approved;
            student.ApprovedAt = DateTime.UtcNow;

            if (student.User != null)
            {
                student.User.AccountStatus = AccountStatus.Approved;
                student.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(student.User);
            }

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();

            return "Student approved successfully.";
        }

        // Reject student
        public async Task<string> RejectAsync(int studentId, string currentUserId, bool isSuperAdmin)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return "Student not found.";
            }

            // Cross-college validation
            if (!isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != student.CollegeId)
                {
                    return "You are not authorized to reject students from another college.";
                }
            }

            if (student.Status == AccountStatus.Rejected)
            {
                return "Student is already rejected.";
            }

            student.Status = AccountStatus.Rejected;

            if (student.User != null)
            {
                student.User.AccountStatus = AccountStatus.Rejected;
                student.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(student.User);
            }

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();

            return "Student rejected successfully.";
        }

        // Suspend student
        public async Task<string> SuspendAsync(int studentId, string currentUserId, bool isSuperAdmin)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return "Student not found.";
            }

            // Cross-college validation
            if (!isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != student.CollegeId)
                {
                    return "You are not authorized to suspend students from another college.";
                }
            }

            if (student.Status == AccountStatus.Suspended)
            {
                return "Student is already suspended.";
            }

            student.Status = AccountStatus.Suspended;

            if (student.User != null)
            {
                student.User.AccountStatus = AccountStatus.Suspended;
                student.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(student.User);
            }

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();

            return "Student suspended successfully.";
        }

        // Reactivate student
        public async Task<string> ReactivateAsync(int studentId, string currentUserId, bool isSuperAdmin)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return "Student not found.";
            }

            // Cross-college validation
            if (!isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != student.CollegeId)
                {
                    return "You are not authorized to reactivate students from another college.";
                }
            }

            if (student.Status == AccountStatus.Approved)
            {
                return "Student is already active.";
            }

            student.Status = AccountStatus.Approved;

            if (student.User != null)
            {
                student.User.AccountStatus = AccountStatus.Approved;
                student.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(student.User);
            }

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();

            return "Student reactivated successfully.";
        }

        // Delete student safely
        public async Task<string> DeleteAsync(int studentId, string currentUserId, bool isSuperAdmin)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return "Student not found.";
            }

            // Cross-college validation
            if (!isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != student.CollegeId)
                {
                    return "You are not authorized to delete students from another college.";
                }
            }

            try
            {
                var user = student.User;
                _studentRepository.Delete(student);
                await _studentRepository.SaveChangesAsync();

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                return "Student deleted successfully.";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return "Student cannot be deleted because related job application records exist. Suspend the account instead.";
            }
        }

        // Helper: Map entity to clean response DTO
        private static StudentResponseDto MapToResponseDto(Student student)
        {
            return new StudentResponseDto
            {
                StudentId = student.StudentId,
                UserId = student.UserId,
                FullName = student.User?.FullName ?? string.Empty,
                Email = student.User?.Email ?? string.Empty,
                PhoneNumber = student.User?.PhoneNumber ?? string.Empty,
                CollegeId = student.CollegeId,
                CollegeName = student.College?.CollegeName ?? string.Empty,
                CollegeCode = student.College?.CollegeCode ?? string.Empty,
                PlacementOfficerId = student.PlacementOfficerId,
                PlacementOfficerName = student.PlacementOfficer?.User?.FullName ?? string.Empty,
                RegisterNumber = student.RegisterNumber,
                Department = student.Department,
                Degree = student.Degree,
                GraduationYear = student.GraduationYear,
                CGPA = student.CGPA,
                ActiveBacklogs = student.ActiveBacklogs,
                ResumeUrl = student.ResumeUrl,
                Status = student.Status,
                ApprovedAt = student.ApprovedAt,
                CreatedAt = student.User?.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
