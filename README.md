# Made It, Ate It

Mod do gry **Timberborn** (wersja 1.1+), który daje bobrom-pracownikom pierwszeństwo do jedzenia i wody, które same wyprodukowały lub zebrały.

W grze bez modyfikacji pracownik piekarni czy pompy wody, gdy zgłodnieje, musi przerwać pracę, iść do magazynu i stać w kolejce po towar, który chwilę wcześniej sam wytworzył. Z tym modem bierze jedną porcję na miejscu i wraca do pracy.

## Jak działa mod

### Kiedy pracownik je

Pracownik pobiera jedzenie lub wodę **w momencie, gdy jego potrzeba Głód (Hunger) lub Pragnienie (Thirst) spadnie poniżej 0.1** (skala potrzeb w grze to od -3 do 1). Mod sprawdza to w każdym ticku gry, więc nie ma opóźnienia.

Za każdym razem pracownik zjada/wypija **jedną sztukę** towaru. Efekt jest taki sam jak przy normalnym jedzeniu w grze: potrzeba rośnie o wartość podaną dla danego towaru, a gra liczy to jako zwykłe spożycie (statystyki, zdarzenia).

### Budynki produkcyjne (piekarnie, grille, pompy wody itp.)

- Gdy potrzeba pracownika, który jest fizycznie w budynku, spadnie poniżej progu, a w ekwipunku budynku jest niezarezerwowany produkt zaspokajający tę potrzebę, pracownik zjada/wypija jedną sztukę.
- Jeśli w tej chwili budynek nic nie ma na stanie, pracownik dostaje porcję w momencie zakończenia najbliższego cyklu produkcji, zanim tragarze zdążą ją zarezerwować.
- Jedzone są tylko **produkty** bieżącej receptury, nigdy surowce wejściowe.
- Gdy kilku pracowników jest głodnych, pierwszeństwo ma ten, któremu brakuje najwięcej punktów potrzeby. Każdy pracownik dostaje najwyżej jedną porcję naraz.

### Farmerzy i zbieracze

- Farmer (Farma) lub zbieracz (Flaga zbieracza), który niesie zebrane jedzenie, zjada jedną sztukę prosto z rąk, gdy tylko jego potrzeba spadnie poniżej progu. Dzieje się to też od razu przy zbiorze, jeśli był już głodny.
- Reszta plonu trafia do budynku normalnie. Rezerwacja miejsca w ekwipunku jest odpowiednio pomniejszana, więc towar się nie duplikuje.

### Kogo nie dotyczy

- Botów (nie mają potrzeb).
- Pracowników, którzy nie są fizycznie w budynku (w przypadku budynków produkcyjnych).
- Towarów, które nie zaspokajają Głodu ani Pragnienia.
- Budynków, w których przełącznik jest wyłączony.

## Ustawienia

Mod działa osobno dla każdego budynku. Domyślnie jest **wyłączony**, więc po instalacji gra zachowuje się jak bez moda, dopóki go nie włączysz. Ustawienie zapisuje się w stanie gry.

Przełącznik **„Pracownicy mają pierwszeństwo”** można zmienić w trzech miejscach:

1. **Panel budynku**: po kliknięciu farmy, flagi zbieracza lub budynku produkcyjnego.
2. **Lista budynków**: w zakładce **Miejsca pracy** w panelu zbiorczym (lista wszystkich budynków) każdy obsługiwany budynek ma w swoim wierszu przełącznik **„Pierwszeństwo”**. Dzięki temu można włączać i wyłączać mod w wielu budynkach bez otwierania każdego z osobna. Zmiana w panelu budynku jest od razu widoczna na liście i odwrotnie.
3. **Kopiowanie ustawień**: ustawienie jest przenoszone razem z resztą ustawień budynku, gdy:
   - kopiujesz ustawienia z jednego budynku na inny (narzędzie kopiowania ustawień w grze),
   - duplikujesz budynek (stawiasz nowy jako kopię istniejącego).

## Wymagania

- Timberborn **1.1** lub nowszy.
- Mod **[Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=3284904751)** (Steam Workshop / mod.io). Jest potrzebny do dodania przełącznika na liście budynków. Gra pokaże ostrzeżenie w Menedżerze modów, jeśli go brakuje.

## Instalacja

Skopiuj zbudowany folder `MadeItAteIt` (z `manifest.json`, `MadeItAteIt.dll`, `Localizations/` i `thumbnail.png`) do:

```
Dokumenty\Timberborn\Mods\MadeItAteIt
```

Następnie włącz mod w Menedżerze modów przy starcie gry (razem z Harmony).

## Jak zbudować moda

Są dwie drogi. Obie dają ten sam wynik.

### Sposób 1: skrypt `build.ps1` (bez Unity)

Wymagania:

