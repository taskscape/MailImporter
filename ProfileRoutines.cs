using System;
using System.Linq;
using TaskBeat.Logic.Profile;
using TaskBeat.Logic.Routines.Interfaces;

namespace MailImporter
{
    public class ProfileRoutines : IProfileRoutines
    {
        ProfileDTO GetProfileByHash(string hash, int contextId = 0) { throw new NotImplementedException(); }
        ProfileDTO GetProfileByName(string name, int contextId = 0)
        {
            return new ProfileDTO()
            {
                Context = 10001
            };
        }
        ProfileDTO GetProfileById(int id, int contextId = 0) { throw new NotImplementedException(); }
        ProfileDTO GetProfileByExternalID(string externalID, int contextId = 0) { throw new NotImplementedException(); }
        ProfileDTO GetProfileByResetPasswordHash(string resetPasswordHash, int contextId = 0) { throw new NotImplementedException(); }

        bool MarkProfileActive(string hash) { throw new NotImplementedException(); }
        bool MarkProfileActive(ProfileDTO profile) { throw new NotImplementedException(); }
        bool MarkProfileInactive(ProfileDTO profile) { throw new NotImplementedException(); }
        bool MarkProfileDeleted(ProfileDTO profile) { throw new NotImplementedException(); }
        bool SetProfileAccessed(ProfileDTO profile, string password, string remoteAddress) { throw new NotImplementedException(); }

        bool EnablePremiumAccount(ProfileDTO dto, TimeSpan howLong, int type) { throw new NotImplementedException(); }

        ProfileDTO CreateNewAccount(string cultureName, string applicationName, string externalID, string externalIP) { throw new NotImplementedException(); }
        ProfileDTO CreateNewAccount(string cultureName, string applicationName, string externalID, string externalIP, string userName, string contextName, string userPassword) { throw new NotImplementedException(); }
        bool UpdateProfile(int profileId, string profileName, string profilePassword, string phoneNumber) { throw new NotImplementedException(); }
        bool ModifyProfileContext(int profileId, int contextId) { throw new NotImplementedException(); }
        void SaveProfileData(ProfileDTO profile, string firstName, string lastName, Guid guid) { throw new NotImplementedException(); }
        void SaveProfileRate(ProfileDTO profile) { throw new NotImplementedException(); }
        void SaveProfileRole(ProfileDTO profile) { throw new NotImplementedException(); }

        string HashPassword(string password) { throw new NotImplementedException(); }
        ProfileDTO ValidateUserPassword(string userName, string password) { throw new NotImplementedException(); }
        bool ValidateUserEmailType(string userName) { throw new NotImplementedException(); }
        bool SetProfileCookie(ProfileDTO profile, string profileContextHash, System.Web.HttpContext context) { throw new NotImplementedException(); }

        bool GetIsProfileUnique(string userName) { throw new NotImplementedException(); }
        bool CheckIsPhoneNumberExists(string phoneNumber) { throw new NotImplementedException(); }
        string RecoverAccount(int? profileId) { throw new NotImplementedException(); }


        bool InvalidateProfileCache(int profileId) { throw new NotImplementedException(); }
        void SetProfileDefaultStartContext(int profileId, string contextHash) { throw new NotImplementedException(); }
        bool SetPasswordResetHash(int profileId, string passwordResetHash) { throw new NotImplementedException(); }
    }
}
