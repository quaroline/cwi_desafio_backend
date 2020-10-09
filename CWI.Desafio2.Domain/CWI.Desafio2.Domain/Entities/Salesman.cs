using CWI.Desafio2.Domain.Entities.Common;

namespace CWI.Desafio2.Domain.Entities
{
    public class Salesman : Entity
    {
        [Required]
        [StringLength(100)]
        public string Cpf { get; set; }

        public string Name { get; set; }

        public decimal Salary { get; set; }
    }
}
