using WebStok.Business.DTOs;

namespace WebStok.Web.Models;

public class StockReceiptPageViewModel
{
    public StockReceiptDto Input { get; set; } = new();

    public StockReceiptPageDataDto Data { get; set; } = new();
}