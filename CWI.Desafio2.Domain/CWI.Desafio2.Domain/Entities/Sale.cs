using CWI.Desafio2.Domain.Entities.Common;
using System.Collections.Generic;

namespace CWI.Desafio2.Domain.Entities
{
    public class Sale : Entity
    {
        public int SalesmanId { get; set; }

        public ICollection<Item> Items { get; set; }
    }
}
