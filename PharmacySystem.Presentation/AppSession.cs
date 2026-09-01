using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // The signed-in user and their resolved permission set for the current process session.
    // Was MainForm.Session / MainForm.oPerson; lives here so the composition root and the WPF
    // shell can both reach it without either referencing the other. The shell sets it on login
    // and refreshes Current whenever the session is re-resolved.
    public static class AppSession
    {
        public static CurrentUser? Current;
        public static Person? Person;

        public static void Set(CurrentUser? user)
        {
            Current = user;
            Person = user?.Person;
        }
    }
}
