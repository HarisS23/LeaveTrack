# LeaveTrack

A company internal leave request management system built with ASP.NET Core and Razor Pages.

## Tech Stack

- C# / ASP.NET Core 8
- Razor Pages
- Entity Framework Core 8
- SQLite

## Project Structure

- LeaveTrack/
- LeaveTrack.Web/        # Razor Pages web application
- LeaveTrack.Core/       # Models and service interfaces
- LeaveTrack.Data/       # EF Core DbContext and migrations
- LeaveTrack.Tests/      # xUnit test project
- data/                  # SQLite database (generated, not committed)

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Setup

1. Clone the repository
git clone https://github.com/HarisS23/LeaveTrack.git

2. Navigate to the solution root
cd LeaveTrack

3. Restore dependencies
dotnet restore

4. Apply database migrations
cd LeaveTrack.Web
dotnet ef database update

5. Run the application
dotnet run

The application will be available at `https://localhost:5001`

## Running Tests
dotnet test

## Business Rules

- Leave requests require a start date, end date, and leave type
- End date cannot be before start date
- Leave requests cannot be created for dates in the past
- Only pending requests can be approved or rejected
- Rejected requests must include a rejection reason
- Approved vacation requests reduce the employee leave balance
- Vacation requests cannot be approved if the employee has insufficient balance
- Overlapping approved leave for the same employee is not allowed

## Roles

| Role | Permissions |
|------|------------|
| Employee | Create, view, edit, and cancel own requests |
| Manager | View, approve, and reject pending requests |
| HR / Administrator | Full access, employee management, reports |

## License

This project was built as part of a company internship practice assignment.
