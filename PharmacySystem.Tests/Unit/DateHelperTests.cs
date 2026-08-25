using System;
using PharmacySystem.Helpers;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    public class DateHelperTests
    {
        [Fact]
        public void FormatDatePresentation_ReturnsDdMmYyyy()
        {
            DateTime date = new DateTime(2026, 3, 5);

            Assert.Equal("05-03-2026", DateHelper.FormatDatePresentation(date));
        }

        [Fact]
        public void FormatDateBackend_ReturnsYyyyMmDd()
        {
            DateTime date = new DateTime(2026, 3, 5);

            Assert.Equal("2026-03-05", DateHelper.FormatDateBackend(date));
        }
    }
}
