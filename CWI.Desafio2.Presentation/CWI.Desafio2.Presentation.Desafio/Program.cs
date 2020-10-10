using CWI.Desafio2.Domain.Entities;
using CWI.Desafio2.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CWI.Desafio2.Presentation
{
    class Program
    {
        private static readonly string SEPARATOR = "ç";

        private static List<Customer> customers;

        private static List<Salesman> salesmen;

        private static List<Sale> sales;

        public Program()
        {
            customers = new List<Customer>();
            salesmen = new List<Salesman>();
            sales = new List<Sale>();
        }

        static void Main(string[] args)
        {
            var rows = new string[] {
                "001ç1234567891234çPedroç50000",
                "001ç3245678865434çPauloç40000.99",
                "002ç2345675434544345çJose da SilvaçRural",
                "002ç2345675433444345çEduardo PereiraçRural",
                "003ç10ç[1-10-100,2-30-2.50,3-40-3.10]çPedro",
                "003ç08ç[1-34-10,2-33-1.50,3-40-0.10]çPaulo"
            };

            var errors = new List<Tuple<ErrorType, string>>();

            var results = new List<ValidationResult>();

            foreach (var row in rows)
            {
                if (!string.IsNullOrEmpty(row))
                {
                    errors.Add(new Tuple<ErrorType, string>(ErrorType.EmptyRow, string.Empty));
                    continue;
                }

                if (!row.Contains(SEPARATOR))
                {
                    errors.Add(new Tuple<ErrorType, string>(ErrorType.MissingSeparator, row));
                    continue;
                }

                var item = row.Split(SEPARATOR);

                var entityCode = format1stParameter(item[0]);

                if (!entityCode.HasValue)
                {
                    errors.Add(new Tuple<ErrorType, string>(ErrorType.InvalidCode, row));
                    continue;
                }

                var secondParameter = format2ndParameter(entityCode.Value, item[1]);

                if (secondParameter.Item1 == PropertyType.Invalid)
                {
                    errors.Add(new Tuple<ErrorType, string>(ErrorType.InvalidData, row));
                    continue;
                }

                item[0] = entityCode.ToString();
                item[1] = secondParameter.Item2;

                addEntity(entityCode.Value, item);

                /*
                    Casos não esperados:
                     - cpf etc errados
                     - com mais parâmetros que o esperado
                     - duas linhas na mesma
                     - salario com , e nao .
                */
            }
        }

        private static int? format1stParameter(string param)
        {
            int parsedCode;

            // Checks if the inserted code is a number and if it is a pre-defined code.
            if (!int.TryParse(param, out parsedCode) || !Enum.IsDefined(typeof(EntityType), param))
            {
                return null;
            }

            return parsedCode;
        }

        private static Tuple<PropertyType, string> format2ndParameter(int entityCode, string param)
        {
            // CPF
            if (entityCode == (int)EntityType.Salesman)
            {
                if (param.Length == 14 && (param.Contains(".") || param.Contains("-")))
                {
                    param = param.Replace(".", string.Empty).Replace("-", string.Empty);
                }

                if (param.Length == 11 && param.All(char.IsDigit))
                {
                    return new Tuple<PropertyType, string>(PropertyType.Cpf, param);
                }
            }
            else if (entityCode == (int)EntityType.Customer)
            {
                if (param.Length == 18 && (param.Contains(".") || param.Contains("/") || param.Contains("-")))
                {
                    param = param.Replace(".", string.Empty).Replace("-", string.Empty).Replace("/", string.Empty);
                }

                if (param.Length == 14 && param.All(char.IsDigit))
                {
                    return new Tuple<PropertyType, string>(PropertyType.Cnpj, param);
                }
            }
            else if (entityCode == (int)EntityType.Sale && param.All(char.IsDigit))
            {
                return new Tuple<PropertyType, string>(PropertyType.SaleId, param);
            }

            // None
            return new Tuple<PropertyType, string>(PropertyType.Invalid, param);
        }

        private static void addEntity(int entityCode, string[] item)
        {
            if (entityCode == (int)EntityType.Customer)
            {
                var customer = addCustomer(item);

                var validationContext = new ValidationContext(customer);

                if (Validator.TryValidateObject(customer, validationContext, results))
                {

                }
            }
            else if (entityCode == (int)EntityType.Sale)
            {
                var sale = addSale(item);

                var validationContext = new ValidationContext(sale);

                if (Validator.TryValidateObject(sale, validationContext, results))
                {

                }
            }
            else if (entityCode == (int)EntityType.Salesman)
            {
                var salesman = addSalesman(item);

                var validationContext = new ValidationContext(salesman);

                if (Validator.TryValidateObject(salesman, validationContext, results))
                {

                }
            }
        }

        private static Customer addCustomer(string[] rawCustomer)
        {
            var cnpj = rawCustomer[1];

            var name = rawCustomer[2];

            var businessArea = rawCustomer[3];

            return new Customer()
            {
                Name = name,
                Cnpj = cnpj,
                BusinessArea = businessArea,
                EntityCode = EntityType.Customer,
            };
        }

        private static Sale addSale(string[] rawSale)
        {
            var saleId = rawSale[1];

            var items = rawSale[2];

            var salesmanName = rawSale[3];

            if (!int.TryParse(saleId, out int castedSaleId))
            {
                return null;
            }

            return new Sale()
            {
                SaleId = castedSaleId,
                EntityCode = EntityType.Sale
            };
        }

        private static Salesman addSalesman(string[] rawSalesman)
        {
            var cpf = rawSalesman[1];

            var name = rawSalesman[2];

            var salary = rawSalesman[3];

            if (!decimal.TryParse(salary, out decimal castedSalary))
            {
                return null;
            }

            return new Salesman()
            {
                Cpf = cpf,
                Name = name,
                Salary = castedSalary,
                EntityCode = EntityType.Salesman
            };
        }
    }
}