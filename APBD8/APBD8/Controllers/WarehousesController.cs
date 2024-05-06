using APBD8.Exceptions;
using APBD8.Models;
using APBD8.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace APBD8.Controllers;

[ApiController]
[Route("api/warehouses")]

public class WarehousesController : ControllerBase
{
    private readonly IWarehouseRepository _warehouseRepository;
    
    public WarehousesController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse(AddProductToWarehouse addProductToWarehouse)
    {
        try
        {
            var inserted = await _warehouseRepository.AddProductToWarehouse(addProductToWarehouse);
            return Ok($"Product added to warehouse. Inserted ID: {inserted}");
        }
        catch (Exception ex) when (ex is WareHouseNotFoundException or OrderNotFoundException
                                       or ProductNotFoundException)
        {
            return NotFound(ex.Message);
        }
        catch (OrderCompleteException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Order while adding product");
        }
    }
}