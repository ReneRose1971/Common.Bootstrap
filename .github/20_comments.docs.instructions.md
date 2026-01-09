# 20_comments.docs.instructions.md
# Copilot – Kommentare & Dokumentation

## Zweck & Geltungsbereich
Diese Datei definiert verbindliche Regeln für:
- Kommentare im Code
- XML-Dokumentation
- begleitende Dokumentationsdateien (z. B. Markdown, README, API-Referenzen)

Sie ergänzt:
- `00_general.behavior.instructions.md`
- `10_code.quality.instructions.md`

---

## Grundsatz: Klarheit vor Menge

- Kommentare und Dokumentation dienen der **Erklärung des gültigen Zustands**,
  nicht der didaktischen Ausführung.
- Mehr Text bedeutet **nicht automatisch** mehr Qualität.
- Überflüssige oder sekundäre Inhalte sind konsequent zu entfernen.

**Kürzen bedeutet:**
- **Themen entfernen**, nicht nur Text verdichten.
- Sekundäre Inhalte sind vollständig zu streichen,
  auch wenn sie fachlich korrekt oder potenziell hilfreich wären.

---

## Keine historischen oder vergleichenden Erzählungen

Kommentare und Dokumentation beschreiben ausschließlich den **aktuellen, gültigen Zustand**.

Unzulässig sind:
- historische Erklärungen („früher“, „damals“, „vorher“)
- Vergleiche („alt/neu“, „vorher/nachher“)
- narrative oder erklärende Geschichten
- große Beispielblöcke ohne zwingenden Contract-Bezug

Dokumentation ist **keine** Schulung und **kein** Blogartikel.

---

## Contract-first-Dokumentation (verbindlich)

Technische Dokumentation beschreibt **primär den technischen Contract**.

Ein Contract umfasst:
- Zweck einer Schnittstelle / Komponente
- Aufruf- oder Lebenszyklusmechanismus
- verbindliche Anforderungen
- zulässige und unzulässige Nutzung

Unzulässig in Contract-Dokumentation:
- Vorteile, Motivationstexte oder Vergleiche
- Best-Practice-Sammlungen
- „Erweiterte Szenarien“
- Patterns, die nicht zwingend Teil des Contracts sind
- mehr als **3 kleine Codebeispiele**

---

## Dokumentation: Scope-Pflicht vor Erstellung oder Überarbeitung

Bei jeder Aufgabe zur **Erstellung, Kürzung oder Überarbeitung** von Dokumentation
muss Copilot **vor der Texterstellung**:

1. den **inhaltlichen Scope explizit benennen**
   (z. B. „technischer Contract“, „öffentliche API“, „Architekturüberblick“)
2. eine **kurze Gliederung** vorschlagen (max. 5–7 Abschnitte)
3. den **Zielumfang** nennen (z. B. „< 100 Zeilen Markdown“)

Ohne explizite Freigabe:
- **keinen Fließtext erzeugen**
- **keine Beispiele ausformulieren**

---

## Kommentare im Code

Kommentare sind nur zulässig, wenn sie:
- das **Warum** erklären
- nicht offensichtliche Randbedingungen beschreiben

Unzulässig:
- Wiederholung des Codes
- Beschreibung veralteter oder hypothetischer Varianten

Wenn Code ohne Kommentar nicht verständlich ist:
- Code verbessern
- erst danach kommentieren

---

## XML-Dokumentation / API-Referenzen

- Öffentliche APIs dürfen dokumentiert werden.
- Dokumentation muss korrekt, aktuell und präzise sein.
- API-Referenzen beschreiben **nur den gültigen Zustand**.

Unzulässig:
- Platzhalter
- automatisch generierte Leerformeln
- historische oder alternative Nutzung

---

## Struktur von README- und Dokumentationsdateien

### Solution-README
- bewusst **kurz**
- Projektübersicht + Links auf Projekt-READMEs
- keine Konzepte, keine API-Details

### Projekt-README
- konzeptionell gegliedert
- darf auf Unterseiten verlinken
- Unterseiten liegen in `Projekt/Docs`

### API-Referenzen
- liegen in `Projekt/Docs`
- dürfen auf Unterseiten verlinken
- beschreiben ausschließlich den Contract

---

## Doc Review (expliziter Auftrag)

Bei einem **Doc Review** hat Copilot:
- alle relevanten Markdown-Dateien zu prüfen
- Aktualität, Konsistenz und Verlinkungen zu bewerten
- Inhalte außerhalb des vereinbarten Scopes zu identifizieren

Ein Doc Review:
- nimmt **keine automatischen Änderungen** vor
- liefert eine strukturierte Rückmeldung

---

## Erzeugung von Markdown-Dateien (restriktiv)

Copilot darf **keine neuen Markdown-Dateien** erzeugen ohne explizite Zustimmung.

Vor Refactoring- oder größeren Umbauaufgaben muss Copilot fragen:
- ob eine Dokumentation gewünscht ist
- ob eine Aufgabenplanung dokumentiert werden soll
- ob ein Fortschritt festgehalten werden soll

Ziel ist eine **bewusst schlanke Dokumentationslandschaft**.

---

## Dokumentation nach Refactorings

Nach Refactoring-Aufgaben muss Copilot fragen,
ob ein Doc Review durchgeführt werden soll.

---

## Kurzform

- ✅ Contract beschreiben
- ✅ Scope vorab festlegen
- ❌ Keine Tutorials
- ❌ Keine Best Practices
- ❌ Keine erweiterten Szenarien
- ❌ Keine ungefragten Markdown-Dateien
