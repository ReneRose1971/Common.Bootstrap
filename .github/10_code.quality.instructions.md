# 10_code.quality.instructions.md
# Copilot – Codequalität & Architekturdisziplin

## Zweck & Geltungsbereich
Diese Datei definiert verbindliche Regeln für **Produktionscode**:
Struktur, Qualität, Erweiterbarkeit und Refactoring.

Sie ergänzt die allgemeinen Verhaltensregeln aus
`00_general.behavior.instructions.md` und darf diese **nicht umgehen**.

---

## Grundsatz: Qualität vor Geschwindigkeit

- Code muss **verständlich, wartbar und strukturell sauber** sein.
- „Schnelle Lösungen“, Workarounds oder implizite Vereinfachungen sind unzulässig.
- Bestehende Architekturentscheidungen sind zu respektieren.

---

## Architektur-Disziplin (Pflicht)

- Abhängigkeiten müssen der bestehenden Schichtenarchitektur folgen.
- Untere Schichten dürfen **keine Abhängigkeiten nach oben** haben
  (z. B. Core/Engine → niemals UI/WPF).
- Verantwortlichkeiten dürfen **nicht vermischt** werden
  (z. B. keine Fachlogik im ViewModel, keine UI-Logik im Core).

Wenn Architekturregeln unklar sind:
- **Kein Code**
- Rückfrage oder Analyse anstoßen

---

## Wiederverwendung vor Neuanlage (zwingend)

- Vor der Einführung neuer Typen ist immer zu prüfen:
  - existieren bereits passende Klassen, Extensions, Services oder Basistypen?
  - kann bestehender Code erweitert oder konfiguriert werden?

- Neue Typen sind **nur zulässig**, wenn:
  - keine geeignete bestehende Lösung existiert **oder**
  - eine bestehende Lösung nachweislich fachlich oder architektonisch ungeeignet ist.

Diese Begründung muss **explizit** erfolgen (siehe Änderungsliste).

---

## Extensions, Helper & Utilities

- Neue Extensions oder Helper sind **nicht die Default-Lösung**.
- Vor einer Neuanlage ist zu prüfen, ob bestehende Lösungen ausreichend sind.
- Der fachliche oder strukturelle Mehrwert einer neuen Extension
  ist klar zu benennen und zu begründen.

---

## Refactoring-Regeln

- Refactorings erfolgen **minimal-invasiv**.
- Ziel ist Verbesserung von:
  - Lesbarkeit
  - Struktur
  - Trennschärfe der Verantwortlichkeiten

Unzulässig:
- „Aufräumen“ ohne fachlichen Anlass
- gleichzeitige Umstrukturierung mehrerer Themen
- Stiländerungen ohne funktionalen Nutzen

---

## Produktionscode vs. Tests

- Produktionscode darf **niemals** angepasst werden,
  um Tests „einfacher“ oder „grün“ zu machen.
- Testbarkeit wird durch saubere Architektur erreicht,
  nicht durch Sonderlogik im Produktivcode.

---

## Fehlerbehandlung & Robustheit

- Fehlerfälle dürfen nicht verschwiegen oder ignoriert werden.
- Exceptions, Rückgabewerte und Validierungen müssen konsistent sein.
- Stille Fehler („catch & ignore“) sind unzulässig.

---

## Kurzform (Codequalität)

- ✅ Architektur respektieren
- ✅ Wiederverwendung vor Neuanlage
- ❌ Kein kosmetisches Refactoring
- ❌ Kein Produktionscode für Tests verbiegen
