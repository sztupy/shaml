using System.Web.Mvc;
using WebBase.Core;
using SharpArch.Core.PersistenceSupport;
using SharpArch.Core.DomainModel;
using System.Collections.Generic;
using System;
using SharpArch.Web.NHibernate;
using NHibernate.Validator.Engine;
using System.Text;
using SharpArch.Web.CommonValidator;
using SharpArch.Core;

namespace WebBase.Controllers
{
    [HandleError]
    public class WebSamplesController : Controller
    {
        public WebSamplesController(IRepository<WebSample> websampleRepository) {
            Check.Require(websampleRepository != null, "websampleRepository may not be null");

            this.websampleRepository = websampleRepository;
        }

        [Transaction]
        public ActionResult Index() {
            IList<WebSample> websamples = websampleRepository.GetAll();
            return View(websamples);
        }

        [Transaction]
        public ActionResult Show(int id) {
            WebSample websample = websampleRepository.Get(id);
            return View(websample);
        }

        public ActionResult Create() {
            WebSampleFormViewModel viewModel = WebSampleFormViewModel.CreateWebSampleFormViewModel();
            return View(viewModel);
        }

        [ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(WebSample websample) {
            if (ViewData.ModelState.IsValid && websample.IsValid()) {
                websampleRepository.SaveOrUpdate(websample);

                TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()] = 
					"The websample was successfully created.";
                return RedirectToAction("Index");
            }

            WebSampleFormViewModel viewModel = WebSampleFormViewModel.CreateWebSampleFormViewModel();
            viewModel.WebSample = websample;
            return View(viewModel);
        }

        [Transaction]
        public ActionResult Edit(int id) {
            WebSampleFormViewModel viewModel = WebSampleFormViewModel.CreateWebSampleFormViewModel();
            viewModel.WebSample = websampleRepository.Get(id);
            return View(viewModel);
        }

        [ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(WebSample websample) {
            WebSample websampleToUpdate = websampleRepository.Get(websample.Id);
            TransferFormValuesTo(websampleToUpdate, websample);

            if (ViewData.ModelState.IsValid && websample.IsValid()) {
                TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()] = 
					"The websample was successfully updated.";
                return RedirectToAction("Index");
            }
            else {
                websampleRepository.DbContext.RollbackTransaction();

				WebSampleFormViewModel viewModel = WebSampleFormViewModel.CreateWebSampleFormViewModel();
				viewModel.WebSample = websample;
				return View(viewModel);
            }
        }

        private void TransferFormValuesTo(WebSample websampleToUpdate, WebSample websampleFromForm) {
          // TODO: Add copy methods
        }

        [ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete(int id) {
            string resultMessage = "The websample was successfully deleted.";
            WebSample websampleToDelete = websampleRepository.Get(id);

            if (websampleToDelete != null) {
                websampleRepository.Delete(websampleToDelete);

                try {
                    websampleRepository.DbContext.CommitChanges();
                }
                catch {
                    resultMessage = "A problem was encountered preventing the websample from being deleted. " +
						"Another item likely depends on this websample.";
                    websampleRepository.DbContext.RollbackTransaction();
                }
            }
            else {
                resultMessage = "The websample could not be found for deletion. It may already have been deleted.";
            }

            TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()] = resultMessage;
            return RedirectToAction("Index");
        }

		/// <summary>
		/// Holds data to be passed to the WebSample form for creates and edits
		/// </summary>
        public class WebSampleFormViewModel
        {
            private WebSampleFormViewModel() { }

			/// <summary>
			/// Creation method for creating the view model. Services may be passed to the creation 
			/// method to instantiate items such as lists for drop down boxes.
			/// </summary>
            public static WebSampleFormViewModel CreateWebSampleFormViewModel() {
                WebSampleFormViewModel viewModel = new WebSampleFormViewModel();
                
                return viewModel;
            }

            public WebSample WebSample { get; internal set; }
        }

        private readonly IRepository<WebSample> websampleRepository;
    }
}

