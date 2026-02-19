namespace FaturaFlow.Domain.ValueObjects
{
    public class Price
    {
        public decimal Value { get; init; }
        public Price(decimal value)
        {
            if (value < 0) throw new Exception("O preço não pode ser negativo.");
            Value = value;
        }
    }
}
