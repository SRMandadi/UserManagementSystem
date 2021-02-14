using System;
using System.Collections.Generic;
using UserManagementSystem.Common;

namespace UserManagementSystem.Data
{
    public class UserViewModel : ViewModelBase
    {

        public UserViewModel()
        {
            Users = new List<User>();
            SearchField = "";
            Entity = new User();
        }

        public List<User> Users { get; set; }
        public User Entity { get; set; }
        public string SearchField { get; set; }

        protected override void Init()
        {
            Users = new List<User>();
            Entity = new User();

            base.Init();
        }

        public override void HandleRequest()
        {
            switch (EventCommand.ToLower())
            {
                case "list":
                case "search":
                    Get(SearchField);
                    break;
                case "add":
                    Add();
                    break;
                case "edit":
                    Edit();
                    break;
                case "delete":
                    ResetSearch();
                    Delete();
                    break;
                case "save":
                    Save();
                    Get();
                    break;
                case "cancel":
                    ListMode();
                    Get();
                    break;
                case "resetsearch":
                    ResetSearch();
                    Get();
                    break;
                case "restore":
                    RestoreDefault();
                    Get();
                    break;
            }
        }

        protected override void Add()
        {
            IsValid = true;

            Entity = new User {Name = string.Empty, Surname = string.Empty, CellphoneNumber = string.Empty};

            base.Add();
        }

        protected override void Edit()
        {
            var userManager = new UserManager();

            Entity = userManager.GetUser(Convert.ToInt32(EventArgument));

            base.Edit();
        }

        protected override void Save()
        {
            var userManager = new UserManager();

            if (Mode == "Add")
                userManager.Insert(Entity);
            else
                userManager.Update(Entity);

            ValidationErrors = userManager.ValidationErrors;

            base.Save();
        }

        protected override void Delete()
        {
            var userManager = new UserManager();

            Entity = new User {Id = Convert.ToInt32(EventArgument)};


            userManager.Delete(Entity);

            Get();

            base.Delete();
        }

        protected override void ResetSearch()
        {
            SearchField = "";

            base.ResetSearch();
        }

        protected override void Get(string searchField = "")
        {
            var userManager = new UserManager();

            Users = userManager.GetUsers(searchField);

            base.Get();
        }

        protected override void RestoreDefault()
        {
            var userManager = new UserManager();

            Users = userManager.RestoreDefault();

            base.Get();
        }
    }
}