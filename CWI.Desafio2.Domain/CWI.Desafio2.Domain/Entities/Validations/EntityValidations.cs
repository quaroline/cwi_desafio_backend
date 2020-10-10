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
                RuleFor(e => (e as Customer).Cnpj).Must(BeAValidCpf).WithMessage(Message.INVALID);
                RuleFor(e => (e as Customer).Cnpj).Length(11).WithMessage(Message.EMPTY);

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
                RuleFor(e => (e as Salesman).Cpf).Must(BeAValidCpf).WithMessage(Message.INVALID);
                RuleFor(e => (e as Salesman).Cpf).Length(11).WithMessage(Message.EMPTY);

                // Salary
                RuleFor(e => (e as Salesman).Salary).GreaterThan(0).LessThan(int.MaxValue).WithMessage(Message.INVALID);
            }

            if (typeof(T) == typeof(Sale))
            {
                RuleFor(e => (e as Sale).SalesmanId).NotNull().WithMessage(Message.EMPTY);
                RuleFor(e => (e as Sale).Items).NotNull().NotEmpty().WithMessage(Message.EMPTY);
            }
        }

        public bool BeAValidCpf(string cpf)
        {
            var mt1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var mt2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string temp;
            string digit;

            int sum;
            int mod;

            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            sum = 0;
            temp = cpf.Substring(0, 9);

            for (int i = 0; i < 9; i++)
                sum += int.Parse(temp[i].ToString()) * mt1[i];

            mod = sum % 11 < 2 ? 0 : 11 - (sum % 11);

            digit = mod.ToString();

            temp += digit;

            sum = 0;

            for (int i = 0; i < 10; i++)
                sum += int.Parse(temp[i].ToString()) * mt2[i];

            mod = sum % 11 < 2 ? 0 : 11 - (sum % 11);

            digit += mod.ToString();

            return cpf.EndsWith(digit);
        }
    }
}