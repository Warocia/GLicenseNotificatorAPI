using GLicenseNotificatorAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Security.Claims;


namespace GLicenseNotificatorAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LicenceUserController : ControllerBase
    {

        private readonly ILogger<LicenseController> _logger;
        private readonly IConfiguration _configuration;
        public LicenceUserController(IConfiguration configuration, ILogger<LicenseController> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

    
        [HttpGet(Name = "GetLicenseUsers")]
        public ActionResult<IEnumerable<Model.LicenceUser>> GetLicenseUsers()
        {
            LicenceUser? user;
            DataContext? db;

            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }
                db = new Model.DataContext(_configuration);
                user = db.Users.FirstOrDefault(u => u.UserName == userName);
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            //Check that user is Admin
            if (user == null || !user.IsAdmin)
            {
                return Unauthorized();
            }

            try
            {
                return db.Users.ToList();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost(Name = "CreateLicenseUser")]
        public ActionResult<Model.LicenceUser> CreateLicenseUser(Model.LicenceUser newUser)
        {
            LicenceUser? user;
            DataContext? db;

            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }
                db = new Model.DataContext(_configuration);
                user = db.Users.FirstOrDefault(u => u.UserName == userName);
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            //Check that user is Admin
            if (user == null || !user.IsAdmin)
            {
                return Unauthorized();
            }

            try
            {
                db.Entry(newUser).State = EntityState.Added;
                db.Users.Add(newUser); ;
                db.SaveChanges();

                return newUser;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut(Name = "UpdateLicenseUser")]
        public ActionResult<Model.LicenceUser> UpdateLicenseUser(Model.LicenceUser updateUser)
        {
            LicenceUser? user;
            DataContext? db;

            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }
                db = new Model.DataContext(_configuration);
                user = db.Users.FirstOrDefault(u => u.UserName == userName);
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            //Check that user is Admin
            if (user == null || !user.IsAdmin)
            {
                return Unauthorized();
            }

            try
            {
                var dbUser = db.Users.FirstOrDefault(l => l.Id == updateUser.Id);

                if (dbUser == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                if (dbUser.IsAdmin != updateUser.IsAdmin ||
                           dbUser.Email != updateUser.Email ||
                           dbUser.Password != dbUser.Password)
                {
                    dbUser.IsAdmin = updateUser.IsAdmin;
                    dbUser.Email = updateUser.Email;
                    dbUser.Password = dbUser.Password;

                    db.SaveChanges();
                }

                return dbUser;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete(Name = "DeleteLicenseUser")]
        public ActionResult DeleteLicenseUser(int id)
        {
            LicenceUser? user;
            DataContext? db;

            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }
                db = new Model.DataContext(_configuration);
                user = db.Users.FirstOrDefault(u => u.UserName == userName);
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            //Check that user is Admin
            if (user == null || !user.IsAdmin)
            {
                return Unauthorized();
            }

            try
            {
                var dbUser = db.Users.FirstOrDefault(l => l.Id == id);

                if (dbUser == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                db.Users.Remove(dbUser);
                db.SaveChanges();

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}