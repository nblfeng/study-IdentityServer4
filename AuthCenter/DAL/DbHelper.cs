using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthCenter.DAL
{
    using System.Reflection;

    public sealed class DbHelper
    {
        private static List<Type> GetModelTypeList( )
        {
            var modelTypeList = new List<Type>( );

            var types = Assembly.GetExecutingAssembly( ).GetTypes( );
            foreach ( var t in types )
            {
                if ( false == t.IsAbstract && typeof( ABaseModel ).IsAssignableFrom( t ) )
                    modelTypeList.Add( t );
            }

            return modelTypeList;
        }

        public static void Compare( AuthContext db )
        {
            var list = GetModelTypeList( );

            var sql = db.Orm.CodeFirst.GetComparisonDDLStatements( list.ToArray( ) );
            Console.WriteLine( $"compare: {Environment.NewLine}{sql}" );

            // 检测导航属性是否生效
            var tableRef = db.Orm.CodeFirst.GetTableByEntity( typeof( Models.T_User ) ).GetTableRef( nameof( Models.T_User.T_Dept ), true );
            Console.WriteLine( tableRef.RefType );
        }

        public static void Update( AuthContext db )
        {
            var list = GetModelTypeList( );

            db.Orm.CodeFirst.SyncStructure( list.ToArray( ) );

            Console.WriteLine( "success" );
        }
    }
}
