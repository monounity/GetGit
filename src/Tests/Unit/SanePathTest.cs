using GetGit;
using NUnit.Framework;

namespace Tests.Unit
{
    public class SanePathTest
    {
        [TestCase("A", "B", ExpectedResult = "A/B")]
        [TestCase("A", "B", "C", ExpectedResult = "A/B/C")]
        [TestCase("A", "B", "", ExpectedResult = "A/B")]
        [TestCase("PathA", "PathB", ExpectedResult = "PathA/PathB")]
        [TestCase("A\\", "\\B", ExpectedResult = "A/B")]
        [TestCase("C:\\A", "B", ExpectedResult = "C:/A/B")]
        public string SanePath_should_combine_paths(params string[] paths)
        {
            return SanePath.Combine(paths);
        }

        [TestCase("A\\B", ExpectedResult = "A/B")]
        [TestCase("A\\B\\C", ExpectedResult = "A/B/C")]
        [TestCase("A\\B\\\\C", ExpectedResult = "A/B/C")]
        public string SanePath_should_normalize_path(string path)
        {
            return SanePath.Normalize(path);
        }
    }
}
