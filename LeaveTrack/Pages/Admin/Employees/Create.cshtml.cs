using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using LeaveTrack.Data;
using LeaveTrack.Data.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LeaveTrack.Web.Pages.Admin.Employees
{
    public class CreateModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly AppDbContext _context;

        public CreateModel(IEmployeeService employeeService, AppDbContext context)
        {
            _employeeService = employeeService;
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; } = new();
        public SelectList RoleList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Employee.YearlyLeaveBalance = 20;
            await PopulateDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _employeeService.CreateAsync(Employee);
            TempData["Success"] = "Employee added successfully.";
            return RedirectToPage("Index");
        }

        private async Task PopulateDropdownsAsync()
        {
            var roles = await _context.Roles.ToListAsync();
            RoleList = new SelectList(roles, "Id", "PrettyName");
        }
    }
}