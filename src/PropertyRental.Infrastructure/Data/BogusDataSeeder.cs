using Bogus;
using Microsoft.EntityFrameworkCore;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;

namespace PropertyRental.Infrastructure.Data;

public static class BogusDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
    
        if (await context.RentalApplications.AnyAsync())
        {
            return;
        }
 
        var managerUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == "manager.demo@example.test");
        
        if (managerUser == null)
        {
            return;
        }

        
        var propertyFaker = new Faker<Property>()
            .RuleFor(p => p.Name, f => $"{f.Company.CompanyName()} Luxury Apartments")
            .RuleFor(p => p.City, f => f.Address.City())
            .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
            .RuleFor(p => p.ManagerUserId, _ => managerUser.Id);

        var properties = propertyFaker.Generate(3);
        context.Properties.AddRange(properties);
        await context.SaveChangesAsync();

 
        foreach (var property in properties)
        {
            for (int i = 1; i <= 3; i++)
            {
                var unit = new Unit
                {
                    PropertyId = property.Id,
                    UnitNumber = $"{property.Id * 100 + i}",
                    MonthlyRent = Math.Round(800m + (i * 250m) + 0.99m, 2),
                    Bedrooms = (byte)i,
                    Bathrooms = 1.0m,
                    Status = UnitStatus.Available,
                    AvailableFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i))
                };
                context.Units.Add(unit);
            }
        }
        await context.SaveChangesAsync();

        var units = await context.Units.ToListAsync();
        var random = new Random();

   
        var allStatuses = Enum.GetValues<ApplicationStatus>();

        foreach (var status in allStatuses)
        {
            var unit = units[random.Next(units.Count)];
            var now = DateTime.UtcNow;

            var application = new RentalApplication
            {
                ApplicantUserId = managerUser.Id,
                UnitId = unit.Id,
                StatusId = (byte)status,
                DesiredMoveInDate = DateOnly.FromDateTime(now.AddDays(random.Next(10, 45))),
                HouseholdSize = (byte)random.Next(1, 4),
                MonthlyIncome = 5500m,
                ApplicantMessage = $"Test application for status {status}.",
                CreatedUtc = now.AddDays(-5),
                UpdatedUtc = now
            };

            context.RentalApplications.Add(application);
        }

        await context.SaveChangesAsync();
    }
}