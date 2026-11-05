using System;
using System.Globalization;
using System.Text;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Pure rules for a player's chosen display name. Identity and persistence
    /// must remain in the authoritative PlayerState/LocalGateway, not PlayerPrefs.
    /// This class deliberately does not store or invent a name.
    /// </summary>
    public static class PlayerNicknameRules
    {
        public const int MinCharacters = 2;
        public const int MaxCharacters = 16;

        /// <summary>Returns a normalized name for a future authoritative name command.</summary>
        public static bool TryNormalize(string input, out string name, out string error)
        {
            name = "";
            error = "";
            if (string.IsNullOrEmpty(input))
            {
                error = "Introduce un nombre.";
                return false;
            }
            // Bound processing before normalizing arbitrary input or loading a save.
            if (input.Length > 128)
            {
                error = "El nombre es demasiado largo.";
                return false;
            }

            string candidate;
            try { candidate = input.Normalize(NormalizationForm.FormC).Trim(' '); }
            catch (ArgumentException)
            {
                error = "El nombre contiene caracteres no válidos.";
                return false;
            }

            var result = new StringBuilder(candidate.Length);
            bool previousSpace = false, containsLetterOrDigit = false;
            foreach (char character in candidate)
            {
                if (character == ' ')
                {
                    if (result.Length > 0 && !previousSpace) result.Append(' ');
                    previousSpace = true;
                    continue;
                }
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                bool mark = category == UnicodeCategory.NonSpacingMark ||
                            category == UnicodeCategory.SpacingCombiningMark;
                // No markup, control characters, bidi overrides, emoji, slashes,
                // newlines or zero-width format codes in a public display name.
                if (!char.IsLetterOrDigit(character) && !mark &&
                    character != '-' && character != '_' && character != '\'')
                {
                    error = "Usa letras, números, espacios, guiones o apóstrofes.";
                    return false;
                }
                if (mark && result.Length == 0)
                {
                    error = "El nombre contiene caracteres no válidos.";
                    return false;
                }
                result.Append(character);
                previousSpace = false;
                if (char.IsLetterOrDigit(character)) containsLetterOrDigit = true;
            }
            candidate = result.ToString().TrimEnd(' ');
            if (!containsLetterOrDigit)
            {
                error = "Introduce un nombre.";
                return false;
            }
            int count = StringInfo.ParseCombiningCharacters(candidate).Length;
            if (count < MinCharacters)
            {
                error = "El nombre debe tener al menos dos caracteres.";
                return false;
            }
            if (count > MaxCharacters)
            {
                error = "El nombre no puede superar 16 caracteres.";
                return false;
            }

            name = candidate;
            return true;
        }
    }
}
