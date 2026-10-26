using Ecommerce.API.BackgroundServices;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Logistics;
using Ecommerce.Application.Payments;
using Ecommerce.Application.Queues;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Context;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoTransacaoRepository, PedidoTransacaoRepository>();
builder.Services.AddSingleton<IFilaPagamento, FilaPagamento>();
builder.Services.AddHostedService<ProcessarPagamentoWorker>();
builder.Services.AddScoped<PagamentoMock>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddSingleton<IFilaLogistica, FilaLogistica>();
builder.Services.AddHostedService<ProcessarLogisticaWorker>();
builder.Services.AddScoped<LogisticaMock>();
builder.Services.AddScoped<ILogisticaService, LogisticaService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();