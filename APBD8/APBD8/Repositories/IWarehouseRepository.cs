using APBD8.Models;

namespace APBD8.Repositories;

public interface IWarehouseRepository
{
    Task<int> AddProductToWarehouse(AddProductToWarehouse addProductToWarehouse); 
    Task<bool> CheckIfWarehouseExists(int id);
    Task<bool> CheckIfProductExists(int id);
    Task<bool> CheckIfOrderExists(int IdProduct, int amount, DateTime dateTime);

    Task<bool> NotFullFiled(AddProductToWarehouse addProductToWarehouse);

    Task UpdateFulfilledAt(AddProductToWarehouse addProductToWarehouse);

    Task<int> AddProduct_Warehouse(AddProductToWarehouse addProductToWarehouse);

    Task<double> GetPrice(int idProduct);

    

}