## Readme

Mame 4 soubory:

1. UserDefinedStuff - jak by vypadalo volani API z pohledu uzivatele je ve tride 
2. PackageDefinedStuff - package by obsahoval standartni tridy podobne temto
3. GeneratedStuff - obsahuje ukazku vygenerovaneho kodu pro tento priklad
4. ChatGptBrainstorm.md - obsahuje diskuzi s AI na tohle tema

### Z Ai diskuze mame tyto pointy:

Analyzator musi zakazat:

* Prirazovani Validation.Fail() atd. do promenych - vcetne napr. ternarnich operatoru, selectu atd.
* Definice validace nesmi pouzivat generiku ktera je definovana mimo validacni metody. Jinak receno validacni metoda muze byt genericka ale nesmi pouzivat generiku tridy ve ktere je definovana.
* Metoda pro definici musi vracet void
* Custom validacni metody maji stejna omezeni popsana vysse
* Definice validace musi byt static a jako prvni parametr brat validovany objekt

Co se da ohandlovat:

* Predcasny return - proste vratime aktualni errory
* Genericka Dto - vygenerujeme generickou validate metodu
* Async kod
* Vnorene Dtocka
* Automaticke dependency injection
* Custom validacni metody - pomoci atributu. Take se transformuji - meni se jejich navratovy typ o predani dependency se stara uzivatel


###  Napady 2.0

#### Jak budou fungovat asserty?

Package definuje pouze jeden assert:

`Validation.Fail()`

Vsechno ostatni jsou custom metody pouzivajici tuto validacni metodu.
Validation Fail muze by chainovane - `Validation.Fail().WithMessage("Message")`

Priklad definice assertu:

```csharp
public static ValidationContinuation IsNotNullOrEmpty(this string str, [CallerArgumentExpression] callerExpression){
    var validationContinuation  = new ValidationContinuation(callerExpression);
    if(string.IsNullOrEmpty(str)){
      ValidationContinuation.Add(Validation.Fail());
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
public static UserErrors ValidationDefinition(this Validator validator, User user)
{
    var errors = new Errors();
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
public static UserErrors ValidationDefinition(User user)
{
    var errors = new Errors();
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
public static UserErrors ValidationDefinition(User user)
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
  


