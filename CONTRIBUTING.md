# Contributing to Common.Bootstrap

Vielen Dank für Ihr Interesse, zu Common.Bootstrap beizutragen! Dieses Dokument beschreibt die Richtlinien für Beiträge.

## 📑 Inhaltsverzeichnis

- [Code of Conduct](#code-of-conduct)
- [Wie kann ich beitragen?](#wie-kann-ich-beitragen)
- [Entwicklungsumgebung einrichten](#entwicklungsumgebung-einrichten)
- [Code-Style-Guidelines](#code-style-guidelines)
- [Pull-Request-Prozess](#pull-request-prozess)
- [Testing-Anforderungen](#testing-anforderungen)
- [Dokumentation](#dokumentation)

---

## Code of Conduct

Dieses Projekt folgt dem [Contributor Covenant Code of Conduct](https://www.contributor-covenant.org/version/2/1/code_of_conduct/).

Durch Ihre Teilnahme verpflichten Sie sich, diesen Code einzuhalten.

---

## Wie kann ich beitragen?

### Bugs melden

Wenn Sie einen Bug gefunden haben:

1. Prüfen Sie, ob der Bug bereits in den [Issues](https://github.com/ReneRose1971/Common.Bootstrap/issues) gemeldet wurde
2. Falls nicht, erstellen Sie ein neues Issue mit:
   - Klarer Beschreibung des Problems
   - Schritten zur Reproduktion
   - Erwartetes vs. tatsächliches Verhalten
   - .NET-Version und Betriebssystem
   - Relevante Code-Snippets oder Stack Traces

### Features vorschlagen

Feature-Vorschläge sind willkommen:

1. Prüfen Sie, ob das Feature bereits in den [Issues](https://github.com/ReneRose1971/Common.Bootstrap/issues) vorgeschlagen wurde
2. Erstellen Sie ein Issue mit:
   - Klarer Beschreibung des Features
   - Use Cases und Beispiele
   - Warum das Feature nützlich wäre
   - Mögliche Implementierungsansätze

### Code beitragen

Wir akzeptieren Pull Requests für:
- Bug-Fixes
- Neue Features (nach Diskussion im Issue)
- Dokumentationsverbesserungen
- Performance-Optimierungen
- Tests

---

## Entwicklungsumgebung einrichten

### Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- IDE: Visual Studio 2022, VS Code oder JetBrains Rider
- Git

### Repository klonen

```bash
git clone https://github.com/ReneRose1971/Common.Bootstrap.git
cd Common.Bootstrap
```

### Projekt bauen

```bash
dotnet build
```

### Tests ausführen

```bash
dotnet test
```

### Lokale NuGet-Pakete erstellen

```bash
dotnet pack -c Release
```

---

## Code-Style-Guidelines

### Allgemeine Regeln

- **Sprache:** Code und Kommentare in Englisch, XML-Dokumentation in Deutsch
- **Formatierung:** Standard .NET-Coding-Conventions
- **Namenskonventionen:** PascalCase für public Members, camelCase für private Fields

### Namespace-Organisation

```csharp
namespace Common.Bootstrap;      // Hauptkomponenten
namespace Common.Extensions;     // Extension-Methoden
```

### XML-Dokumentation

Alle öffentlichen APIs müssen XML-Dokumentation haben:

```csharp
/// <summary>
/// Beschreibung der Methode.
/// </summary>
/// <param name="services">Beschreibung des Parameters.</param>
/// <returns>Beschreibung des Rückgabewerts.</returns>
/// <exception cref="ArgumentNullException">Wann wird diese Exception geworfen.</exception>
public static IServiceCollection AddModule(this IServiceCollection services)
{
    // Implementierung
}
```

### Code-Beispiele

- Code-Beispiele in XML-Dokumentation verwenden `<example>` und `<code>` Tags
- Beispiele müssen kompilierbar und funktionsfähig sein

### Null-Handling

- Verwenden Sie Nullable Reference Types (`#nullable enable`)
- Null-Checks für öffentliche APIs:

```csharp
public void Register(IServiceCollection services)
{
    if (services == null)
        throw new ArgumentNullException(nameof(services));
    
    // Implementierung
}
```

### Exception-Handling

- Werfen Sie spezifische Exceptions (z.B. `ArgumentNullException`, `InvalidOperationException`)
- Dokumentieren Sie alle geworfenen Exceptions in XML-Docs

---

## Pull-Request-Prozess

### 1. Fork und Branch erstellen

```bash
# Repository forken (auf GitHub)
git clone https://github.com/IhrUsername/Common.Bootstrap.git
cd Common.Bootstrap

# Feature-Branch erstellen
git checkout -b feature/mein-neues-feature
```

### 2. Änderungen implementieren

- Schreiben Sie klaren, wartbaren Code
- Folgen Sie den Code-Style-Guidelines
- Fügen Sie Tests hinzu
- Aktualisieren Sie die Dokumentation

### 3. Commits

Verwenden Sie aussagekräftige Commit-Messages:

```bash
# Format: <type>: <kurze Beschreibung>
#
# Typen:
# - feat: Neues Feature
# - fix: Bug-Fix
# - docs: Dokumentation
# - test: Tests
# - refactor: Code-Refactoring
# - perf: Performance-Verbesserung
# - chore: Build/Config-Änderungen

# Beispiele:
git commit -m "feat: Add support for conditional module registration"
git commit -m "fix: Handle ReflectionTypeLoadException in assembly scanning"
git commit -m "docs: Update EqualityComparer documentation"
```

### 4. Tests ausführen

```bash
# Alle Tests müssen erfolgreich sein
dotnet test

# Build prüfen
dotnet build --configuration Release
```

### 5. Pull Request erstellen

1. Pushen Sie Ihren Branch:
   ```bash
   git push origin feature/mein-neues-feature
   ```

2. Erstellen Sie einen Pull Request auf GitHub

3. Beschreiben Sie in der PR-Beschreibung:
   - Was wurde geändert?
   - Warum wurde es geändert?
   - Wie wurde es getestet?
   - Referenzen zu Issues (z.B. `Fixes #123`)

### 6. Review-Prozess

- Ein Maintainer wird Ihren PR reviewen
- Nehmen Sie Feedback konstruktiv entgegen
- Aktualisieren Sie den PR bei Bedarf
- Nach Genehmigung wird der PR gemerged

---

## Testing-Anforderungen

### Test-Framework

- **xUnit** für Unit- und Integration-Tests
- **FluentAssertions** für Assertions (optional, aber empfohlen)

### Test-Coverage

- Neue Features müssen Tests haben
- Bug-Fixes sollten Regression-Tests enthalten
- Ziel: >80% Code-Coverage für Produktionscode

### Test-Struktur

```csharp
public sealed class MyFeatureTests
{
    [Fact]
    public void Register_WhenCalledWithValidServices_ShouldRegisterModule()
    {
        // Arrange
        var services = new ServiceCollection();
        var module = new MyModule();
        
        // Act
        module.Register(services);
        
        // Assert
        services.Should().ContainSingle(sd => sd.ServiceType == typeof(IMyService));
    }
    
    [Fact]
    public void Register_WhenCalledWithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var module = new MyModule();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => module.Register(null!));
    }
}
```

### Test-Kategorien

- **Unit-Tests:** Testen einzelne Komponenten isoliert
- **Integration-Tests:** Testen Zusammenspiel mehrerer Komponenten
- **Fixture-basierte Tests:** Für komplexe Szenarien mit DI-Container

### Test-Best-Practices

- ✅ Ein Test pro Szenario
- ✅ Klare Arrange/Act/Assert-Struktur
- ✅ Aussagekräftige Testnamen
- ✅ Keine Shared State zwischen Tests
- ✅ Tests müssen isoliert und reproduzierbar sein

---

## Dokumentation

### README-Dateien

- **Solution-README:** Kurzer Überblick, Links zu Projekt-READMEs
- **Projekt-README:** Ausführliche Dokumentation mit Beispielen

### API-Referenz

- Vollständige XML-Dokumentation für alle öffentlichen APIs
- Code-Beispiele in `<example>`-Tags
- Dokumentation in `Common.BootStrap/Docs/API-Referenz.md` pflegen

### Dokumentations-Updates

Bei Änderungen an öffentlichen APIs:

1. XML-Dokumentation aktualisieren
2. Code-Beispiele in README aktualisieren
3. API-Referenz aktualisieren
4. Ggf. neue Docs-Seiten erstellen

### Markdown-Richtlinien

- Verwenden Sie klare Überschriften
- Fügen Sie Inhaltsverzeichnisse für längere Dokumente hinzu
- Verwenden Sie Code-Blöcke mit Syntax-Highlighting
- Verwenden Sie Emojis sparsam und konsistent

---

## Weitere Fragen?

Bei Fragen:

- Erstellen Sie ein [Issue](https://github.com/ReneRose1971/Common.Bootstrap/issues)
- Schreiben Sie eine E-Mail an den Maintainer (siehe Profil)

---

## Lizenz

Durch Ihre Beiträge stimmen Sie zu, dass Ihre Arbeit unter der [MIT License](LICENSE) lizenziert wird.

---

**Vielen Dank für Ihre Beiträge! 🙏**
