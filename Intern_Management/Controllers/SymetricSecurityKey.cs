using System.Security.Cryptography.X509Certificates;

namespace Intern_Management.Controllers
{
    internal class SymetricSecurityKey : X509Certificate2
    {
        private object value;

        public SymetricSecurityKey(object value)
        {
            this.value = value;
        }
    }
}