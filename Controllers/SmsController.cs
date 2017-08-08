using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;

namespace Feedback.Controllers
{
    [Route("api/[controller]")]
    public class SmsController : Controller
    {
        ConcurrentBag<PhoneNumber> PhoneNumbers { get; set; }
        public SmsController()
        {
            if (PhoneNumbers == null)
            {
                PhoneNumbers = new ConcurrentBag<PhoneNumber>();
            }
        }

        [HttpPost]
        public IActionResult Post(string from, string body)
        {
            // Grab number and feedback and store somewhere
            var entry = (from, body);

            if (body.StartsWith("080985") || from.Contains(Helper.FACILITATOR_NUMBER))
            {
                return GetRaffleWinner();
            }
            PhoneNumbers.Add(new PhoneNumber(from));

             // 3. Put together response body and thank for feedback
            var response = new MessagingResponse();
            response.Message("Thank you for providing your feedback. This is now your entry into the raffle. Keep submitting more feedback or ");
            return Ok(response.ToString());
        }

        protected IActionResult GetRaffleWinner()
        {
            // Remove my phone number
            var entry = PhoneNumbers.FirstOrDefault(x => x.ToString().Equals(Helper.FACILITATOR_NUMBER));
            if (entry != null || PhoneNumbers.Count == 0)
            {
                // do something if my number is in the list
            }

            // Send winner announcement
            var random = new Random(PhoneNumbers.Count).Next();
            var winner = PhoneNumbers.ElementAt(random);

            TwilioClient.Init(Helper.TWILIO_ACCOUNT_SID, Helper.TWILIO_AUTH_TOKEN);
            // SEND ME THE WINNER
            MessageResource.Create(
                to: new PhoneNumber(Helper.FACILITATOR_NUMBER),
                from: new PhoneNumber(Helper.TWILIO_PHONE_NUMBER),
                body: $"The Winner's Phone Number is {winner.ToString()}");

            // NOTIFY OTHER PARTICIPANTS
            Parallel.ForEach(PhoneNumbers.Distinct(), number =>
            { 
                if (number == winner)
                {
                    MessageResource.Create(
                        to: winner,
                        from: new PhoneNumber(Helper.TWILIO_PHONE_NUMBER),
                        body: "CONGRATULATIONS!!!! You are the proud winner of a new AMAZON ECHO device. Come see me to pick it up"
                    );
                }
                else
                {
                    MessageResource.Create(
                        to: number,
                        from: new PhoneNumber(Helper.TWILIO_PHONE_NUMBER),
                        body: "SORRY!!! You have not won the prize. We thank you for your feedback. For more information on Twilio, check out www.twilio.com"
                        );
                }
            });
            return Ok();
        } 
    }
}