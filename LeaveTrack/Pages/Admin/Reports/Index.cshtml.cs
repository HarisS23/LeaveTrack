using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LeaveTrack.Web.Pages.Admin.Reports
{
    public class IndexModel : PageModel
    {
        private readonly IReportService _reportService;

        public IndexModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IEnumerable<(Employee Employee, int ApprovedDays)> ApprovedDaysPerEmployee { get; set; }
            = new List<(Employee, int)>();

        public IEnumerable<(string StatusName, int Count)> RequestsByStatus { get; set; }
            = new List<(string, int)>();

        public IEnumerable<LeaveRequest> UpcomingLeave { get; set; }
            = new List<LeaveRequest>();

        public async Task OnGetAsync()
        {
            ApprovedDaysPerEmployee = await _reportService.GetApprovedDaysPerEmployeeAsync();
            RequestsByStatus = await _reportService.GetRequestsGroupedByStatusAsync();
            UpcomingLeave = await _reportService.GetUpcomingApprovedLeaveAsync();
        }
    }
}