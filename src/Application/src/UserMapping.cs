using System.Diagnostics.CodeAnalysis;

namespace GetGit
{
    public class UserMapping
    {
        public UserMapping(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; }
        public string Email { get; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Name + " => " + Email;
        }
    }
}
