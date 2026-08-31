using System;
using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeSecurityLogView : ISecurityLogView
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;

        public IReadOnlyList<SecurityEventRow> ShownEvents { get; private set; }
        public string ShownError { get; private set; }

        public void ShowEvents(IReadOnlyList<SecurityEventRow> events) => ShownEvents = events;
        public void ShowError(string message) => ShownError = message;
    }
}
