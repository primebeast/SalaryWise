namespace SalaryWise.Models
{
    /// <summary>Financial goal types available for investment planning</summary>
    public enum FinancialGoalType
    {
        EmergencyFund,
        WealthCreation,
        Retirement,
        HomePurchase,
        ChildEducation,
        Vacation
    }

    /// <summary>Risk tolerance levels for investment allocation</summary>
    public enum RiskTolerance
    {
        Safe,
        Medium
    }

    /// <summary>Employment types supported by the planner</summary>
    public enum EmploymentType
    {
        Salaried,
        SelfEmployed,
        Freelancer,
        Government,
        Other
    }

    /// <summary>Investment time horizons</summary>
    public enum InvestmentHorizon
    {
        ShortTerm,   // < 3 years
        MediumTerm,  // 3–7 years
        LongTerm     // > 7 years
    }

    /// <summary>Investment category groupings</summary>
    public enum InvestmentCategory
    {
        Safe,
        Medium
    }

    /// <summary>Status of an investment plan</summary>
    public enum PlanStatus
    {
        Draft,
        Active,
        Archived
    }
}
