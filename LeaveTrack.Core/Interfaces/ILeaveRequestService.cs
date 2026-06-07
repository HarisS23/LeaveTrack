using LeaveTrack.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveTrack.Core.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<LeaveRequest>> FilterAsync(int? employeeId, int? statusId, int? leaveTypeId, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<LeaveRequest>> SearchByEmployeeNameAsync(string name);
        Task CreateAsync(LeaveRequest request);
        Task UpdateAsync(LeaveRequest request);
        Task CancelAsync(int id);
    }
}
