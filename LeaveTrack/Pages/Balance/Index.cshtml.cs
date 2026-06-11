using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LeaveTrack.Web.Pages.Balance
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILeaveRequestService _leaveRequestService;

        public IndexModel(
            IEmployeeService employeeService,
            ILeaveRequestService leaveRequestService)
        {
            _employeeService = employeeService;
            _leaveRequestService = leaveRequestService;
        }

        public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
        public Employee? SelectedEmployee { get; set; }
        public int? SelectedEmployeeId { get; set; }
        public int RemainingBalance { get; set; }
        public int UsedDays { get; set; }
        public int UsedPercentage { get; set; }
        public IEnumerable<LeaveRequest> ApprovedRequests { get; set; } = new List<LeaveRequest>();

        public async Task OnGetAsync(int? employeeId)
        {
            Employees = await _employeeService.GetAllAsync();

            if (employeeId.HasValue)
            {
                SelectedEmployeeId = employeeId;
                SelectedEmployee = await _employeeService.GetByIdAsync(employeeId.Value);

                if (SelectedEmployee != null)
                {
                    RemainingBalance = await _employeeService
                        .GetRemainingLeaveBalanceAsync(employeeId.Value);

                    UsedDays = SelectedEmployee.YearlyLeaveBalance - RemainingBalance;

                    UsedPercentage = SelectedEmployee.YearlyLeaveBalance > 0
                        ? (int)Math.Round((double)UsedDays / SelectedEmployee.YearlyLeaveBalance * 100)
                        : 0;

                    ApprovedRequests = await _leaveRequestService.FilterAsync(
                        employeeId,
                        (int)LeaveRequestStatusEnum.Approved,
                        null, null, null);
                }
            }
        }
    }
}