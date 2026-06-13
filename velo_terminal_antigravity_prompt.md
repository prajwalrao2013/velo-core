# Velo Terminal — Antigravity UI Prompt

## CONTEXT

You are building **Velo Terminal**, a premium .NET 8 WPF desktop trading simulator. This is a pure UI build — no real data connections yet. Use **hardcoded mock data** everywhere. The goal is a pixel-perfect, fully rendered shell that looks and feels like a Bloomberg Terminal built for 2030.

**Tech Stack:**
- .NET 8, WPF, XAML
- `LiveCharts2` (NuGet: `LiveChartsCore.SkiaSharpView.WPF`) for all charts
- `SkiaSharp` for custom canvas elements (discipline arc, sparklines)
- `MaterialDesignInXamlToolkit` for base component styles
- Font: `JetBrains Mono` (numbers/prices), `Inter` (labels/text) — load via embedded font resources
- MVVM pattern — all ViewModels implement `INotifyPropertyChanged`, bound via `DataContext`

---

## GLOBAL DESIGN TOKENS

Define all of these as static resources in `Styles/Tokens.xaml` and merge into `App.xaml`:

```
Background:         #050505
Surface:            #0D0D0F
Surface-2:          #111113
Panel Border:       #27272A  (1px solid)
Text Primary:       #E4E4E7
Text Muted:         #71717A
Text Faint:         #3F3F46
Price UP:           #22C55E  + DropShadowEffect (Color=#22C55E, BlurRadius=8, Opacity=0.5)
Price DOWN:         #EF4444  + DropShadowEffect (Color=#EF4444, BlurRadius=8, Opacity=0.5)
Accent Purple:      #A855F7
Accent Amber:       #F59E0B
MA20 color:         #F59E0B
MA50 color:         #3B82F6
VWAP color:         #8B5CF6
Active Pill Border: #22C55E  + DropShadowEffect (Color=#22C55E, BlurRadius=12, Opacity=0.4)
Corner Radius:      8px (panels), 4px (rows), 9999px (pills)
Panel Shadow:       DropShadowEffect Color=Black, BlurRadius=24, Opacity=0.5, Direction=270
Transition speed:   150ms for state changes, 300ms for panel open/close
```

Apply `FontFamily="JetBrains Mono"` to all `TextBlock` elements showing prices, numbers, timers.
Apply `FontFamily="Inter"` to all labels, headings, nav items.

---

## WINDOW STRUCTURE — MainWindow.xaml

Root: `DockPanel` filling the entire window. Window chrome: custom title bar, no default Windows chrome (`WindowStyle="None"`, `AllowsTransparency="True"`, `Background="Transparent"`). Custom title bar shows draggable region.

```
DockPanel
├── TopBar          (DockPanel.Dock="Top",    Height=48)
├── NewsTicker      (DockPanel.Dock="Bottom", Height=32)
├── PositionsRail   (DockPanel.Dock="Bottom", Height=48, collapses to 4px dot when empty)
└── MainGrid        (fills remaining space)
    ├── Column 0:   LeftDock     (Width=240)
    ├── Column 1:   ChartZone    (Width=*)
    └── Column 2:   CommandPanel (Width=300)
```

All panels: `Background="#0D0D0F"`, `BorderBrush="#27272A"`, `BorderThickness="1"`, `CornerRadius="8"`, apply panel shadow.

---

## ZONE 1 — TOP BAR (Height=48)

Layout: `DockPanel` horizontal.

**Left:** Custom SVG-style `Path` logo — geometric "V" mark in `#22C55E`, next to text "VELO" in `Inter Bold 18px #E4E4E7`. Immediately right: `Border` pill (`Background="#1C1C3A"`, `BorderBrush="#6366F1"`, `CornerRadius="4"`) containing text "PRO" in `#6366F1` `11px Inter SemiBold`.

**Center:** `StackPanel Orientation="Horizontal"` with 5 `RadioButton` items styled as flat nav tabs:
- `TERMINAL`, `ARENA`, `PULSE`, `JOURNAL`, `ACADEMY`
- Default style: `Foreground="#71717A"`, `FontFamily="Inter"`, `FontSize="13"`, no border, transparent background
- Checked style: `Foreground="#E4E4E7"`, bottom border `2px solid #22C55E`, background transparent
- Spacing: `Margin="0,0,32,0"` between items
- Only `TERMINAL` is checked by default

