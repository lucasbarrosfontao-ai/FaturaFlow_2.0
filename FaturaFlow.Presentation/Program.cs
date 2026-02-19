using FaturaFlow.Application.Services;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Infrastructure.Data;   
using FaturaFlow.Infrastructure.Repositories;
using FaturaFlow.Infrastructure.Services;
using FaturaFlow.Presentation.Components;
using FaturaFlow.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar o Banco de Dados (MySQL via Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/root/.aspnet/DataProtection-Keys"));
// 2. Registrar Repositórios (Domain Interface -> Infra Implementation)
// Certifique-se que o nome das classes na Infra é exatamente este:
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();

// 3. Registrar Serviços de Infraestrutura
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>(); // BCrypt
builder.Services.AddScoped<IMessageService, RabbitMQService>(); // RabbitMQ

// 4. Registrar Serviços de Aplicação (Lógica de Negócio)
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<SalesService>();

// 5. Registrar Estado da UI (Sessão)
builder.Services.AddScoped<UserSession>();

// 6. Configurações do Blazor (Padrão .NET 8)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configuração do Pipeline (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

// Mapear os componentes do Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();