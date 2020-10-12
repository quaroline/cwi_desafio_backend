using FluentValidation;
using System.Text.RegularExpressions;

namespace CWI.Desafio2.Domain.Entities.Validations
{
    public static class Message
    {
        public const string EMPTY = "\"{PropertyName}\" cannot be empty.";

        public const string INVALID = "\"{PropertyName}\" is not valid.";

        public const string LENGTH_OUTSIDE_RANGE = "\"{PropertyName}\" must be between {MinLength} and {MaxLength} characters long.";

        public const string LENGTH = "\"{PropertyName}\" must be {TotalLength} characters long.";

        public const string ONLY_NUMBERS = "\"{PropertyName}\" must contain only numbers.";
    }

    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            // CPF
            RuleFor(e => e.Cnpj).NotNull().NotEmpty().WithMessage(Message.EMPTY);
            RuleFor(x => x.Cnpj).Must(Utils.BeANumber).WithMessage(Message.ONLY_NUMBERS);

            // Name
            RuleFor(e => e.Name).NotNull().NotEmpty().WithMessage(Message.EMPTY);
            RuleFor(e => e.Name).Length(3, 100).WithMessage(Message.LENGTH_OUTSIDE_RANGE);

            // EntityCode
            RuleFor(e => e.EntityCode).NotEmpty().WithMessage(Message.EMPTY);
        }
    }

    public class SaleValidator : AbstractValidator<Sale>
    {
        public SaleValidator()
        {
            RuleFor(e => e.SalesmanId).NotNull().WithMessage(Message.EMPTY);
            RuleFor(e => e.Items).NotNull().NotEmpty().WithMessage(Message.EMPTY);

            // EntityCode
            RuleFor(e => e.EntityCode).NotEmpty().WithMessage(Message.EMPTY);
        }
    }

    public class SalesmanValidator : AbstractValidator<Salesman>
    {
        public SalesmanValidator()
        {
            // CPF
            RuleFor(e => e.Cpf).NotNull().NotEmpty().WithMessage(Message.EMPTY);
            RuleFor(x => x.Cpf).Must(Utils.BeANumber).WithMessage(Message.ONLY_NUMBERS);

            // Salary
            RuleFor(e => e.Salary).GreaterThan(0).LessThan(int.MaxValue).WithMessage(Message.INVALID);

            // EntityCode
            RuleFor(e => (e as Salesman).EntityCode).NotEmpty().WithMessage(Message.EMPTY);
        }
    }
   
    public static class Utils
    {
        public static bool BeANumber(string data) => new Regex("^\\d+$").IsMatch(data);
    }
}