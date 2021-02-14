using System.ComponentModel.DataAnnotations;

namespace UserManagementSystem.Data
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        [Display(Name = "Cellphone Number")] public string CellphoneNumber { get; set; }
    }
}