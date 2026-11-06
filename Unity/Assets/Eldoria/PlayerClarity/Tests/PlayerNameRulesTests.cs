using Eldoria.PlayerClarity;
using NUnit.Framework;

namespace Eldoria.PlayerClarity.Tests
{
    public sealed class PlayerNameRulesTests
    {
        [TestCase("  Álvaro_27  ", "Álvaro_27")]
        [TestCase("Sir   Aldric", "Sir Aldric")]
        [TestCase("Valoria-1", "Valoria-1")]
        [TestCase("123", "123")]
        [TestCase("Jugadora Ñ", "Jugadora Ñ")]
        public void NamesAreNormalizedForThePersistedDisplay(string input, string expected)
        {
            Assert.That(PlayerNameRules.TryNormalize(input, out var actual, out var error), Is.True, error);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("AB")]
        [TestCase("0123456789012345678")]
        [TestCase("<script>")]
        [TestCase("A\nB")]
        [TestCase("A\tB")]
        [TestCase("Luz🙂")]
        public void InvalidNamesNeverReachTheSave(string input)
        {
            Assert.That(PlayerNameRules.TryNormalize(input, out var actual, out var error), Is.False);
            Assert.That(actual, Is.Empty);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void NamesAtTheMaximumLengthAreAccepted()
        {
            const string name = "012345678901234567";
            Assert.That(name.Length, Is.EqualTo(PlayerNameRules.MaximumLength));
            Assert.That(PlayerNameRules.TryNormalize(name, out var actual, out _), Is.True);
            Assert.That(actual, Is.EqualTo(name));
        }
    }
}
