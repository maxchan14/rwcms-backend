using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace rWCMS.Utilities
{
    public class ActiveDirectoryUtility
    {
        private readonly string _domainName;

        public ActiveDirectoryUtility(string domainName)
        {
            _domainName = domainName ?? throw new ArgumentNullException(nameof(domainName));
        }

        public List<string> GetUserGroupSids(string userSid)
        {
            var sids = new List<string> { userSid }; // Include the user's own SID

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domainName))
                {
                    // Find the user by SID
                    var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, userSid);
                    if (userPrincipal == null)
                    {
                        throw new Exception($"User with SID {userSid} not found in Active Directory.");
                    }

                    // Get all groups (including nested) the user is a member of
                    var groups = userPrincipal.GetAuthorizationGroups()
                        .OfType<GroupPrincipal>()
                        .Select(g => g.Sid.Value)
                        .Distinct()
                        .ToList();

                    sids.AddRange(groups);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve group SIDs for user SID {userSid}: {ex.Message}", ex);
            }

            return sids.Distinct().ToList();
        }
    }
}