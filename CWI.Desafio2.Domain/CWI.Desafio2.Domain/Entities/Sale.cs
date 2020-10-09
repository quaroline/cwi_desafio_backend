using CWI.Desafio2.Domain.Entities.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CWI.Desafio2.Domain.Entities
{
    public class Sale : Entity
    {
        [Required]
        public int SaleId { get; set; }

        [Required]
        public int SalesmanId { get; set; }

        public ICollection<Item> Items { get; set; }
    }
}
