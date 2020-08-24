using HomeServing.SSO.Models;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServing.SSO.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProfileService(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private async Task<List<Claim>> GetClaimsFromUser(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
                new Claim(JwtClaimTypes.PreferredUserName, user.UserName),
                new Claim(JwtClaimTypes.NickName, user.NikeName),
                new Claim(JwtClaimTypes.Name, user.NormalizedUserName),
                new Claim(JwtClaimTypes.Profile, user.Bio),
                new Claim(JwtClaimTypes.Picture, user.Avatar),
                new Claim(JwtClaimTypes.Gender, user.Gender.ToString())
            };

            var roleBuilder = new StringBuilder();

            foreach (var role in await _roleManager.Roles.AsNoTracking().ToListAsync())
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    roleBuilder.Append($"{role.Name}/");
                }
            }

            roleBuilder.Remove(roleBuilder.Length - 1, 1);

            claims.Add(new Claim(JwtClaimTypes.Role, roleBuilder.ToString()));

            return claims;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(context => context.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);

            var claims = await GetClaimsFromUser(user);
            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = false;

            var subjectId = context.Subject.Claims.Where(q => q.Type == "sub").FirstOrDefault().Value;
            var user = await _userManager.FindByIdAsync(subjectId);

            context.IsActive = user != null && user.LockoutEnabled == false;
        }
    }
}
