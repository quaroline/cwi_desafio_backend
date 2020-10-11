using CWI.Desafio2.Domain.Entities;
using CWI.Desafio2.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CWI.Desafio2.Domain.Services.Common
{
    public class Service<T> where T : Entity
    {
        #region DbSet/Context wannabe
        public static List<Customer> Customers { get; set; }

        public static List<Sale> Sales { get; set; }

        public static List<Salesman> Salesmen { get; set; }
        #endregion

        public Service()
        {
            Customers = new List<Customer>();
            Sales = new List<Sale>();
            Salesmen = new List<Salesman>();
        }

        public void Add(T entity)
        {
            if (typeof(T) == typeof(Customer))
            {
                Customers.Add(entity as Customer);
            }
            else if (typeof(T) == typeof(Salesman))
            {
                Salesmen.Add(entity as Salesman);
            }
            else if (typeof(T) == typeof(Sale))
            {
                Sales.Add(entity as Sale);
            }
        }

        public T Find(Func<T, bool> predicate)
        {
            if (typeof(T) == typeof(Customer))
            {
                return Customers.FirstOrDefault((Func<Customer, bool>)predicate) as T;
            }
            else if (typeof(T) == typeof(Salesman))
            {
                return Salesmen.FirstOrDefault((Func<Salesman, bool>)predicate) as T;
            }
            else if (typeof(T) == typeof(Sale))
            {
                return Sales.FirstOrDefault((Func<Sale, bool>)predicate) as T;
            }

            return null;
        }

        public IEnumerable<T> FindAll(Func<T, bool> predicate)
        {

            if (typeof(T) == typeof(Customer))
            {
                return Customers.Where((Func<Customer, bool>)predicate) as IEnumerable<T>;
            }
            else if (typeof(T) == typeof(Salesman))
            {
                return Salesmen.Where((Func<Salesman, bool>)predicate) as IEnumerable<T>;
            }
            else if (typeof(T) == typeof(Sale))
            {
                return Sales.Where((Func<Sale, bool>)predicate) as IEnumerable<T>;
            }

            return null;
        }
    }
}
