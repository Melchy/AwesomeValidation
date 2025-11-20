namespace Example;

// object ktery chceme validovat
public class User
{
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public required string AccountCreatedDate { get; init; }
    public UserSettings? Settings { get; init; }
}

public class UserSettings
{
    public required string IsAngry { get; init; }

    // Easy validace
    // Zatim jsem dal ze to generator hleda podle attributu
    [ValidationDefinition]
    public void Validation()
    {
        IsAngry.Should().Be().EqualTo("true");
    }
}

// Validace v jine tride
public class ValidationForUser
{
    [ValidationDefinitionFor<User>()]
    public async Task ValidationDefinition(
        User user,
        UserRepo userRepo) // Injectovana zavislost
    {
        user.Name.Should().Be().LongerThan(5);
        user.Surname.Should().Be().LongerThan(5);
        // Custom assert definovany dale
        user.AccountCreatedDate.ShouldNotBeEmpty();

        if (await userRepo.UserExists(user.Name))
        {
            Validation.Fail("User already exists");
        }
    }
}

// Custom validace ukazka
public static class CustomValidationsClass
{
    // Tohle je potreba doresit lepe aby to navazovalo na Should().Be() pattern
    [CustomValidation]
    public static void ShouldNotBeEmpty(
        this string text)
    {
        if (text.Length > 110)
        {
            Validation.Fail("Text is too long");
        }
    }
}

// Placeholder repo
public class UserRepo
{
    public async Task<bool> UserExists(
        string name)
    {
        return await Task.FromResult(true);
    }
}

// Volani assertu
public class ControllerExample(Validator validator)
{
    public async Task CreateUser(User user)
    {
        var result = await validator.Validate(user);

        if (result.IsError)
        {
            throw new Exception("Je to v pic....");
        }
    }
}