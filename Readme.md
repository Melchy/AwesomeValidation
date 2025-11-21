## Readme

Mame 4 soubory:

1. UserDefinedStuff - jak by vypadalo volani API z pohledu uzivatele je ve tride 
2. PackageDefinedStuff - package by obsahoval standartni tridy podobne temto
3. GeneratedStuff - obsahuje ukazku vygenerovaneho kodu pro tento priklad
4. ChatGptBrainstorm.md - obsahuje diskuzi s AI na tohle tema

### Z Ai diskuze mame tyto pointy:

Analyzator musi zakazat:

* Prirazovani Should() or Validation.Fail() atd. do promenych - vcetne napr. ternarnich operatoru, selectu atd.
* Definice validace nesmi pouzivat privatni promene, metody fieldy atd.
* Definice validace nesmi pouzivat instancni promene
* Definice validace nesmi byt v genericke tride pokud je to validator mimo Dto
* Metoda pro definici musi vracet void
* Custom validacni metody maji stejna omezeni popsana vysse

Co se da ohandlovat:

* Predcasny return - proste vratime aktualni errory
* Genericka Dto - vygenerujeme generickou validate metodu
* Async kod
* Vnorene Dtocka
* Automaticke dependency injection
* Custom validacni metody - pomoci atributu. Take se transformuji - meni se jejich navratovy typ o predani dependency se stara uzivatel
