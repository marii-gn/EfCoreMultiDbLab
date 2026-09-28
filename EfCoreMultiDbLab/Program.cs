using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EfCoreMultiDbLab.Data;
using EfCoreMultiDbLab.Services;


Console.OutputEncoding = System.Text.Encoding.UTF8;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string provider = config["DatabaseProvider"] ?? "Sqlite";
Console.WriteLine($"\n>>> ПОТОЧНА СУБД: {provider.ToUpper()} <<<\n");

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    optionsBuilder.UseSqlite(config.GetConnectionString("Sqlite"));
}
else if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
{
    optionsBuilder.UseNpgsql(config.GetConnectionString("PostgreSql"));
}

await using var context = new AppDbContext(optionsBuilder.Options);

Console.WriteLine("Перевірка та створення структури БД...");
await context.Database.EnsureCreatedAsync();

var service = new OrderService(context);

Console.WriteLine("1. Створення записів...");
var customer = await service.CreateCustomerAsync("Марія Тестова", $"maria_{Guid.NewGuid().ToString()[..4]}@test.com");
var order = await service.CreateOrderAsync(customer.Id, new()
{
    ("Книга з алгоритмів", 1, 600m),
    ("Кава зернова", 2, 250m)
});
Console.WriteLine($"Клієнт: {customer.FullName}, Замовлення #{order.Id} на суму: {order.TotalAmount} грн");

Console.WriteLine("\n2. Вибірка даних (Include + Where + OrderBy):");
var orders = await service.GetOrdersFilteredAsync(100m);
foreach (var o in orders)
{
    Console.WriteLine($"Замовлення #{o.Id} від {o.Customer.FullName} | Разом: {o.TotalAmount} грн");
    foreach (var item in o.OrderItems)
    {
        Console.WriteLine($"   - {item.ProductName}: {item.Quantity} шт. по {item.UnitPrice} грн");
    }
}

Console.WriteLine("\n3. Статистика (Агрегація):");
var stats = await service.GetCustomerOrderStatsAsync();
foreach (var s in (dynamic)stats)
{
    Console.WriteLine($"Клієнт: {s.CustomerName}, Кількість замовлень: {s.TotalOrders}, Витрачено: {s.TotalSpent} грн");
}

Console.WriteLine("\n4. Оновлення та видалення...");
await service.UpdateEmailAsync(customer.Id, "updated@test.com");
await service.DeleteOrderAsync(order.Id);
Console.WriteLine("Дані успішно оновлено та видалено!");

Console.WriteLine("\n=== УСПІШНО ВИКОНАНО ===");