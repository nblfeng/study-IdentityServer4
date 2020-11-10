using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthCenter.DAL
{
    using AuthCenter.DAL.Models;
    using FreeSql;

    public sealed partial class SeedData
    {
        static IEnumerable<T_Dept> Depts => new List<T_Dept>
        {
            new T_Dept { Id = "1", Name = "xxx有限公司" }
        };

        static IEnumerable<T_User> Users => new List<T_User>
        {
            new T_User { Account = "admin", Password="123", Name = "系统管理员", IsAdmin = true, DeptId = "1" }
        };
    }

    partial class SeedData
    {
        public static async Task AddSeedDataAsync( AuthContext db )
        {
            if ( false == db.T_Depts.Select.Any( ) )
            {
                await db.AddRangeAsync( Depts );
                Console.WriteLine( "添加种子数据: T_Dept" );
            }

            if ( false == db.T_Users.Select.Any( ) )
            {
                await db.AddRangeAsync( Users );
                Console.WriteLine( "添加种子数据: T_User" );
            }

            await db.SaveChangesAsync( );
        }

        public static async Task ClearDataAsync( AuthContext db )
        {
            if ( db.T_Users.Select.Any( ) )
            {
                var list = db.T_Users.Select.ToList( );
                db.RemoveRange( list );
                Console.WriteLine( "清除数据: T_User" );
            }

            if ( db.T_Depts.Select.Any( ) )
            {
                var list = db.T_Depts.Select.ToList( );
                db.RemoveRange( list );
                Console.WriteLine( "清除数据: T_Dept" );
            }

            await db.SaveChangesAsync( );
        }
    }
}
