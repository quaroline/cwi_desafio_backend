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

        #region Dependencies & Validations
        private static readonly FileManagerAppService _fileManagerAppService = new FileManagerAppService();

        private static Service<Customer> _customerService;
        private static Service<Salesman> _salesmanService;
        private static Service<Sale> _saleService;

        private static List<ValidationResult> results;
        private static List<Tuple<ErrorType, string>> errors;
        #endregion

        #region CTOR
        public Program()
        {
            results = new List<ValidationResult>();
        }
        #endregion

        static void Main(string[] args)
        {
            var files = _fileManagerAppService.ReadFiles();

            foreach (var file in files)
            {
                // For each file, create a new instance to override previous file data.
                _customerService = new Service<Customer>();
                _salesmanService = new Service<Salesman>();
                _saleService = new Service<Sale>();

                errors = new List<Tuple<ErrorType, string>>();

                formatRows(file.Content);

                if (errors.Any())
                    writeErrors(file.Filename);
                else
                {
                    var @return = new string[4];

                    var sales = _saleService.FindAll();

                    var mostExpensiveSale = sales.Aggregate((i, j) => i.Items.Sum(s => s.Price) > j.Items.Sum(s => s.Price) ? i : j);

                    var lowestSalesmanNumbers = sales.GroupBy(s => s.SalesmanId).Select(group => new { SalesmanId = group.Key, Count = group.Count() }).OrderByDescending(s => s.Count).FirstOrDefault();

                    @return[0] = $"No. Customers: {_customerService.FindAll().Count()}";
                    @return[1] = $"No. Salesmen: {_salesmanService.FindAll().Count()}";
                    @return[2] = $"Most expensive sale: {mostExpensiveSale.Id} ($ {mostExpensiveSale.Items.Sum(i => i.Price)})";
                    @return[3] = $"Salesman with lowest numbers: {lowestSalesmanNumbers.SalesmanId} ({lowestSalesmanNumbers.Count} sales)";

                    _fileManagerAppService.WriteFile(@return, file.Filename);
                }
            }
        }

        #region Private Methods
        #region Format Raw Data
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
        #endregion

        #region Create Entities
        private static Customer createCustomer(string name, string cnpj, string businessArea) => new Customer()
        {
            Name = name,
            Cnpj = cnpj,
            BusinessArea = businessArea,
            EntityCode = EntityType.Customer,
        };

        private static Sale createSale(string saleId, string rawItems, string salesmanName)
        {
            if (!int.TryParse(saleId, out int castedSaleId))
            {
                notify(ErrorType.InvalidPrimaryKey, saleId);
                return null;
            }

            if (_saleService.Find(s => s.Id == castedSaleId) != null)
            {
                notify(ErrorType.DuplicateData, saleId);
                return null;
            }

            var salesman = _salesmanService.Find(s => s.Name.ToLower().Trim() == salesmanName);

            if (salesman == null)
            {
                notify(ErrorType.InvalidForeignKey, salesmanName, "No salesman registered for this name.");
                return null;
            }

            if (!rawItems.Any())
            {
                notify(ErrorType.EmptySaleItems, saleId);
                return null;
            }

            var separatedItems = rawItems.Split(",");

            var items = new List<Item>();

            foreach (var item in separatedItems)
            {
                var properties = item.Split("-");

                var itemId = properties[0];

                var quantity = properties[1];

                var price = properties[2];

                if (!int.TryParse(itemId, out int castedItemId))
                {
                    notify(ErrorType.InvalidData, quantity, "Couldn't cast salary into integer.");
                }

                if (!int.TryParse(quantity, out int castedQuantity))
                {
                    notify(ErrorType.InvalidData, quantity, "Couldn't cast salary into integer.");
                }

                if (!decimal.TryParse(price, out decimal castedPrice))
                {
                    notify(ErrorType.InvalidData, price, "Couldn't cast salary into decimal.");
                }

                items.Add(new Item()
                {
                    Quantity = castedQuantity,
                    Price = castedPrice,
                    Id = castedItemId,
                    EntityCode = EntityType.Item
                });
            }

            return new Sale()
            {
                Id = castedSaleId,
                SalesmanId = salesman.Id,
                EntityCode = EntityType.Sale,
                Items = items
            };
        }

        private static Salesman createSalesman(string cpf, string name, string salary)
        {
            if (!decimal.TryParse(salary, out decimal castedSalary))
            {
                notify(ErrorType.InvalidData, salary, "Couldn't cast salary into decimal.");
            }

            return new Salesman()
            {
                Cpf = cpf,
                Name = name,
                Salary = castedSalary,
                EntityCode = EntityType.Salesman
            };
        }
        #endregion

        #region Service
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
        #endregion

        #region Notifications
        private static void notify(ErrorType errorType, string data, string message = null)
        {
            var errorMessage = data + (!string.IsNullOrEmpty(message) ? ". " + message : string.Empty);

            errors.Add(new Tuple<ErrorType, string>(errorType, errorMessage));
        }

        private static void writeErrors(string filename)
        {
            var errorMessages = new string[errors.Count + 1];

            errorMessages[0] = "-- Errors List --";

            for (int i = 1; i < errorMessages.Length; i++)
            {
                errorMessages[i] = $"{i + 1} - {errors[i].Item1}: {errors[i].Item2}";
            }

            _fileManagerAppService.WriteFile(errorMessages, filename);
        }
        #endregion
        #endregion
    }
}