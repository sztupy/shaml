using System;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Rhino.Mocks;
using Shaml.Core.PersistenceSupport;
using Shaml.Testing;
using System.Collections.Generic;
using System.Web.Mvc;
using WebBase;
using WebBase.Config;
using WebBase.Core;
using WebBase.Controllers;

namespace Tests.Blog.Web.Controllers
{
    [TestFixture]
    public class WebSamplesControllerTests
    {
        [SetUp]
        public void SetUp() {
            ServiceLocatorInitializer.Init();
            controller = new WebSamplesController(CreateMockWebSampleRepository());
        }

        /// <summary>
        /// Add a couple of objects to the list within CreateWebSamples and change the 
        /// "ShouldEqual(0)" within this test to the respective number.
        /// </summary>
        [Test]
        public void CanListWebSamples() {
            ViewResult result = controller.Index().AssertViewRendered();

            result.ViewData.Model.ShouldNotBeNull();
            (result.ViewData.Model as List<WebSample>).Count.ShouldEqual(0);
        }

        [Test]
        public void CanShowWebSample() {
            ViewResult result = controller.Show(1).AssertViewRendered();

			result.ViewData.ShouldNotBeNull();
			
            (result.ViewData.Model as WebSample).Id.ShouldEqual(1);
        }

        [Test]
        public void CanInitWebSampleCreation() {
            ViewResult result = controller.Create().AssertViewRendered();
            
            result.ViewData.Model.ShouldNotBeNull();
            result.ViewData.Model.ShouldBeOfType(typeof(WebSamplesController.WebSampleFormViewModel));
            (result.ViewData.Model as WebSamplesController.WebSampleFormViewModel).WebSample.ShouldBeNull();
        }

        [Test]
        public void CanEnsureWebSampleCreationIsValid() {
            WebSample websampleFromForm = new WebSample();
            ViewResult result = controller.Create(websampleFromForm).AssertViewRendered();

            result.ViewData.Model.ShouldNotBeNull();
            result.ViewData.Model.ShouldBeOfType(typeof(WebSamplesController.WebSampleFormViewModel));
        }

        [Test]
        public void CanCreateWebSample() {
            WebSample websampleFromForm = CreateTransientWebSample();
            RedirectToRouteResult redirectResult = controller.Create(websampleFromForm)
                .AssertActionRedirect().ToAction("Index");
            controller.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()].ToString()
				.ShouldContain("was successfully created");
        }

        [Test]
        public void CanUpdateWebSample() {
            WebSample websampleFromForm = CreateTransientWebSample();
            EntityIdSetter.SetIdOf<int>(websampleFromForm, 1);
            RedirectToRouteResult redirectResult = controller.Edit(websampleFromForm)
                .AssertActionRedirect().ToAction("Index");
            controller.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()].ToString()
				.ShouldContain("was successfully updated");
        }

        [Test]
        public void CanInitWebSampleEdit() {
            ViewResult result = controller.Edit(1).AssertViewRendered();

			result.ViewData.Model.ShouldNotBeNull();
            result.ViewData.Model.ShouldBeOfType(typeof(WebSamplesController.WebSampleFormViewModel));
            (result.ViewData.Model as WebSamplesController.WebSampleFormViewModel).WebSample.Id.ShouldEqual(1);
        }

        [Test]
        public void CanDeleteWebSample() {
            RedirectToRouteResult redirectResult = controller.Delete(1)
                .AssertActionRedirect().ToAction("Index");
            
            controller.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()].ToString()
				.ShouldContain("was successfully deleted");
        }

		#region Create Mock WebSample Repository

        private IRepository<WebSample> CreateMockWebSampleRepository() {

            IRepository<WebSample> mockedRepository = MockRepository.GenerateMock<IRepository<WebSample>>();
            mockedRepository.Expect(mr => mr.GetAll()).Return(CreateWebSamples());
            mockedRepository.Expect(mr => mr.Get(1)).IgnoreArguments().Return(CreateWebSample());
            mockedRepository.Expect(mr => mr.SaveOrUpdate(null)).IgnoreArguments().Return(CreateWebSample());
            mockedRepository.Expect(mr => mr.Delete(null)).IgnoreArguments();

			IDbContext mockedDbContext = MockRepository.GenerateStub<IDbContext>();
			mockedDbContext.Stub(c => c.CommitChanges());
			mockedRepository.Stub(mr => mr.DbContext).Return(mockedDbContext);
            
            return mockedRepository;
        }

        private WebSample CreateWebSample() {
            WebSample websample = CreateTransientWebSample();
            EntityIdSetter.SetIdOf<int>(websample, 1);
            return websample;
        }

          private List<WebSample> CreateWebSamples() {
              List<WebSample> websamples = new List<WebSample>();

            // Create a number of domain object instances here and add them to the list

            return websamples;
        }
        
        #endregion

        /// <summary>
        /// Creates a valid, transient WebSample; typical of something retrieved back from a form submission
        /// </summary>
        private WebSample CreateTransientWebSample() {
            WebSample websample = new WebSample() {
              //TODO: Create the properties of the object
            };
            
            return websample;
        }

        private WebSamplesController controller;
    }
}
