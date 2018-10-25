using GetGit.Migration;
using NUnit.Framework;

namespace Tests.Unit.Migration
{
    public class HashTest
    {
        [TestCase("Hello", new byte[] { 139, 26, 153, 83, 196, 97, 18, 150, 168, 39, 171, 248, 196, 120, 4, 215 }, ExpectedResult = true)]
        [TestCase("olleH", new byte[] { 139, 26, 153, 83, 196, 97, 18, 150, 168, 39, 171, 248, 196, 120, 4, 215 }, ExpectedResult = false)]
        public bool Hash_should_compare_equality(string content, byte[] expectedHash)
        {
            var actualHash = Hash.Compute(content);
            return Hash.AreEqual(actualHash, expectedHash);
        }
    }
}
