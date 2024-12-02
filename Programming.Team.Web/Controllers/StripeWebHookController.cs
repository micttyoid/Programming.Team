using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Programming.Team.Core;
using Programming.Team.PurchaseManager.Core;
using Stripe;
using Stripe.Checkout;

namespace Programming.Team.Web.Controllers
{
    [Route("/api/stripehooks")]
    [ApiController]
    public class StripeWebhookController : Controller
    {
        protected string WebHookSecret { get; }
        protected ILogger Logger { get; }
        protected SessionService SessionService { get; }
        protected IPurchaseManager PurchaseManager { get; }
        public StripeWebhookController(IConfiguration configuration, IPurchaseManager purchaseManager,
            ILogger<StripeWebhookController> logger, SessionService sessionService)
        {
            WebHookSecret = configuration["Stripe:WebHookKey"] ?? throw new InvalidDataException();
            Logger = logger;
            SessionService = sessionService;
            PurchaseManager = purchaseManager;
        }
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            try
            {
                var str = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                Logger.LogInformation(str);
                var stripeEvent = EventUtility.ConstructEvent(str, Request.Headers["Stripe-Signature"], WebHookSecret);
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null)
                        throw new InvalidDataException();
                    var options = new SessionGetOptions();
                    options.AddExpand("line_items");

                    // Retrieve the session. If you require line items in the response, you may include them by expanding line_items.
                    var sessionWithLineItems = await SessionService.GetAsync(session.Id, options);
                    if (sessionWithLineItems.Metadata.TryGetValue(nameof(Purchase.Id), out var subscriptionId))
                    {
                        if (sessionWithLineItems.AmountTotal != null)
                            await PurchaseManager.FinishPurchase(Guid.Parse(subscriptionId));
                    }
                    else
                        throw new InvalidDataException();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return BadRequest(ex);
            }
        }
    }
}
