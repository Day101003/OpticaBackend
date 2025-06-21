using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models
{
    public class Clients
    {
        [Key]
        [Display(Name = "Id del Cliente")]
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Display(Name = "Teléfono")]
        public string Phone { get; set; }
    }
}
