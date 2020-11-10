using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthCenter.DAL
{
    using FreeSql.DataAnnotations;
    using System.Collections.Specialized;

    public abstract class ABaseModel
    {
        [Column( IsPrimary = true, StringLength = 32 )]
        public string Id { get; set; } = Guid.NewGuid( ).ToString( "N" ).ToUpper( );

        [Column( IsIgnore = true )]
        public HybridDictionary Fileds { get; set; } = new HybridDictionary( );
    }
}
