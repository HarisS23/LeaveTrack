using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Models;
using LeaveTrack.Data;
using LeaveTrack.Services.Services;
using Microsoft.EntityFrameworkCore;

namespace LeaveTrack.Tests
{
    public class ApprovalServiceTests
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

        private Employee CreateEmployee(int id, int balance = 20)
        {
            return new Employee
            {
                Id = id,
                FirstName = "Test",
                LastName = "User",
                Email = $"test{id}@company.com",
                RoleId = (int)RoleEnum.Employee,
                YearlyLeaveBalance = balance
            };
        }

        private LeaveRequest CreatePendingRequest(
            int id,
            int employeeId,
            DateTime start,
            DateTime end,
            int leaveTypeId = (int)LeaveTypeEnum.Vacation)
        {
            return new LeaveRequest
            {
                Id = id,
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                StartDate = start,
                EndDate = end,
                LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task ApproveAsync_ValidRequest_ReturnsSuccess()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1, balance: 20));
            context.LeaveRequests.Add(CreatePendingRequest(1, 1,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(5)));
            await context.SaveChangesAsync();

            var service = new ApprovalService(context);
            var (success, error) = await service.ApproveAsync(1);

            Assert.True(success);
            Assert.Null(error);

            var updated = await context.LeaveRequests.FindAsync(1);
            Assert.Equal((int)LeaveRequestStatusEnum.Approved, updated!.LeaveRequestStatusId);
        }

        [Fact]
        public async Task ApproveAsync_AlreadyApproved_ReturnsError()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1));
            var request = CreatePendingRequest(1, 1,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(3));
            request.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Approved;
            context.LeaveRequests.Add(request);
            await context.SaveChangesAsync();

            var service = new ApprovalService(context);
            var (success, error) = await service.ApproveAsync(1);

            Assert.False(success);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task ApproveAsync_InsufficientBalance_ReturnsError()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1, balance: 3));
            context.LeaveRequests.Add(CreatePendingRequest(1, 1,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(10)));
            await context.SaveChangesAsync();

            var service = new ApprovalService(context);
            var (success, error) = await service.ApproveAsync(1);

            Assert.False(success);
            Assert.Contains("Insufficient", error);
        }

        [Fact]
        public async Task ApproveAsync_OverlappingApprovedLeave_ReturnsError()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1, balance: 20));

            var approved = CreatePendingRequest(1, 1,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(5));
            approved.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Approved;

            var overlapping = CreatePendingRequest(2, 1,
                DateTime.Today.AddDays(3),
                DateTime.Today.AddDays(7));

            context.LeaveRequests.AddRange(approved, overlapping);
            await context.SaveChangesAsync();

            var service = new ApprovalService(context);
            var (success, error) = await service.ApproveAsync(2);

            Assert.False(success);
            Assert.Contains("overlap", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ApproveAsync_NonVacationRequest_DoesNotCheckBalance()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1, balance: 0));
            context.LeaveRequests.Add(CreatePendingRequest(1, 1,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(3),
                leaveTypeId: (int)LeaveTypeEnum.Sick));
            await context.SaveChangesAsync();

            var service = new ApprovalService(context);
            var (success, error) = await service.ApproveAsync(1);

            Assert.True(success);
            Assert.Null(error);
        }

        [Fact]
        public async Task RejectAsync_WithReason_ReturnsSuccess()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1));
            context.LeaveRequests.Add(CreatePendingRequest(1, 1,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(3)));
            await context.SaveChangesAsync();

            var service = new ApprovalService(context);
            var (success, error) = await service.RejectAsync(1, "Not enough coverage.");

            Assert.True(success);
            Assert.Null(error);

            var updated = await context.LeaveRequests.FindAsync(1);
            Assert.Equal((int)LeaveRequestStatusEnum.Rejected, updated!.LeaveRequestStatusId);
            Assert.Equal("Not enough coverage.", updated.RejectionReason);
        }

        [Fact]
        public async Task RejectAsync_WithoutReason_ReturnsError()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1));
            context.LeaveRequests.Add(CreatePendingRequest(1, 1,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(3)));
            await context.SaveChangesAsync();

            var service = new ApprovalService(context);
            var (success, error) = await service.RejectAsync(1, "");

            Assert.False(success);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task RejectAsync_AlreadyRejected_ReturnsError()
        {
            using var context = CreateInMemoryContext();
            context.Employees.Add(CreateEmployee(1));
            var request = CreatePendingRequest(1, 1,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(3));
            request.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Rejected;
            request.RejectionReason = "Already rejected.";
            context.LeaveRequests.Add(request);
            await context.SaveChangesAsync();

            var service = new ApprovalService(context);
            var (success, error) = await service.RejectAsync(1, "Trying again.");

            Assert.False(success);
            Assert.NotNull(error);
        }
    }
}