**Right:** 
- Simulated year badge: `Border` (`Background="#1A1A1A"`, `BorderBrush="#27272A"`, `CornerRadius="4"`, `Padding="8,2"`) with text `"SIM · 2003"` in `#71717A Inter 11px`
- Discipline Arc: `SKElement` (SkiaSharp canvas, 36x36px). Draw a circular arc track (`#27272A`, stroke 3px) and a filled arc (`#22C55E`, stroke 3px) representing 78% fill. Add glow. On hover, show tooltip `"Discipline: 78/100 · 🔥 7-day streak"`
- Clock: `TextBlock` showing `"15:32:04"` in `JetBrains Mono 12px #71717A`, updates every second via `DispatcherTimer`

---

## ZONE 2 — LEFT DOCK (Width=240)

Three sections inside a `ScrollViewer` with custom thin scrollbar (4px width, thumb `#27272A`, no arrows):

### Section A — MarketWatch

Header: `"MARKETWATCH"` label — `Inter 10px #3F3F46 LetterSpacing=2 Uppercase`.

Search bar: `TextBox` with magnifier icon (`Path` data for search icon, `#3F3F46`), `Background="#111113"`, `BorderBrush="#27272A"`, `CornerRadius="6"`, placeholder `"Search symbol..."` in `#3F3F46 JetBrains Mono 11px`.

Instrument list: `ListView` with custom `ItemTemplate`. Each row (Height=44):
```
[Symbol: "NIFTY50"  Inter Bold 13px #E4E4E7]
[Subtext: "NSE · INDEX"  Inter 10px #3F3F46]

[Bid: "22388.12"  JetBrains Mono 12px #EF4444]
[Ask: "22389.12"  JetBrains Mono 12px #22C55E]

[SpreadBar: Canvas 48x6px — horizontal bar, left half red, right half green,
 proportional to bid/ask spread. Centered between bid and ask.]

[Sparkline: Canvas 48x18px — SkiaSharp polyline, color matches last tick direction]
```

Row hover: `Background="#111113"`, reveal 52W range bar as a thin `ProgressBar`-style element sliding in from below the row (animate Height 0→14px on hover).

Mock instruments: NIFTY50, BANKNIFTY, FINNIFTY, SENSEX, MIDCAP150.
On row click: update `ChartViewModel.SelectedSymbol` (binding-ready, mock only).

### Section B — Smart Money Tracker

Header: `"SMART MONEY"` label same style as above. 

A `Border` panel (`Background="#0A0A0A"`, `BorderBrush="#27272A"`, `CornerRadius="6"`, `Padding="10"`) containing:

```
Row 1: [Dot #22C55E 6px] "FII NET"    ["+₹2,340 Cr"  JetBrains Mono 12px #22C55E]
Row 2: [Dot #EF4444 6px] "DII NET"    ["-₹890 Cr"    JetBrains Mono 12px #EF4444]
Row 3: Separator #27272A
Row 4: "MAX PAIN"   ["22,400"  JetBrains Mono 12px #F59E0B]  ["↑ 12pts"  10px #71717A]
Row 5: "PCR"        ["0.82"    JetBrains Mono 12px #EF4444]  ["BEARISH"  pill]
Row 6: "OI CHANGE"  ["+1.2L"   JetBrains Mono 12px #22C55E]
```

"BEARISH" pill: `Border` (`Background="rgba(239,68,68,0.1)"`, `BorderBrush="#EF4444"`, `CornerRadius="9999"`, `Padding="4,1"`) text `"BEARISH"` in `#EF4444 Inter 9px`.

Subtle `"LIVE"` badge top-right: blinking green dot + `"LIVE"` text `#22C55E 9px`.

### Section C — Today's Macro Calendar

Header: `"TODAY'S EVENTS"` label.

