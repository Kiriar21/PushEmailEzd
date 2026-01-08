# 📧 PushEmailEzd

Aplikacja Blazor Server do pobierania emaili i rejestracji ich jako wpływów w systemie EZD PUW.

## ⚡ Szybki start

### Wymagania
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Instalacja i uruchomienie

```bash
# Sklonuj repozytorium
git clone https://github.com/twoje-repo/PushEmailEzd.git
cd PushEmailEzd

# Przywróć pakiety i uruchom
dotnet restore
dotnet run
```

Otwórz przeglądarkę: **https://localhost:7100**

## ⚙️ Konfiguracja

Edytuj plik `appsettings.json`:

```json
{
  "EmailSettings": {
    "ImapServer": "mail.twoja-domena.pl",
    "Port": 993,
    "UseSsl": true,
    "Username": "twoj@email.pl",
    "Password": "twoje-haslo"
  },
  "EzdSettings": {
    "ApiUrl": "https://ezd-api.example.pl/",
    "CID": 123,
    "IdPracownikaWlasciciela": 456,
    "IdStanowiskaWlasciciela": 789
  }
}
```

## 🎯 Funkcje

- ✅ Pobieranie emaili przez IMAP
- ✅ Podgląd treści i załączników
- ✅ Formularz rejestracji z edycją pól
- ✅ Wybór załączników do przesłania
- ✅ Nawigacja między wieloma mailami
- ✅ Usuwanie emaili ze skrzynki
- ✅ Wyświetlanie numeru RPW po rejestracji

## 📁 Struktura projektu

```
PushEmailEzd/
├── Components/
│   ├── Pages/Home.razor          # Główna strona
│   ├── Layout/MainLayout.razor   # Layout + style
│   └── RegistrationFormModal.razor
├── Models/
│   ├── EmailMessage.cs
│   ├── EzdRegistrationForm.cs
│   └── EzdRegistrationResult.cs
├── Services/
│   ├── EmailService.cs           # IMAP (MailKit)
│   └── EzdApiService.cs          # API EZD (SOAP)
└── appsettings.json              # Konfiguracja
```

## 🛠️ Technologie

- Blazor Server (.NET 8)
- MailKit (IMAP)
- Bootstrap 5
