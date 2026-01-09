# 30_tests.rules.instructions.md
# Copilot – Tests: Strategie, Isolation, Fixtures, Context

## Zweck & Geltungsbereich
Diese Datei definiert verbindliche Regeln für das Entwerfen und Implementieren von Tests.
Sie ergänzt:
- `00_general.behavior.instructions.md` (Workflow, Genehmigung, Solution-Suche)
- `10_code.quality.instructions.md` (Architektur, minimal-invasive Änderungen)
- `20_comments.docs.instructions.md` (keine Geschichten, keine Alt/Neu-Erzählungen)

---

## Grundsatz: Erst Testtyp bestimmen (Pflicht)
Vor jeder Testimplementierung muss Copilot den Testtyp bestimmen und begründen:

- **Typ A: Unit / Small-Component-Test**
  - wenige Abhängigkeiten, klarer fachlicher Kern
  - kein kompletter Bootstrap notwendig

- **Typ B: DI-/Bootstrap-/Integration-Test**
  - SUT hat viele transitive Abhängigkeiten
  - Registrierungen/Wiring sind Teil der Korrektheit
  - Persistenz/Stores/Repositories sind real beteiligt

Ohne klare Typbestimmung: **keinen Code erzeugen**.

---

## Entscheidungslogik: On-the-fly vs Fixture (Pflicht)

### On-the-fly (Typ A) ist korrekt, wenn:
- Abhängigkeitsgraph ist klein und explizit verdrahtbar
- Mocks/Fakes sind fachlich sinnvoll (nicht entwertend)
- keine Notwendigkeit besteht, DI-Registrierungen zu prüfen

### Fixture + per-Test Context (Typ B) ist korrekt, wenn:
- Bootstrapping/Registrierungen/Wiring relevant sind
- SUT nur sinnvoll über DI auflösbar ist
- echte Persistenz/Stores/Repositories Teil des Szenarios sind
- On-the-fly zu Duplikation, unklarem Setup oder unvollständiger Verdrahtung führt

Copilot darf nicht „kurzschlussartig“ neue Test-Utilities erfinden.
Vor jeder Neuanlage ist Solution-weit nach bestehenden Fixtures/Helpern/Factories zu suchen.

---

## Verbot: Globaler Shared State
Tests dürfen keinen geteilten, veränderlichen Zustand verwenden.

Unzulässig:
- gemeinsame DB-Dateien / gemeinsame Persistenzartefakte zwischen Tests
- statische/global gecachte Service-Instanzen, die Zustand tragen
- Tests, die sich gegenseitig beeinflussen oder Reihenfolge voraussetzen

Zulässig:
- geteilte **Infrastruktur** innerhalb einer Testklasse (siehe IClassFixture),
  sofern pro Test ein isolierter Context erzeugt wird.

---

## Fixtures: erlaubt, aber nur als Infrastruktur

### Erlaubt: IClassFixture (Infrastruktur-Fixture)
Für Typ-B-Tests ist `IClassFixture<TFixture>` zulässig, wenn:
- die Fixture **einmal pro Testklasse** die Infrastruktur vorbereitet
  (Registrierungen/Bootstrap/Config)
- die Fixture **keinen** fachlichen Act global ausführt
- pro Test ein **neuer isolierter Testlauf** erzeugt wird (siehe TestContext)

### Verboten: Collection Fixtures
- `Collection` / `CollectionDefinition` / shared fixtures über Testklassen hinweg sind **verboten**.

Begründung: zu hohes Risiko für Shared State und unklare Kopplungen.

---

## TestContext: per Test isolierter Lauf (für Typ B)

### Definition (Pflichtverständnis)
„TestContext“ bezeichnet hier eine **konkrete Klasse/Instanz pro Test**, die:
- einen **neuen DI-Scope** besitzt
- Services/SUT **aus diesem Scope** auflöst
- isolierte Ressourcen (z. B. Zufallspfade/Stores) **pro Test** verwendet
- am Ende sauber freigibt (Dispose)

### Rolle
- Fixture = Infrastruktur (einmal pro Testklasse)
- TestContext = isolierter Testlauf (einmal pro Test)
- Testmethode = Assertions (und ggf. minimaler Orchestrationscode)

### Act im TestContext
Fachliches Act darf im TestContext stattfinden, **aber nur pro Test**.
Unzulässig ist fachliches Act in der Fixture-Initialisierung, wenn dadurch Zustand für mehrere Tests geteilt würde.

---

## Arrange/Act-Ort: Constructor-Regel wird differenziert

### Typ A (On-the-fly)
- Arrange/Act **dürfen** im Konstruktor der Testklasse stattfinden,
  wenn der Aufbau klein, schnell und vollständig isoliert ist.
- Alternativ darf Arrange/Act direkt in der Testmethode erfolgen,
  wenn das die Verständlichkeit erhöht.

### Typ B (Fixture + Context)
- Der Konstruktor der Testklasse darf **nicht** den vollständigen Bootstrap ausführen.
- Der Konstruktor darf:
  - Fixture entgegennehmen
  - pro Test die Erstellung eines TestContext anstoßen (oder vorbereiten)
- Die Isolation muss pro Test gewährleistet bleiben.

---

## Szenarienprinzip: Stabil, aber präziser gefasst

- Eine Testklasse beschreibt ein fachliches Szenario (Ausgangslage + Zielverhalten).
- Innerhalb einer Testklasse dürfen mehrere Testmethoden existieren, wenn:
  - sie denselben Ausgangszustand teilen **und**
  - sie unterschiedliche fachliche Aspekte desselben Szenarios prüfen

Wenn unterschiedliche Ausgangslagen oder fachlich andere Abläufe nötig sind:
- eigene Testklasse oder eigener Context pro Test (je nach Typ)

---

## Assertions & Inhalt der Tests

- Tests sind **prüfend**, nicht erklärend.
- Keine „Story“-Tests, keine Alt/Neu-Vergleiche in Kommentaren.
- Assertions sollen klar machen:
  - was erwartet wird
  - welche Invariante gilt
  - welche Nebenwirkung relevant ist

Mehrere Assertions sind zulässig, wenn sie fachlich untrennbar zusammengehören.

---

## Produktionscode vs Tests (hart)
- Produktionscode wird **niemals** geändert, nur um Tests zu vereinfachen oder „grün“ zu machen.
- Wenn Testbarkeit schwierig ist:
  - zuerst Analyse + Optionen + Genehmigung (siehe `00_`)
  - dann minimal-invasive Architekturverbesserung **nur wenn fachlich/strukturell begründet**

---

## Cleanup & Reproduzierbarkeit

Für Typ B (Persistenz beteiligt) gilt zwingend:
- pro Test isolierte Pfade/Artefakte
- reproduzierbare Erzeugung
- vollständige Bereinigung der Testartefakte

Tests dürfen nicht abhängig sein von:
- Reihenfolge
- vorherigen Testläufen
- globalen Pfaden oder Zuständen

---

## Kurzform (Entscheidung)
1) Testtyp bestimmen (A oder B)
2) A → on-the-fly (direkt/mocks), optional Constructor-Arrange/Act
3) B → IClassFixture nur Infrastruktur + pro Test TestContext (Scope + Isolation)
4) Collections verboten
5) Kein Shared State
