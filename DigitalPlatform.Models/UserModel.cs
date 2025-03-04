using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class UserModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public string RealName { get; set; }
        public int UserType { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public int Gender { get; set; }
    }
}