`ItemsControl` list. Each row (Height=36):
```
[Time: "09:00"  JetBrains Mono 11px #71717A]
[EventName: "RBI Policy"  Inter 12px #E4E4E7]
[ImpactDot: Circle 8px — #EF4444 for HIGH, #F59E0B for MED, #22C55E for LOW]
[Countdown: "in 2h 14m"  JetBrains Mono 10px #71717A]
```

Mock events: 
- `09:00  RBI Policy  HIGH  in 2h 14m`
- `11:30  IIP Data    MED   in 4h 44m`
- `14:00  US CPI      HIGH  in 7h 14m`

---

## ZONE 3 — CHART ZONE (fills *)

Split vertically into 3 rows: `* | 80 | 180`

### Row 1 — Main Chart

**Toolbar** (Height=40, `DockPanel.Dock="Top"`):

Left side: `ComboBox` styled as a flat selector showing `"NIFTY50"` with dropdown arrow, `Background="#111113" BorderBrush="#27272A" Foreground="#E4E4E7" FontFamily="Inter" FontWeight="SemiBold" FontSize="14"`.

Center: Timeframe pill group — `StackPanel Orientation="Horizontal"` of `RadioButton` items: `1m 5m 15m 30m 1h 1D 1W`.
- Default: `Background="#111113"`, `BorderBrush="#27272A"`, `Foreground="#71717A"`, `CornerRadius="4"`, `Padding="8,4"`, `Margin="2,0"`
- Checked: `BorderBrush="#22C55E"` + glow `DropShadowEffect Color=#22C55E BlurRadius=8 Opacity=0.4`, `Foreground="#22C55E"`
- `30s` checked by default

Right side:
- `Button` "Indicators ▾" — flat style, `Foreground="#71717A"`, hover `#E4E4E7`
- `ToggleButton` "Ghost Mode ◈" — when ON: `Foreground="#A855F7"` + glow, when OFF: `Foreground="#3F3F46"`. Tooltip: `"Overlay 10Y seasonal average path"`
- `Button` fullscreen icon

**Chart:** `CartesianChart` (LiveCharts2) filling remaining space.
- `Background="#050505"`
- `DrawMarginFrame`: no border
- Grid lines: `#111113` horizontal only, no vertical
- `CandlestickSeries` bound to `ChartViewModel.Candles` — generate 120 mock candles starting at 22,000, random walk ±0.3% per candle, bullish candles `Fill="#22C55E" Stroke="#22C55E"`, bearish `Fill="#EF4444" Stroke="#EF4444"`
- `LineSeries` MA20: `Stroke="#F59E0B"`, `StrokeThickness=1.5`, no fill, no geometry
- `LineSeries` MA50: `Stroke="#3B82F6"`, `StrokeThickness=1.5`, no fill, no geometry
- `LineSeries` VWAP: `Stroke="#8B5CF6"`, `StrokeThickness=1`, `StrokeDashArray="4,4"`, no fill
- `LineSeries` Seasonality Ghost (visible only when Ghost Mode ON): `Stroke="#A855F7"`, `Opacity=0.4`, `StrokeThickness=2`, `StrokeDashArray="6,3"`, no fill, no geometry. Mock data: smooth sinusoidal curve scaled to current price range.
- X-axis: `FontFamily="JetBrains Mono"`, `FontSize=10`, `LabelsPaint=#52525B`, no separator line
- Y-axis: right-aligned, same font/color
- Crosshair: `TooltipFindingStrategy.CompareOnlyX`, custom tooltip showing O/H/L/C/V in a `Border` panel `Background="#111113" BorderBrush="#27272A"`

### Row 2 — Volume Sub-chart (Height=80)

`CartesianChart` sharing X-axis range with main chart (synchronized scroll/zoom).
- `ColumnSeries` bound to `ChartViewModel.VolumeData` — each bar color matches corresponding candle direction
- Y-axis hidden
- X-axis hidden (shared visual only)
- Background `#050505`
- Thin `Separator` `#27272A` between this and main chart

### Row 3 — Intelligence Strip (Height=180, collapsible)

Collapse chevron `▲▼` button top-right. When collapsed: Height animates to 32px showing only the tab headers.

Three `TabItem` tabs, custom styled (no default TabControl chrome):