- Windows z zainstalowaną grą Timberborn (biblioteki gry z `Timberborn_Data\Managed` są potrzebne do kompilacji).
- [.NET SDK](https://dotnet.microsoft.com/download) 6.0 lub nowszy (`dotnet --version` w terminalu).
- Zasubskrybowany mod Harmony w Steam Workshop (skrypt sam znajdzie `0Harmony.dll`).

W PowerShellu, w folderze repozytorium:

```powershell
.\build.ps1 -Install
```

Skrypt:

1. Szuka gry w `C:\Program Files (x86)\Steam\steamapps\common\Timberborn` i biblioteki Harmony w folderze Workshopu.
2. Kompiluje `MadeItAteIt.dll` (projekt `Build~\MadeItAteIt.csproj`). Przy pierwszym uruchomieniu pobierze z NuGet pakiet `BepInEx.AssemblyPublicizer.MSBuild`, który udostępnia wewnętrzne elementy gry tak samo, jak robią to oficjalne narzędzia Unity.
3. Składa gotowego moda w `dist~\MadeItAteIt` i pakuje go do `dist~\MadeItAteIt-<wersja>.zip` (gotowe do wysłania na mod.io / Workshop).
4. Z `-Install` kopiuje moda do `Dokumenty\Timberborn\Mods\MadeItAteIt`.

Dodatkowe parametry:

```powershell
.\build.ps1 -GameDir "D:\SteamLibrary\steamapps\common\Timberborn"   # gra w innej bibliotece Steam
.\build.ps1 -HarmonyDll "C:\ścieżka\do\0Harmony.dll"                 # Harmony w nietypowym miejscu
```

Jeśli PowerShell blokuje skrypty, uruchom raz: `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`.

### Sposób 2: oficjalne narzędzia Unity (Mod Builder)

1. Przygotuj projekt Unity z [timberborn-modding](https://github.com/mechanistry/timberborn-modding) według [instrukcji](https://github.com/mechanistry/timberborn-modding/wiki/Unity-setup) (narzędzia same importują biblioteki gry).
2. Skopiuj `0Harmony.dll` z folderu moda Harmony do `Assets/Plugins` w projekcie Unity.
3. Skopiuj zawartość tego repozytorium do `Assets/Mods/MadeItAteIt` (foldery `Build~` i `dist~` Unity pomija).
4. Otwórz `Timberborn → Show Mod Builder`, zaznacz **Made It, Ate It** i kliknij **Build**. Mod trafi do `Dokumenty\Timberborn\Mods`. **Clean build** może dodatkowo utworzyć ZIP do publikacji.

## Publikacja i aktualizacja na Steam Workshop

Mod wysyła się z poziomu gry, przez Menedżer modów:

1. Podbij `Version` w `manifest.json`.
2. Zbuduj i zainstaluj moda: `.\build.ps1 -Install` (albo Mod Builder w Unity). Mod musi być w `Dokumenty\Timberborn\Mods\MadeItAteIt`.
3. Uruchom grę. W menu głównym otwórz **Mody** (Menedżer modów) i przy **Made It, Ate It** otwórz panel wysyłania (Upload).
4. Przy aktualizacji zaznacz, co jeszcze nadpisać (opis, widoczność, miniaturę). **Nie zaznaczaj „Upload as new”**, bo to tworzy nowy, osobny mod na Workshopie.
5. Kliknij **Upload**.

Ważne:

- Po pierwszym wysłaniu gra tworzy w folderze moda plik `workshop_data.json` z numerem moda na Workshopie (`ItemId`). Dzięki niemu kolejne wysłanie **aktualizuje** istniejący mod, a nie tworzy nowy. `build.ps1 -Install` zachowuje ten plik przy reinstalacji. Warto też skopiować go do repozytorium, obok `manifest.json`. Wtedy skrypt odtworzy go nawet po usunięciu folderu moda.
- Nazwa moda pochodzi z `manifest.json` i nie da się jej zmienić w panelu. Opis również jest brany z manifestu, ale w panelu można go nadpisać.
- Miniatura to `thumbnail.png` (lub `.jpg`) z głównego folderu moda. Musi mieć mniej niż 1 MB.
- Jeśli mod Harmony jest wymagany, warto też dodać go jako wymagany element w ustawieniach moda na stronie Workshopu.

Na [mod.io](https://mod.io/g/timberborn) mod aktualizuje się ręcznie: na stronie moda wgrywa się nowy plik ZIP, np. `dist~\MadeItAteIt-<wersja>.zip` z `build.ps1`.

## Struktura projektu

| Plik | Opis |
| --- | --- |
| `manifest.json` | Metadane moda (nazwa, wersja, minimalna wersja gry, zależność od Harmony). |
| `Scripts/MadeItAteItConfigurator.cs` | Rejestracja komponentów, panelu i listy budynków w kontenerze gry. |
| `Scripts/WorkersFirstSettings.cs` | Ustawienie przełącznika per budynek: zapis do save'a, kopiowanie ustawień (`IDuplicable`). |
| `Scripts/WorkersFirstFragment.cs` | Przełącznik w panelu budynku. |
| `Scripts/WorkersFirstBatchControl.cs` | Przełącznik w wierszach listy budynków (zakładka Miejsca pracy) i uruchomienie Harmony. |
| `Scripts/WorkerPriority.cs` | Logika dla budynków produkcyjnych. |
| `Scripts/HarvesterSnack.cs` | Logika dla farmerów i zbieraczy. |
| `Scripts/WorkerMealPicker.cs` | Wspólne funkcje: próg 0.1, wybór najbardziej potrzebującego pracownika, zastosowanie efektów jedzenia. |
| `Data/Localizations/` | Tłumaczenia (polski, angielski). |
| `Root/thumbnail.png` | Miniatura moda. |
| `build.ps1`, `Build~/` | Budowanie bez Unity. |

Próg można zmienić w jednym miejscu: stała `NeedPointsThreshold` w `Scripts/WorkerMealPicker.cs`.

## Diagnostyka

Każdy posiłek zostawia wpis w logu gry z prefiksem `[MadeItAteIt]`, np.:

```
[MadeItAteIt] Bóbr ate Bread at Bakery (need dropped)
[MadeItAteIt] Bóbr ate Berries from hands (harvested)
```
