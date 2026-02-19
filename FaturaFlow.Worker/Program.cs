using FaturaFlow.Infrastructure.Data;
using FaturaFlow.Infrastructure.Repositories;
using FaturaFlow.Infrastructure.Services;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

builder.Services.AddScoped<IPdfService, PdfGeneratorService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();