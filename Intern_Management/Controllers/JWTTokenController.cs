﻿using Intern_Management.Data;
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
                var userData = await GetUser(user.Email, user.Password);
                var jwt = _configuration.GetSection("Jwt").Get<Jwt>();

                if (userData != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                        new Claim(JwtRegisteredClaimNames.Jti, JwtRegisteredClaimNames.Jti),
                        new Claim(JwtRegisteredClaimNames.Iat, JwtRegisteredClaimNames.Iat),
                        new Claim("Id", userData.Id.ToString()),
                        new Claim("Email", userData.Email),
                        new Claim("Password", userData.Password)
                    };

                    if (userData.RoleId != 0 && userData.Role != null)
                    {
                        var roleClaim = new Claim(ClaimTypes.Role, userData.Role.Name);
                        claims.Add(roleClaim);

                        // Get the permissions for the role
                        var rolePermissions = await _context.RolePermissions
                            .Where(rp => rp.RoleCode == userData.Role.Code)
                            .Select(rp => rp.Permission)
                            .ToListAsync();

                        foreach (var permission in rolePermissions)
                        {
                            var permissionClaim = new Claim("Permission", permission.Description);
                            claims.Add(permissionClaim);
                        }
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
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
        public async Task<User> GetUser(string username, string password)
        {
            return await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == username && u.Password == password);
        }
    }
}
