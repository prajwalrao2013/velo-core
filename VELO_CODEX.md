# VELO TERMINAL: Master Architecture & Agentic Guidelines

## 1. System Identity
Velo is a high-performance, ultra-lightweight trading terminal and journaling platform. It prioritizes mental clarity, educational onboarding through contextual UI, and absolute execution speed. 

## 2. Agentic Update Loop (MANDATORY INSTRUCTION FOR AI)
Before modifying any code in this repository, the AI Agent MUST:
1. Read this `VELO_CODEX.md` file to understand the current architecture.
2. Ensure the proposed changes do not violate SEBI compliance rules (Section 4).
3. Execute the code changes.
4. Update this `VELO_CODEX.md` file's "Changelog & Current State" section to reflect the new architecture or component relationships.

## 3. Technology Stack
* **Frontend/Desktop UI:** C# .NET 8, Avalonia UI (Native desktop, XAML-based).
* **Architecture:** Decoupled MVVM Event-driven architecture.
* **Simulation Engine:** Local C# background tasks (MarketSimulator, MacroEvent, PsychologyEngine).
* **Deployment:** Self-contained, single-file executable (Linux AppImage target).

## 4. SEBI Compliance Guardrails
* **Strictly a Tool:** Velo acts exclusively as a passthrough for the user's chosen broker API (e.g., Kite Connect).
* **No Predictive Models:** The system must never generate signals stating "Buy" or "Sell" based on prediction.
* **Risk Analysis Only:** Algorithmic functions are strictly limited to calculating risk (e.g., Margin requirements, R:R ratios, Greeks calculation).

## 5. UI/UX Philosophy
* **Render Light:** Only render data visible in the viewport.
* **Contextual Education:** Complex data points (like IV Percentile) must have subtle, hoverable tooltips explaining their definition.
* **Gamified Discipline:** Visual feedback loop rewards risk management and journaling, not P&L fluctuations.

## 6. Changelog & Current State
* **Current State:** Bootstrapped Avalonia UI Native Desktop architecture. Maintained strict UI tracking via `IMarketDataClient` abstract interface. Implemented local MVVM decoupled structure replacing traditional events with `WeakReferenceMessenger` (CommunityToolkit.Mvvm) to strictly prevent high-frequency memory leaks. Replaced Canvas with GPU-accelerated SkiaSharp via `LiveCharts2`. Expanded structure with TabControl integration separating Execution Engine from local JSON Datastore engine. F1/F2 global KeyBindings override UI clicks natively. Gamified performance loops instantiated across `AnalyticsEngine.cs` scoring `DisciplineScore` gamification parameters safely.
* **2026-04-10:** Pivoted architecture entirely from Blazor WebAssembly to Avalonia Native Desktop. Bootstrapped native layout (`MainWindow.axaml`). Implemented realistic mock backend using `MarketSimulatorService` (Geometric Brownian Motion), `MacroEventService` (random news triggers causing volatility spikes), and a `PsychologyEngine` to monitor tilt (consecutive loss threshold triggering Pomodoro breakdown). Included zero-dependency `JournalService.cs` saving execution payloads instantly using standard System.Text.Json into AppData. Built out native Analytics Dashboard Tab resolving KPI models internally and driving RectangularSection annotations tracking X-axis news volatility.
