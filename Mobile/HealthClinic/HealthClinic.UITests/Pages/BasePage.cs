using Xamarin.UITest;

namespace HealthClinic.UITests
{
    public abstract class BasePage
    {
        #region Constructors
        protected BasePage(IApp app, string pageTitle)
        {
            App = app;
            PageTitle = pageTitle;
        }
        #endregion

        #region Properties
        public string PageTitle { get; }
        protected IApp App { get; }
        #endregion

        #region Methods
        public virtual void WaitForPageToLoad()
        {
            App.WaitForElement(PageTitle);
            App.Screenshot("Page Loaded");
        }
        #endregion
    }
}
