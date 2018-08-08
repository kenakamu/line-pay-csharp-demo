using Line.Pay;
using Line.Pay.Models;
using Line.Messaging;
using Line.Messaging.Webhooks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinePayCSharpDemo.Models;

namespace LinePayCSharpDemo
{
    internal class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient;
        private LinePayClient payClient;
        private AppSettings appsettings;
        public LineBotApp(LineMessagingClient lineMessagingClient, LinePayClient linePayClient, AppSettings appsettings)
        {
            this.messagingClient = lineMessagingClient;
            this.payClient = linePayClient;
            this.appsettings = appsettings;
        }

        #region Handlers

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            switch (ev.Message.Type)
            {
                case EventMessageType.Text:
                    await HandleTextAsync(ev.ReplyToken, ((TextEventMessage)ev.Message).Text, ev.Source.UserId);
                    break;
            }
        }

        #endregion

        private async Task HandleTextAsync(string replyToken, string userMessage, string userId)
        {
            userMessage = userMessage.ToLower().Replace(" ", "");
            if (userMessage == "チョコレート")
            {
                var reserve = new Reserve()
                {
                    ProductName = "チョコレート",
                    Amount = 1,
                    Currency = Currency.JPY,
                    OrderId = Guid.NewGuid().ToString(),
                    ConfirmUrl = $"{appsettings.ServerUri}/api/linebot/confirm",
                    ConfirmUrlType = ConfirmUrlType.SERVER
                };

                var response = await payClient.ReserveAsync(reserve);
                // ユーザーの情報を設定
                reserve.Mid = userId;
                CacheService.Cache.Add(response.Info.TransactionId, reserve);
                var replyMessage = new TemplateMessage(
                     "Button Template",
                     new ButtonsTemplate(
                         text: $"{reserve.ProductName}を購入するには下記のボタンで決済に進んでください",
                         actions: new List<ITemplateAction> {
                         new UriTemplateAction("LINE Pay で決済", response.Info.PaymentUrl.Web)
                    }));

                await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });
            }
        }

        public async Task SendPayConfirm(Reserve reserve)
        {
            await messagingClient.PushMessageAsync(reserve.Mid, new List<ISendMessage>(){
                new StickerMessage("2", "144"),
                new TextMessage("ありがとうございます、チョコレートの決済が完了しました。")
            });
        }
    }
}