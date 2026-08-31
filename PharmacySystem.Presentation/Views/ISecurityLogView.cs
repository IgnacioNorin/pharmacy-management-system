using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface ISecurityLogView
    {
        DateTime StartDate { get; }
        DateTime EndDate { get; }

        void ShowEvents(IReadOnlyList<SecurityEventRow> events);
        void ShowError(string message);
    }
}
