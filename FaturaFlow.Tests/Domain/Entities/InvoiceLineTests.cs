using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;

namespace FaturaFlow.Tests.Domain.Entities
{
    public class InvoiceLineTests
    {
        [Fact]
        public void Deve_Calcular_Subtotal_E_Iva_Corretamente()
        {
            var invoiceId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 2;
            var unitPrice = new Price(100.00m); 
            var vatRate = new VatRate(23.0m);   

            var line = new InvoiceLine(invoiceId, productId, quantity, unitPrice, vatRate);

            line.Subtotal.Should().Be(200.00m);
            line.VatAmount.Should().Be(46.00m);
        }
    }
}