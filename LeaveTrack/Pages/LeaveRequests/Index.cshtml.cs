using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using LeaveTrack.Data;
using LeaveTrack.Data.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LeaveTrack.Web.Pages.LeaveRequests
{
    public class IndexModel : PageModel
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly AppDbContext _context;

        public IndexModel(ILeaveRequestService leaveRequestService, AppDbContext context)
        {
            _leaveRequestService = leaveRequestService;
            _context = context;
        }

        public IEnumerable<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public IEnumerable<LeaveRequestStatus> Statuses { get; set; } = new List<LeaveRequestStatus>();
        public IEnumerable<LeaveType> LeaveTypes { get; set; } = new List<LeaveType>();

        public string? SearchName { get; set; }
        public int? StatusId { get; set; }
        public int? LeaveTypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync(
            string? searchName,
            int? statusId,
            int? leaveTypeId,
            DateTime? startDate,
            DateTime? endDate)
        {
            SearchName = searchName;
            StatusId = statusId;
            LeaveTypeId = leaveTypeId;
            StartDate = startDate;
            EndDate = endDate;

            Statuses = await _context.LeaveRequestStatuses.ToListAsync();
            LeaveTypes = await _context.LeaveTypes.ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                LeaveRequests = await _leaveRequestService.SearchByEmployeeNameAsync(searchName);
            }
            else
            {
                LeaveRequests = await _leaveRequestService.FilterAsync(
                    null, statusId, leaveTypeId, startDate, endDate);
            }
        }
    }
}