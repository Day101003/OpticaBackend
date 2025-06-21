using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ProyectoFinal.Models
{
    public class Products
    {
        [Key]
        [Display(Name = "Id del Producto")]
        public int Id { get; set; }

        [Display(Name = "Código")]
        public int Code { get; set; }

        [Display(Name = "Nombre")]
        public string? Name { get; set; }

        [Display(Name = "Precio")]
        public int Price { get; set; }

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "¿Está activo?")]
        public bool IsActive { get; set; } = true;


        
        [ForeignKey("Categories")]
        public int CategoriaId { get; set; }
        public Categories? Categories { get; set; }

        
        [ForeignKey("Images")]
        public int ImageId { get; set; }
        public Images? Images { get; set; }
    }
}
