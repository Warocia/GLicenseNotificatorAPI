using GLicenseNotificatorAPI.Crypto;
using GLicenseNotificatorAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private DataContext _db;
        private PasswordHasher passwordHasher;
        public LicenceUserController(DataContext db, ILogger<LicenseController> logger)
        {
            _logger = logger;
            _db = db;
            passwordHasher = new PasswordHasher();
        }

    
        [HttpGet(Name = "GetLicenseUsers")]
        public ActionResult<IEnumerable<Model.LicenceUser>> GetLicenseUsers()
        {
            LicenceUser? user;

            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }

                user = _db.Users.FirstOrDefault(u => u.UserName == userName);
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
                return _db.Users.ToList();
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

            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }

                user = _db.Users.FirstOrDefault(u => u.UserName == userName);
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
                _db.Entry(newUser).State = EntityState.Added;
                newUser.Password = passwordHasher.Hash(newUser.Password);
                _db.Users.Add(newUser); ;
                _db.SaveChanges();

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

            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }

                user = _db.Users.FirstOrDefault(u => u.UserName == userName);
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
                var dbUser = _db.Users.FirstOrDefault(l => l.Id == updateUser.Id);

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

                    dbUser.Password = passwordHasher.Hash(updateUser.Password);

                    _db.SaveChanges();
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

            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }

                user = _db.Users.FirstOrDefault(u => u.UserName == userName);
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
                var dbUser = _db.Users.FirstOrDefault(l => l.Id == id);

                if (dbUser == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                _db.Users.Remove(dbUser);
                _db.SaveChanges();

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}