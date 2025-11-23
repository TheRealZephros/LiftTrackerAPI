using System.ComponentModel.DataAnnotations;

namespace api.Dtos.TrainingProgram
{
    public class TrainingProgramGetAllDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}