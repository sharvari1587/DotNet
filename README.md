# DotNet

1. Project Setup

Create a new Web API project:

dotnet new webapi -n MyApiProject


Use .NET 8 framework.

Install required NuGet packages:

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools


In appsettings.json, define connection string:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MyDb;Trusted_Connection=True;"
  }
}

2. Define the Model / Entity

Create a folder “Models” (or “Entities”). Example:

namespace MyApiProject.Models
{
    public class Employee   // or whatever the entity is
    {
        public Guid Id { get; set; }    // unique identifier
        public string Name { get; set; }
        public string Email { get; set; }
        public long Phone { get; set; }
        public string Address { get; set; }
    }
}


– The tutorial uses Guid for ID in many cases. 
DEV Community
+2
c-sharpcorner.com
+2

– Use data-annotations if needed (e.g., [Required], [StringLength]).

3. Define the DbContext

Create a folder “Data” and add ApplicationDbContext.cs:

using Microsoft.EntityFrameworkCore;
using MyApiProject.Models;

namespace MyApiProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        // add DbSet for each model
    }
}


Then in Program.cs (or Startup) register it:

var builder = WebApplication.CreateBuilder(args);

// register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// swagger etc
var app = builder.Build();
// … configure middleware, endpoints
app.MapControllers();
app.Run();


This aligns with typical EF Core + Web API setup. 
Microsoft Learn
+1

4. Migrations and Database Update

Using EF Core Code First:

Add-Migration InitialCreate
Update-Database


This will generate the database schema for your models. 
DEV Community
+1

5. Create Controller & CRUD Endpoints

Create a controller, e.g., EmployeesController.cs under “Controllers”:

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiProject.Data;
using MyApiProject.Models;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        return await _context.Employees.ToListAsync();
    }

    // GET: api/Employees/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Employee>> GetEmployee(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        return employee;
    }

    // POST: api/Employees
    [HttpPost]
    public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    // PUT: api/Employees/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutEmployee(Guid id, Employee employee)
    {
        if (id != employee.Id)
            return BadRequest();

        _context.Entry(employee).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Employees.Any(e => e.Id == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/Employees/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


This pattern matches the four HTTP methods: GET, POST, PUT, DELETE. 
Microsoft Learn
+1

6. Notes & Tips

Always check id mismatches in PUT operations to avoid bad request.

Use CreatedAtAction in POST to return a 201 with location header.

For GET all, you might want async, e.g., .ToListAsync().

Handle exceptions (e.g., concurrency) gracefully.

Consider adding DTOs (Data Transfer Objects) instead of exposing entities directly.

For production, consider layers (Service, Repository) and mapping (AutoMapper) for clean architecture.