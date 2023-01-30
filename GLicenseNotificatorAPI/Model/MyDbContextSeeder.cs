using GLicenseNotificatorAPI.Crypto;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace GLicenseNotificatorAPI.Model
{
    public static class MyDbContextSeeder
    {
        //Create Admin user to database
        public static void Seed(DataContext context, string username, string email, string password)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            License testLicense1 = new License()
            {
                Id = Guid.NewGuid(),
                LicenseNumber = "G3434343",
                IsValidUtc = DateTime.UtcNow.AddDays(200),
                NotificationSent= false
            };

            License testLicense2 = new License()
            {
                Id = Guid.NewGuid(),
                LicenseNumber = "X13",
                IsValidUtc = DateTime.UtcNow.AddDays(100),
                NotificationSent = false
            };

            PasswordHasher passwordHasher = new PasswordHasher();

            var newAdmin = new LicenceUser()
            {

                UserName = username,
                Password = passwordHasher.Hash(password),
                Email = email,
                IsAdmin = true,
                Licenses = new List<License>() { testLicense1 , testLicense2 }
            };

            context.Users.Add(newAdmin);

            context.SaveChanges();
        }
    }
}
