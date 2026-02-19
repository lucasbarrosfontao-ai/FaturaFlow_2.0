namespace FaturaFlow.Domain.ValueObjects
{
    public class VatRate
    {
        public decimal Value { get; init; }
        public VatRate(decimal value)
        {
            if (value < 0 || value > 100) 
                throw new Exception("A taxa de IVA deve estar entre 0 e 100.");
            Value = value;
        }
    }
}
