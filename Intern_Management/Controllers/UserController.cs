using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using Intern_Management.Data;
using Intern_Management.Models;
using Intern_Management.Models.DTO;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public UserController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("profile")]
        [Authorize] // Ensure the user is authenticated to access this endpoint
        public IActionResult GetUserProfile()
        {
            // Retrieve the user's email from the HttpContext
            var currentUserEmail = HttpContext.Items["Email"]?.ToString();

            // Find the current user based on the email
            User? user = _dbContext.User.FirstOrDefault(u => u.Email == currentUserEmail);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Return the user information
            return Ok(user);
        }

        //
        [HttpGet("picture")]
        [Authorize] // Ensure the user is authenticated to access this endpoint
        public IActionResult GetUserPicture()
        {
            // Retrieve the user's email from the HttpContext
            var userEmail = HttpContext.Items["Email"]?.ToString();

            // Find the user record based on the email
            User? user = _dbContext.User.FirstOrDefault(u => u.Email == userEmail);

            if (user == null || string.IsNullOrEmpty(user.PicturePath))
            {
                return NotFound("User picture not found.");
            }

            // Get the base64 image data from the PicturePath field
            string base64ImageData = user.PicturePath.Substring(user.PicturePath.IndexOf(',') + 1);

            // Create the PictureDTO instance to transfer the image data to the client
            var pictureDTO = new PictureDTO
            {
                FileName = "user_picture.jpg", // You can set the file name dynamically based on your implementation
                ContentType = "image/jpeg", // Set the correct content type based on the image format
                Data = base64ImageData // Store the base64 image data in the Data property
            };

            return Ok(pictureDTO);
        }

    }
}
