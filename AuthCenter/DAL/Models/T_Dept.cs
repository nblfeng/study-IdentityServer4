using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthCenter.DAL.Models
{
    public class T_Dept : ABaseModel
    {
        public string Name { get; set; }
        public string ParentId { get; set; }

        public virtual ICollection<T_User> T_User { get; set; }
    }
}
