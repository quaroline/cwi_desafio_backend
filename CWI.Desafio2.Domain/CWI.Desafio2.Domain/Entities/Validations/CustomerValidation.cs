using FluentValidation;

namespace CWI.Desafio2.Domain.Entities.Validations
{
    public static class Message
    {
        public const string EMPTY = "\"{PropertyName}\" cannot be empty.";

        public const string INVALID = "\"{PropertyName}\" is not valid.";
    }

    public class EntityValidation<T> : AbstractValidator<T> where T : class
    {
        public EntityValidation()
        {
            if (typeof(T) == typeof(Customer))
            {
                RuleFor(e => (e as Customer).Cpf).NotEmpty().WithMessage(Message.EMPTY);
                RuleFor(e => typeof(T).GetProperty("Cpf").GetValue(e).ToString()).Must(CustomValidators.IsCpfValid).WithMessage(Message.INVALID);
            }
        }
    }

    public static class CustomValidators
    {
        public static bool IsCpfValid(string cpf)
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