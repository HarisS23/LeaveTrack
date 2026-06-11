using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LeaveTrack.Web.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IEmployeeService _employeeService;
        private readonly IReportService _reportService;

        public IndexModel(
            ILeaveRequestService leaveRequestService,
            IEmployeeService employeeService,
            IReportService reportService)
        {
            _leaveRequestService = leaveRequestService;
            _employeeService = employeeService;
            _reportService = reportService;
        }

        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalEmployees { get; set; }
        public IEnumerable<LeaveRequest> RecentPending { get; set; } = new List<LeaveRequest>();
        public IEnumerable<LeaveRequest> UpcomingLeave { get; set; } = new List<LeaveRequest>();

        public async Task OnGetAsync()
        {
            var allRequests = await _leaveRequestService.GetAllAsync();

            PendingCount = allRequests
                .Count(r => r.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Pending);
            ApprovedCount = allRequests
                .Count(r => r.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Approved);
            RejectedCount = allRequests
                .Count(r => r.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Rejected);

            var employees = await _employeeService.GetAllAsync();
            TotalEmployees = employees.Count();

            RecentPending = allRequests
                .Where(r => r.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5);

            UpcomingLeave = await _reportService.GetUpcomingApprovedLeaveAsync();
        }
    }
}