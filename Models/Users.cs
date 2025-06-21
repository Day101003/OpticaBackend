using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    public class Users
    {
        [Key]
        [Display(Name = "Id del Usuario")]
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Teléfono")]
        public string Phone { get; set; }

        [Display(Name = "Fecha de Registro")]
        [DataType(DataType.Date)]
        public DateTime DateRegister { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("Image")]
        [Display(Name = "Imagen")]
        public int ImageId { get; set; }
        public Images Images { get; set; }
    }
}
