using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeChangePasswordView : IChangePasswordView
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public bool Mandatory { get; set; }

        public string ShownError { get; private set; }
        public bool? ClosedWithChanged { get; private set; }

        public void ShowError(string message) => ShownError = message;
        public void Close(bool changed) => ClosedWithChanged = changed;
    }
}
