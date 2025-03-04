using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class UserTypeModel
    {
        public UserTypeModel(int typeId, string typeName)
        {
            TypeId = typeId;
            TypeName = typeName;
        }

        public int TypeId { get; set; }
        public string TypeName { get; set; }
    }
}
