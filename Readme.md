## Readme

Mame 4 soubory:

1. UserDefinedStuff - jak by vypadalo volani API z pohledu uzivatele je ve tride 
2. PackageDefinedStuff - package by obsahoval standartni tridy podobne temto
3. GeneratedStuff - obsahuje ukazku vygenerovaneho kodu pro tento priklad
4. ChatGptBrainstorm.md - obsahuje diskuzi s AI na tohle tema

### Z Ai diskuze mame tyto pointy:

Uzivatel musi tridy s validatorem definovat jako partial.

Dalsi body:
* Analyzer Rules (The Guard Rails)
   You will use Roslyn Analyzers to enforce a Strict DSL, ensuring the Generator only receives code it can safely translate.
   Rule 1 (No Assignments): Users cannot assign the result of Should() or Validation.Fail() to variables.
   Rule 2 (No Complex Expressions): Validation logic cannot be used inside Ternary Operators (? :) or complex compound expressions.
   Rule 3 (Enforce Partial): The class containing validation methods must be declared partial.