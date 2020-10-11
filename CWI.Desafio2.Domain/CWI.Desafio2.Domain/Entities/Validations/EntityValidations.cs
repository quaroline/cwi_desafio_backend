using FluentValidation;

namespace CWI.Desafio2.Domain.Entities.Validations
{
    public static class Message
    {
        public const string EMPTY = "\"{PropertyName}\" cannot be empty.";

        public const string INVALID = "\"{PropertyName}\" is not valid.";

        public const string LENGTH_OUTSIDE_RANGE = "\"{PropertyName}\" must be between {MinLength} and {MaxLength}.";
    }

    public class EntityValidation<T> : AbstractValidator<T> where T : class
    {
        public EntityValidation()
        {
            if (typeof(T) == typeof(Customer))
            {
                // CPF
                RuleFor(e => (e as Customer).Cnpj).NotNull().NotEmpty().WithMessage(Message.EMPTY);
                RuleFor(e => (e as Customer).Cnpj).Length(16).WithMessage(Message.EMPTY); // 14

                // Name
                RuleFor(e => (e as Customer).Name).NotNull().NotEmpty().WithMessage(Message.EMPTY);
                RuleFor(e => (e as Customer).Name).Length(3, 100).WithMessage(Message.LENGTH_OUTSIDE_RANGE);

                // EntityCode
                RuleFor(e => (e as Customer).EntityCode).NotEmpty().WithMessage(Message.EMPTY);

            }

            if (typeof(T) == typeof(Salesman))
            {
                // CPF
                RuleFor(e => (e as Salesman).Cpf).NotNull().NotEmpty().WithMessage(Message.EMPTY);
                RuleFor(e => (e as Salesman).Cpf).Length(13).WithMessage(Message.EMPTY); // 11

                // Salary
                RuleFor(e => (e as Salesman).Salary).GreaterThan(0).LessThan(int.MaxValue).WithMessage(Message.INVALID);

                // EntityCode
                RuleFor(e => (e as Salesman).EntityCode).NotEmpty().WithMessage(Message.EMPTY);
            }

            if (typeof(T) == typeof(Sale))
            {
                RuleFor(e => (e as Sale).SalesmanId).NotNull().WithMessage(Message.EMPTY);
                RuleFor(e => (e as Sale).Items).NotNull().NotEmpty().WithMessage(Message.EMPTY);

                // EntityCode
                RuleFor(e => (e as Sale).EntityCode).NotEmpty().WithMessage(Message.EMPTY);
            }
        }
    }
}