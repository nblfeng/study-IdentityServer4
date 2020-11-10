using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthCenter.DAL.Models
{
    using FreeSql.DataAnnotations;

    public class T_User : ABaseModel
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 授权功能点
        /// </summary>
        public string Authority { get; set; }

        public string DeptId { get; set; }
        public virtual T_Dept T_Dept { get; set; }
    }
}
