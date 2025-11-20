namespace Example;

// Placeholder class

public class Validator
{
    public required IServiceProvider ServiceProvider { get; init; }
}

// Validation Result

public class ValidationResult
{
    public bool IsError { get; init; }
    public List<string> Errors { get; init; }
}


// Attributes
[AttributeUsage(AttributeTargets.Method)]
public class ValidationDefinition : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class ValidationDefinitionFor<T> : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class CustomValidation : Attribute;



// Validation definition 
public static class ShouldClass
{
    public static BeClass Should(this string str)
    {
        return new BeClass();
    }
}

public class BeClass
{
    public StringStuff Be()
    {
        return new StringStuff();
    }
}

public class StringStuff
{
    public Terminator NotEmpty()
    {
        return new Terminator();
    }
    
    public Terminator LongerThan(
        int length)
    {
        return new Terminator();
    }
    
    public Terminator EqualTo(
        string text)
    {
        return new Terminator();
    }
    
}

public static class Validation
{
    public static Terminator Fail(string message)
    {
        return new Terminator();
    }
}

public class Terminator
{
    // Tohle je tady jen aby se nezobrazovali chyby
    public static implicit operator string(Terminator terminator)
    {
        return ""; 
    }
}