**Tab 1 — SEASONALITY:**
A `Grid` of 12 columns × 11 rows (years 2015–2025 + header row).
- Header row: `Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec` — `Inter 9px #52525B`
- Year labels left: `2015...2025` — `JetBrains Mono 9px #52525B`
- Each data cell: `Border` with `CornerRadius="2"`, `Margin="1"`.
  - Background: interpolate from `#7F1D1D` (−4%) → `#111113` (0%) → `#14532D` (+4%) based on mock return value
  - `TextBlock` inside: return value like `"2.4%"` or `"-1.1%"`, `JetBrains Mono 8px`, color white if abs(val)>1.5 else `#71717A`
  - Current month column: add `BorderBrush="#22C55E" BorderThickness="1"` with subtle pulse animation (`OpacityAnimation` 0.4→1.0→0.4, Duration=2s, repeat forever)
- Mock data: generate plausible random returns — NIFTY50 historically positive in Nov/Dec, weak in Jun/Sep

**Tab 2 — FLOW MAP:**
A `WrapPanel` of 6 sector `Border` tiles sized proportionally by market cap:
- BANK (largest, ~220×80), IT (180×80), FMCG (160×80), ENERGY (140×80), PHARMA (120×80), REALTY (100×80)
- Each tile background: interpolate from `rgba(239,68,68,0.3)` to `rgba(34,197,94,0.3)` based on mock daily return
- Inside: Sector name `Inter Bold 13px #E4E4E7`, return `"+1.4%"` or `"-0.8%"` in `JetBrains Mono 14px` colored green/red
- Hover: slight scale transform `1.02`, border glow

**Tab 3 — CORRELATIONS:**
A 5×5 `Grid` matrix. Headers: `NIFTY BANK DXY GOLD CRUDE`.
- Each cell: `Border CornerRadius="3" Margin="2"`. Background interpolated: `#7F1D1D` (−1.0) → `#111113` (0.0) → `#14532D` (+1.0)
- Text: correlation value `"0.87"` or `"-0.32"`, `JetBrains Mono 10px`
- Diagonal cells: background `#1C1C1C`, text `"1.00"` in `#3F3F46`
- Mock values: NIFTY-BANK=0.87, NIFTY-GOLD=-0.12, NIFTY-CRUDE=0.34, NIFTY-DXY=-0.45, BANK-GOLD=-0.18, BANK-CRUDE=0.28, DXY-GOLD=-0.67, DXY-CRUDE=-0.31, GOLD-CRUDE=0.22

---

## ZONE 4 — RIGHT COMMAND PANEL (Width=300)

`StackPanel` with three `Expander` sections, all expanded by default.

### Expander 1 — EXECUTION

Header style: `"EXECUTION"` in `Inter SemiBold 11px #71717A LetterSpacing=2 Uppercase`.

**Symbol header row:**
- `"NIFTY50"` in `Inter Bold 16px #E4E4E7`
- LTP `"22,388.12"` in `JetBrains Mono Bold 20px #22C55E` with green glow
- Change `"+0.42%"` in `Inter 11px #22C55E`

**Order type pills:** `StackPanel Orientation="Horizontal"` of 4 `RadioButton` pills: `MKT LMT SL-M SL-L`. MKT checked by default. Checked style: `Background="rgba(34,197,94,0.15)" BorderBrush="#22C55E" Foreground="#22C55E"`.

**Quantity row:**
- Label `"QTY (LOTS)"` in `Inter 10px #71717A`
- `TextBox` value `"1"` + `-` `+` `Button` spinners
- Helper text: `"1 lot = 50 units"` in `Inter 9px #3F3F46`

**SL / Target row (side by side):**
- Left: `TextBox` labelled `"STOP LOSS"` with `BorderBrush="#EF4444"` on focus
- Right: `TextBox` labelled `"TARGET"` with `BorderBrush="#22C55E"` on focus
- Both: `JetBrains Mono 12px`

**Risk:Reward visual bar (Height=8, full width):**
- `Grid` with 2 columns. Left column width proportional to risk, right proportional to reward.
- Left: `#7F1D1D` background. Right: `#14532D` background. `CornerRadius="4"`.
- Below: `TextBlock` `"RR · 1 : 2.4"` in `JetBrains Mono 11px #71717A`

