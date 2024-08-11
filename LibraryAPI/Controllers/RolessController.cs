using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

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
        public async Task<IActionResult> CreateRoles()
        {
            var roles = new[] { "Member", "Worker", "Admin" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            return Ok();
        }
    }
}
