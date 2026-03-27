# Dokumentacja projektu – Zarządzanie Budżetem

## 1. Opis projektu

Aplikacja **Zarządzanie Budżetem** jest narzędziem konsolowym do rejestrowania przychodów i wydatków oraz monitorowania aktualnego bilansu.

System zapisuje dane do plików lokalnych `.json`, dzięki czemu dane są zachowywane między uruchomieniami.

Aplikacja jest przeznaczona głównie dla:

* osób prywatnych chcących kontrolować swoje finanse,
* studentów uczących się podstaw programowania i pracy z plikami JSON,
* użytkowników potrzebujących prostego narzędzia bez bazy danych.

---

## 2. Wymagania funkcjonalne

System umożliwia:

1. Dodawanie przychodów przez użytkownika (opcja `1`).
2. Dodawanie wydatków przez użytkownika (opcja `2`).
3. Obliczanie i wyświetlanie aktualnego bilansu (przychody - wydatki) z dokładnością do 2 miejsc po przecinku (opcja `3`).
4. Wyświetlanie historii wydatków pogrupowanej po dacie (opcja `4`).
5. Wyświetlanie historii przychodów pogrupowanej po dacie (opcja `5`).
6. Czyszczenie wszystkich danych (`income.json`, `expense.json`, `balance.json`) po potwierdzeniu `y/n` (opcja `6`).
7. Zakończenie działania programu (opcja `7`).
8. Zapisywanie przychodów do pliku `income.json` i wydatków do pliku `expense.json` z kluczem daty (`yyyy-MM-dd`).
9. Zapisywanie bieżącego bilansu do pliku `balance.json`.
10. Automatyczne tworzenie plików danych, jeśli nie istnieją.
11. Obsługę błędnych danych wejściowych (np. niepoprawne kwoty lub błędna opcja menu).
12. Wstrzymanie programu po każdej operacji do momentu naciśnięcia Enter.

---

## 3. Wymagania niefunkcjonalne

1. **Wydajność** - aplikacja działa natychmiastowo dla operacji dodawania i odczytu danych.
2. **Niezawodność** - błędy operacji plikowych są obsługiwane i nie powinny kończyć programu awarią.
3. **Użyteczność** - prosty interfejs tekstowy (menu) umożliwia intuicyjną obsługę.
4. **Przenośność** - aplikacja działa na systemach wspierających .NET.
5. **Trwałość danych** - dane są zapisywane w lokalnych plikach JSON.
6. **Bezpieczeństwo danych** - brak autoryzacji i ról; aplikacja przeznaczona do lokalnego, prostego użytku.
7. **Skalowalność (ograniczona)** - rozwiązanie przeznaczone dla małej liczby rekordów.

---

## 4. Role użytkowników

### Użytkownik końcowy

* dodaje przychody,
* dodaje wydatki,
* sprawdza bieżący bilans,
* przegląda historię wydatków i przychodów,
* czyści wszystkie dane po potwierdzeniu.

> System nie implementuje ról administracyjnych ani mechanizmów autoryzacji - wszyscy użytkownicy mają te same uprawnienia.

---

## 5. Przypadki użycia

### Przypadek 1: Dodanie przychodu

**Aktor:** Użytkownik  
**Scenariusz:**

1. Użytkownik wybiera opcję `1`.
2. Wprowadza kwotę przychodu.
3. System zapisuje wartość do `income.json` pod kluczem bieżącej daty.
4. System aktualizuje i zapisuje bilans do `balance.json`.

---

### Przypadek 2: Dodanie wydatku

**Aktor:** Użytkownik  
**Scenariusz:**

1. Użytkownik wybiera opcję `2`.
2. Wprowadza kwotę wydatku.
3. System zapisuje wartość do `expense.json` pod kluczem bieżącej daty.
4. System aktualizuje i zapisuje bilans do `balance.json`.

---

### Przypadek 3: Sprawdzenie bilansu

**Aktor:** Użytkownik  
**Scenariusz:**

1. Użytkownik wybiera opcję `3`.
2. System odczytuje sumy przychodów i wydatków z plików.
3. System wyświetla bilans w formacie z 2 miejscami po przecinku.

---

### Przypadek 4: Sprawdzenie historii wydatków

**Aktor:** Użytkownik  
**Scenariusz:**

1. Użytkownik wybiera opcję `4`.
2. System wyświetla wydatki pogrupowane po dacie (suma dzienna i liczba wpisów).

---

### Przypadek 5: Sprawdzenie historii przychodów

**Aktor:** Użytkownik  
**Scenariusz:**

1. Użytkownik wybiera opcję `5`.
2. System wyświetla przychody pogrupowane po dacie (suma dzienna i liczba wpisów).

---

### Przypadek 6: Czyszczenie danych

**Aktor:** Użytkownik  
**Scenariusz:**

1. Użytkownik wybiera opcję `6`.
2. System pyta o potwierdzenie (`y/n`).
3. Po potwierdzeniu usuwa pliki danych i tworzy je ponownie jako puste.

---

## 6. Model danych

System operuje na następujących strukturach:

### Income (Przychód)

* `amount` (`double`)
* `date` (`string`, format `yyyy-MM-dd`) - jako klucz w JSON

### Expense (Wydatek)

* `amount` (`double`)
* `date` (`string`, format `yyyy-MM-dd`) - jako klucz w JSON

### Balance (Bilans)

* `currentBalance` (`double`)
* `updatedAt` (`DateTimeOffset`)

### Pliki:

* `income.json` - słownik: data -> lista kwot przychodów
* `expense.json` - słownik: data -> lista kwot wydatków
* `balance.json` - aktualny bilans i znacznik czasu aktualizacji

---

## 7. Architektura systemu

### Typ architektury:

Aplikacja monolityczna (konsolowa)

### Warstwy:

#### 1. Interfejs użytkownika

* Konsola (`Console UI`)
* Obsługa menu, walidacji i komunikatów

#### 2. Logika biznesowa

* Klasy:
  * `Incomes`
  * `Expenses`
* Aktualizacja wartości finansowych

#### 3. Warstwa dostępu do danych

* Klasa `Files`
* Operacje na plikach: tworzenie, odczyt, zapis, usuwanie
* Serializacja i deserializacja JSON

#### Schemat:

```
[Użytkownik]
     ↓
[Console UI]
     ↓
[Logika biznesowa]
     ↓
[System plików (JSON)]
```

---

## 8. Technologie i uzasadnienie

### C#

* Główny język programowania
* Silne typowanie i dobra integracja z .NET

### .NET

* Platforma uruchomieniowa
* Obsługa konsoli i systemu plików

### System.IO

* Operacje na plikach (`create`, `read`, `write`, `delete`)

### System.Text.Json

* Serializacja danych do formatu JSON
* Odczyt i zapis struktur typu słownik/lista

### Aplikacja konsolowa

* Prosta implementacja
* Brak potrzeby GUI
* Wystarczająca dla projektu edukacyjnego i małych zastosowań
