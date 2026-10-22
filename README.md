# Made It, Ate It

A Timberborn mod (version 1.1.1, game 1.1+) that lets beavers eat or drink some of what they produced or harvested themselves before it leaves their hands.

[Polski opis poniżej](#polski)

## What it does

Each workplace gets a **Workers get priority** toggle. It is off by default. When it is on:

- **Production buildings** (water pumps, bakeries, grills and other manufactories): when a production cycle finishes, one unit of each food or water product goes to the neediest worker inside the building, before haulers can reserve it.
- **Farmhouses**: a farmer carries the harvest back as usual. The moment it is delivered to the farmhouse, a hungry or thirsty farmer eats one unit of it, before haulers can reserve it.
- **Gatherer flags**: a gatherer who picks something edible eats one unit straight from their hands, right at the bush. The rest is delivered as usual.

A beaver only uses this when its **Hunger or Thirst drops below 0.1**. Otherwise the goods go to storage as normal. Bots are never fed. Eating applies the same effects as a normal meal or drink.

## Where to switch it on

- **Building panel**: the *Workers get priority* checkbox appears for workplaces that support it.
- **Workplaces list** (batch control): a round button with a bowl icon at the end of each supported building's row. The tooltip shows the current state.
- **Duplicate** and **Copy settings** carry the toggle over to the target building, if that building supports it.

The setting is saved with the game. A building with the toggle off adds nothing to the save.

## Installation

Copy the `MadeItAteIt` folder (containing `manifest.json`, `thumbnail.png`, `Localizations/` and `Scripts/MadeItAteIt.dll`) into `Documents\Timberborn\Mods`, then enable the mod in the game's mod manager.

## Project layout

| Path | Contents |
| --- | --- |
| `Scripts/` | C# source, compiled into `MadeItAteIt.dll` |
| `Data/Localizations/` | English and Polish texts |
| `Root/` | Mod thumbnail |
| `manifest.json` | Mod name, id and version |

The scripts reference the game's assemblies from `Timberborn_Data/Managed` and can be built with the Timberborn modding tools or a plain .NET SDK project.

---

## Polski

Mod do Timberborna (wersja 1.1.1, gra 1.1+). Pozwala bobrom zjeść lub wypić część tego, co same wyprodukowały albo zebrały, zanim to od nich odejdzie.

### Działanie

Każde miejsce pracy dostaje przełącznik **Pracownicy mają pierwszeństwo**. Domyślnie jest wyłączony. Po włączeniu:

- **Budynki produkcyjne** (pompy wody, piekarnie, grille i inne warsztaty): po zakończeniu cyklu produkcji jedna sztuka każdego produktu spożywczego lub wody trafia do najbardziej potrzebującego pracownika w budynku, zanim zarezerwują ją tragarze.
- **Gospodarstwa**: rolnik normalnie odnosi plon do gospodarstwa. W chwili, gdy go tam odda, zjada jedną sztukę, jeśli jest głodny lub spragniony, zanim zarezerwują ją tragarze.
- **Stanowiska zbieraczy**: zbieracz, który zbiera coś jadalnego, zjada jedną sztukę prosto z rąk, przy krzaku. Resztę odnosi normalnie.

Bóbr korzysta z tego tylko wtedy, gdy jego **głód lub pragnienie spadnie poniżej 0,1**. W pozostałych przypadkach towar trafia normalnie do magazynu. Boty nigdy nie są karmione. Zjedzenie działa tak samo jak zwykły posiłek lub napój.

### Gdzie to włączyć

- **Panel budynku**: pole *Pracownicy mają pierwszeństwo* w miejscach pracy, które to obsługują.
- **Lista „Miejsca pracy”**: okrągły przycisk z ikoną miski na końcu wiersza obsługiwanego budynku. Podpowiedź pokazuje aktualny stan.
- **Powiel** i **Kopiuj ustawienia** przenoszą przełącznik na budynek docelowy, jeśli ten budynek go obsługuje.

Ustawienie zapisuje się razem z grą. Budynek z wyłączonym przełącznikiem nic nie dodaje do zapisu.

### Instalacja

Skopiuj folder `MadeItAteIt` (z plikami `manifest.json`, `thumbnail.png`, folderem `Localizations/` i `Scripts/MadeItAteIt.dll`) do `Documents\Timberborn\Mods`, a potem włącz mod w menedżerze modów gry.
