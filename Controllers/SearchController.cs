using JWTdemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JWTdemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 👈 (ต้องล็อกอิน)
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // [GET] /api/Search?query=...
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 3)
            {
                return BadRequest(new { message = "Search query must be at least 3 characters." });
            }

            var userId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin");

            var results = await _searchService.SearchAsync(query, userId, isAdmin);
            return Ok(results);
        }

        // --- (Helper Function) ---
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }
    }
}