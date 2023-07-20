using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Intern_Management.Controllers
{
    internal class SymetricSecurityKey : SymmetricSecurityKey
    {
        public SymetricSecurityKey(object value) : base(GetBytesFromValue(value))
        {
        }

        private static byte[] GetBytesFromValue(object value)
        {
            return Encoding.UTF8.GetBytes(value?.ToString() ?? string.Empty);
        }
    }
}
