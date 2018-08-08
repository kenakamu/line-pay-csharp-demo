using Line.Pay;
using Line.Pay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using LinePayCSharpDemo.Models;

namespace LinePayCSharpDemo.Controllers
{
    [Route("api/[controller]")]
    public class PayController : Controller
    {
        private LinePayClient client;
        private AppSettings appsettings;

        public PayController(IOptions<AppSettings> options)
        {
            appsettings = options.Value;

            // LinePay クライアントの作成
            client = new LinePayClient(
                appsettings.LinePay.ChannelId,
                appsettings.LinePay.ChannelSecret,
                appsettings.LinePay.IsSandbox);
        }

        [HttpGet]
        [Route("reserve")]
        public async Task<IActionResult> Reserve()
        {
            // 決済予約の作成
            var reserve = new Reserve()
            {
                ProductName = "チョコレート",
                Amount = 1,
                Currency = Currency.JPY,
                OrderId =  Guid.NewGuid().ToString(),
                ConfirmUrl = $"{appsettings.ServerUri}/api/pay/confirm"            
            };

            var response = await client.ReserveAsync(reserve);
            CacheService.Cache.Add(response.Info.TransactionId, reserve);

            return Redirect(response.Info.PaymentUrl.Web);
        }

        [HttpGet]
        [Route("confirm")]
        public async Task<IActionResult> Confirm()
        {
            // transactionId を取得して、キャッシュから決済予約を取得
            var transactionId = Int64.Parse(HttpContext.Request.Query["transactionId"]);
            var reserve = CacheService.Cache[transactionId] as Reserve;

            // 決済確認の作成
            var confirm = new Confirm()
            {
                Amount = reserve.Amount,
                Currency = reserve.Currency
            };

            var response = await client.ConfirmAsync(transactionId, confirm);
            return new OkObjectResult("決済が完了しました。");
        } 
    }
}
