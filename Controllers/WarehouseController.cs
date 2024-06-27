using APBD_zad6.Context;
using APBD_zad6.DTOs;
using APBD_zad6.Migrations;
using Microsoft.AspNetCore.Mvc;

namespace APBD_zad6.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly MyDbContext _dbContext = new();

    [HttpPost]
    public IActionResult AddWarehouse(ProductWarehouseDto productWarehouseDto)
    {
        var product = _dbContext.Products.FirstOrDefault(p => p.IdProduct == productWarehouseDto.IdProduct);
        var isWarehouseInDb = _dbContext.Warehouses.Any(w => w.IdWarehouse == productWarehouseDto.IdWarehouse);
        var isAmountGraterThanZero = productWarehouseDto.Amount > 0;

        if (product == null || !isWarehouseInDb || !isAmountGraterThanZero)
            return StatusCode(StatusCodes.Status400BadRequest);

        var order = _dbContext.Orders.FirstOrDefault(o => o.IdProduct == productWarehouseDto.IdProduct &&
                                                          o.Amount == productWarehouseDto.Amount &&
                                                          o.CreatedAt < productWarehouseDto.CreatedAt);
        if (order == null) return StatusCode(StatusCodes.Status400BadRequest);

        var isOrderFinished = _dbContext.ProductWarehouses.Any(pw => pw.IdOrder == order.IdOrder);
        if (isOrderFinished) return StatusCode(StatusCodes.Status400BadRequest);

        order.FulfilledAt = DateTime.Now;

        var productWarehouse = new ProductWarehouse
        {
            IdProduct = productWarehouseDto.IdProduct,
            IdWarehouse = productWarehouseDto.IdWarehouse,
            Amount = productWarehouseDto.Amount,
            Price = product.Price * productWarehouseDto.Amount,
            CreatedAt = DateTime.Now
        };

        _dbContext.ProductWarehouses.Add(productWarehouse);

        _dbContext.SaveChanges();

        return Ok(productWarehouse.IdProductWarehouse);
    }
}