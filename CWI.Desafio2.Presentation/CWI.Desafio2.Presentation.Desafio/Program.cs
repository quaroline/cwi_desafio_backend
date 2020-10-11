using CWI.Desafio2.Application.FileManager;
using CWI.Desafio2.Application.FileManager.ViewModels;
using CWI.Desafio2.Domain.Entities;
using CWI.Desafio2.Domain.Entities.Enums;
using CWI.Desafio2.Domain.Entities.Validations;
using CWI.Desafio2.Domain.Services.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
        private static List<Tuple<ErrorType, string>> errors = new List<Tuple<ErrorType, string>>();
        #endregion

        #region CTOR
        public Program()
        {
            results = new List<ValidationResult>();
        }
        #endregion

        static void Main(string[] args)
        {
            try
            {
                var watcher = new FileSystemWatcher(_fileManagerAppService.HOMEPATH + "\\in\\", "*.csv")
                {
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
                };

                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                Console.WriteLine($" - IN folder:\t\"{_fileManagerAppService.HOMEPATH + "in\\"}\".");
                Console.WriteLine($" - OUT folder:\t\"{_fileManagerAppService.HOMEPATH + "out\\"}\".\n");

                Console.ReadKey();
            }
            catch (Exception e)
            {
                notify(ErrorType.Undefined, string.Empty, e.Message);
                writeErrors("Program");
            }
        }

        #region Event Handlers
        public static void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("\nFile \"{0}\" has been {1}", e.Name, e.ChangeType.ToString().ToLower());

            var file = _fileManagerAppService.ReadFile(e.Name);

            var message = "File \"" + e.Name + "\" has{0} been processed";

            if (file != null && file.Content.Any())
            {
                formatFile(file);
                Console.WriteLine(message, string.Empty);
            }
            else
            {
                notify(ErrorType.EmptyRow, e.Name, "No content.");
                Console.WriteLine(message, " NOT");
            }
        }
        public static void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine("File \"{0}\" has been renamed to \"{1}\", and it won't be processed again.", e.OldName, e.Name);
        }
        #endregion

        #region Private Methods
        #region Format Raw Data
        private static void formatRows(string[] rows)
        {
            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row))
                {
                    notify(ErrorType.EmptyRow, row);
                    return;
                }

                if (!row.Contains(SEPARATOR))
                {
                    notify(ErrorType.MissingSeparator, row);
                    return;
                }

                var item = row.Split(SEPARATOR);

                var entityCode = getEntityCode(item[0]);

                if (!entityCode.HasValue)
                {
                    notify(ErrorType.InvalidCode, row);
                    return;
                }

                var secondParameter = getSecondParameter(entityCode.Value, item[1]);

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

        private static void formatFile(FileViewModel file)
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

                var mostExpensiveSale = sales.Aggregate((i, j) => i.Items.Sum(s => s.FinalPrice) >= j.Items.Sum(s => s.FinalPrice) ? i : j);

                var cheaperSale = sales.Aggregate((i, j) => i.Items.Sum(s => s.FinalPrice) <= j.Items.Sum(s => s.FinalPrice) ? i : j);

                var bestSalesman = _salesmanService.Find(s => s.Id == mostExpensiveSale.SalesmanId);

                var worstSalesman = _salesmanService.Find(s => s.Id == cheaperSale.SalesmanId);

                if (mostExpensiveSale == null || cheaperSale == null || bestSalesman == null || worstSalesman == null)
                {
                    notify(ErrorType.Undefined, ErrorType.Undefined.ToString(), "One or more entities hadn't returned corretly.");
                }

                @return[0] = $"No. Customers: {_customerService.FindAll().Count()}";
                @return[1] = $"No. Salesmen: {_salesmanService.FindAll().Count()}";
                @return[2] = $"Most expensive sale Id: {mostExpensiveSale.Id} (${mostExpensiveSale.Items.Sum(i => i.FinalPrice)} by {bestSalesman.Name})";
                @return[3] = $"Salesman with lowest numbers: {worstSalesman.Name} (${cheaperSale.Items.Sum(i => i.FinalPrice)})";

                _fileManagerAppService.WriteFile(@return, file.Filename);
            }
        }
        #endregion

        #region Create Entities
        private static Customer createCustomer(string cnpj, string name, string businessArea) => new Customer()
        {
            Id = _customerService.FindAll().Select(s => s.Id).DefaultIfEmpty(0).Max() + 1,
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

            var separatedItems = rawItems.Replace("[", string.Empty).Replace("]", string.Empty).Split(",");

            var items = new List<Item>();

            foreach (var item in separatedItems)
            {
                var properties = item.Split("-");

                var itemId = properties[0];

                var quantity = properties[1];

                var price = properties[2];

                if (!int.TryParse(itemId, out int castedItemId))
                {
                    notify(ErrorType.InvalidData, quantity, "Couldn't cast item ID into integer.");
                }

                if (!int.TryParse(quantity, out int castedQuantity))
                {
                    notify(ErrorType.InvalidData, quantity, "Couldn't cast quantity into integer.");
                }

                if (!decimal.TryParse(price, out decimal castedPrice))
                {
                    notify(ErrorType.InvalidData, price, "Couldn't cast price into decimal.");
                }

                var itemOriginal = items.FirstOrDefault(i => i.Id == castedItemId);

                // If there's already an item with this ItemId, add new values to previous ones.
                if (itemOriginal != null)
                {
                    itemOriginal.Price += castedPrice;
                    itemOriginal.Quantity += castedQuantity;
                }
                else
                {
                    items.Add(new Item()
                    {
                        Quantity = castedQuantity,
                        Price = castedPrice,
                        Id = castedItemId,
                        EntityCode = EntityType.Item
                    });
                }
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
                Id = _salesmanService.FindAll().Select(s => s.Id).DefaultIfEmpty(0).Max() + 1,
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

                if (customer != null)
                {
                    var validator = new CustomerValidator();

                    var result = validator.Validate(customer);

                    if (result.IsValid)
                    {
                        _customerService.Add(customer);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            notify(ErrorType.InvalidData, error.ErrorMessage);
                        }
                    }
                }
            }
            else if (entityCode == (int)EntityType.Sale)
            {
                var sale = createSale(firstParam, secondParam, thirdParam);

                if (sale != null)
                {
                    var validator = new SaleValidator();

                    var result = validator.Validate(sale);

                    if (result.IsValid)
                    {
                        _saleService.Add(sale);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            notify(ErrorType.InvalidData, error.ErrorMessage);
                        }
                    }
                }
            }
            else if (entityCode == (int)EntityType.Salesman)
            {
                var salesman = createSalesman(firstParam, secondParam, thirdParam);

                if (salesman != null)
                {
                    var validator = new SalesmanValidator();

                    var result = validator.Validate(salesman);

                    if (result.IsValid)
                    {
                        _salesmanService.Add(salesman);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            notify(ErrorType.InvalidData, null, error.ErrorMessage);
                        }
                    }
                }
            }
        }
        #endregion

        #region Notifications
        private static void notify(ErrorType errorType, string data, string message = null)
        {
            var errorMessage = string.Empty;

            if (!string.IsNullOrEmpty(data))
            {
                errorMessage = data;

                if (!string.IsNullOrEmpty(message))
                {
                    errorMessage += $". {message}";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                {
                    errorMessage = message;
                }
            }

            errors.Add(new Tuple<ErrorType, string>(errorType, errorMessage));
        }

        private static void writeErrors(string filename)
        {
            Console.WriteLine("-- Errors List --");

            for (int i = 0; i < errors.Count; i++)
            {
                Console.WriteLine($"{i} - {errors[i].Item1}: {errors[i].Item2}");
            }
        }
        #endregion
        #endregion
    }
}