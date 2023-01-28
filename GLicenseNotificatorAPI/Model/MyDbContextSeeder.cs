using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace GLicenseNotificatorAPI.Model
{
    public static class MyDbContextSeeder
    {
        public static void Seed(DataContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var l1 = new License()
            {
                Id = Guid.NewGuid(),
                LicenseNumber = "1111111",
                IsValidUtc = DateTime.UtcNow
            };

            var l2 = new License()
            {
                Id = Guid.NewGuid(),
                LicenseNumber = "222222222",
                IsValidUtc = DateTime.UtcNow
            };

            var l3 =new License()
            {
                Id = Guid.NewGuid(),
                LicenseNumber = "33333333",
                IsValidUtc = DateTime.UtcNow
            };

            var u = new LicenceUser()
            {

                UserName = "pekka",
                Password = "1234",
                Email = "amnnrgf@gmail.com",
                IsAdmin = false,
                Licenses = new List<License>() { l1, l2, l3 }
            };

            context.Users.Add(u);

            var l4 = new License()
            {
                Id = Guid.NewGuid(),
                LicenseNumber = "44444",
                IsValidUtc = DateTime.UtcNow
            };

            var u2 = new LicenceUser()
            {

                UserName = "seppo",
                Password = "1234",
                Email = "amnnrgf@gmail.com",
                IsAdmin = true,
                Licenses = new List<License>() { l4 }
            };

            context.Users.Add(u2);

            context.SaveChanges();
        }
    }
}
