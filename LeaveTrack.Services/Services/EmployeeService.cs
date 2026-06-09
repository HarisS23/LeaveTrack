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
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Role)
                .Include(e => e.LeaveRequests)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Role)
                .OrderBy(e => e.LastName)
                .ToListAsync();
        }

        public async Task<int> GetRemainingLeaveBalanceAsync(int employeeId)
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

        public async Task CreateAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }
    }
}
