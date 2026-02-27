using FluentAssertions;
using FluentValidation.TestHelper;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Api.Validators;

namespace OptimizationHeuristics.Api.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new RegisterRequest("test@example.com", "Password1", "Test User");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Fails()
    {
        var request = new RegisterRequest("", "Password1", "Test User");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Invalid_Email_Fails()
    {
        var request = new RegisterRequest("not-an-email", "Password1", "Test User");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Short_Password_Fails()
    {
        var request = new RegisterRequest("test@example.com", "Pass1", "Test User");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Without_Uppercase_Fails()
    {
        var request = new RegisterRequest("test@example.com", "password1", "Test User");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Without_Digit_Fails()
    {
        var request = new RegisterRequest("test@example.com", "Password", "Test User");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Empty_DisplayName_Fails()
    {
        var request = new RegisterRequest("test@example.com", "Password1", "");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Fact]
    public void Long_DisplayName_Fails()
    {
        var request = new RegisterRequest("test@example.com", "Password1", new string('A', 101));
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }
}

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new LoginRequest("test@example.com", "password");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Fails()
    {
        var request = new LoginRequest("", "password");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Empty_Password_Fails()
    {
        var request = new LoginRequest("test@example.com", "");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

public class RefreshRequestValidatorTests
{
    private readonly RefreshRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new RefreshRequest("some-token");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Token_Fails()
    {
        var request = new RefreshRequest("");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}

public class RevokeRequestValidatorTests
{
    private readonly RevokeRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new RevokeRequest("some-token");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Token_Fails()
    {
        var request = new RevokeRequest("");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
