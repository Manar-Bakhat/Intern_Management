using System.IdentityModel.Tokens.Jwt;


namespace Intern_Management.Models
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                AttachUserToContext(context, token);
            }

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                var claims = jwt.Claims;

                // Add user information to context claims to access it in controllers
                context.Items["UserId"] = claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                context.Items["Email"] = claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                // Ajoutez d'autres informations utilisateur si nécessaire

            }
            catch (Exception)
            {
                // Error handling in case of invalid or expired token, etc.
            }
        }
    }
}
