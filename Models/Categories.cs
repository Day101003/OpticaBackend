using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models
{
    public class Categories
    {
        [Key]
        [Display(Name = "Id de Categoría")]
        public int Id { get; set; }

        [Display(Name = "Nombre de Categoría")]
        public string Name { get; set; }

        [Display(Name = "Ruta")]
        public string Route { get; set; } 
    }
}
