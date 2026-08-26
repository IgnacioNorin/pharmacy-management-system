using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeHomeView : IHomeView
    {
        public HomeSummary Summary { get; private set; }

        public void SetSummary(HomeSummary summary) => Summary = summary;
    }
}
