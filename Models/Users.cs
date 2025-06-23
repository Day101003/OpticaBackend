using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models
{
    public class Users
    {
        [Key]
        [Display(Name = "Id del Usuario")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Teléfono")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Fecha de Registro")]
        [DataType(DataType.Date)]
        public DateTime DateRegister { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Ruta Imagen de Perfil")]
        public string ImagePath { get; set; } = "assets/img/FotoPerfil/default.jpg";
    }
}
