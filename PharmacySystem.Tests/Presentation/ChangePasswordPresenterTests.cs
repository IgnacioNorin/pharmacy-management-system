using PharmacySystem.Infrastructure;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ChangePasswordPresenterTests
    {
        private static ChangePasswordPresenter CreatePresenter(FakeChangePasswordView view, FakePasswordChangeService service, int personId = 3)
            => new ChangePasswordPresenter(view, service, personId);

        [Fact]
        public void OnSave_ConfirmDoesNotMatch_ShowsErrorAndNeverCallsTheService()
        {
            var view = new FakeChangePasswordView { NewPassword = "brandnew", ConfirmPassword = "different" };
            var service = new FakePasswordChangeService();

            CreatePresenter(view, service).OnSave();

            Assert.NotNull(view.ShownError);
            Assert.Null(service.ChangeOwnCall);
            Assert.Null(view.ClosedWithChanged);
        }

        [Fact]
        public void OnSave_Ok_ClosesTheDialogAsChanged()
        {
            var view = new FakeChangePasswordView { CurrentPassword = "old", NewPassword = "brandnew", ConfirmPassword = "brandnew" };
            var service = new FakePasswordChangeService { ChangeOwnResult = PasswordChangeResult.Ok };

            CreatePresenter(view, service, personId: 3).OnSave();

            Assert.Equal((3, "old", "brandnew"), service.ChangeOwnCall);
            Assert.True(view.ClosedWithChanged);
        }

        [Theory]
        [InlineData(PasswordChangeResult.WrongCurrent)]
        [InlineData(PasswordChangeResult.TooShort)]
        [InlineData(PasswordChangeResult.SameAsOld)]
        public void OnSave_ServiceRejects_ShowsErrorAndKeepsTheDialogOpen(PasswordChangeResult rejection)
        {
            var view = new FakeChangePasswordView { NewPassword = "abc", ConfirmPassword = "abc" };
            var service = new FakePasswordChangeService { ChangeOwnResult = rejection };

            CreatePresenter(view, service).OnSave();

            Assert.NotNull(view.ShownError);
            Assert.Null(view.ClosedWithChanged);
        }

        [Fact]
        public void OnSave_DatabaseUnavailable_ShowsTheConnectionError()
        {
            var view = new FakeChangePasswordView { NewPassword = "brandnew", ConfirmPassword = "brandnew" };
            var service = new FakePasswordChangeService { ChangeOwnThrows = new DataUnavailableException() };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(DataUnavailableException.DefaultMessage, view.ShownError);
        }

        [Fact]
        public void OnCancel_OptionalDialog_ClosesAsNotChanged()
        {
            var view = new FakeChangePasswordView { Mandatory = false };

            CreatePresenter(view, new FakePasswordChangeService()).OnCancel();

            Assert.False(view.ClosedWithChanged);
        }

        [Fact]
        public void OnCancel_MandatoryDialog_DoesNothing()
        {
            var view = new FakeChangePasswordView { Mandatory = true };

            CreatePresenter(view, new FakePasswordChangeService()).OnCancel();

            Assert.Null(view.ClosedWithChanged);
        }
    }
}
