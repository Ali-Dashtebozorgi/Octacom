using Octacom.Domain.ValueObjects;

namespace Octacom.UnitTests;

using FluentAssertions;

public class EmailTests
{
    [Fact]
    public void Email_WhenValidAddress_ShouldCreateSuccessfully()
    {
        var emailAddress = "ali@gmail.com";
        var email = new Email(emailAddress);

        email.Value.Should().Be(emailAddress);
    }

    [Fact]
    public void Email_WhenInvalidAddress_ShouldThrowArgumentException()
    {
        var emailAddress = "not-valid-address";
        Action act = () => new Email(emailAddress);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_WhenEmpty_ShouldThrowArgumentException()
    {
        Action act = () => new Email("");

        act.Should().Throw<ArgumentException>();
    }
    [Fact]
    public void Email_WhenTwoEmailsHaveSameValue_ShouldBeEqual()
    {
        
        var email1 = new Email("ali@test.com");
        var email2 = new Email("ali@test.com");

        email1.Should().Be(email2);
    }
}