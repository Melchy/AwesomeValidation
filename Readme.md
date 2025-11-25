## AwesomeValidations

Je package ktery umoznuje validace stejne jako FluentValidation ale lepe.

### Jak to funguje

User definuje metody ValidationDefinition. Ty pak prezvyka source generator a udela trochu upravenou kopii metody.
Tato kopie je extension metodou nad tridou "Validator" coz je instancni trida dodana touto knihovnou kterou si uzivatel zaregistruje
do sveho DI.


#### Jak budou fungovat asserty?

Package nedefinuje zadny assert. Pouze custom assert metody (package bude davat nejake basic asserty, zbytek je na uzivateli):

```csharp
public static ValidationContext IsNotNullOrEmpty(this string str, [CallerArgumentExpression("str")] callerExpression){
    var validationContinuation  = new ValidationContext();
    if(string.IsNullOrEmpty(str)){
      validationContinuation.AddError($"String can not be null or empty {x.CallerExpression}");
    }

    return validationContinuation;
}
```

**Generator tyhle meotdy vubec netransformuje.**

#### Jak bude vypadat validace?

Validace je pak easy. Metody musi splnovat:

* Musi byt static
* Musi vracet void nebo `Task`
* Jako prni parametr musi brat to co validuji
* Musi byt public
* ValidationContinuation nesmi byt nikdy prirazeno do promene
* Samy validacni metody nesmi byt extension metody
* Zakazat local functions
* 

Priklad:
```csharp
public static void ValidationDefinition(User user)
{
    user.Name.IsNotNullOrEmpty().WithMessage("Bla bla");
}
```

tohle se pak transformuje:

```csharp
// Validator slouzi ke ziskani depedencies ze service provideru
// ValidationContext context slouzi k predani informaci v pripade vnorenych validacnich metod
public static UserErrors Validate(this Validator validator, User user, ValidationContext? context = null)
{
    var errors = new Errors(context);
    errors.Add(user.Name.IsNotNullOrEmpty().WithMessage("Bla bla"));

    return Errors.ToUserError();
}
```

### Problemy a jejich reseni:

#### Co s volanim privatnich promenych?
Pravdepodobne muzeme vsechny privatni veci generatorem zkopirovat.

#### Co kdyz chce user na zaklade validace udelat nejake rozhodovani. Neco jako If(valid) doX else doY

Pravdepodobne muzeme udelat metody .ValidateAndGetResult():

```csharp
public static void ValidationDefinition(User user)
{
    var bool = user.Name.IsNotNullOrEmpty().WithMessage("Bla bla").ValidateAndGetResult();
    if(bool){
       // validace prosla neco udelej
    }else{
      // validace neprosla udelej neco jineho
    }
}
```

Generator pak udela:

```csharp
public static UserErrors Validate(User user, ValidationContext context, ValidationContext? context = null)
{
    var errors = new Errors(context);
    var tempResult = user.Name.IsNotNullOrEmpty().WithMessage("Bla bla");
    errors.Add(tempResult);

    if(tempResult.Success){
       // validace prosla neco udelej
    }else{
      // validace neprosla udelej neco jineho
    }


    return Errors.ToUserError();
    
}
```

#### Predcasny return


```csharp
public static void ValidationDefinition(User user)
{
    var bool = user.Name.IsNotNullOrEmpty().WithMessage("Bla bla").ValidateAndGetResult();
    if(bool){
       return;
    }

    user.Surname.IsNotNullOrEmpty();
}
```

Generator:

```csharp
public static UserErrors Validate(User user, ValidationContext? context = null)
{
    var errors = new Errors();
    var tempResult = user.Name.IsNotNullOrEmpty().WithMessage("Bla bla");
    errors.Add(tempResult);

    if(tempResult.Success){
       return Errors.ToUserError();
    }else{
      // validace neprosla udelej neco jineho
    }

    return Errors.ToUserError()
}
```

#### List validation

