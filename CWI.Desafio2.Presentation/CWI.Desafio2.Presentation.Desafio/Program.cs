using CWI.Desafio2.Application.FileManager;
using CWI.Desafio2.Domain.Entities;
using CWI.Desafio2.Domain.Entities.Enums;
using CWI.Desafio2.Domain.Services.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CWI.Desafio2.Presentation
{
    class Program
    {
        private static readonly string SEPARATOR = "ç";

        private static readonly FileManagerAppService _fileManagerAppService = new FileManagerAppService();

        private static readonly Service<Customer> _customerService = new Service<Customer>();
        private static readonly Service<Salesman> _salesmanService = new Service<Salesman>();
        private static readonly Service<Sale> _saleService = new Service<Sale>();

        private static List<ValidationResult> results;
        private static List<Tuple<ErrorType, string>> errors;

        public Program()
        {
            results = new List<ValidationResult>();
            errors = new List<Tuple<ErrorType, string>>();
        }

        static void Main(string[] args)
        {
            var files = _fileManagerAppService.ReadFiles();

            foreach (var content in files)
                formatRows(content);
        }

        private static void formatRows(string[] rows)
        {
            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row))
                {
                    notify(ErrorType.EmptyRow, row);
                    continue;
                }

                if (!row.Contains(SEPARATOR))
                {
                    notify(ErrorType.MissingSeparator, row);
                    continue;
                }

                var item = row.Split(SEPARATOR);

                var entityCode = getEntityCode(item[0]);

                if (!entityCode.HasValue)
                {
                    notify(ErrorType.InvalidCode, row);
                    continue;
                }

                var secondParameter = getSecondParameter(entityCode.Value, item[1]);

                if (secondParameter.Item1 == PropertyType.Invalid)
                {
                    notify(ErrorType.InvalidData, row);
                    continue;
                }

                item[0] = entityCode.ToString();
                item[1] = secondParameter.Item2;

                addEntity(entityCode.Value, item);

                if (errors.Any())
                {
                    var errorMessages = new string[errors.Count + 1];

                    errorMessages[0] = "-- Errors List --";

                    for (int i = 1; i < errorMessages.Length; i++)
                    {
                        errorMessages[i] = $"{i + 1} - {errors[i].Item1}: {errors[i].Item2}";
                    }

                    _fileManagerAppService.WriteFile(errorMessages);
                }
                else
                {

                }
            }
        }

        private static int? getEntityCode(string param)
        {
            int parsedCode;

            // Checks if the inserted code is a number and if it is a pre-defined code.
            if (!int.TryParse(param, out parsedCode) || !Enum.IsDefined(typeof(EntityType), parsedCode))
            {
                return null;
            }

            return parsedCode;
        }

        private static Tuple<PropertyType, string> getSecondParameter(int entityCode, string param)
        {
            if (entityCode == (int)EntityType.Salesman) // CPF
            {
                if (param.Length > 13 && (param.Contains(".") || param.Contains("-"))) // char[14]
                {
                    param = param.Replace(".", string.Empty).Replace("-", string.Empty);
                }

                if (param.Length == 13 && param.All(char.IsDigit)) // char[11]
                {
                    return new Tuple<PropertyType, string>(PropertyType.Cpf, param);
                }
            }
            else if (entityCode == (int)EntityType.Customer) // CNPJ
            {
                if (param.Length > 16 && (param.Contains(".") || param.Contains("/") || param.Contains("-"))) // char[18]
                {
                    param = param.Replace(".", string.Empty).Replace("-", string.Empty).Replace("/", string.Empty);
                }

                if (param.Length == 16 && param.All(char.IsDigit)) // char[14]
                {
                    return new Tuple<PropertyType, string>(PropertyType.Cnpj, param);
                }
            }
            else if (entityCode == (int)EntityType.Sale && param.All(char.IsDigit)) // SaleId
            {
                return new Tuple<PropertyType, string>(PropertyType.SaleId, param);
            }

            // None
            return new Tuple<PropertyType, string>(PropertyType.Invalid, param);
        }

        private static void addEntity(int entityCode, string[] item)
        {
            var firstParam = item[1].Trim().ToLower();

            var secondParam = item[2].Trim().ToLower();

            var thirdParam = item[3].Trim().ToLower();

            if (entityCode == (int)EntityType.Customer)
            {
                var customer = createCustomer(firstParam, secondParam, thirdParam);

                var validationContext = new ValidationContext(customer);

                if (Validator.TryValidateObject(customer, validationContext, results))
                {
                    _customerService.Add(customer);
                }
            }
            else if (entityCode == (int)EntityType.Sale)
            {
                var sale = createSale(firstParam, secondParam, thirdParam);

                var validationContext = new ValidationContext(sale);

                if (Validator.TryValidateObject(sale, validationContext, results))
                {
                    _saleService.Add(sale);
                }
            }
            else if (entityCode == (int)EntityType.Salesman)
            {
                var salesman = createSalesman(firstParam, secondParam, thirdParam);

                var validationContext = new ValidationContext(salesman);

                if (Validator.TryValidateObject(salesman, validationContext, results))
                {
                    _salesmanService.Add(salesman);
                }
            }
        }

        private static Customer createCustomer(string name, string cnpj, string businessArea) => new Customer()
        {
            Name = name,
            Cnpj = cnpj,
            BusinessArea = businessArea,
            EntityCode = EntityType.Customer,
        };

        private static Sale createSale(string saleId, string items, string salesmanName)
        {
            if (!int.TryParse(saleId, out int castedSaleId))
            {
                notify(ErrorType.InvalidPrimaryKey, saleId);
            }

            if (_saleService.Find(s => s.Id == castedSaleId) != null)
            {
                notify(ErrorType.DuplicateData, saleId);
            }

            var salesman = _salesmanService.Find(s => s.Name.ToLower().Trim() == salesmanName);

            if (salesman == null)
            {
                notify(ErrorType.InvalidForeignKey, salesmanName);
            }

            return new Sale()
            {
                SaleId = castedSaleId,
                SalesmanId = salesman.Id,
                EntityCode = EntityType.Sale
            };
        }

        private static Salesman createSalesman(string cpf, string name, string salary)
        {
            if (!decimal.TryParse(salary, out decimal castedSalary))
            {
                return null;//retornar erro
            }

            return new Salesman()
            {
                Cpf = cpf,
                Name = name,
                Salary = castedSalary,
                EntityCode = EntityType.Salesman
            };
        }

        private static void notify(ErrorType errorType, string data)
        {
            errors.Add(new Tuple<ErrorType, string>(errorType, data));
        }
    }
}