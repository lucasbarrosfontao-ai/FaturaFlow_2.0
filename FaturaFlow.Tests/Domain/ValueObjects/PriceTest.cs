using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.ValueObjects;

public class PriceTests
{
    [Fact]
    public void Nao_Deve_Aceitar_Preco_Negativo()
    {
        Action acao = () => new Price(-0.01m);
        acao.Should().Throw<Exception>();
    }
}
public class VatRateTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Deve_Lancar_Erro_Se_Fora_Do_Intervalo_0_A_100(decimal taxaInvalida)
    {
        Action acao = () => new VatRate(taxaInvalida);
        acao.Should().Throw<Exception>();
    }
}