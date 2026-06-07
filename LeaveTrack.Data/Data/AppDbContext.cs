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

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "EMPLOYEE", PrettyName = "Employee" },
                new Role { Id = 2, Name = "MANAGER", PrettyName = "Manager" },
                new Role { Id = 3, Name = "HR", PrettyName = "HR / Administrator" }
            );

            // Seed LeaveRequestStatuses
            modelBuilder.Entity<LeaveRequestStatus>().HasData(
                new LeaveRequestStatus { Id = 1, Name = "PENDING", PrettyName = "Pending" },
                new LeaveRequestStatus { Id = 2, Name = "APPROVED", PrettyName = "Approved" },
                new LeaveRequestStatus { Id = 3, Name = "REJECTED", PrettyName = "Rejected" },
                new LeaveRequestStatus { Id = 4, Name = "CANCELLED", PrettyName = "Cancelled" }
            );

            // Seed LeaveTypes
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType { Id = 1, Name = "VACATION", PrettyName = "Vacation" },
                new LeaveType { Id = 2, Name = "SICK", PrettyName = "Sick Leave" },
                new LeaveType { Id = 3, Name = "PERSONAL", PrettyName = "Personal Leave" }
            );
        }
    }
}
