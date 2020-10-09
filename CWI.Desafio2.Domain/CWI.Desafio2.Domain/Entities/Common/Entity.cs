using CWI.Desafio2.Domain.Entities.Enums;

namespace CWI.Desafio2.Domain.Entities.Common
{
    public abstract class Entity
    {
        public EntityType EntityCode { get; set; }

        public int Id { get; set; }
    }
}
