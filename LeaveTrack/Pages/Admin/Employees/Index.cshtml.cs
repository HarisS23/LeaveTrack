using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LeaveTrack.Web.Pages.Admin.Employees
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public IndexModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IEnumerable<(Employee Employee, int RemainingBalance)> Employees { get; set; }
            = new List<(Employee, int)>();

        public async Task OnGetAsync()
        {
            var employees = await _employeeService.GetAllAsync();

            Employees = await Task.WhenAll(employees.Select(async e => (
                e,
                await _employeeService.GetRemainingLeaveBalanceAsync(e.Id)
            )));
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _employeeService.DeleteAsync(id);
            TempData["Success"] = "Employee deleted successfully.";
            return RedirectToPage();
        }
    }
}