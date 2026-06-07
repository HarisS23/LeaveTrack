using LeaveTrack.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveTrack.Core.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<(Employee Employee, int ApprovedDays)>> GetApprovedDaysPerEmployeeAsync();
        Task<IEnumerable<(string StatusName, int Count)>> GetRequestsGroupedByStatusAsync();
        Task<IEnumerable<LeaveRequest>> GetUpcomingApprovedLeaveAsync();
    }
}
