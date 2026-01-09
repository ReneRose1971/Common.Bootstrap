# 20_comments.docs.instructions.md
# Copilot – Kommentare & Dokumentation

## Zweck & Geltungsbereich
Diese Datei definiert verbindliche Regeln für:
- Kommentare im Code
- XML-Dokumentation
- begleitende Dokumentationsdateien (z. B. Markdown)

Sie ergänzt die allgemeinen Verhaltensregeln aus
`00_general.behavior.instructions.md` und die Codequalitätsregeln aus
`10_code.quality.instructions.md`.

---

## Grundsatz: Klarheit vor Menge

- Kommentare und Dokumentation dienen der **Erklärung von Entscheidungen**,
  nicht der Wiederholung von Code.
- Mehr Text bedeutet **nicht automatisch** bessere Verständlichkeit.
- Überflüssige, redundante oder ausschweifende Texte sind zu vermeiden.

---

## Keine historischen oder vergleichenden Erzählungen

Kommentare und Dokumentation müssen den **aktuellen, gültigen Zustand** beschreiben.

Unzulässig sind insbesondere:
- historische Erklärungen wie  
  „früher wurde das so gemacht …“
- vergleichende Darstellungen wie  
  „vorher / nachher“, „alt / neu“, „damals / jetzt“
- große Code-Snippets, die ausschließlich zeigen,
  wie etwas **nicht mehr** gemacht wird
- narrative oder erklärende „Geschichten“ zur Code-Evolution

Kommentare und Dokumentation sind **keine Migrationsberichte**
und **keine Entscheidungsprotokolle**.

Zulässig sind ausschließlich:
- Beschreibung dessen, **was gilt**
- Erklärung dessen, **wie es aktuell umgesetzt wird**
- Begründung von **bestehenden** Designentscheidungen,
  sofern sie für das Verständnis notwendig sind

---

## Kommentare im Code

### Wann Kommentare sinnvoll sind
Kommentare sind zulässig und sinnvoll, wenn sie:
- das **Warum** erklären (Designentscheidung, fachlicher Hintergrund),
- nicht offensichtliche Randbedingungen oder Annahmen erläutern,
- vor bekannten Fallstricken oder Nebenwirkungen warnen.

Kommentare dürfen **nicht** erklären,
wie eine frühere Lösung aussah oder warum sie ersetzt wurde.

### Wann Kommentare unzulässig sind
Kommentare sind unzulässig, wenn sie:
- lediglich beschreiben, **was** der Code offensichtlich tut,
- Codezeilen paraphrasieren,
- veraltete, spekulative oder hypothetische Informationen enthalten.

Wenn der Code ohne Kommentar nicht verständlich ist:
- zuerst Code verbessern,
- erst dann kommentieren.

---

## XML-Dokumentation / API-Doku

- Öffentliche APIs (public / protected) **dürfen** dokumentiert werden,
  private Implementierungsdetails **nicht zwingend**.
- Dokumentation muss:
  - korrekt,
  - aktuell,
  - präzise
  sein.

Unzulässig:
- Platzhalter-Texte
- automatisch generierte Leerformeln
- Dokumentation, die dem Code widerspricht

---

## Dokumentationsdateien (Markdown, README, API-Referenzen)

- Diese Regeln gelten explizit für:
  - Solution-README-Dateien
  - Projekt-README-Dateien
  - API-Referenzdokumente
  - weitere konzeptionelle Markdown-Dokumentationen

- Dokumentation ist **normativ**, nicht dekorativ.
- Beispiele dienen der Erläuterung,
  **ersetzen keine formale Beschreibung**.

---

## Struktur von README- und Dokumentationsdateien

### Solution-README
- Die Solution-README ist **bewusst kurz** zu halten.
- Sie beschreibt:
  - die enthaltenen Projekte in knapper Form
  - und verlinkt auf die jeweiligen Projekt-README-Dateien.
- Sie enthält **keine ausführlichen Konzepte** und keine API-Details.

### Projekt-README
- Jede Projekt-README darf **ausführlich und konzeptionell** sein.
- Sie darf auf Unterseiten verlinken, wenn der Umfang es erfordert.
- Unterseiten müssen im Ordner `Projekt/Docs` liegen.

### API-Referenzen
- API-Referenzen liegen ebenfalls im Ordner `Projekt/Docs`.
- Je nach Umfang dürfen sie auf weitere Unterseiten verlinken.
- API-Referenzen beschreiben den **aktuellen gültigen Zustand**,
  nicht historische oder alternative Nutzung.

---

## Doc Review (expliziter Auftrag)

Wenn der Auftrag **„Doc Review“** erteilt wird, hat Copilot:

- **alle relevanten Markdown-Dokumentationen** zu prüfen,
  nicht nur einzelne Dateien
- zu bewerten:
  - ob Inhalte noch **aktuell** sind
  - ob Beschreibungen zum aktuellen Code- und Architekturstand passen
  - ob **alle Verlinkungen funktionieren** und sinnvoll sind

Ein Doc Review umfasst:
- keine inhaltliche Neugestaltung
- keine Umformulierungen
- keine Erweiterungen über den Ist-Zustand hinaus

Ergebnis eines Doc Reviews ist:
- eine **strukturierte Rückmeldung**
- mit konkreten Hinweisen, was veraltet oder inkonsistent ist
- **keine automatischen Änderungen**, sofern nicht explizit beauftragt

---

## Erzeugung von Markdown-Dateien (restriktiv)

Copilot darf **nicht eigenständig** neue `.md`-Dateien erzeugen
(z. B. Testpläne, Refactoring-Pläne, Aufgabenlisten).

Vor Refactoring- oder größeren Umbauaufgaben muss Copilot:
- **explizit fragen**, ob:
  - eine schriftliche Dokumentation der Aufgabe gewünscht ist
  - eine Aufgabenplanung als Datei erstellt werden soll
  - ein Aufgabenfortschritt dokumentiert werden soll

Ohne explizite Zustimmung:
- keine neuen Markdown-Dateien
- keine persistierten Pläne oder Berichte

Ziel ist:
- eine **bewusst schlanke Dokumentationslandschaft**
- kein „Ertrinken“ in automatisch erzeugten `.md`-Dateien

---

## Dokumentation nach Refactorings

Nach Abschluss einer Refactoring-Aufgabe muss Copilot **nachfragen**,
ob ein Doc Review durchgeführt werden soll.

Ein Doc Review erfolgt **nicht automatisch**, sondern nur nach Bestätigung.

---

## Änderungsdisziplin bei Dokumentation

- Bestehende Dokumentationsdateien dürfen **nicht**:
  - gekürzt,
  - umformuliert,
  - neu strukturiert
  werden, ohne explizite Anweisung.

- Ergänzungen müssen:
  - klar als Ergänzung erkennbar sein,
  - den bestehenden Stil respektieren,
  - keinen bestehenden Inhalt implizit verändern.

---

## Kurzform (Kommentare & Doku)

- ✅ Ist-Zustand beschreiben
- ✅ Wie es gemacht wird erklären
- ✅ Doc Review nur auf expliziten Auftrag
- ❌ Keine historischen Erzählungen
- ❌ Keine Alt/Neu- oder Vorher/Nachher-Vergleiche
- ❌ Keine ungefragten Markdown-Dateien
