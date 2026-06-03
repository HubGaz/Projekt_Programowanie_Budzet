# Dokumentacja projektu – Zarządzanie Budżetem

## 1. Opis projektu

Aplikacja **Zarządzanie Budżetem** to obsługujące wiele użytkowników narzędzie konsolowe do rejestrowania przychodów i wydatków oraz monitorowania aktualnego bilansu.

Każdy użytkownik posiada własne konto (login i hasło). Dane finansowe oraz plik konta są zapisywane lokalnie w formie **zaszyfrowanej** — odczyt bez poprawnego hasła nie jest możliwy. Starszy wspólny plik `users.json` (jeśli istnieje) jest obsługiwany wyłącznie przy migracji do nowego formatu.

Aplikacja jest przeznaczona głównie dla:

* osób prywatnych chcących kontrolować swoje finanse na jednym komputerze,
* studentów uczących się podstaw programowania, serializacji JSON i kryptografii w .NET,
* użytkowników potrzebujących prostego narzędzia bez bazy danych.

### Uruchomienie

Wymagany jest [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project Projekt_Programowanie_Budzet.csproj
```

Pliki danych powstają w katalogu roboczym procesu (zwykle katalog projektu lub `bin/Debug/net8.0` przy uruchomieniu z IDE).

---

## 2. Wymagania funkcjonalne

### 2.1. Uwierzytelnianie (ekran logowania)

1. Logowanie istniejącego użytkownika (opcja `1`) — hasło wprowadzane w trybie maskowanym.
2. Rejestracja nowego konta (opcja `2`) — walidacja pustych pól i zgodności haseł.
3. Reaktywacja zawieszonego konta (opcja `3`).
4. Wyjście z programu (opcja `4`).
5. Odrzucenie logowania dla kont zawieszonych (`IsSuspended`).
6. Automatyczna migracja kont z legacy `users.json` do pliku `{login}.account` przy pierwszym udanym logowaniu.

### 2.2. Zarządzanie budżetem (menu główne, po zalogowaniu)

1. Dodawanie przychodu (opcja `1`).
2. Dodawanie wydatku (opcja `2`).
3. Wyświetlanie aktualnego bilansu z dokładnością do 2 miejsc po przecinku (opcja `3`).
4. Historia wydatków pogrupowana po dacie — suma dzienna i liczba wpisów (opcja `4`).
5. Historia przychodów pogrupowana po dacie (opcja `5`).
6. Czyszczenie wszystkich wpisów finansowych po potwierdzeniu `y/n` (opcja `6`) — plik finansów użytkownika jest resetowany, konto pozostaje.
7. Zakończenie programu (opcja `7`).
8. Włączanie/wyłączanie dźwięków systemowych konsoli (opcja `8`).
9. Zmiana koloru tekstu interfejsu: biały, zielony, niebieski, żółty (opcja `9`).
10. Ustawienia konta (opcja `10`) — patrz sekcja 2.3.

Dodatkowo:

* Zapisywanie przychodów i wydatków w jednym pliku finansów użytkownika z kluczem daty (`yyyy-MM-dd`).
* Automatyczne tworzenie zaszyfrowanego pliku finansów przy pierwszym dostępie.
* Migracja nieszyfrowanego JSON finansów do formatu zaszyfrowanego przy pierwszym odczycie po logowaniu.
* Obsługa błędnych danych wejściowych (np. niepoprawne kwoty lub błędna opcja menu).
* Wstrzymanie programu po każdej operacji do momentu naciśnięcia Enter.
* Wylogowanie do ekranu logowania po wyjściu z ustawień konta (zawieszenie, usunięcie) bez kończenia całej aplikacji — opcja `7` kończy program.

### 2.3. Ustawienia konta (opcja `10`)

1. Zmiana nazwy użytkownika (loginu) — wymaga hasła; pliki `.account` i `_finance.json` są przenosiane.
2. Zmiana hasła — wymaga bieżącego hasła; po zmianie sesja szyfrowania finansów jest odświeżana.
3. Zmiana aliasu wyświetlanego (opcjonalnie, puste pole usuwa alias).
4. Zawieszenie konta — opcjonalny powód; logowanie zablokowane do reaktywacji.
5. Usunięcie konta — potwierdzenie przez wpisanie `DELETE` i hasło; usuwa pliki konta i finansów.
6. Powrót do menu głównego.

---

## 3. Wymagania niefunkcjonalne

1. **Wydajność** — operacje dodawania i odczytu danych są natychmiastowe przy typowym wolumenie wpisów.
2. **Niezawodność** — błędy operacji plikowych i kryptograficznych są obsługiwane komunikatami; program nie powinien kończyć się awarią przy typowych błędach użytkownika.
3. **Użyteczność** — prosty interfejs tekstowy (menu), maskowane hasła, opcjonalne dźwięki i personalizacja koloru.
4. **Przenośność** — aplikacja działa na systemach z .NET 8 (Windows, Linux, macOS).
5. **Trwałość danych** — dane zapisywane lokalnie w plikach per użytkownik.
6. **Bezpieczeństwo danych** — hasła przechowywane jako hash SHA-256 z solą; pliki kont i finansów szyfrowane AES-256-GCM z kluczem pochodzącym z hasła (PBKDF2-SHA256, 100 000 iteracji). Brak szyfrowania dysku ani ochrony przed atakami z pełnym dostępem do plików i hasła.
7. **Skalowalność (ograniczona)** — rozwiązanie przeznaczone dla małej liczby użytkowników i rekordów na jednej maszynie.

---

## 4. Role użytkowników

### Użytkownik zarejestrowany

Po zalogowaniu może:

* dodawać przychody i wydatki,
* sprawdzać bilans i historię,
* czyścić wpisy finansowe,
* konfigurować dźwięki i kolor interfejsu,
* zarządzać własnym kontem (alias, hasło, login, zawieszenie, usunięcie).

### Użytkownik niezalogowany

Może jedynie: zalogować się, zarejestrować, reaktywować zawieszone konto lub wyjść.

> System nie implementuje ról administracyjnych — każdy użytkownik ma dostęp wyłącznie do własnych plików po poprawnym uwierzytelnieniu.

---

## 5. Przypadki użycia

### Przypadek 1: Rejestracja i pierwsze logowanie

**Aktor:** Nowy użytkownik  
**Scenariusz:**

1. Wybiera opcję `2` na ekranie logowania.
2. Podaje login, hasło (maskowane) i powtórzenie hasła.
3. System tworzy zaszyfrowany plik `{login}.account`.
4. Użytkownik loguje się (opcja `1`); system tworzy zaszyfrowany plik `{login}_finance.json` przy pierwszym wejściu do menu.

---

### Przypadek 2: Dodanie przychodu

**Aktor:** Zalogowany użytkownik  
**Scenariusz:**

1. Wybiera opcję `1` w menu głównym.
2. Wprowadza kwotę przychodu.
3. System dopisuje wpis pod kluczem bieżącej daty (`yyyy-MM-dd`) w zaszyfrowanym pliku finansów i aktualizuje bilans w strukturze danych.

---

### Przypadek 3: Dodanie wydatku

**Aktor:** Zalogowany użytkownik  
**Scenariusz:**

1. Wybiera opcję `2`.
2. Wprowadza kwotę wydatku.
3. System zapisuje wpis analogicznie do przychodu w sekcji `expense`.

---

### Przypadek 4: Sprawdzenie bilansu

**Aktor:** Zalogowany użytkownik  
**Scenariusz:**

1. Wybiera opcję `3`.
2. System odszyfrowuje plik finansów, sumuje przychody i wydatki, wyświetla bilans (`F2`).

---

### Przypadek 5: Historia operacji

**Aktor:** Zalogowany użytkownik  
**Scenariusz:**

1. Wybiera opcję `4` (wydatki) lub `5` (przychody).
2. System wyświetla daty posortowane chronologicznie z sumą dzienną i liczbą wpisów.

---

### Przypadek 6: Czyszczenie danych finansowych

**Aktor:** Zalogowany użytkownik  
**Scenariusz:**

1. Wybiera opcję `6`.
2. Potwierdza `y`.
3. System zapisuje pustą strukturę finansów (konto użytkownika bez zmian).

---

### Przypadek 7: Zmiana hasła

**Aktor:** Zalogowany użytkownik  
**Scenariusz:**

1. Opcja `10` → `2`.
2. Podaje bieżące i nowe hasło (maskowane).
3. System ponownie szyfruje plik `.account` i odświeża sesję szyfrowania pliku finansów nowym hasłem.

---

### Przypadek 8: Zawieszenie i reaktywacja konta

**Aktor:** Zalogowany użytkownik / użytkownik zawieszony  
**Scenariusz:**

1. Zawieszenie: opcja `10` → `4`, opcjonalny powód, hasło — konto oznaczone jako `IsSuspended`, powrót do logowania.
2. Reaktywacja: na ekranie logowania opcja `3`, login i hasło — flaga zawieszenia usunięta.

---

### Przypadek 9: Usunięcie konta

**Aktor:** Zalogowany użytkownik  
**Scenariusz:**

1. Opcja `10` → `5`, wpisanie `DELETE`, hasło.
2. System usuwa pliki `.account` i `_finance.json` użytkownika.

---

## 6. Model danych

### 6.1. Konto użytkownika (`UserAccount`)

Zapisywane w zaszyfrowanym pliku `{safeUsername}.account` (JSON po odszyfrowaniu):

| Pole | Typ | Opis |
|------|-----|------|
| `Username` | `string` | Login (unikalny, case-insensitive przy rejestracji) |
| `PasswordHash` | `string` | Hash SHA-256(hasło + sól), Base64 |
| `Salt` | `string` | Sól hasła, Base64 |
| `Alias` | `string?` | Opcjonalna nazwa wyświetlana w UI |
| `IsSuspended` | `bool` | Czy konto jest zawieszone |
| `SuspendedAt` | `DateTimeOffset?` | Data zawieszenia |
| `SuspensionReason` | `string?` | Opcjonalny powód zawieszenia |

`safeUsername` — login po normalizacji: trim, spacje → `_`, znaki niedozwolone w nazwie pliku → `_`, małe litery.

### 6.2. Dane finansowe (`UserFinanceData`)

Jeden plik `{safeUsername}_finance.json` na dysku (zaszyfrowany binarnie):

| Pole | Typ | Opis |
|------|-----|------|
| `Entries` | słownik | Klucze: `"income"`, `"expense"` |
| `Entries[income\|expense]` | `Dictionary<string, List<double>>` | Data `yyyy-MM-dd` → lista kwot |
| `CurrentBalance` | `double` | Bilans (przeliczany przy zapisie) |
| `UpdatedAt` | `DateTimeOffset` | Znacznik ostatniej aktualizacji |

### 6.3. Pliki na dysku

| Plik | Format | Zawartość |
|------|--------|-----------|
| `{safeUsername}.account` | Binarny `BMENC1` + AES-GCM | Konto użytkownika (JSON wewnątrz) |
| `{safeUsername}_finance.json` | Binarny `BMENC1` + AES-GCM | Przychody, wydatki, bilans |
| `users.json` | JSON (legacy) | Lista kont — tylko migracja; usuwany wpis po migracji użytkownika |

### 6.4. Format szyfrowania (`BMENC1`)

Wspólny dla plików konta i finansów:

1. Nagłówek: `BMENC1` (6 bajtów)
2. Sól pliku: 16 bajtów (losowa przy tworzeniu, stała przy kolejnych zapisach)
3. Nonce AES-GCM: 12 bajtów
4. Tag uwierzytelniający: 16 bajtów
5. Długość ciphertextu: 4 bajty (`int`)
6. Ciphertext

Klucz AES-256: `PBKDF2-SHA256(hasło UTF-8, sól_pliku + safeUsername UTF-8, 100 000 iteracji)`.

---

## 7. Architektura systemu

### Typ architektury

Aplikacja monolityczna (konsolowa), modułowa według przestrzeni nazw.

### Struktura katalogów

```
BudgetManagement/
├── main.cs                    # Interfejs konsoli, menu, przepływ aplikacji
├── Authentication/
│   ├── AuthService.cs         # Rejestracja, logowanie, reaktywacja
│   ├── UserStore.cs           # Pliki .account, migracja users.json
│   ├── ProfileService.cs      # Zmiana loginu/hasła/aliasu, zawieszenie, usunięcie
│   ├── UserAccount.cs         # Model konta
│   ├── UserFilePaths.cs       # Ścieżki plików per użytkownik
│   └── PasswordHasher.cs      # Sól i hash hasła (SHA-256)
├── FileManagement/
│   ├── Files.cs               # CRUD danych finansowych (JSON + szyfrowanie)
│   ├── FileCrypto.cs          # PBKDF2, AES-GCM, format BMENC1
│   └── EncryptedFileSession.cs# Sesja klucza na czas sesji użytkownika
├── MoneyManagement/
│   ├── Incomes.cs             # Stan przychodów w pamięci
│   └── Expenses.cs            # Stan wydatków w pamięci
└── Miscellaneous/
    ├── Aestetics.cs           # Logo, czyszczenie ekranu, Enter
    ├── AsyncSoundPlayer.cs    # Kolejka dźwięków konsoli
    └── ConsoleInput.cs        # Maskowane wprowadzanie hasła
```

### Warstwy

#### 1. Interfejs użytkownika

* Konsola (`Console UI`)
* Ekran logowania i menu główne / ustawień
* `ConsoleInput` — maskowanie hasła

#### 2. Logika biznesowa

* `Incomes`, `Expenses` — agregaty w pamięci podczas sesji
* `AuthService`, `ProfileService` — reguły kont użytkowników

#### 3. Warstwa dostępu do danych

* `UserStore` — konta (szyfrowane `.account`)
* `Files` — finanse (szyfrowany jeden plik JSON na użytkownika)
* `FileCrypto` / `EncryptedFileSession` — kryptografia plików

### Schemat przepływu

```
[Użytkownik]
     ↓
[Console UI / main.cs]
     ↓
[AuthService / ProfileService] ──→ [{login}.account]
     ↓
[Incomes / Expenses] ←→ [Files] ──→ [{login}_finance.json]
     ↑
[EncryptedFileSession + FileCrypto]
```

---

## 8. Technologie i uzasadnienie

| Technologia | Zastosowanie |
|-------------|--------------|
| **C# / .NET 8** | Język i platforma uruchomieniowa |
| **System.IO** | Operacje na plikach lokalnych |
| **System.Text.Json** | Serializacja modeli konta i finansów |
| **System.Security.Cryptography** | PBKDF2, AES-GCM, SHA-256, losowe sole i nonce |
| **Aplikacja konsolowa** | Prosty interfejs bez GUI; wystarczająca dla projektu edukacyjnego |

---

## 9. Ograniczenia i uwagi implementacyjne

* Hasła przy logowaniu w konsoli są maskowane, ale nadal trafiają do pamięci procesu — to narzędzie lokalne, nie przeznaczone do środowisk wysokiego ryzyka.
* Nazwy plików wynikają z loginu (`safeUsername`) — lista loginów może być widoczna w katalogu roboczym.
* Zmiana loginu wymaga przeniesienia obu plików (`.account`, `_finance.json`); kolizja nazwy jest blokowana.
* Opcja `7` w menu głównym kończy całą aplikację; wylogowanie bez zamykania następuje po zawieszeniu lub usunięciu konta.
* Brak synchronizacji między uruchomieniami — dane są wyłącznie lokalne.
