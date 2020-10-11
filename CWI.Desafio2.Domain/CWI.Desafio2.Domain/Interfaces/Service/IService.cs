using CWI.Desafio2.Domain.Entities.Common;
using System;
using System.Collections.Generic;

namespace CWI.Desafio2.Domain.Interfaces.Common
{
    public interface IService<T> where T : Entity
    {
        void Add(T entity);

        T Find(Func<T, bool> predicate);

        IEnumerable<T> FindAll(Func<T, bool> predicate);
    }
}
