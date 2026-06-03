# ADR 0003 — Web + app approach

**Status:** Open (decide at the start of Phase 2)

## Context

Phase 2 wants the search-and-view-results functionality on both web and mobile
(iOS + Android). The app is **read-only**: pick a year, pick a race, read a results
table, see the next race. It needs no camera, GPS, or other device hardware. The one
future feature that points toward "real app" territory is push notifications before a
race starts (F15).

## Decision

**Not yet decided.** Leaning toward: build a responsive web app first, make it an
installable PWA, and only go native if/when push notifications justify it.

## Options considered

- **Responsive web app → PWA.** One codebase, runs everywhere, installable to a phone
  home screen. Because the app is read-only, this likely covers the entire "app"
  requirement with the least effort. *Recommended starting point.*
- **React Native + Expo.** Single JS/TS codebase for iOS + Android, huge ecosystem,
  strong job-market relevance, Expo makes setup and builds easy. The right call if you
  want true app-store presence and native push. *Recommended if going native.*
- **.NET MAUI (or Blazor Hybrid).** Keeps you in C#, lets you share DTOs/models with
  the API. Pragmatic given your .NET strength; smaller ecosystem and community than
  React Native. *Strong alternative if going native and you want one language.*
- **Flutter.** Excellent UI and single codebase, but means learning Dart for no
  particular gain here.

## Trade-off summary

| Option | Codebases | New language? | App-store + push | Effort |
|--------|-----------|---------------|------------------|--------|
| PWA | 1 (shared with web) | no | limited (esp. iOS) | lowest |
| React Native + Expo | 1 | maybe (JS/TS) | yes | medium |
| .NET MAUI | 1 | no | yes | medium |
| Flutter | 1 | yes (Dart) | yes | medium-high |

## Recommendation

Start with the PWA. Revisit and pick React Native + Expo or .NET MAUI only when you
actually want push notifications for race start (F15) or app-store distribution —
that's the point where a native client earns its keep.
