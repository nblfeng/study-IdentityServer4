using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthCenter.Ids4
{
    using AuthCenter.DAL;
    using IdentityServer4.Validation;
    using System.Security.Claims;

    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly AuthContext _db;

        public ResourceOwnerPasswordValidator( DAL.AuthContext db )
        {
            _db = db;
        }

        public async Task ValidateAsync( ResourceOwnerPasswordValidationContext context )
        {
            try
            {
                var account = context.UserName;
                var password = context.Password;

                var user = await _db.T_Users.Where( i => i.Account == account && i.Password == password ).ToOneAsync( );
                if ( null == user ) throw new Exception( "用户不存在" );

                var claims = new List<Claim>
                {
                    new Claim( ClaimTypes.NameIdentifier, user.Account ),
                    new Claim( ClaimTypes.Name, user.Name ),
                    new Claim( ClaimTypes.Role, user.IsAdmin ? "admin" : "user" ),
                };

                context.Result = new GrantValidationResult( subject: user.Account, authenticationMethod: "custom", claims: claims );
            }
            catch ( Exception ex )
            {
                context.Result = new GrantValidationResult
                {
                    IsError = true,
                    Error = ex.Message
                };
            }
        }
    }
}
