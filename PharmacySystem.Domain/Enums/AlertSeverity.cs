namespace PharmacySystem.Model
{
    // Ordered so that sorting a list of ProductAlert by Severity puts the most urgent items first.
    public enum AlertSeverity
    {
        Critical = 0,
        Expired = 1,
        Low = 2,
        ExpiringSoon = 3
    }
}
