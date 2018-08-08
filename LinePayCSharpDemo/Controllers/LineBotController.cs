using System;
using Line.Pay;
using Line.Pay.Models;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using LinePayCSharpDemo.Models;

namespace LinePayCSharpDemo.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LineBotController : Controller
    {
        private static LineMessagingClient lineMessagingClient;
        private static LinePayClient linePayClient;
        private AppSettings appsettings;

        public LineBotController(IOptions<AppSettings> options)
        {
            appsettings = options.Value;
            lineMessagingClient = new LineMessagingClient(appsettings.LineBot.ChannelAccessToken);
            linePayClient = new LinePayClient(
                appsettings.LinePay.ChannelId,
                appsettings.LinePay.ChannelSecret,
                appsettings.LinePay.IsSandbox);
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JToken req)
        { 
            var events = WebhookEventParser.Parse(req.ToString());
          
            var app = new LineBotApp(lineMessagingClient, linePayClient, appsettings);
            await app.RunAsync(events);
            return new OkResult();
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

            var response = await linePayClient.ConfirmAsync(transactionId, confirm);
            var app = new LineBotApp(lineMessagingClient, linePayClient, appsettings);
            await app.SendPayConfirm(reserve);
            return new OkResult();
        } 
    }
}