Validace nad listem jsou trochu komplikovanejsi. Mame 2 situace:

1. Vstupen je list a vystopem je max jeden error - napr pocet musi byt vic nez X
2. Vstupen je list a vystupem je list erroru - napr. kazda hodnota musi byt vic nez X

Prvni varianta je easy:
```csharp
public static ValidationContext IsLongerThanX<T>(this IEnumerable<T> input, int minLength, [CallerArgumentExpression("input")] callerExpression){
    var validationContinuation  = new ValidationContext();
    if(input.Length < minLength){
      validationContinuation.AddError("$List {callerExpression} must be longer than X");
    }

    return validationContinuation;
}
```

Druha varianta kdy kazdy prvek potrebuje validaci samostatne:
```csharp
public static ValidationContext EachMustBeLongerThanX(this IEnumerable<string> inputs, int length, [CallerArgumentExpression("inputs")] callerExpression){
    var validationContinuation  = new ValidationContext();
    foreach(var (input, index) in inputs.Index())
    {
        if(input.Length < lenght){
            validationContinuation.AddError("${callerExpression} element {index} must be longer than X");
        }
    }
    
    return validationContinuation;
}
```

#### Quick validace

V nekterych pripadech se hodi genericka kontrola ktera "neco" checkuje:

```csharp
public static ValidationContext FailValidation(string errorMessage)
{
    var validationContext = new ValidationContext();
    validationContext.AddError(errorMessage);
    return validationContext;
}
```

pouziti:

```csharp
public static void ValidationDefinition(User user)
{
    if(x)
    {
        Validation.FailValidation("Error x is true");
    }
    
```    

### Dependency injection

User muze neco nainjektovat:

```csharp
public static void ValidationDefinition(User user, [Inject] UserRepository userRepository)
{
    user.Surname.IsNotNullOrEmpty();
    // userRepository.. neco
}
```

Generator:

```csharp
public static UserErrors Validate(this Validator validor, User user, ValidationContext? context = null)
{
    var userRepository = validor.ServiceProvider.GetService<UserRepository>();
    user.Surname.IsNotNullOrEmpty();
    // userRepository.. neco
}
```
### Custom parametry

Pokud uzivatel nepouzije [Inject] attribut tak pocitame ze je to bezny parametr ktery chce predat pri validaci.

#### Validace complexnich objektu - vyuziti , ValidationContext

Pokud ma objekt vnorene struktury tak je muzeme by default take validovat:

```csharp

public class User
{
    public string Name {get; set;}
    public Address Address {get;set;}
    public string IEnumerable<Order> Orders {get;set;}
}

public static void ValidationDefinition(Address address)
{
    // validace pro Address
}

public static void ValidationDefinition(Order address)
{
    // validace pro order
}

public static void ValidationDefinition(User user)
{
    user.Name.IsNotNullOrEmpty().WithMessage("Bla bla");
}
```

Generator vytvori:

```csharp
public static UserErrors Validate(this Validator validor, User user, ValidationContext? context = null)
{
    // Klasika
    var errors = new Errors(context);
    errors.Add(user.Name.IsNotNullOrEmpty().WithMessage("Bla bla"));

    // Nyni se podiva na dalsi public property a fieldy objektu user a zjisti jejich typy.
    // Pokud zjisti ze pro nektery z nich ma definovanou metody ValidationDefinition. 
    // Tak vy ze se musi vygenerovat validacni metoda pro ne a zde ji zavola:
    
    if(user.Address != null)
    {
        errors.Add(validor.Validate(user.Address, ValidationContext.CreateChildContext(context, user.Address)));
    }
    
    // Predani kontextu zajisti ze chybova hlaska bude obsahovat uplne informace
    foreach(var (order, index) in user.Orders.Indexed())
    {
        if(order != null)
        {
            errors.Add(validor.Validate(user.Address, ValidationContext.CreateChildContext(context, user.Orders, order, index)));
        }
    }
    
    
    return Errors.ToUserError()
}
```
### Co kdyz chce uzivatel validaci nested objektu rucne?

