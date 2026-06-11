using LeaveTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveTrack.Data.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
        public DbSet<LeaveRequestStatus> LeaveRequestStatuses => Set<LeaveRequestStatus>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "EMPLOYEE", PrettyName = "Employee" },
                new Role { Id = 2, Name = "MANAGER", PrettyName = "Manager" },
                new Role { Id = 3, Name = "HR", PrettyName = "HR / Administrator" }
            );

            modelBuilder.Entity<LeaveRequestStatus>().HasData(
                new LeaveRequestStatus { Id = 1, Name = "PENDING", PrettyName = "Pending" },
                new LeaveRequestStatus { Id = 2, Name = "APPROVED", PrettyName = "Approved" },
                new LeaveRequestStatus { Id = 3, Name = "REJECTED", PrettyName = "Rejected" },
                new LeaveRequestStatus { Id = 4, Name = "CANCELLED", PrettyName = "Cancelled" }
            );

            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType { Id = 1, Name = "VACATION", PrettyName = "Vacation" },
                new LeaveType { Id = 2, Name = "SICK", PrettyName = "Sick Leave" },
                new LeaveType { Id = 3, Name = "PERSONAL", PrettyName = "Personal Leave" }
            );

            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, FirstName = "John", LastName = "Smith", Email = "john.smith@company.com", RoleId = 1, YearlyLeaveBalance = 20 },
                new Employee { Id = 2, FirstName = "Sarah", LastName = "Jones", Email = "sarah.jones@company.com", RoleId = 2, YearlyLeaveBalance = 20 },
                new Employee { Id = 3, FirstName = "Mike", LastName = "Brown", Email = "mike.brown@company.com", RoleId = 3, YearlyLeaveBalance = 20 }
            );
        }
    }
}
