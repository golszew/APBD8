using APBD8.Models;
using System.Data.SqlClient;
using System.Transactions;
using APBD8.Exceptions;

namespace APBD8.Repositories;




public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;

    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<int> AddProductToWarehouse(AddProductToWarehouse addProductToWarehouse)
    {
        
        double price, IdOrder;
        
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            using (var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
            {
                await con.OpenAsync();

                
                var warehouseExists = await CheckIfWarehouseExists(addProductToWarehouse.IdWarehouse);
                if (!warehouseExists)
                {
                    throw new WareHouseNotFoundException("Warehouse not found.");
                }

               
                var productExists = await CheckIfProductExists(addProductToWarehouse.IdProduct);
                if (!productExists)
                {
                    throw new ProductNotFoundException("Product not found.");
                }

                var orderExists = await CheckIfOrderExists(addProductToWarehouse.IdProduct,
                    addProductToWarehouse.Amount, addProductToWarehouse.CreatedAt);
                if (!orderExists)
                    throw new OrderNotFoundException("Order not found");

                var orderNotComplete = await NotFullFiled(addProductToWarehouse);
                if (!orderNotComplete)
                    throw new OrderCompleteException("Order already complete");
                
                
                await UpdateFulfilledAt(addProductToWarehouse);
                
                price = await GetPrice(addProductToWarehouse.IdProduct);
                price *= addProductToWarehouse.Amount;
                IdOrder = await GetIdOrder(addProductToWarehouse);
                
                await using (var cmd = new SqlCommand())

                {
                    cmd.Connection = con;
                    cmd.CommandText =
                        "insert into Product_Warehouse values (@idWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
                    cmd.Parameters.AddWithValue("@IdProduct", addProductToWarehouse.IdProduct);
                    cmd.Parameters.AddWithValue("@Amount", addProductToWarehouse.Amount);
                    cmd.Parameters.AddWithValue("@CreatedAt", addProductToWarehouse.CreatedAt);
                    cmd.Parameters.AddWithValue("@idWarehouse", addProductToWarehouse.IdWarehouse);
                    cmd.Parameters.AddWithValue("@IdOrder", IdOrder);
                    cmd.Parameters.AddWithValue("@Price", price);
                    return (int)await cmd.ExecuteScalarAsync();
                }
                
                scope.Complete();
            }
        }
    }

    public async Task<bool> CheckIfWarehouseExists(int id)
    {
        using (var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            await con.OpenAsync();

            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                cmd.Parameters.AddWithValue("@IdWarehouse", id);
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
        }
    }

    public async Task<bool> CheckIfProductExists(int id)
    {
        using (var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            await con.OpenAsync();

            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
                cmd.Parameters.AddWithValue("@IdProduct", id);
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
        }
    }


    public async Task<bool> CheckIfOrderExists(int IdProduct, int amount, DateTime dateTime)
    {
        await using (var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            await con.OpenAsync();

            await using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = "SELECT 1 FROM Order WHERE IdProduct = @IdProduct AND Amount = @amount AND CreatedAt<@date";
                cmd.Parameters.AddWithValue("@IdProduct", IdProduct);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@date", dateTime);
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
        }
    }

    public async Task<bool> NotFullFiled(AddProductToWarehouse addProductToWarehouse)
    {
        await using (var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            await con.OpenAsync();

            await using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = "SELECT 1 FROM Product_Warehouse WHERE IdProduct = @IdProduct AND Amount = @amount AND CreatedAt=@date";
                cmd.Parameters.AddWithValue("@IdProduct", addProductToWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("@amount", addProductToWarehouse.Amount);
                cmd.Parameters.AddWithValue("@date", addProductToWarehouse.CreatedAt);
                
                var result = await cmd.ExecuteScalarAsync();
                return result == null;
            }
        }
    }

    public async Task UpdateFulfilledAt(AddProductToWarehouse addProductToWarehouse)
    {
        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await connection.OpenAsync();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "update order set FulfilledAt = @currentDate WHERE IdProduct = @IdProduct AND Amount = @amount AND CreatedAt = @date";
                cmd.Parameters.AddWithValue("@IdProduct", addProductToWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("@amount", addProductToWarehouse.Amount);
                cmd.Parameters.AddWithValue("@date", addProductToWarehouse.CreatedAt);
                cmd.Parameters.AddWithValue("@currentDate", DateTime.Today.Date);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<int> AddProduct_Warehouse(AddProductToWarehouse addProductToWarehouse)
    {
        
        double price, IdOrder;
        price = await GetPrice(addProductToWarehouse.IdProduct);
        price *= addProductToWarehouse.Amount;
        IdOrder = await GetIdOrder(addProductToWarehouse);
        
        await using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))

        {
            {
                await connection.OpenAsync();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "insert into Product_Warehouse values (@idWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
                    cmd.Parameters.AddWithValue("@IdProduct", addProductToWarehouse.IdProduct);
                    cmd.Parameters.AddWithValue("@Amount", addProductToWarehouse.Amount);
                    cmd.Parameters.AddWithValue("@CreatedAt", addProductToWarehouse.CreatedAt);
                    cmd.Parameters.AddWithValue("@idWarehouse", addProductToWarehouse.IdWarehouse);
                    cmd.Parameters.AddWithValue("@IdOrder", IdOrder);
                    cmd.Parameters.AddWithValue("@Price", price);
                    return (int) await cmd.ExecuteScalarAsync();
                }
            }
        }
    }

    public async Task<double> GetPrice(int idProduct)
    {
        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await connection.OpenAsync();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "SELECT price from Product where IdProduct = @IdProduct";
                cmd.Parameters.AddWithValue("@IdProduct", idProduct);


                return (double)await cmd.ExecuteScalarAsync();

            }
        }
    }

    public async Task<int> GetIdOrder(AddProductToWarehouse addProductToWarehouse)
    {
        await using (var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            await con.OpenAsync();

            await using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = "SELECT IdOrder FROM Product_Warehouse WHERE IdProduct = @IdProduct AND Amount = @amount AND CreatedAt=@date";
                cmd.Parameters.AddWithValue("@IdProduct", addProductToWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("@amount", addProductToWarehouse.Amount);
                cmd.Parameters.AddWithValue("@date", addProductToWarehouse.CreatedAt);
                
                return (int) await cmd.ExecuteScalarAsync();
                
            }
        }
    }
}