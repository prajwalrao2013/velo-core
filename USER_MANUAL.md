# Velo Terminal - User Manual & Help Guide

Welcome to **Velo Terminal**, your high-performance, ultra-lightweight trading terminal and journaling platform. Designed for mental clarity and execution speed, Velo helps you stay disciplined and focused on risk management.

## 1. Installation

Velo Terminal is distributed as a self-contained AppImage for Linux, meaning you don't need to install any external dependencies.

### How to Install and Run:
1. Download the `Velo_Terminal-x86_64.AppImage` from the GitHub Releases page.
2. Open your terminal and navigate to the folder where you downloaded the file.
3. Make the file executable by running:
   ```bash
   chmod +x Velo_Terminal-x86_64.AppImage
   ```
4. Run the application:
   ```bash
   ./Velo_Terminal-x86_64.AppImage
   ```
*(Alternatively, you can right-click the file in your file manager, go to Properties -> Permissions, and check "Allow executing file as program", then double-click it).*

## 2. Core Features & Philosophy

* **Psychology Engine:** Velo monitors your trading "tilt". If you hit a specific consecutive loss threshold, the engine will trigger a mandatory Pomodoro break to prevent revenge trading.
* **Contextual Education:** Hover over complex data points (like IV Percentile) to see subtle tooltips defining the metrics.
* **Gamified Discipline:** Your performance is measured by a `DisciplineScore` tracking your risk management and journaling habits, rather than purely focusing on P&L fluctuations.
* **Zero-Dependency Journaling:** Executions and trades are instantly logged and saved locally on your machine for absolute privacy.

## 3. Keyboard Shortcuts

To maximize execution speed, Velo Terminal supports global keybindings that override UI clicks:
* **F1:** Fast-action execution shortcut (Configurable in settings)
* **F2:** Fast-action execution shortcut (Configurable in settings)

## 4. Compliance & Risk

Velo is strictly a passthrough tool. It does **not** provide predictive "Buy" or "Sell" signals. All algorithmic features are strictly focused on risk analysis (such as Margin requirements, Risk:Reward ratios, and Options Greeks calculation) to help you make your own informed decisions.
