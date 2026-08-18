# 💰 SalaryWise – Salary-Based Investment Planner!!

> **Disclaimer:** SalaryWise is an educational tool. All recommendations are for informational purposes only and do not constitute professional financial advice. Always consult a SEBI-registered financial advisor before investing.

---

## 📌 Project Overview

SalaryWise is a full-stack **ASP.NET Core 8 MVC** web application that helps Indian salaried employees make smarter investment decisions.

Instead of recommending generic investment products, SalaryWise:
- Analyses your salary, expenses, and savings to check **affordability**
- Generates a **diversified portfolio** across 17+ instruments (PPF, Nifty 50, Gold ETF, SGB, Flexi-Cap, etc.)
- Projects your **wealth growth** over 5, 10, and 20 years using the SIP compound formula
- Computes a **financial health score** (0–100) with actionable tips
- Lets you simulate "what if I invest 30% instead of 20%?" instantly

---

## ✨ Features

| Feature | Details |
|---|---|
| 🔐 Authentication | Registration, Login, Logout, Remember Me, Forgot/Reset Password, Change Password |
| 👤 User Profile | Full name, DOB, salary, expenses, goals, risk preference — editable |
| 📊 Recommendation Engine | Rule-based allocation across 17 instruments based on age, risk, horizon, goal |
| 💸 Affordability Analysis | Checks if chosen % is sustainable; suggests safe range |
| 📈 Projections | SIP-formula compound growth for 5, 10, 20 years with inflation adjustment |
| 🩺 Health Score | 0–100 score across savings rate, emergency fund, expense ratio, diversification |
| 📉 Charts | Pie chart (allocation) + Line chart (20-year growth) via Chart.js |
| 🔄 What-If Simulator | Adjust % and risk live via AJAX |
| 💼 Salary Simulator | +5%, +10%, +20% salary increase buttons |
| 🗂 Plan Management | Create, view, edit, delete, activate, compare two plans |
| 🕐 History | Full plan history with status (Active / Archived) |
| 🌙 Dark Theme | Glassmorphism dark UI with indigo/emerald palette |

---

## 🛠 Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Database | SQLite |
| Auth | ASP.NET Core Identity |
| UI | Bootstrap 5 + Vanilla CSS (custom dark theme) |
| Charts | Chart.js 4 |
| Icons | Bootstrap Icons |
| Fonts | Google Fonts – Inter |

---

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [dotnet-ef CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

---

## 🚀 Installation & Setup

### 1. Clone / Open the project

```bash
cd C:\Users\HemantYadav\.gemini\antigravity\scratch\SalaryWise
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Run EF Core migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates `salarywise.db` (SQLite) in the project root and seeds:
- Demo user: `demo@salarywise.in` / `Demo@123`

### 4. Run the application

```bash
dotnet run
```

Navigate to `https://localhost:5001` (or the port shown in the terminal).

---

## 🏗 Folder Structure

```
SalaryWise/
├── Controllers/
│   ├── AccountController.cs     # Auth: register, login, forgot/reset password
│   ├── DashboardController.cs   # Main dashboard
│   ├── HomeController.cs        # Landing page
│   ├── PlanController.cs        # Plan CRUD + simulator API
│   └── ProfileController.cs     # Profile view/edit
├── Data/
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   └── SeedData.cs              # Demo user seed
├── Migrations/                  # EF Core auto-generated
├── Models/
│   ├── Enums.cs                 # RiskTolerance, FinancialGoalType, etc.
│   ├── InvestmentPlan.cs
│   ├── InvestmentRecommendation.cs
│   ├── PortfolioSnapshot.cs
│   ├── ProjectionResult.cs
│   └── UserProfile.cs
├── Repositories/
│   ├── InvestmentPlanRepository.cs
│   └── UserProfileRepository.cs
├── Services/
│   ├── AffordabilityService.cs   # Checks if investment % is sustainable
│   ├── FinancialHealthService.cs # 0–100 health score
│   ├── ProjectionService.cs      # SIP compound projections
│   └── RecommendationEngine.cs  # Rule-based portfolio allocation
├── ViewModels/
│   ├── AccountViewModels.cs
│   ├── ComparePlanViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── PlanDetailsViewModel.cs
│   ├── PlanInputViewModel.cs
│   └── ProfileViewModel.cs
├── Views/
│   ├── Account/   Login, Register, ForgotPassword, ResetPassword, ChangePassword
│   ├── Dashboard/ Index (main dashboard)
│   ├── Home/      Index (landing page)
│   ├── Plan/      Create, Details, Edit, History, Compare, Simulator
│   ├── Profile/   Index, Edit
│   └── Shared/    _Layout, _ValidationScriptsPartial
├── wwwroot/
│   ├── css/site.css       # Custom dark theme
│   └── js/site.js
├── appsettings.json
├── Program.cs
└── README.md
```

---

## 🧠 Recommendation Engine

The engine (`Services/RecommendationEngine.cs`) is **rule-based** and works as follows:

### Step 1 – Determine Safe vs Medium split

| Factor | Effect |
|---|---|
| Age < 30 | −10% safe (more equity) |
| Age 40–50 | +10% safe |
| Age 50+ | +20% safe |
| Risk = Safe | +20% safe |
| Risk = Medium | −10% safe |
| Long-term horizon | −10% safe |
| Short-term horizon | +15% safe |
| Goal = EmergencyFund | +25% safe |
| Goal = WealthCreation | −10% safe |
| Low existing savings | +10% safe |

Final ratio is clamped to 30–90% safe.

### Step 2 – Allocate Safe instruments

PPF (30%), Govt Bonds (20%), Liquid Fund (15%), Short-term Debt (15%), RBI Bonds (20%) — weighted by profile

### Step 3 – Allocate Medium instruments

Nifty 50 (35%), Gold ETF (15%), SGB (10%), Flexi-Cap / BAF / Corporate Bonds / Hybrid — weighted by age and horizon

### Expected Returns Used

| Instrument | Expected Annual Return |
|---|---|
| PPF | 7.1% |
| EPF | 8.1% |
| NSS | 7.7% |
| Govt Bond | 7.3% |
| RBI Bond | 7.5% |
| Liquid Fund | 6.8% |
| Nifty 50 | 12.0% |
| Flexi-Cap | 12.5% |
| Gold ETF | 8.0% |
| SGB | 8.5% |
| Hybrid | 10.5% |

---

## 📊 Projection Formula

Uses the standard SIP future value formula:

```
FV = PMT × [((1 + r/12)^n - 1) / (r/12)] × (1 + r/12)
```

Where `r` = annual return, `n` = months invested, `PMT` = monthly investment per instrument.

Inflation adjustment: divide by `(1 + 0.06)^years`

---

## 🔐 Default Dev Credentials

| Account | Email | Password |
|---|---|---|
| Demo user | demo@salarywise.in | Demo@123 |

---

## 🔮 Future Improvements

- Direct Mutual Fund NAV integration via MFAPI
- Goal-based SIP tracker
- Tax calculator (80C deductions)
- Step-up SIP simulation
- PDF plan export
- Email notifications for goal milestones
- Mobile app (MAUI)

---

## ⚠️ Disclaimer

SalaryWise is an **educational portfolio project** built to demonstrate ASP.NET Core MVC skills. The investment recommendations, return assumptions, and projections are illustrative only and should not be treated as professional financial advice. Always consult a qualified, SEBI-registered financial advisor before making investment decisions.
