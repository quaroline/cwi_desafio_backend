using CWI.Desafio2.Domain.Entities.Common;

namespace CWI.Desafio2.Domain.Entities
{
    public class Item : Entity
    {
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal FinalPrice => Price * Quantity;
    }
}
