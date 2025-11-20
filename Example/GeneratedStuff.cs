namespace Example;

// Prvni krok - najdu vsechny metody s atributem CustomValidation
// Zkopiruji je a nahradim v nich vsechny definice validaci za realne validace
public static class CustomExtensions
{
    public static ValidationResult ShouldNotBeEmptyGenerated(
        this string text)
    {
        var validationResult = new ValidationResult();
        if (text.Length > 110)
        {
            // Bylo by nahrazeno
            validationResult.Errors.Add(Validation.Fail("Text is too long"));
        }
        
        return validationResult;
    }
    
}


// Tyhle extension metody jsou vygenerovane ve stejne assembly jako byly definovany schema tim zajistime ze mame nareferencovane vsechny assemblies co potrebujeme
public static class GeneratedValidator
{
    public static void Validate(
        this Validator validator, // tahle dependency slouzi hlavne pro usera jako marker aby mel odkud to zavolat
        UserSettings user)
    {
        user.IsAngry.Should().Be().EqualTo("true");
    }
    
    public static async Task<ValidationResult> Validate(
        this Validator validator, // tahle dependency slouzi jen pro ziskani zavislosti a jako marker pro usera
        User user) // validace pro usera
    {
        // zavislosti vime podle definice v metode tak ze je muzeme jednodusse resolvovat
        var userRepo = (UserRepo)validator.ServiceProvider.GetService(typeof(UserRepo))!;

        var validationResult = new ValidationResult();
        // Tentokrat ale ty co uz neco delaji
        validationResult.Errors.Add(user.Name.Should().Be().LongerThan(5));
        validationResult.Errors.Add(user.Surname.Should().Be().LongerThan(5));
        // Custom metody jednoduse take nahradime (vime ze budou existovat protoze budou take vygenerovany)
        // Tady je dulezite ze neni potreba delat nejake rekurzivni prochazeni nebo tak neco. My vime ze existuje metoda
        // "ShouldNotBeEmptyGenerated" - tak ze ji sem proste dame. Jedine co tak budeme muset predat jeji parametry spravne ale to by melo byt ok.
        validationResult.Errors.AddRange(user.AccountCreatedDate.ShouldNotBeEmptyGenerated().Errors);
        
        // tohle zustava stejne
        if (await userRepo.UserExists(user.Name))
        {
            validationResult.Errors.Add(Validation.Fail("User already exists"));
        }

        // Muzeme take analyzovat vsechny property a pokud zjistime ze k nektere z nich ma uzivatel definovany validator tak ho zavolame
        if (user.Settings != null)
        {
            validator.Validate(user.Settings);
        }

        return validationResult;
    }
}