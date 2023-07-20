using Intern_Management.Data;
using Intern_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JWTTokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public JWTTokenController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post(User user)
        {
            if (user != null && user.Email != null && user.Password != null)
            {
                // Crypter le mot de passe
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                var userData = await GetUser(user.Email);
                var jwt = _configuration.GetSection("Jwt").Get<Jwt>();

                if (userData != null && BCrypt.Net.BCrypt.Verify(user.Password, userData.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject?? ""),
                        new Claim(JwtRegisteredClaimNames.Jti, JwtRegisteredClaimNames.Jti),
                        new Claim(JwtRegisteredClaimNames.Iat, JwtRegisteredClaimNames.Iat),
                        new Claim("Id", userData.Id.ToString()),
                        new Claim("Email", userData.Email?? ""),
                    };

                    if (userData.RoleId != 0 && userData.Role != null)
                    {
                        var roleClaim = new Claim(ClaimTypes.Role, userData.Role.Name ?? "");
                        claims.Add(roleClaim);

                        // Get the permissions for the role
                        var rolePermissions = await _context.RolePermissions
                            .Where(rp => rp.RoleCode == userData.Role.Code)
                            .Select(rp => rp.Permission)
                            .ToListAsync();

                        foreach (var permission in rolePermissions)
                        {
                            if (permission != null && permission.Description != null)
                            {
                                var permissionClaim = new Claim("Permission", permission.Description);
                                claims.Add(permissionClaim);
                            }
                        }
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key ?? ""));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

                    var token = new JwtSecurityToken(
                        jwt.Issuer,
                        jwt.Audience,
                        claims,
                        expires: DateTime.Now.AddMinutes(20),
                        signingCredentials: signIn
                    );

                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
            }

            return BadRequest("Invalid Credential");
        }

        [HttpGet]
        public async Task<User?> GetUser(string email)
        {
            return await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        [HttpPost("CreateAdministrator")]
        public async Task<IActionResult> CreateAdministrator()
        {
            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "Admin",
                Gender = GenderType.Male,
                Email = "admin@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                PicturePath = null,
                RoleId = 1
            };

            _context.User.Add(adminUser);
            await _context.SaveChangesAsync();

            return Ok("Administrator created successfully");
        }


        
    }
}
