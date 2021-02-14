using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using log4net;

namespace UserManagementSystem.Data
{
    public class UserManager
    { 

        public static string FilePath = HostingEnvironment.ApplicationPhysicalPath + WebConfigurationManager.AppSettings["UserFilePath"];
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserManager));

        public List<User> Users;
        public List<KeyValuePair<string, string>> ValidationErrors { get; set; }

        public UserManager()
        {
            Users = GetUsers(null);

            if (Users == null)
            {
                Users = new List<User>();
                Users = CreateInitialUsers();
            }

            ValidationErrors = new List<KeyValuePair<string, string>>();
        }

        public List<User> GetUsers(string searchField = "")
        {
            try
            {
                Log.Info("Trying to get the users from xml");
                var usersLoad = new List<User>();
                var xmlDoc = new XmlDocument();

                 if (File.Exists(FilePath))
                    xmlDoc.Load(FilePath);
                else
                    CreateInitialUsers();

                var nodelist = xmlDoc.SelectNodes("users/user");

                if (nodelist != null)
                    foreach (XmlNode node in nodelist)
                    {
                        var user = new User
                        {
                            Id = Convert.ToInt32(node.SelectSingleNode("id")?.InnerText),
                            Name = Convert.ToString(node.SelectSingleNode("name")?.InnerText),
                            Surname = Convert.ToString(node.SelectSingleNode("surname")?.InnerText),
                            CellphoneNumber = Convert.ToString(node.SelectSingleNode("cellphoneNumber")?.InnerText)
                        };


                        usersLoad.Add(user);
                    }

                if (!string.IsNullOrEmpty(searchField))
                    usersLoad = usersLoad.FindAll(b => b.Name.ToLower().Contains(searchField.ToLower())
                                                       || b.Surname.ToLower().Contains(searchField.ToLower())
                                                       || b.CellphoneNumber.ToString().Contains(searchField.ToLower()));

                return usersLoad;
            }
            catch (Exception e)
            {
                throw new Exception("Error on load users: " + e);
            }
        }

        public User GetUser(int id)
        {
            try
            {
                Log.Info("Get the user by Id from xml");
                User entity;

                entity = Users.Find(p => p.Id == id);

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Error on load user: " + ex);
            }
        }

        public bool Update(User entity)
        {
            try
            {
                Log.Info("Update the user by User model into xml");
                bool ret;

                ret = Validate(entity);

                if (ret)
                {
                    var file = FilePath;
                    var xml = XElement.Load(file);
                    var elements = xml.Elements();

                    foreach (var item in elements)
                        if (item.Element("id")?.Value == entity.Id.ToString())
                        {
                            item.Element("name").Value = entity.Name;
                            item.Element("surname").Value = entity.Surname;
                            item.Element("cellphoneNumber").Value = entity.CellphoneNumber;
                        }

                    xml.Save(file);
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error on update record on database: " + ex);
            }
        }

        public bool Delete(User entity)
        {
            try
            {
                Log.Info("Delete the user from xml");
                var file = FilePath;
                var xml = XElement.Load(file);
                var elements = xml.Elements();

                foreach (var item in elements)
                    if (item.Element("id")?.Value == entity.Id.ToString())
                        item.Remove();
                xml.Save(file);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error on delete record: " + ex);
            }
        }

        public bool Validate(User entity)
        {
            try
            {
                Log.Info("Validate the entity");
                ValidationErrors.Clear();

                if (!ValidateIsNullOrEmpty("Name", entity.Name))
                    ValidateIsValidInput("Name", entity.Name);

                if (!ValidateIsNullOrEmpty("Surname", entity.Surname))
                    ValidateIsValidInput("Surname", entity.Surname);

                if (!ValidateIsNullOrEmpty("CellphoneNumber", entity.CellphoneNumber))
                    if (entity.CellphoneNumber.Any(char.IsLetter))
                        ValidationErrors.Add(new KeyValuePair<string, string>("CellphoneNumber", "Cellphone number must be digits only.")); 

                return ValidationErrors.Count == 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error on validate: " + ex);
            }
        }


        private bool ValidateIsNullOrEmpty(string fieldName, string entityName)
        {
            var isNull = string.IsNullOrEmpty(entityName);
            if (isNull)
                ValidationErrors.Add(new KeyValuePair<string, string>(fieldName, $"Please Enter {fieldName}"));
            return isNull;
        }

        private void ValidateIsValidInput(string fieldName, string entityName)
        {
            if (entityName.Length < 2 || entityName.Length > 100)
                ValidationErrors.Add(new KeyValuePair<string, string>(fieldName, $"{fieldName}  must be greater than 2 characters and less than 100 characters."));

            if (!entityName.Any(char.IsLetter))
                ValidationErrors.Add(new KeyValuePair<string, string>(fieldName, $"Please Enter Valid {fieldName},  Numbers are not allowed."));

        }

        public bool Insert(User entity)
        {
            try
            {
                Log.Info("Insert the User Entity to XML File");

                bool ret;

                ret = Validate(entity);

                if (!ret) return false;
                var lastId = (from b in Users
                              orderby b.Id descending
                              select b.Id).First();

                var newId = lastId + 1;

                var doc = new XmlDocument();
                doc.Load(FilePath);
                XmlNode root = doc.DocumentElement;
                if (root != null)
                {
                    var newNode = root.SelectSingleNode("descendant::users");
                }

                doc.AppendChild(root);

                var user = doc.CreateElement("user");
                root.AppendChild(user);

                var id = doc.CreateElement("id");
                user.AppendChild(id);
                id.InnerText = Convert.ToString(newId);

                var name = doc.CreateElement("name");
                user.AppendChild(name);
                name.InnerText = Convert.ToString(entity.Name);

                var surname = doc.CreateElement("surname");
                user.AppendChild(surname);
                surname.InnerText = Convert.ToString(entity.Surname);

                var cellphoneNumber = doc.CreateElement("cellphoneNumber");
                user.AppendChild(cellphoneNumber);
                cellphoneNumber.InnerText = Convert.ToString(entity.CellphoneNumber);

                doc.Save(FilePath);

                return true;

            }
            catch (Exception ex)
            {
                throw new Exception("Error on insert record: " + ex);
            }
        }

        protected List<User> CreateInitialUsers()
        {
            try
            {
                var doc = new XmlDocument();

                var root = doc.CreateElement("users");
                doc.AppendChild(root);

                var pi = doc.CreateProcessingInstruction(@"xml", "version=\"1.0\" encoding=\"utf-8\"");

                doc.InsertBefore(pi, root);

                Users = new List<User>
                {
                    new User() {Id = 1, Name = "Harvey", Surname = "Specter", CellphoneNumber = "0845651111"},
                    new User() {Id = 2, Name = "Scott", Surname = "Allen", CellphoneNumber = "0845652222"},
                    new User() {Id = 3, Name = "Bucky", Surname = "Roberts", CellphoneNumber = "0845653333"},
                    new User() {Id = 4, Name = "Siva", Surname = "Udadth", CellphoneNumber = "0845654444"},
                    new User() {Id = 5, Name = "Nick", Surname = "Sheldon", CellphoneNumber = "0845655555"},
                    new User() {Id = 6, Name = "Rick", Surname = "Mart", CellphoneNumber = "0845656666"},
                    new User() {Id = 7, Name = "Jared", Surname = "Roy", CellphoneNumber = "0845657777"}
                };


                foreach (var item in Users)
                {
                    var user = doc.CreateElement("user");
                    root.AppendChild(user);

                    var id = doc.CreateElement("id");
                    user.AppendChild(id);
                    id.InnerText = Convert.ToString(item.Id);

                    var name = doc.CreateElement("name");
                    user.AppendChild(name);
                    name.InnerText = Convert.ToString(item.Name);

                    var surname = doc.CreateElement("surname");
                    user.AppendChild(surname);
                    surname.InnerText = Convert.ToString(item.Surname);

                    var cellphoneNumber = doc.CreateElement("cellphoneNumber");
                    user.AppendChild(cellphoneNumber);
                    cellphoneNumber.InnerText = Convert.ToString(item.CellphoneNumber);
                }


                doc.Save(FilePath);


                return Users;
            }
            catch (Exception ex)
            {
                throw new Exception("Error on load initial records: " + ex);
            }
        }

        public List<User> RestoreDefault()
        {
            try
            {
                var usersLoad = new List<User>();
                var xmlDoc = new XmlDocument();

                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                    CreateInitialUsers();
                }
                else
                {
                    CreateInitialUsers();
                }

                var nodelist = xmlDoc.SelectNodes("users/user");

                if (nodelist != null)
                    foreach (XmlNode node in nodelist)
                    {
                        var user = new User
                        {
                            Id = Convert.ToInt32(node.SelectSingleNode("id")?.InnerText),
                            Name = Convert.ToString(node.SelectSingleNode("name")?.InnerText),
                            Surname = Convert.ToString(node.SelectSingleNode("surname")?.InnerText),
                            CellphoneNumber = Convert.ToString(node.SelectSingleNode("cellphoneNumber")?.InnerText)
                        };


                        usersLoad.Add(user);
                    }

                return usersLoad;
            }
            catch (Exception e)
            {
                throw new Exception("Error on restore records: " + e);
            }
        }
    }
}