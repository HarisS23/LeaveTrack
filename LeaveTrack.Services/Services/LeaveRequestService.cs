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
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly AppDbContext _context;

        public LeaveRequestService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveRequest?> GetByIdAsync(int id)
        {
            return await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Include(r => r.LeaveRequestStatus)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Include(r => r.LeaveRequestStatus)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Include(r => r.LeaveRequestStatus)
                .Where(r => r.EmployeeId == employeeId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> FilterAsync(
            int? employeeId,
            int? statusId,
            int? leaveTypeId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Include(r => r.LeaveRequestStatus)
                .AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (statusId.HasValue)
                query = query.Where(r => r.LeaveRequestStatusId == statusId.Value);

            if (leaveTypeId.HasValue)
                query = query.Where(r => r.LeaveTypeId == leaveTypeId.Value);

            if (startDate.HasValue)
                query = query.Where(r => r.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.EndDate <= endDate.Value);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> SearchByEmployeeNameAsync(string name)
        {
            return await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Include(r => r.LeaveRequestStatus)
                .Where(r => r.Employee.FirstName.Contains(name)
                         || r.Employee.LastName.Contains(name))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(LeaveRequest request)
        {
            request.CreatedAt = DateTime.UtcNow;
            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LeaveRequest request)
        {
            _context.LeaveRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task CancelAsync(int id)
        {
            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null) return;

            request.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Cancelled;
            await _context.SaveChangesAsync();
        }
    }
}
