using GetGit.Tfs;
using GetGit.Tfs.Api.Interface;
using Moq;
using NUnit.Framework;

namespace Tests.Unit
{
    public class VersionTest
    {
        [Test]
        public void Version_should_construct_from_empty_constructor()
        {
            var version = new Version();
            Assert.That(version.ToString(), Is.Null);
            Assert.That(version.Value, Is.Null);
        }

        [Test]
        public void Version_should_construct_from_int()
        {
            var version = new Version(1);
            Assert.That(version.ToString(), Is.EqualTo("C1"));
            Assert.That(version.Value, Is.EqualTo(1));
        }

        [Test]
        public void Version_should_construct_from_number_string()
        {
            var version = new Version("1");
            Assert.That(version.ToString(), Is.EqualTo("C1"));
            Assert.That(version.Value, Is.EqualTo(1));
        }

        [Test]
        public void Version_should_construct_from_version_string()
        {
            var version = new Version("C1");
            Assert.That(version.ToString(), Is.EqualTo("C1"));
            Assert.That(version.Value, Is.EqualTo(1));
        }

        [Test]
        public void Version_should_construct_from_changeset()
        {
            var changeset = new Mock<IChangeset>();
            changeset.Setup(c => c.ChangesetId).Returns(1);

            var version = new Version(changeset.Object);
            Assert.That(version.ToString(), Is.EqualTo("C1"));
            Assert.That(version.Value, Is.EqualTo(1));
        }

        [Test]
        public void Version_should_create_version_spec()
        {
            var version = new Version(1);
            Assert.That(version.Spec().DisplayString, Is.EqualTo("C1"));
        }
    }
}
