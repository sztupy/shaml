using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

using NHibernate.Validator.Engine;

using Shaml.Web.CommonValidator;
using Shaml.Web.NHibernate;
using Shaml.Web.HtmlHelpers;
using Shaml.Core;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.DomainModel;
using Shaml.Core.PersistenceSupport.NHibernate;

using WebBase.Core;

namespace WebBase.Controllers
{
    [HandleError]
    public class WebSamplesController : Controller
    {
        public WebSamplesController(INHibernateQueryRepository<WebSample> WebSampleRepository) {
            Check.Require(WebSampleRepository != null, "WebSampleRepository may not be null");

            this.WebSampleRepository = WebSampleRepository;
        }

        [Transaction]
        public ActionResult Index(int? Page, string OrderBy, bool? Desc) {
            long numResults;
            int page = 0;
            if (Page != null)
            {
                page = (int)Page;
            }
            IList<WebSample> WebSamples = null;
            WebSamples = WebSampleRepository.GetAll(20, page, out numResults, WebSampleRepository.CreateOrder(OrderBy,Desc==true));
            PaginationData pd = new ThreeWayPaginationData(page, 20, numResults);
            ViewData["Pagination"] = pd;
            return View(WebSamples);
        }

        [Transaction]
        public ActionResult Show(int id) {
            WebSample WebSample = WebSampleRepository.Get(id);
            return View(WebSample);
        }

        public ActionResult Create() {
            WebSampleFormViewModel viewModel = WebSampleFormViewModel.CreateWebSampleFormViewModel();
            return View(viewModel);
        }

        //[ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(WebSample WebSample) {
            if (ViewData.ModelState.IsValid && WebSample.IsValid()) {
                WebSampleRepository.SaveOrUpdate(WebSample);

                TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()] = 
					"The WebSample was successfully created.";
                return RedirectToAction("Index");
            }

            WebSampleFormViewModel viewModel = WebSampleFormViewModel.CreateWebSampleFormViewModel();
            viewModel.WebSample = WebSample;
            return View(viewModel);
        }

        [Transaction]
        public ActionResult Edit(int id) {
            WebSampleFormViewModel viewModel = WebSampleFormViewModel.CreateWebSampleFormViewModel();
            viewModel.WebSample = WebSampleRepository.Get(id);
            return View(viewModel);
        }

        //[ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(WebSample WebSample) {
            WebSample WebSampleToUpdate = WebSampleRepository.Get(WebSample.Id);
            TransferFormValuesTo(WebSampleToUpdate, WebSample);

            if (ViewData.ModelState.IsValid && WebSample.IsValid()) {
                TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()] = 
					"The WebSample was successfully updated.";
                return RedirectToAction("Index");
            }
            else {
                WebSampleRepository.DbContext.RollbackTransaction();

				WebSampleFormViewModel viewModel = WebSampleFormViewModel.CreateWebSampleFormViewModel();
				viewModel.WebSample = WebSample;
				return View(viewModel);
            }
        }

        private void TransferFormValuesTo(WebSample WebSampleToUpdate, WebSample WebSampleFromForm) {
            // __BEGIN__PROPERTY__
            WebSampleToUpdate.Property = WebSampleFromForm.Property;
            // __END__PROPERTY__
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Delete(int id)
        {
            WebSample websampleToDelete = WebSampleRepository.Get(id);
            return View(websampleToDelete);
        }

        //[ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteConfirmed(int id) {
            string resultMessage = "The WebSample was successfully deleted.";
            WebSample WebSampleToDelete = WebSampleRepository.Get(id);

            if (WebSampleToDelete != null) {
                WebSampleRepository.Delete(WebSampleToDelete);

                try {
                    WebSampleRepository.DbContext.CommitChanges();
                }
                catch {
                    resultMessage = "A problem was encountered preventing the WebSample from being deleted. " +
						"Another item likely depends on this WebSample.";
                    WebSampleRepository.DbContext.RollbackTransaction();
                }
            }
            else {
                resultMessage = "The WebSample could not be found for deletion. It may already have been deleted.";
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

        private readonly IRepository<WebSample> WebSampleRepository;
    }
}

