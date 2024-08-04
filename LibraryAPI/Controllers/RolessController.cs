using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolessController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolessController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpPost]
        public void CreateRoles()
        {
            IdentityRole identityRole = new IdentityRole("Member");

            _roleManager.CreateAsync(identityRole).Wait();

            identityRole = new IdentityRole("Worker");
            _roleManager.CreateAsync(identityRole).Wait();
            identityRole = new IdentityRole("Admin");
            _roleManager.CreateAsync(identityRole).Wait();
        }
    }
}
