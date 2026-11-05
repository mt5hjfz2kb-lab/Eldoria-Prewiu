using Eldoria.PlayerClarity;
using NUnit.Framework;

namespace Eldoria.PlayerClarity.Tests
{
    public sealed class PlayerNicknameRulesTests
    {
        [TestCase("  María   Luisa  ", "María Luisa")]
        [TestCase("Sir-Aldric", "Sir-Aldric")]
        [TestCase("Señor_7", "Señor_7")]
        [TestCase("王小明", "王小明")]
        public void AcceptsDistinctNamesAndNormalizesWhitespace(string raw, string expected)
        {
            Assert.That(PlayerNicknameRules.TryNormalize(raw, out var value, out var error), Is.True, error);
            Assert.That(value, Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("A")]
        [TestCase("abcdefghijklmnopq")]
        [TestCase("<script>")]
        [TestCase("A\\B")]
        [TestCase("Ana\nLuis")]
        [TestCase("A\u202EB")]
        [TestCase("👑")]
        [TestCase("---")]
        public void RejectsEmptyTooShortTooLongOrUnsafeInput(string raw)
        {
            Assert.That(PlayerNicknameRules.TryNormalize(raw, out var value, out var error), Is.False);
            Assert.That(value, Is.Empty);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void SameInputProducesTheSameCanonicalName()
        {
            Assert.That(PlayerNicknameRules.TryNormalize("  Álvaro  42  ", out var a, out _), Is.True);
            Assert.That(PlayerNicknameRules.TryNormalize("Álvaro 42", out var b, out _), Is.True);
            Assert.That(a, Is.EqualTo(b));
        }
    }
}
