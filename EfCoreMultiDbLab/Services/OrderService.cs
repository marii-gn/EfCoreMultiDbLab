using Microsoft.EntityFrameworkCore;
using EfCoreMultiDbLab.Data;
using EfCoreMultiDbLab.Models;

namespace EfCoreMultiDbLab.Services;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email)
    {
        var customer = new Customer { FullName = name, Email = email };
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Order> CreateOrderAsync(int customerId, List<(string product, int qty, decimal price)> items)
    {
        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            OrderItems = items.Select(i => new OrderItem
            {
                ProductName = i.product,
                Quantity = i.qty,
                UnitPrice = i.price
            }).ToList()
        };

        order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<List<Order>> GetOrdersFilteredAsync(decimal minAmount)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Where(o => o.TotalAmount >= minAmount)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<object> GetCustomerOrderStatsAsync()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Orders)
            .ToListAsync();

        return customers.Select(c => new
        {
            CustomerName = c.FullName,
            TotalOrders = c.Orders.Count,
            TotalSpent = c.Orders.Sum(o => o.TotalAmount)
        }).ToList();
    }

    public async Task UpdateEmailAsync(int customerId, string newEmail)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer != null)
        {
            customer.Email = newEmail;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}