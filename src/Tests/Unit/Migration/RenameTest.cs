using GetGit.Migration;
using NUnit.Framework;

namespace Tests.Unit.Migration
{
    public class RenameTest
    {
        [TestCase("A", "B", ExpectedResult = true)]
        [TestCase("A", "", ExpectedResult = false)]
        [TestCase("", "B", ExpectedResult = false)]
        [TestCase("", "", ExpectedResult = false)]
        [TestCase("A", null, ExpectedResult = false)]
        [TestCase(null, "B", ExpectedResult = false)]
        [TestCase(null, null, ExpectedResult = false)]
        public bool Rename_should_validate_that_paths_are_not_equal_or_empty(string oldPath, string newPath)
        {
            var rename = new Rename(oldPath, newPath);
            return rename.Valid();
        }
    }
}
