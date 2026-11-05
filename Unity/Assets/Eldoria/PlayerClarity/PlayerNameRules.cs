using Eldoria.Domain;

namespace Eldoria.PlayerClarity
{
    /// <summary>UGUI-facing alias of the one canonical domain name policy.</summary>
    public static class PlayerNameRules
    {
        public const int MinimumLength = PlayerIdentityPolicy.MinLength;
        public const int MaximumLength = PlayerIdentityPolicy.MaxLength;

        public static bool TryNormalize(string proposed, out string normalized, out string error)
        {
            return PlayerIdentityPolicy.TryNormalize(proposed, out normalized, out error);
        }
    }
}
