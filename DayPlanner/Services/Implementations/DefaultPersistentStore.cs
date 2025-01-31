using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace DayPlanner.Services.Implementations;

internal class DefaultPersistentStore(IMemoryCache cache) : IPersistentStore
{
    private static string UserFile => Path.Combine(FileSystem.Current.AppDataDirectory, _userFile);

    private const string _userFile = "user";
    private const string _accessKey = "userAuthKey";

    private const string _userCacheKey = "user";

    public async Task SetUserAsync(User user, string accessToken)
    {
        using BinaryWriter claimsWriter = new(File.Create(UserFile));
        user.ToClaimsPrincipial().WriteTo(claimsWriter);

        await SecureStorage.Default.SetAsync(_accessKey, accessToken);

        cache.Remove(_userCacheKey);
    }

    public async Task<(User user, string accessToken)?> GetUserAsync()
    {
        if (!File.Exists(UserFile))
            return null;

        return await cache.GetOrCreateAsync<(User, string)?>(_userCacheKey, async entry =>
        {
            using BinaryReader claimsReader = new(File.OpenRead(UserFile));
            User user = new ClaimsPrincipal(claimsReader).ToUser();

            string accessToken = await SecureStorage.Default.GetAsync(_accessKey)
                ?? throw new InvalidOperationException("Unable to access user's access token");

            return (user, accessToken);
        });
    }

    public Task RemoveUserAsync()
    {
        if (File.Exists(UserFile))
        {
            File.Delete(UserFile);
            SecureStorage.Default.Remove(_accessKey);

            cache.Remove(_userCacheKey);
        }

        return Task.CompletedTask;
    }
}
