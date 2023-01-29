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
    public class LicenseController : ControllerBase
    {

        private readonly ILogger<LicenseController> _logger;
        private readonly IConfiguration _configuration;
        public LicenseController(IConfiguration configuration, ILogger<LicenseController> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost(Name = "UpdateUserLicenseList")]
        public ActionResult UpdateUserLicenseList(List<Model.License> licenses)
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
                user = db.Users.Include(u => u.Licenses).FirstOrDefault(u => u.UserName == userName);

                if (user == null)
                {
                    return Unauthorized();
                } 
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            try
            {
                var dbLicenses = user.Licenses.ToList();

                foreach (var dbLicense in dbLicenses)
                {
                    var license = licenses.FirstOrDefault(l => l.Id == dbLicense.Id);

                    if (license == null)
                    {
                        // dbLicense does not exist in licenses, so delete from db
                        user.Licenses.Remove(dbLicense);
                    }
                    else
                    {
                        // dbLicense exists in licenses, so update if needed
                        if (dbLicense.LicenseNumber != license.LicenseNumber ||
                            dbLicense.IsValidUtc != license.IsValidUtc ||
                            dbLicense.NotificationSent != license.NotificationSent)
                        {
                            dbLicense.LicenseNumber = license.LicenseNumber;
                            dbLicense.IsValidUtc = license.IsValidUtc;
                            dbLicense.NotificationSent = license.NotificationSent;
                        }

                        //Remove that we don't try add these
                        licenses.Remove(license);
                    }
                }

                // Check for new licenses that were not in the db
                foreach (var license in licenses)
                {
                    db.Entry(license).State = EntityState.Added;
                    user.Licenses.Add(license);
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok();
        }

        [HttpGet(Name = "GetUserLicenseList")]
        public ActionResult<IEnumerable<Model.License>> GetUserLicenseList()
        {
            LicenceUser? user;
            try
            {
                string? userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userName == null)
                {
                    return Unauthorized();
                }
                var db = new Model.DataContext(_configuration);
                user = db.Users.Include(u => u.Licenses).FirstOrDefault(u => u.UserName == userName);
            }
            catch (Exception)
            {
                return Unauthorized();
            }
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                return user.Licenses.ToList();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}