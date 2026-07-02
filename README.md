## Wymagania

- .NET 10
- Microsoft SQL Server lub SQL Server Express

## Instalacja SQL

Za pomocą **SQL Server Management Studio** należy uruchomić:
1. `schema.sql` — tworzy strukturę bazy
a następnie:
2. `data.sql` — wgrywa przykładowe dane

## Konfiguracja połączenia

Konfigurujemy plik `dyplomy.cfg`:

```json
{
  "Server": "localhost\\SQLEXPRESS",
  "Database": "system_dyplomow",
  "IntegratedSecurity": true,
  "UserId": "",
  "Password": ""
}
```

- **IntegratedSecurity: true** — logowanie userem Windows
- **IntegratedSecurity: false** — logowanie userem SQL - należy wpisać login i hasło do bazy
