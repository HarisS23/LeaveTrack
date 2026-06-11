using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Models;
using LeaveTrack.Data;
using LeaveTrack.Services.Services;
using Microsoft.EntityFrameworkCore;

namespace LeaveTrack.Tests
{
    public class LeaveRequestServiceTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            context.LeaveTypes.AddRange(
                new LeaveType { Id = (int)LeaveTypeEnum.Vacation, Name = "Vacation", PrettyName = "Vacation" },
                new LeaveType { Id = (int)LeaveTypeEnum.Sick, Name = "Sick", PrettyName = "Sick Leave" },
                new LeaveType { Id = (int)LeaveTypeEnum.Personal, Name = "Personal", PrettyName = "Personal Leave" }
            );

            context.LeaveRequestStatuses.AddRange(
                new LeaveRequestStatus { Id = (int)LeaveRequestStatusEnum.Pending, Name = "Pending", PrettyName = "Pending" },
                new LeaveRequestStatus { Id = (int)LeaveRequestStatusEnum.Approved, Name = "Approved", PrettyName = "Approved" },
                new LeaveRequestStatus { Id = (int)LeaveRequestStatusEnum.Rejected, Name = "Rejected", PrettyName = "Rejected" },
                new LeaveRequestStatus { Id = (int)LeaveRequestStatusEnum.Cancelled, Name = "Cancelled", PrettyName = "Cancelled" }
            );

            context.Roles.AddRange(
                new Role { Id = (int)RoleEnum.Employee, Name = "Employee", PrettyName = "Employee" },
                new Role { Id = (int)RoleEnum.Manager, Name = "Manager", PrettyName = "Manager" },
                new Role { Id = (int)RoleEnum.HR, Name = "HR", PrettyName = "HR / Administrator" }
            );

            context.SaveChanges();
            return context;
        }

        private Employee CreateEmployee(int id)
        {
            return new Employee
            {
                Id = id,
                FirstName = "Test",
                LastName = "User",
                Email = $"test{id}@company.com",
                RoleId = (int)RoleEnum.Employee,
                YearlyLeaveBalance = 20
            };
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_SavesCorrectly()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1));
            await context.SaveChangesAsync();

            var service = new LeaveRequestService(context);

            var request = new LeaveRequest
            {
                EmployeeId = 1,
                LeaveTypeId = (int)LeaveTypeEnum.Vacation,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(5),
                LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending
            };

            await service.CreateAsync(request);

            var saved = await context.LeaveRequests.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal((int)LeaveRequestStatusEnum.Pending, saved.LeaveRequestStatusId);
        }

        [Fact]
        public async Task CancelAsync_PendingRequest_SetsCancelledStatus()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1));
            context.LeaveRequests.Add(new LeaveRequest
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = (int)LeaveTypeEnum.Vacation,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3),
                LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var service = new LeaveRequestService(context);
            await service.CancelAsync(1);

            var updated = await context.LeaveRequests.FindAsync(1);
            Assert.Equal((int)LeaveRequestStatusEnum.Cancelled, updated!.LeaveRequestStatusId);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsRequest()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1));
            context.LeaveRequests.Add(new LeaveRequest
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = (int)LeaveTypeEnum.Vacation,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3),
                LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var service = new LeaveRequestService(context);
            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            using var context = CreateInMemoryContext();
            var service = new LeaveRequestService(context);
            var result = await service.GetByIdAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task FilterAsync_ByStatus_ReturnsCorrectRequests()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1));
            context.LeaveRequests.AddRange(
                new LeaveRequest
                {
                    Id = 1,
                    EmployeeId = 1,
                    LeaveTypeId = (int)LeaveTypeEnum.Vacation,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(3),
                    LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending,
                    CreatedAt = DateTime.UtcNow
                },
                new LeaveRequest
                {
                    Id = 2,
                    EmployeeId = 1,
                    LeaveTypeId = (int)LeaveTypeEnum.Vacation,
                    StartDate = DateTime.Today.AddDays(5),
                    EndDate = DateTime.Today.AddDays(7),
                    LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Approved,
                    CreatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();

            var service = new LeaveRequestService(context);
            var result = await service.FilterAsync(
                null,
                (int)LeaveRequestStatusEnum.Pending,
                null, null, null);

            Assert.Single(result);
            Assert.All(result, r =>
                Assert.Equal((int)LeaveRequestStatusEnum.Pending, r.LeaveRequestStatusId));
        }
    }
}