Muzeme pridat "direktyvy" pro generator ktere tuto informaci predaji:

```csharp
Validation.DoNotAutoValidateNestedObjects();
Validation.DoNotAutoValidate<TType>();
Validation.AutoValidateAllExcept<TType>();
```
uzivatel si pak muze zavolat validaci rucne treba:

```csharp
if(x)
{
    Validation.Validate<TType>();
}
```
My pak doplnime zbytek.

### Rodice a deti

Podobnou otazkou jsou parent validace. Myslim ze validator by automaticky mel spoustet parent validator.
Logika je stejna jako pro nested objekty a muzeme pridat direktyvu pro vypnuti:

```csharp
Validation.DoNotAutoValidateParentObjects();
Validation.DoNotAutoValidateParent<TType>(); // skipne jednoho z parentu v hyerarchii
```

### Validace interfacu

Technicky vzato je mozne pridat validaci i pro interface. Z pohledu validatoru se nic nemeni.
Pouze je potreba s tim pocitat.

### Preklady

Fluent validation nejak resi i preklady. Potrebujeme taky vyresit.

### Jak vyresit usingy

Normalne je zkopirujeme jedna ku jedne.

### Context ve withMessage

V nekterych pripadech chceme custom message ale chceme i originalni context.
Muzeme pridat metodu:

```csharp
.WithMessage(context => $"Custom validation meesage s contextem {context.CallerExpression}, {context.OriginalMessage}...")
```

### Nasledujici problemy jsou TOP voted problemy s fluent validation (na stack overflow) a jejich reseni v AwesomeValidations:

#### Conditional validation
https://stackoverflow.com/questions/8084374/conditional-validation-using-fluent-validation/8086267#8086267

```csharp
RuleFor(customer => customer.CustomerDiscount)
    .GreaterThan(0)
    .When(customer => customer.IsPreferredCustomer);
```
AwesomeAssertions:

```csharp
public static void ValidationDefinition(User customer)
{
    if(customer.IsPreferredCustomer){
       customer.CustomerDiscount.GreaterThan(0);
    }
}
```
### Class hierarchy

https://stackoverflow.com/questions/30730937/c-sharp-fluentvalidation-for-a-hierarchy-of-classes/36022690#36022690

```csharp
public class Base
{
    // Fields to be validated
}

public class Derived1 : Base
{
    // More fields to be validated
}

public class Derived2 : Base
{
    // More fields to be validated
}

public class Derived2Validator : AbstractValidator<Derived2>
{
    public Derived2Validator()
    {
        Include(new BaseValidator());
        Include(new Derived1Validator());
        RuleFor(d => d.Derived1Name).NotNull();
    }
}

```

AwesomeValidation - automaticky handluje:

```csharp
public static void ValidationDefinition(Derived2 derived2)
{
    derived2.NotNull();
}
```

### Multi property validation

```csharp

public class FooArgs
{
    public string Zip { get; set; }
    public System.Guid CountyId { get; set; }
}

public class FooValidator : AbstractValidator<FooArgs>
{
    RuleFor(m => new {m.CountyId, m.Zip}).Must(x => ValidZipCounty(x.Zip, x.CountyId))
                                      .WithMessage("Wrong Zip County");
}
```

AwesomeValidation:

```csharp
public static void ValidationDefinition(FooArgs fooArgs)
{
    if(!ValidZipCounty(x.Zip, x.CountyId))
    {
        Validation.FailValidation("Wrong Zip County");
    }
}
```

### Validace pole

https://stackoverflow.com/questions/21309747/fluentvalidation-validating-a-view-model-that-contains-a-list-of-an-object/53227565#53227565

