using AsyncAcademy.Data;
using AsyncAcademy.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Stripe; 
using Stripe.Checkout;

namespace AsyncAcademy.Controllers
{
    public class PaymentRequest
    {
        public int amount { get; set; }
        public string cardHolderName { get; set; }
        public string cardNumber { get; set; }
        public int cvc { get; set; }
        public string description { get; set; }
    }



    [Route("create-checkout-session")]
    [ApiController]
    public class CheckoutApiController : Controller
    {




        [HttpPost]
        public async Task<ActionResult> CreateCheckoutSession()
        {
            string testDomain = "https://localhost:7082";
            string asyncDomain = "https://asyncacademy20241005173644.azurewebsites.net/";
            var currentUserID = HttpContext.Session.GetInt32("CurrentUserId");
            var body = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var parsedBody = JObject.Parse(body);
            var amount = parsedBody["amount"].Value<int>(); //Will read the amount field from JSON

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = amount * 100,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Tuition Due",
                        },
                    },
                    Quantity = 1,
                },
            },
                Mode = "payment",
                SuccessUrl = testDomain + "/PaymentSuccess?session_id={CHECKOUT_SESSION_ID}",  // Add session ID to the success URL 
                CancelUrl = testDomain + "/PaymentCanceled",
                ClientReferenceId = currentUserID.ToString()
            };


            var service = new SessionService();
            Session session = service.Create(options);

            // Log the SuccessUrl for debugging
            Console.WriteLine("SuccessUrl sent to Stripe: " + options.SuccessUrl);

            TempData["Session"] = session.Id;
            //TempData["PaymentIntent"] = session.PaymentIntentId;//COMING NULL HERE BASED ON BREAKPOINT INFO
            // TempData["Receipt"] = session.PaymentIntent.LatestCharge.ReceiptUrl;//COMING NULL HERE BASED ON BREAKPOINT INFO

            return Json(new { id = session.Id });
        }


    }








    [Route("PaymentSuccess")]
    public class PaymentSuccessController : Controller
    {
        private readonly AsyncAcademyContext _context;

        public PaymentSuccessController(AsyncAcademyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> OnPaymentSuccess([FromQuery] string session_id)
        {
            // Fetch the Stripe session using the session ID
            var service = new SessionService();
            System.Diagnostics.Debug.WriteLine(Request);
            var session = await service.GetAsync(session_id);


            //TempData["card"] = session.PaymentMethodOptions.Card.RawJObject;

            // Check that the session was completed successfully
            if (session.PaymentStatus == "paid")
            {
                // Get the client reference ID (which is the userId) that we passed as "options"
                // in the checkout code 
                var userId = int.Parse(session.ClientReferenceId);

                // Get the total amount paid (Stripe stores amounts in cents)
                decimal amountPaid = (decimal)session.AmountTotal / 100m;

                // Find the user's payment record
                var paymentRecord = await _context.Payments.FirstOrDefaultAsync(s => s.UserId == userId);

                if (paymentRecord != null)
                {
                    // Update existing payment record
                    paymentRecord.AmountPaid += amountPaid;
                    paymentRecord.Timestamp = DateTime.Now;

                    _context.Payments.Update(paymentRecord);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Create a new payment record if none exists
                    var newPayment = new Payment
                    {
                        UserId = userId,
                        AmountPaid = amountPaid,
                        Timestamp = DateTime.Now
                    };
                    _context.Payments.Add(newPayment);
                    await _context.SaveChangesAsync();
                }



            }


            return RedirectToPage("/PaymentSuccess2", new { session_id = session_id });

        }


        //-----------------------THIS WILL BE THE NEW CHECKOUT CONTROLLER (AS OF 10/19/24)----------------------------


        //[Route("create-checkout-session")]
        //[ApiController]
        //public class CheckoutController : Controller
        //{
        //    [HttpPost]
        //    public async Task<ActionResult> CreateCheckOutSession([FromBody] PaymentRequest request)
        //    {
        //        var currentUserID = HttpContext.Session.GetInt32("CurrentUserId");
        //        var body = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        //        var parsedBody = JObject.Parse(body);
        //        var amount = parsedBody["amount"].Value<int>(); //Will read the amount field from JSON

        //        var options = new SessionCreateOptions
        //        {
        //            LineItems = new List<SessionLineItemOptions>
        //            {
        //                new SessionLineItemOptions
        //                {
        //                    PriceData = new SessionLineItemPriceDataOptions
        //                    {
        //                        UnitAmount = request.amount * 100,
        //                        Currency = "usd",
        //                        ProductData = new SessionLineItemPriceDataProductDataOptions
        //                        {
        //                            Name = "Tuition Due",
        //                        },
        //                    },
        //                    Quantity = 1,
        //                },
        //            },
        //            //new Payment

        //        };

        //        return null;
        //    }







        }
    }