**Margin row:** `"Margin Required"` label + `"₹89,556.48"` in `JetBrains Mono 12px #F59E0B`

**Pre-Trade Gate row:**
- `Border` `Background="rgba(251,191,36,0.06)" BorderBrush="rgba(251,191,36,0.2)" CornerRadius="6" Padding="8"` 
- Icon: `⚡` amber + text: `"Rule #3: Never trade within 30 min of a news event"` in `Inter 10px #F59E0B`
- This panel fades out (opacity 1→0, Duration=1s) after BUY/SELL is clicked, then fades back in

**BUY / SELL buttons (side by side, Height=44):**
- BUY: `Background="#14532D"`, `BorderBrush="#22C55E"`, `Foreground="#22C55E"`, `CornerRadius="6"`, `FontFamily="Inter" FontWeight="Bold" FontSize="14"`, content `"BUY  F1"`
- SELL: `Background="#7F1D1D"`, `BorderBrush="#EF4444"`, `Foreground="#EF4444"`, same style, content `"SELL  F2"`
- Click animation: `ScaleTransform` 1.0→0.95→1.0 over 150ms (button press feel)

### Expander 2 — PSYCHOLOGY ENGINE

**Discipline Score row:**
- `SKElement` canvas (full width, Height=8) — horizontal progress bar, `#27272A` track, filled portion color-transitions: `#EF4444` (0–40) → `#F59E0B` (40–70) → `#22C55E` (70–100). Mock value: 78.
- Text: `"78 / 100"` `JetBrains Mono 13px #22C55E` right-aligned

**Streak row:**
- `"🔥 7-day rule-following streak"` in `Inter 11px #F59E0B`

**Habit checklist:** `ItemsControl` with 4 mock items:
```
✅  Pre-market brief done
✅  Position size rule followed
⬜  Max 5 trades/day  · "3/5 used"  #71717A
⬜  No trading after 2pm
```
Checkmark `✅` = `#22C55E`, empty `⬜` = `#3F3F46`, label `Inter 11px #E4E4E7`, sub-label `JetBrains Mono 10px #71717A`

**Mood Check row:**
- Label `"CURRENT MOOD"` `Inter 10px #71717A`
- `StackPanel Orientation="Horizontal"` of 5 emoji `RadioButton` items: `😤 😰 😐 😊 🔥`
- Each: transparent background, `FontSize=18`, on hover scale 1.2
- `😐` selected by default (amber glow border when selected)
- Below selected mood: `"😐 Neutral — logged against trades"` in `Inter 9px #71717A`

**Pomodoro:**
- Large countdown: `"21:14"` `JetBrains Mono Bold 28px #E4E4E7`
- Thin arc progress ring beneath (SkiaSharp, 120x12px wide): track `#27272A`, progress `#22C55E`, represents 21:14 of 25:00
- Three buttons: `[▶ Start]` `[⏸ Pause]` `[⟳ Reset]` — flat style, `Inter 11px #71717A`, hover `#E4E4E7`

### Expander 3 — ACTIVE POSITIONS (mock data)

**DataGrid** with columns: Symbol | Type | Qty | Entry | LTP | P&L | Action
- Grid style: no default chrome, `Background="#0D0D0F"`, `RowBackground` alternates `#0D0D0F` / `#111113`
- Column headers: `Inter 9px #52525B LetterSpacing=2 Uppercase`
- Type cell: pill badge `"L"` (green) or `"S"` (red)
- P&L cell: text color green/red + background `rgba(34,197,94,0.08)` or `rgba(239,68,68,0.08)`
- Action cell: `"✕"` button `#3F3F46`, hover `#EF4444`

Mock rows:
```
NIFTY50  |  L  |  1  |  22,250  |  22,388  |  +₹6,900  +0.62%  |  ✕
BANKNIFTY|  S  |  1  |  48,120  |  48,050  |  +₹3,500  +0.14%  |  ✕
```

