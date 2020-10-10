using CWI.Desafio2.Domain.Entities.Common;

namespace CWI.Desafio2.Domain.Entities
{
    public class Customer : Entity
    {
        public string Cnpj { get; set; }

        public string Name { get; set; }

        public string BusinessArea { get; set; }
    }
}
