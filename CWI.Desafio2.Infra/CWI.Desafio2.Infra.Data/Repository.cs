using CWI.Desafio2.Domain.Entities;
using System.Collections.Generic;

namespace CWI.Desafio2.Infra.Data.Repositories.Common
{
    public static class Repository
    {
        #region DbSet/Context wannabe
        public static List<Customer> Customers { get; set; }

        public static List<Sale> Sales { get; set; }

        public static List<Salesman> Salesmen { get; set; }
        #endregion
    }
}