```csharp
public class CustomerViewModel
{
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Display(Name = "Phone")]
    public string Phone { get; set; }

    [Display(Name = "Email")]
    public string EmailAddress { get; set; }

    public List<Guitar> Guitars { get; set; } 
}


RuleForEach(x => x.Guitars).SetValidator(new GuitarValidator());
```

AwesomeValidation - **validuje automaticky staci nadefinovat validator pro Guitar**.
Popripade jde udelat:

```csharp
public static void ValidationDefinition(CustomerViewModel customer)
{
    Validation.DoNotAutoValidateNestedObjects();
    foreach(var (guitar, i) in customer.Guitars.Index())
    {
        guitar.X.NotNull().WithMessage(x => $"Guitar {i} has null X");
        guitar.T.NotNull().WithMessage(x => $"Guitar {i} has null T");
    }
}
```

### Stop on first failure

https://stackoverflow.com/questions/21605534/stop-fluent-validation-on-first-failure/21607365#21607365

Odpoved resi trochu jiny problem - vznika tam obava ze objekt bude null.
```csharp
this.CascadeMode = CascadeMode.StopOnFirstFailure;
RuleFor(x => x.TechnicalHeader).NotNull().WithMessage("Header cannot be null");

// Ensure TechnicalHeader is provided
When(x => x.TechnicalHeader != null, () => {
    RuleFor(x => x.TechnicalHeader.Userid).NotEmpty().WithMessage("Userid cannot be null or an empty string");
    RuleFor(x => x.TechnicalHeader.CabCode).GreaterThan(0).WithMessage("CabCode cannot be or less than 0");
    RuleFor(x => x.TechnicalHeader.Ndg).NotEmpty().WithMessage("Ndg cannot be null or an empty string");
});
```
nebo

```csharp
RuleFor(object => object.String)
    .NotNull()
    .DependentRules(() =>
    {
        RuleFor(object => object.String)
            .NotEmpty()
            .Matches("^[A-Z]{3}$");
    });
```


AwesomeAssertions:


```csharp
public static void ValidationDefinition(User user)
{
    var isTechnicalHeader = user.TechnicalHeader.IsNull().WithMessage("Bla bla").ValidateAndGetResult();
    if(!isTechnicalHeader)
    {
       return;
    }
    
    // dalsi validace
}
```

### Slozita validace

https://stackoverflow.com/questions/27213058/how-can-i-access-the-collection-item-being-validated-when-using-ruleforeach/29809446#29809446

Typek chce:
For example, suppose we have Customer and Orders, and we want to ensure that no customer order has a total value that exceeds the maximum allowed for that customer:
a k tomu do error message dat adresu daneho zakaznika.

```csharp
public class CustomerValidator
{
    public CustomerValidator()
    {
        RuleFor(customer => customer.Orders).SetCollectionValidator(new OrderValidator());
    }
}

public class OrderValidator
{
    public OrderValidator()
    {
         RuleFor(order => order.TotalValue)
             .Must((order, total) => total <= order.Customer.MaxOrderValue)
             .WithState(order => GetErrorInfo(order)); // pass order info into state
    }
}
```
nebo 

```csharp
public class CustomerValidator
{
    public CustomerValidator()
    {
        RuleForEach(customer => customer.Orders)
            .Must((customer, order) => order.TotalValue) <= customer.MaxOrderValue)
            .WithMessage("order with Id = {0} have error. It's total value exceeds {1}, that is maximum for {2}",
                (customer, order) => order.Id,
                (customer, order) => customer.MaxOrderValue,
                (customer, order) => customer.Name);
    }
}
```

AwesomeAssertions - just do what you need:

```csharp
public static void ValidationDefinition(IEnumberable<Customer> customers)
{
    Validation.DoNotAutoValidate<Order>();
    foreach(var customer in customers)
    {
        if(customer.Orders.Max(order => order.TotalValue) <= customer.MaxOrderValue)
        {
            Validation.FailValidation($"Max orders value exceeded. Cutomer name: {customer.Name}");
        }
    }
   

```
