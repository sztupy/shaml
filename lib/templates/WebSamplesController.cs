using System.Web.Mvc;
using System.Collections.Generic;
using System.Text;
using System;
using Shaml.Core;
using Shaml.Core.DomainModel;
using Shaml.Core.PersistenceSupport;
using Shaml.Web.NHibernate;
using Shaml.Web.CommonValidator;
using NHibernate.Validator.Engine;
using WebBase.Core;
using Shaml.Data.NHibernate;

namespace WebBase.Controllers
{
    [HandleError]
    [GenericLogger]
    public class WebSamplesController : Controller
    {
        private readonly IRepository<WebSample> websampleRepository;

        public WebSamplesController()
        {
            websampleRepository = new Repository<WebSample>();
        }

        public WebSamplesController(IRepository<WebSample> rep)
        {
            websampleRepository = rep;
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
            return View();
        }

        //[ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(WebSample websample) {
            if (websample.IsValid()) {
                websampleRepository.SaveOrUpdate(websample);

                TempData["message"] = "The websample was successfully created.";
                return RedirectToAction("Index");
            }

            MvcValidationAdapter.TransferValidationMessagesTo(ViewData.ModelState,
                websample.ValidationResults());
            return View();
        }

        [Transaction]
        public ActionResult Edit(int id) {
            WebSample websample = websampleRepository.Get(id);
            return View(websample);
        }

        //[ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(int id, [ModelBinder(typeof(DefaultModelBinder))] WebSample websample) {
            WebSample websampleToUpdate = websampleRepository.Get(id);
            TransferFormValuesTo(websampleToUpdate, websample);

            if (websampleToUpdate.IsValid()) {
                TempData["message"] = "The websample was successfully updated.";
                return RedirectToAction("Index");
            }
            else {
                websampleRepository.DbContext.RollbackTransaction();
                MvcValidationAdapter.TransferValidationMessagesTo(ViewData.ModelState, 
                    websampleToUpdate.ValidationResults());
                return View(websampleToUpdate);
            }
        }

        private void TransferFormValuesTo(WebSample websampleToUpdate, WebSample websampleFromForm) {
__BEGIN__PROPERTY__
          websampleToUpdate.Property = websampleFromForm.Property;
__END__PROPERTY__
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Delete(int id)
        {
            WebSample websampleToDelete = websampleRepository.Get(id);
            return View(websampleToDelete);
        }

        //[ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteConfirmed(int id) {
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

            TempData["Message"] = resultMessage;
            return RedirectToAction("Index");
        }

    }
}
