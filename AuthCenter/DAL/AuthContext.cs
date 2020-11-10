using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthCenter.DAL
{
    using AuthCenter.DAL.Models;
    using FreeSql;

    public partial class AuthContext : DbContext
    {
        public AuthContext( )
        {

        }

        protected override void OnConfiguring( DbContextOptionsBuilder options )
        {
            base.OnConfiguring( options );
        }

        protected override void OnModelCreating( ICodeFirst codefirst )
        {
            codefirst.ConfigEntity<Models.T_User>( builder =>
            {
                builder.Property( i => i.DeptId ).StringLength( 32 );
                builder.Navigate( i => i.T_Dept, nameof( T_User.DeptId ) );
            } );
            codefirst.ConfigEntity<Models.T_Dept>( builder =>
            {
                builder.Navigate( i => i.T_User, null );
            } );
        }
    }

    partial class AuthContext
    {
        public DbSet<T_User> T_Users { get; set; }
        public DbSet<T_Dept> T_Depts { get; set; }
    }
}
