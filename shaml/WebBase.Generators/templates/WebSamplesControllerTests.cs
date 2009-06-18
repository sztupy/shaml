using System;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Rhino.Mocks;
using NUnit.Framework.SyntaxHelpers;
using Shaml.Core.PersistenceSupport;
using Shaml.Testing;
using System.Collections.Generic;
using System.Web.Mvc;
using WebBase.Core;
using WebBase.Controllers;

namespace Tests.Blog.Web.Controllers
{
    [TestFixture]
    public class WebSamplesControllerTests
    {
        [SetUp]
        public void SetUp() {
            controller = new WebSamplesController(CreateMockWebSampleRepository());
        }

        /// <summary>
        /// Add a couple of objects to the list within CreateWebSamples and change the 
        /// "Is.EqualTo(0)" within this test to the respective number.
        /// </summary>
        [Test]
        public void CanListWebSamples() {
            ViewResult result = controller.Index().AssertViewRendered();

            Assert.That(result.ViewData.Model as List<WebSample>, Is.Not.Null);
            Assert.That((result.ViewData.Model as List<WebSample>).Count, Is.EqualTo(0));
        }

        [Test]
        public void CanShowWebSample() {
            ViewResult result = controller.Show(1).AssertViewRendered();

            Assert.That(result.ViewData.Model as WebSample, Is.Not.Null);
            Assert.That((result.ViewData.Model as WebSample).Id, Is.EqualTo(1));
        }

        [Test]
        public void CanInitWebSampleCreation() {
            ViewResult result = controller.Create().AssertViewRendered();

            Assert.That(result.ViewData.Model as WebSample, Is.Null);
        }

        [Test]
        public void CanEnsureWebSampleCreationIsValid() {
           WebSample websampleFromForm = new WebSample();
            ViewResult result = controller.Create(websampleFromForm).AssertViewRendered();

            Assert.That(result.ViewData.Model as WebSample, Is.Null);
            Assert.That(result.ViewData.ModelState.Count, Is.GreaterThan(0));

            // Example validation message test for lower level testing
            // Assert.That(result.ViewData.ModelState["WebSample.Name"].Errors[0].ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void CanCreateWebSample() {
            WebSample websampleFromForm = CreateTransientWebSample();
            RedirectToRouteResult redirectResult = controller.Create(websampleFromForm)
                .AssertActionRedirect().ToAction("Index");
            Assert.That(controller.TempData["message"].ToString().Contains("was successfully created"));
        }

        [Test]
        public void CanUpdateWebSample() {
            WebSample websampleFromForm = CreateTransientWebSample();
            RedirectToRouteResult redirectResult = controller.Edit(1, websampleFromForm)
                .AssertActionRedirect().ToAction("Index");
            Assert.That(controller.TempData["message"].ToString().Contains("was successfully updated"));
        }

        [Test]
        public void CanInitWebSampleEdit() {
            ViewResult result = controller.Edit(1).AssertViewRendered();

            Assert.That(result.ViewData.Model as WebSample, Is.Not.Null);
            Assert.That((result.ViewData.Model as WebSample).Id, Is.EqualTo(1));
        }

        [Test]
        public void CanDeleteWebSample() {
            RedirectToRouteResult redirectResult = controller.DeleteConfirmed(1)
                .AssertActionRedirect().ToAction("Index");
            Assert.That(controller.TempData["message"].ToString().Contains("was successfully deleted"));
        }

		#region Create Mock WebSample Repository

        private IRepository<WebSample> CreateMockWebSampleRepository() {
            MockRepository mocks = new MockRepository();

            IRepository<WebSample> mockedRepository = mocks.StrictMock<IRepository<WebSample>>();
            Expect.Call(mockedRepository.GetAll())
                .Return(CreateWebSamples());
            Expect.Call(mockedRepository.Get(1)).IgnoreArguments()
                .Return(CreateWebSample());
            Expect.Call(mockedRepository.SaveOrUpdate(null)).IgnoreArguments()
                .Return(CreateWebSample());
            Expect.Call(delegate { mockedRepository.Delete(null); }).IgnoreArguments();

            IDbContext mockedDbContext = mocks.StrictMock<IDbContext>();
            Expect.Call(delegate { mockedDbContext.CommitChanges(); });
            SetupResult.For(mockedRepository.DbContext).Return(mockedDbContext);
            
            mocks.Replay(mockedRepository);

            return mockedRepository;
        }

        private WebSample CreateWebSample() {
            WebSample websample = CreateTransientWebSample();
            EntityIdSetter.SetIdOf<int>(websample, 1);
            return websample;
        }

        private List<WebSample> CreateWebSamples() {
            List<WebSample> websamples = new List<WebSample>();

            // Create a number of websample object instances here and add them to the list

            return websamples;
        }
        
        #endregion

        /// <summary>
        /// Creates a valid, transient WebSample; typical of something retrieved back from a form submission
        /// </summary>
        private WebSample CreateTransientWebSample() {
            WebSample websample = new WebSample() {
__BEGIN__PROPERTY__
            Property = new PropertyType();
__END__PROPERTY__
            };
            
            return websample;
        }

        private WebSamplesController controller;
    }
}
