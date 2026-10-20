# Made It, Ate It

Mod do gry **Timberborn** (wersja 1.1+), który daje bobrom-pracownikom pierwszeństwo do jedzenia i wody, które same wyprodukowały lub zebrały.

W grze bez modyfikacji pracownik piekarni czy pompy wody, gdy zgłodnieje, musi przerwać pracę, iść do magazynu i stać w kolejce po towar, który chwilę wcześniej sam wytworzył. Z tym modem po prostu bierze jedną porcję na miejscu.

## Jak to działa

Pracownik pobiera jedzenie **w momencie, gdy jego potrzeba Głód (Hunger) lub Pragnienie (Thirst) spadnie poniżej 0.1** (skala potrzeb w grze to od -3 do 1). Mod sprawdza to w każdym ticku gry.

### Budynki produkcyjne (piekarnie, grille, pompy wody itp.)

- Gdy potrzeba pracownika przebywającego w budynku spadnie poniżej progu, a w ekwipunku budynku jest niezarezerwowany produkt, który zaspokaja tę potrzebę, pracownik zjada/wypija jedną sztukę.
- Jeśli w tej chwili budynek nic nie ma na stanie, pracownik dostaje porcję w momencie zakończenia najbliższego cyklu produkcji, zanim tragarze zdążą ją zarezerwować.
- Jedzone są tylko **produkty** bieżącej receptury, nigdy surowce wejściowe.
- Gdy kilku pracowników jest głodnych, pierwszeństwo ma ten, któremu brakuje najwięcej punktów potrzeby. Każdy pracownik dostaje najwyżej jedną porcję naraz.

### Farmerzy i zbieracze

- Farmer (Farma) lub zbieracz (Flaga zbieracza), który niesie zebrane jedzenie, zjada jedną sztukę prosto z rąk, gdy tylko jego potrzeba spadnie poniżej progu. Dzieje się to także od razu przy zbiorze, jeśli był już głodny.
- Reszta plonu trafia do budynku normalnie (rezerwacja miejsca w ekwipunku jest odpowiednio pomniejszana, więc towar się nie duplikuje).

### Kogo nie dotyczy

- Boty (nie mają potrzeb).
- Pracownicy, którzy nie są fizycznie w budynku (w przypadku budynków produkcyjnych).
- Towary, które nie zaspokajają Głodu ani Pragnienia.

## Ustawienia

W panelu każdego obsługiwanego budynku (produkcja, farma, flaga zbieracza) pojawia się przełącznik **„Pracownicy mają pierwszeństwo”**. Domyślnie jest **wyłączony**, a ustawienie zapisuje się w stanie gry dla każdego budynku osobno.

## Struktura projektu

| Plik | Opis |
| --- | --- |
| `manifest.json` | Metadane moda (nazwa, wersja, minimalna wersja gry). |
| `Scripts/MadeItAteItConfigurator.cs` | Rejestracja komponentów i fragmentu panelu w kontenerze gry. |
| `Scripts/WorkersFirstSettings.cs` | Ustawienie przełącznika per budynek, z zapisem do save'a. |
| `Scripts/WorkersFirstFragment.cs` | Przełącznik w panelu budynku. |
| `Scripts/WorkerPriority.cs` | Logika dla budynków produkcyjnych. |
| `Scripts/HarvesterSnack.cs` | Logika dla farmerów i zbieraczy. |
| `Scripts/WorkerMealPicker.cs` | Wspólne funkcje: próg 0.1, wybór najbardziej potrzebującego pracownika, zastosowanie efektów jedzenia. |
| `Data/Localizations/` | Tłumaczenia (polski, angielski). |

Próg można zmienić w jednym miejscu: stała `NeedPointsThreshold` w `Scripts/WorkerMealPicker.cs`.

## Budowanie

Projekt budujemy narzędziami z oficjalnego repozytorium [timberborn-modding](https://github.com/mechanistry/timberborn-modding): skopiuj ten folder do `Assets/Mods/MadeItAteIt` w projekcie Unity i zbuduj mod przez Timberborn → Show Mod Builder. Gotowy mod trafia do `Documents/Timberborn/Mods`.

## Diagnostyka

Każdy posiłek zostawia wpis w logu gry z prefiksem `[MadeItAteIt]`, np.:

```
[MadeItAteIt] Bóbr ate Bread at Bakery (need dropped)
```
