using System;
using System.Text;

namespace Eldoria.Domain
{
    /// <summary>
    /// Canonical one-time player name rules, independent from UGUI.
    /// The player nickname is display text, never an ID, command key or save path.
    /// </summary>
    public static class PlayerIdentityPolicy
    {
        public const int MinLength = 3;
        public const int MaxLength = 18;

        public static bool TryNormalize(string proposed, out string nickname, out string error)
        {
            nickname = "";
            error = "";
            if (string.IsNullOrWhiteSpace(proposed))
            {
                error = "Escribe un nombre para continuar.";
                return false;
            }
            var canonical = proposed.Normalize(NormalizationForm.FormC);
            var output = new StringBuilder(canonical.Length);
            var previousSpace = false;
            foreach (char c in canonical.Trim())
            {
                if (c == ' ')
                {
                    if (!previousSpace) output.Append(' ');
                    previousSpace = true;
                    continue;
                }
                previousSpace = false;
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                {
                    error = "Usa letras, números, espacios, guiones o _.";
                    return false;
                }
                output.Append(c);
            }
            var result = output.ToString().Trim();
            if (result.Length < MinLength || result.Length > MaxLength)
            {
                error = "El nombre debe tener entre 3 y 18 caracteres.";
                return false;
            }
            nickname = result;
            return true;
        }
    }
}
