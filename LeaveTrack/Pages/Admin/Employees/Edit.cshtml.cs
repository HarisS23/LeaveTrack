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
    public class EditModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly AppDbContext _context;

        public EditModel(IEmployeeService employeeService, AppDbContext context)
        {
            _employeeService = employeeService;
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;
        public SelectList RoleList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null) return NotFound();

            Employee = employee;
            await PopulateDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return Page();
            }

            await _employeeService.UpdateAsync(Employee);
            TempData["Success"] = "Employee updated successfully.";
            return RedirectToPage("Index");
        }

        private async Task PopulateDropdownsAsync()
        {
            var roles = await _context.Roles.ToListAsync();
            RoleList = new SelectList(roles, "Id", "PrettyName");
        }
    }
}