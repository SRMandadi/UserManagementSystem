using System;
using System.Web.Mvc;
using log4net;
using UserManagementSystem.Data;

namespace UserManagementSystem.Controllers
{
    /// <summary>
    /// This Controller use to manage users
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class UserController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserController));

        // GET: User
        public ActionResult Index()
        {
            var userViewModel = new UserViewModel();

            try
            {
                Log.Info("Trying to load all the users");
                userViewModel.HandleRequest();
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message);

            }
            return View(userViewModel);
        }

        /// <summary>
        /// Does CRUD Operations
        /// </summary>
        /// <param name="userViewModel">The user view model.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(UserViewModel userViewModel)
        {
            try
            {
                Log.Info("Trying to manipulate the UserViewModel"); 
                userViewModel.IsValid = ModelState.IsValid;
                userViewModel.HandleRequest();
                if (userViewModel.IsValid)
                    ModelState.Clear();
                else
                    foreach (var item in userViewModel.ValidationErrors)
                        ModelState.AddModelError(item.Key, item.Value);
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message);
            }

            return View(userViewModel);

        }
    }
}