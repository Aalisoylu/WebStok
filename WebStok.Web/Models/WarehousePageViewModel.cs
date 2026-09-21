using WebStok.Business.DTOs;

namespace WebStok.Web.Models;

public class WarehousePageViewModel
{
    public CreateWarehouseDto Input { get; set; } = new();

    public List<WarehouseListDto> Warehouses { get; set; } = new();
}