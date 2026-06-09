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
    public class ApprovalService : IApprovalService
    {
        private readonly AppDbContext _context;

        public ApprovalService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage)> ApproveAsync(int leaveRequestId)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == leaveRequestId);

            if (request == null)
                return (false, "Leave request not found.");

            if (request.LeaveRequestStatusId != (int)LeaveRequestStatusEnum.Pending)
                return (false, "Only pending requests can be approved.");

            // Balance check (only for vacation)
            if (request.LeaveTypeId == (int)LeaveTypeEnum.Vacation)
            {
                var remainingBalance = await GetRemainingBalanceAsync(request.EmployeeId);
                var requestedDays = (request.EndDate - request.StartDate).Days + 1;

                if (requestedDays > remainingBalance)
                    return (false, $"Insufficient leave balance. Requested: {requestedDays} days, Available: {remainingBalance} days.");
            }

            // Overlap check
            var hasOverlap = await _context.LeaveRequests
                .AnyAsync(r =>
                    r.Id != request.Id &&
                    r.EmployeeId == request.EmployeeId &&
                    r.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Approved &&
                    r.StartDate <= request.EndDate &&
                    r.EndDate >= request.StartDate);

            if (hasOverlap)
                return (false, "Employee already has approved leave that overlaps with these dates.");

            request.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Approved;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RejectAsync(int leaveRequestId, string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return (false, "A rejection reason is required.");

            var request = await _context.LeaveRequests
                .FirstOrDefaultAsync(r => r.Id == leaveRequestId);

            if (request == null)
                return (false, "Leave request not found.");

            if (request.LeaveRequestStatusId != (int)LeaveRequestStatusEnum.Pending)
                return (false, "Only pending requests can be rejected.");

            request.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Rejected;
            request.RejectionReason = rejectionReason;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        private async Task<int> GetRemainingBalanceAsync(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return 0;

            var approvedVacationRequests = await _context.LeaveRequests
                .Where(r =>
                    r.EmployeeId == employeeId &&
                    r.LeaveTypeId == (int)LeaveTypeEnum.Vacation &&
                    r.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Approved)
                .ToListAsync();

            var approvedVacationDays = approvedVacationRequests
                .Sum(r => (r.EndDate - r.StartDate).Days + 1);

            return employee.YearlyLeaveBalance - approvedVacationDays;
        }
    }
}
