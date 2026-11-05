using System;
using System.Text;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Shared nickname validation for the native first-entry UI and the eventual
    /// authoritative GameCommand. A name is never a save key or a PlayerId.
    /// </summary>
    public static class PlayerNameRules
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 18;

        public static bool TryNormalize(string proposed, out string normalized, out string error)
        {
            normalized = "";
            error = "";
            if (string.IsNullOrWhiteSpace(proposed))
            {
                error = "Escribe un nombre para continuar.";
                return false;
            }

            var result = new StringBuilder(proposed.Length);
            bool previousSpace = false;
            foreach (char ch in proposed.Trim())
            {
                if (char.IsWhiteSpace(ch))
                {
                    // Never let line breaks or zero-width/control whitespace enter
                    // a player name or surprise an existing UGUI text layout.
                    if (ch != ' ')
                    {
                        error = "Usa un nombre sin saltos de línea.";
                        return false;
                    }
                    if (!previousSpace) result.Append(' ');
                    previousSpace = true;
                    continue;
                }
                previousSpace = false;
                if (!char.IsLetterOrDigit(ch) && ch != '-' && ch != '_')
                {
                    error = "Usa letras, números, espacios, guiones o _.";
                    return false;
                }
                result.Append(ch);
            }
            var name = result.ToString().Trim();
            if (name.Length < MinimumLength || name.Length > MaximumLength)
            {
                error = "El nombre debe tener entre 3 y 18 caracteres.";
                return false;
            }
            normalized = name;
            return true;
        }
    }
}
