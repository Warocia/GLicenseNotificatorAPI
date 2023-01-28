using GLicenseNotificatorAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet(Name = "GetLicense")]
        public IEnumerable<License> Get()
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var db = new Model.DataContext(_configuration);
          
            var user = db.Users.Include(u => u.Licenses).FirstOrDefault(u => u.UserName == userName);

            if (user == null) {return new List<License>();}

            return user.Licenses.ToList();
        }
    }
}