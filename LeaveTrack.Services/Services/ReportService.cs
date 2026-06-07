using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using LeaveTrack.Data.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveTrack.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<(Employee Employee, int ApprovedDays)>> GetApprovedDaysPerEmployeeAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.LeaveRequests)
                .ToListAsync();

            return employees.Select(e => (
                e,
                e.LeaveRequests
                    .Where(r => r.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Approved)
                    .Sum(r => (r.EndDate - r.StartDate).Days + 1)
            ));
        }

        public async Task<IEnumerable<(string StatusName, int Count)>> GetRequestsGroupedByStatusAsync()
        {
            var results = await _context.LeaveRequests
                .Include(r => r.LeaveRequestStatus)
                .GroupBy(r => r.LeaveRequestStatus.PrettyName)
                .Select(g => new { StatusName = g.Key, Count = g.Count() })
                .ToListAsync();

            return results.Select(x => (x.StatusName, x.Count));
        }

        public async Task<IEnumerable<LeaveRequest>> GetUpcomingApprovedLeaveAsync()
        {
            var today = DateTime.UtcNow.Date;
            var thirtyDaysFromNow = today.AddDays(30);

            return await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Where(r =>
                    r.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Approved &&
                    r.StartDate >= today &&
                    r.StartDate <= thirtyDaysFromNow)
                .OrderBy(r => r.StartDate)
                .ToListAsync();
        }
    }
}
