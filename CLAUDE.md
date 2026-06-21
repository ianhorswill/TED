# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

TED ("Typed, Embedded Datalog") is a C# library implementing a strongly-typed, bottom-up Datalog variant intended for game AI / simulation use. It is *embedded*: rules and predicates are constructed in C# as expression DSLs (operator overloading, generic indexers, fluent `.If(...)`), not parsed from a separate source file. Recursive rules are **not** supported.

## Build & test

The solution targets multiple frameworks: `TED` is `netstandard2.1` (LangVersion 9, nullable enabled), `Tests` is `net10.0` (MSTest), and `TablePredicateViewer` is `net10.0-windows7.0` WinForms (Windows-only).

```sh
dotnet build TED.sln                       # build everything
dotnet build TED/TED.csproj                # build the library only (cross-platform)
dotnet test  Tests/Tests.csproj            # run the full MSTest suite
dotnet test  Tests/Tests.csproj --filter "FullyQualifiedName~CompilerTests.Exhaustive"   # single test
dotnet test  Tests/Tests.csproj --filter "ClassName=Tests.IntensionalPredicateTests"     # one class
```

Both Debug and Release define the `PROFILER` constant — `Rule` and related types have `#if PROFILER` blocks that record per-rule execution time. Don't strip `#if PROFILER` guards without checking who reads them.

`TablePredicateViewer` is a Windows-only WinForms debug UI for inspecting tables; it won't build on Linux/macOS, and CI-like environments should skip it (build just `TED.csproj` or `Tests.csproj` instead of the whole solution).

## Architecture

Read these files to orient before non-trivial work: `TED/Predicate.cs`, `TED/TablePredicate.cs`, `TED/Language.cs`, `TED/Program.cs`, `TED/Simulation.cs`, `TED/Interpreter/Rule.cs`, `TED/Compiler/Compiler.cs`.

**Predicates form a layered type hierarchy.** `Predicate` (abstract) → `TablePredicate` (untyped) → `TablePredicate<T1,...,Tn>` (one generic per arity, up to 8). A `TablePredicate` owns a `Tables.Table` that physically stores all rows. There is also `PrimitivePredicate` (in `Primitives/`) for built-ins like `Not`, `And`, `Or`, `Eval`, comparison/arithmetic operators — these implement the logical operation directly rather than holding a table. `Definition` is a macro-like predicate that is in-lined at call sites.

**Predicates are either *extensional* (rows loaded via `AddRow`/CSV) or *intensional* (rows computed from rules defined with `.If(...)`).** A `TablePredicate<...>` indexed with variables produces a `Goal`; goals compose into rule bodies. `Language.cs` is the user-facing entry point — it's a `static class` of primitives and combinators, normally imported via `using static TED.Language;` (see any test file).

**Evaluation is bottom-up and demand-driven.** `Program` is a namespace/container; `Simulation : Program` adds tick-based update semantics. The flow:
1. `BeginPredicates()` / `EndPredicates()` brackets predicate construction (predicates auto-register with the top-of-stack `Program`).
2. `EndPredicates` runs `FindDependents`, `CheckForCycles` (recursion is rejected here), and on `Simulation` also `FindInitializationOnlyPredicates`, `FindDynamicPredicates`, `CheckTableDefinitions`, `InitializeTables`.
3. `Simulation.Update()` is one tick: `RecomputeAll()` → optional `Problems.ForceRecompute()` → `UpdateDynamicBaseTables()` (column `Set()` updates) → `AppendAllInputs()` (accumulators).

**Preprocessing** (`TED/Preprocessing/`) rewrites raw rules before execution: `GoalAnalyzer` does mode analysis (each variable's bound/unbound state is statically determined per call site), `Preprocessor` constant-folds and normalizes, `Substitution` powers definition expansion. Modes drive which `Call` subclass gets generated for each goal — see `Interpreter/TableCall*.cs` (the variants for key vs. general index, single-row, row-set, exhaustive scan).

**Tables and indexes** (`TED/Tables/`) are hand-tuned to avoid GC pressure and runtime type checks: a `Table<TRow>` is essentially a specialized `List<TRow>` with optional `KeyIndex` (unique) or `GeneralIndex` (non-unique, hash bucket chain) per column or column-tuple. Index choice is driven by `IndexMode` and is what makes mode-specialized calls fast.

**The compiler** (`TED/Compiler/Compiler.cs`) is optional: it ahead-of-time generates C# source for a `Program`'s rules that matches the interpreter's behavior. `CompilerTests` exercise this by running the same predicate interpreted, calling `Compiler.GenerateSource()` + `Compiler.Link()`, then calling `ForceRecompute()` and asserting the row sets match. Generated files land in `Tests/CompilerOutput/` (one `*__Compiled.cs` per test) and are checked in — they're useful as worked examples of what the compiler emits.

## Conventions worth knowing

- Variables are constructed via `(Var<int>)"x"` (the `string→Var<T>` cast). This idiom is everywhere; preserve it.
- Tests universally `using static TED.Language;` so bare `Predicate(...)`, `Not`, `And`, `Eval(...)` refer to `Language` members.
- There is a project-local `TED.InvalidProgramException` that shadows the BCL type — tests import it explicitly (`using InvalidProgramException = TED.InvalidProgramException;`). Keep the alias when adding new tests that throw it.
- Generic arities top out at 8 — `TablePredicate.Create(name, columns)` throws for >8 columns, and several `switch` ladders mirror this. Don't add a 9-arg overload without auditing those ladders.
