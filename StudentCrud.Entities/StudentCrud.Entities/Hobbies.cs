using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace StudentCrud.Entities
{
    public class Hobbies
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HobbyId { get; set; }
        [Required]
        [MaxLength(50)]
        public string HobbyName { get; set;}
        [NotMapped]
        public bool IsChecked { get; set; }

    }
}
