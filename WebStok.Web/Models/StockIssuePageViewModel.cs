using Microsoft.AspNetCore.Mvc.Rendering;
using WebStok.Business.DTOs;

namespace WebStok.Web.Models;

public class StockIssuePageViewModel
{
    public StockIssueDto Input { get; set; } = new();

    public List<SelectListItem> ProductOptions { get; set; } = new();

    public List<SelectListItem> WarehouseOptions { get; set; } = new();

    public List<StockBalanceDto> Balances { get; set; } = new();
}