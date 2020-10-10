using CWI.Desafio2.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace CWI.Desafio2.Domain.Entities
{
    public class Salesman : Entity
    {
        public string Cpf { get; set; }

        public string Name { get; set; }

        public decimal Salary { get; set; }
    }
}
