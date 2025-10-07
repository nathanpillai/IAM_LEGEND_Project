using IAMLegend.Dtos;
using IAMLegend.Entities;
using IAMLegend.Models;
using IAMLegend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IAMLegend.Controllers
{
    //[Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) { _userService = userService; }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            return View(users); // pass the list to the view
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var users = await _userService.GetAllAsync(ct);
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken ct)
        {
            var u = await _userService.GetByIdAsync(id, ct);
            return u == null ? NotFound() : Ok(u);
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
        //{
        //    // createdBy in real app from user principal; for now pass a header or env
        //    string createdBy = User?.Identity?.Name ?? Request.Headers["X-Run-As"].FirstOrDefault() ?? "system";
        //    var id = await _userService.CreateAsync(req, createdBy, ct);
        //    return CreatedAtAction(nameof(Get), new { id }, new { id });
        //}

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req, CancellationToken ct)
        {
            string modifiedBy = User?.Identity?.Name ?? Request.Headers["X-Run-As"].FirstOrDefault() ?? "system";
            await _userService.UpdateAsync(id, req, modifiedBy, ct);
            return NoContent();
        }

        // GET: Users/Delete/5
        [HttpGet("Users/Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var user = await _userService.GetByIdAsync(id, ct);
            if (user == null) return NotFound();
            return View(user); // pass user to Delete.cshtml view
        }

        // POST: Users/DeleteConfirmed/5
        [HttpPost("Users/DeleteConfirmed/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            string modifiedBy = User?.Identity?.Name ?? "system";
            await _userService.SoftDeleteUserProfileAsync(id, modifiedBy, ct);
            return RedirectToAction(nameof(Index)); // back to list after soft delete
        }

        // GET: Users/Details/5
        [HttpGet("Users/Details/{id:int}")]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var user = await _userService.GetByIdAsync(id, ct);
            if (user == null) return NotFound();
            return View(user); // pass the user to the view
        }

        // GET: Users/Edit/5
        [HttpGet("Users/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var vm = await _userService.GetUserProfileWithPermissionsAsync(id, ct);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: Users/Edit/5
        [HttpPost("Users/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserProfileEditViewModel model, CancellationToken ct)
        {
            if (id != model.UserProfile.userprofileid) return BadRequest();

            //if (ModelState.IsValid)
            //{
                string modifiedBy = User?.Identity?.Name ?? "system";
                await _userService.UpdateUserPermissionsAsync(model, modifiedBy);
                return RedirectToAction(nameof(Index));
            //}
            //return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserProfile(int id,UserProfileEditViewModel model, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(model.UserProfile.userprofileid.ToString())) return BadRequest();

            //if (!ModelState.IsValid)
            //{
            //    // re-populate dropdowns and branches again (like in GET)
            //    model = await _userService.GetUserProfileWithPermissionsAsync(model.UserProfile.userprofileid, ct);
            //    return View(model);
            //}
            //else
            //{
                string modifiedBy = User?.Identity?.Name ?? "system";
                await _userService.UpdateUserPermissionsAsync(model, modifiedBy, ct);
                return RedirectToAction("Index");
            //}                                     
        }

        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = await _userService.GetBlankUserProfileWithPermissionsAsync(ct);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserProfileEditViewModel model, CancellationToken ct)
        {
            //if (!ModelState.IsValid)
            //{
            //    // Reload blank permission rows if model invalid
            //    model = await _userService.GetBlankUserProfileWithPermissionsAsync(ct);
            //    return View(model);
            //}

            string createdBy = User?.Identity?.Name ?? "system";
            await _userService.CreateUserAsync(model, createdBy, ct);

            return RedirectToAction(nameof(Index));
        }

    }
}