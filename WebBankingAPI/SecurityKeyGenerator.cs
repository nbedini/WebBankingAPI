using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebBankingAPI
{
    public class SecurityKeyGenerator
    {
        public static SecurityKey GetSecurityKey()
        {
            var key = Encoding.ASCII.GetBytes(Startup.MasterKey);
            return new SymmetricSecurityKey(key);
        }
    }
}
