using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Models;
using LeaveTrack.Data;
using LeaveTrack.Services.Services;
using Microsoft.EntityFrameworkCore;

namespace LeaveTrack.Tests
{
    public class EmployeeServiceTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetRemainingLeaveBalanceAsync_NoApprovedRequests_ReturnsFullBalance()
        {
            using var context = CreateInMemoryContext();

            var employee = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "john@company.com",
                RoleId = (int)RoleEnum.Employee,
                YearlyLeaveBalance = 20
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);
            var balance = await service.GetRemainingLeaveBalanceAsync(1);

            Assert.Equal(20, balance);
        }

        [Fact]
        public async Task GetRemainingLeaveBalanceAsync_WithApprovedVacation_ReturnsReducedBalance()
        {
            using var context = CreateInMemoryContext();

            var employee = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "john@company.com",
                RoleId = (int)RoleEnum.Employee,
                YearlyLeaveBalance = 20
            };

            var approved = new LeaveRequest
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = (int)LeaveTypeEnum.Vacation,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(5), 
                LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Approved,
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            context.LeaveRequests.Add(approved);
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);
            var balance = await service.GetRemainingLeaveBalanceAsync(1);

            Assert.Equal(15, balance); 
        }

        [Fact]
        public async Task GetRemainingLeaveBalanceAsync_SickLeaveNotCounted_ReturnsFullBalance()
        {
            using var context = CreateInMemoryContext();

            var employee = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "john@company.com",
                RoleId = (int)RoleEnum.Employee,
                YearlyLeaveBalance = 20
            };

            var sickLeave = new LeaveRequest
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = (int)LeaveTypeEnum.Sick,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3),
                LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Approved,
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            context.LeaveRequests.Add(sickLeave);
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);
            var balance = await service.GetRemainingLeaveBalanceAsync(1);

            Assert.Equal(20, balance); 
        }

        [Fact]
        public async Task CreateAsync_NewEmployee_SavesCorrectly()
        {
            using var context = CreateInMemoryContext();

            var service = new EmployeeService(context);

            var employee = new Employee
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@company.com",
                RoleId = (int)RoleEnum.Employee,
                YearlyLeaveBalance = 20
            };

            await service.CreateAsync(employee);

            var saved = await context.Employees.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal("Jane", saved.FirstName);
            Assert.Equal("Doe", saved.LastName);
        }

        [Fact]
        public async Task DeleteAsync_ExistingEmployee_RemovesFromDatabase()
        {
            using var context = CreateInMemoryContext();

            var employee = new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "john@company.com",
                RoleId = (int)RoleEnum.Employee,
                YearlyLeaveBalance = 20
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);
            await service.DeleteAsync(1);

            var result = await context.Employees.FindAsync(1);
            Assert.Null(result);
        }
    }
}