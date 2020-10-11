using CWI.Desafio2.Domain.Entities;
using CWI.Desafio2.Domain.Entities.Common;
using CWI.Desafio2.Domain.Interfaces.Common;
using System;
using System.Collections.Generic;

namespace CWI.Desafio2.Domain.Services.Common
{
    public class Service<T> : IService<T> where T : Entity
    {
        #region DbSet/Context wannabe
        public static List<Customer> Customers { get; set; }

        public static List<Sale> Sales { get; set; }

        public static List<Salesman> Salesmen { get; set; }
        #endregion

        public void Add(T entity)
        {
            if (typeof(T) == typeof(Customer))
            {
                Customers.Add(entity as Customer);
            }
            else if (typeof(T) == typeof(Sale))
            {
                Sales.Add(entity as Sale);
            }
            else if (typeof(T) == typeof(Salesman))
            {
                Salesmen.Add(entity as Salesman);
            }
        }

        public T Find(Func<T, bool> predicate)
        {
            if (typeof(T) == typeof(Customer))
            {
                Customers.Add(entity as Customer);
            }
            else if (typeof(T) == typeof(Sale))
            {
                Sales.Add(entity as Sale);
            }
            else if (typeof(T) == typeof(Salesman))
            {
                Salesmen.Add(entity as Salesman);
            }
        }

        public IEnumerable<T> FindAll(Func<T, bool> predicate) => _repository.FindAll(predicate);
    }
}