**Drawdown alarm**: If any row has P&L < -1.5% from entry, add `BorderBrush="#EF4444" BorderThickness="1"` animated pulse to the entire Expander 3 panel. (Mock: don't trigger — keep both rows positive.)

**Empty state design** (shown when no rows): centered `TextBlock` `"No open positions"` `Inter 13px #3F3F46` + subtext `"The best trade is sometimes no trade."` `Inter 10px #3F3F46 italic`.

---

## ZONE 5 — POSITIONS RAIL (Height=48, DockPanel.Dock="Bottom")

`Border` `Background="#0A0A0A" BorderBrush="#27272A" BorderThickness="1,0,0,0"` (top border only).

Horizontal `StackPanel` showing open positions as compact chips:
```
[NIFTY50 · L · +₹6,900 +0.62%]   [BANKNIFTY · S · +₹3,500 +0.14%]   [TOTAL PNL: +₹10,400]
```
Each chip: `Border CornerRadius="4" Padding="8,4" Background="#111113" BorderBrush="#27272A" Margin="4,6"`.
PnL text: `JetBrains Mono 11px` green/red.
Total PnL: right-docked, `JetBrains Mono Bold 13px #22C55E`.

When no positions: rail collapses to `Height=4`, `Background` gradient `#22C55E → transparent` (a thin green accent line).

---

## ZONE 6 — NEWS TICKER (Height=32, DockPanel.Dock="Bottom")

`Border` `Background="#030303" BorderBrush="#27272A" BorderThickness="1,0,0,0"` (top border only).

Inner: `Canvas` clipping a `StackPanel Orientation="Horizontal"` containing ticker items. Apply `TranslateTransform` animated via `DoubleAnimation` From=0 To=-(total content width) Duration=60s LinearEasing RepeatBehavior=Forever.

Each ticker item (separated by `"  ·  "` spacer in `#27272A`):
```
[CATEGORY PILL] [EVENT TEXT]
```

Category pill styles:
- `ECO`: `Background="rgba(251,191,36,0.15)" BorderBrush="#F59E0B" Foreground="#F59E0B"`
- `GEO`: `Background="rgba(239,68,68,0.15)" BorderBrush="#EF4444" Foreground="#EF4444"`
- `MKT`: `Background="rgba(34,197,94,0.15)" BorderBrush="#22C55E" Foreground="#22C55E"`

All pills: `CornerRadius="3" Padding="4,1" Margin="0,0,6,0" FontFamily="Inter" FontSize="9" FontWeight="SemiBold"`.
Event text: `Inter 11px #71717A`.

Mock ticker items (at minimum 8 unique, no repeats):
1. `[ECO]` Unemployment figures dropped to 3.8% — lowest since 2020
2. `[GEO]` Unexpected geopolitical tensions in Eastern Europe escalate
3. `[ECO]` RBI Rate decision: Hold at 6.5% — third consecutive pause
4. `[MKT]` Global markets showing broad risk-on sentiment
5. `[ECO]` India CPI at 4.2% — within RBI comfort band
6. `[MKT]` FII net buyers for 4th straight session — ₹2,340 Cr inflow
7. `[GEO]` Oil supply disruption fears push Brent above $92
8. `[ECO]` US Non-Farm Payrolls beat expectations at 287K vs 230K forecast

---

## VIEWMODELS TO SCAFFOLD

Create these ViewModel classes with mock data wired in constructors. No service calls — pure hardcoded mock:

```
MainViewModel          — owns all sub-VMs, set as MainWindow DataContext
ChartViewModel         — Candles[], VolumeData[], MA20[], MA50[], VWAP[], 
                         SeasonalityOverlay[], SelectedSymbol, SelectedTimeframe, GhostModeOn
MarketWatchViewModel   — Instruments[] (Symbol, Bid, Ask, SpreadPct, Sparkline[])
SmartMoneyViewModel    — FiiNet, DiiNet, MaxPain, PCR, OiChange
MacroCalendarViewModel — Events[] (Time, Name, Impact, Countdown)
SeasonalityViewModel   — MonthlyReturns[year][month], SelectedSymbol
FlowMapViewModel       — Sectors[] (Name, DailyReturn, MarketCap)
CorrelationViewModel   — Matrix[5][5]
ExecutionViewModel     — Symbol, LTP, OrderType, Qty, SL, Target, 
                         MarginRequired, RiskReward, PreTradeRule
PositionsViewModel     — OpenPositions[], TotalPnL
PsychologyViewModel    — DisciplineScore, StreakDays, Habits[], 
                         CurrentMood, PomodoroSeconds, PomodoroRunning
NewsTickerViewModel    — Items[] (Category, Text)
```

---

## SECONDARY TABS — ARENA & PULSE (stub only)

When `ARENA` tab is clicked, replace the MainGrid content with:

**Arena panel:**
- Header: `"ARENA — Live Trade Ideas"` + `[+ Publish Idea]` button (right-aligned)
- Sub-tab row: `TRENDING | FOLLOWING | TOP ANALYSTS | MY IDEAS`
- 3 mock idea cards in a `ScrollViewer`:

Each card: `Border CornerRadius="8" Background="#0D0D0F" BorderBrush="#27272A" Padding="16" Margin="0,0,0,8"`:
```
[@TraderX]  [NIFTY50 · LONG pill #22C55E]            [ACTIVE badge #22C55E]
"Bounce from demand zone at 22,200. Target: 22,600. SL: 22,050"
[mini placeholder chart area 200x60 Background="#111113" CornerRadius="4"]
[❤ 42]  [💬 7]  [📈 Live P&L: +1.3%  #22C55E JetBrains Mono Bold]
```

When `PULSE` tab is clicked, replace MainGrid content with:

**Pulse panel — Global Markets Grid:**
- `WrapPanel` of 9 market tiles: NIFTY, S&P500, NASDAQ, NIKKEI, DAX, DXY, GOLD, CRUDE, BTC
- Each tile: `Border CornerRadius="8" Background="#0D0D0F" BorderBrush="#27272A" Width="160" Height="90" Margin="4"`
- Inside: market name `Inter SemiBold 12px #71717A`, price `JetBrains Mono Bold 18px #E4E4E7`, change `JetBrains Mono 11px` green/red, sparkline `SKElement 100x24`

Below: Fear & Greed semicircle gauge — `SKElement Width="240" Height="120"`. Draw semicircle arc divided into 5 color zones: `#EF4444 → #F59E0B → #22C55E`. Needle pointing to mock value 62 ("Greed"). Label below: `"62 · GREED"` `JetBrains Mono Bold 14px #F59E0B`.

---

## ANIMATIONS & POLISH

1. **Price tick flash**: When Bid/Ask values update (simulate via `DispatcherTimer` every 800ms with ±0.01% random change), flash the TextBlock background to `rgba(34,197,94,0.2)` or `rgba(239,68,68,0.2)` for 300ms then fade to transparent.

2. **Panel entrance**: On app load, each of the 3 main columns slides in with `TranslateTransform` — Left dock from X=-20, Right panel from X=+20, Chart from Y=+10. Duration=400ms, `CubicEaseOut`.

3. **Expander animation**: Expander open/close uses `DoubleAnimation` on Height (not the default WPF snap). Duration=250ms `CubicEaseInOut`.

4. **BUY/SELL press**: `ScaleTransform` 1.0→0.95→1.0 on button click, 150ms.

5. **Mood emoji hover**: `ScaleTransform` 1.0→1.25 on `MouseEnter`, reverse on `MouseLeave`, 120ms.

6. **Discipline arc on TopBar**: Rotate the arc fill using `DoubleAnimation` on a `RotateTransform` from 0 to (score/100*270) degrees on app load, Duration=1.2s `CubicEaseOut`.

---

## DELIVERABLES EXPECTED FROM ANTIGRAVITY

1. `MainWindow.xaml` + `MainWindow.xaml.cs`
2. `Styles/Tokens.xaml` (all design tokens as StaticResources)
3. `Styles/Controls.xaml` (RadioButton pill, Expander, DataGrid, ScrollBar overrides)
4. All ViewModel `.cs` files with mock data
5. `App.xaml` merging all ResourceDictionaries, registering fonts
6. NuGet packages list: `LiveChartsCore.SkiaSharpView.WPF`, `SkiaSharp.Views.WPF`, `MaterialDesignThemes`
7. Font embedding instructions for JetBrains Mono + Inter in the `.csproj`

**The app must compile and run showing the full terminal with mock data on first launch. No placeholder grey boxes — every panel must render populated content.**

