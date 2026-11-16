# DR musiksamling – UI-tests

Separat Visual Studio-solution med MSTest og Selenium.
Kræver .NET 8 SDK og Chrome/Chromium. Selenium Manager henter en passende driver
og kræver internetadgang første gang.

1. Start backend på http://localhost:5000 med de tre eksempelplader.
2. Start frontend på http://localhost:5500.
3. Kør `dotnet test MusicRecords.UITests.sln`.

Valgfrie miljøvariabler: `FRONTEND_URL` og `CHROME_BINARY` (sti til Chromium).
Testen kører headless, venter på API-kaldet og kontrollerer antal plader samt titel,
kunstner, formateret varighed og udgivelsesår. Backend eller CDN-fejl giver fejl i testen.

Til login- og logout-testen sættes `UI_TEST_USERNAME`, `UI_TEST_PASSWORD` og
`UI_TEST_ROLE` til den lokale testkonto fra backendens loginopsætning.
Testen for ugyldigt login kan køres uden en testkonto.

Kør hele suiten mod en frisk testdatabase med de tre eksempelplader. Testen for
tilføjelse og sletning kræver rollen `Editor` og opretter en plade med unikt navn,
som den sletter igen som en del af testen.
