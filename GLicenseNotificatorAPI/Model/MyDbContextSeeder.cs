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


            var newAdmin = new LicenceUser()
            {

                UserName = username,
                Password = password,
                Email = email,
                IsAdmin = true,
                Licenses = new List<License>() {}
            };

            context.Users.Add(newAdmin);

            context.SaveChanges();
        }
    }
}
