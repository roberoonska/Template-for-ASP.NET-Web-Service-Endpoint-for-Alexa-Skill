using System;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;


namespace YOURPROJECTNAME.Controllers
{
    public class YOURCONTROLLERNAME : ApiController
    {
        [HttpPost]
        public async Task<Object> AlexaResponseAsync(SkillRequest skillRequest)
        {
            SkillResponse response = new SkillResponse();
            int securityCode = await SecurityHelper.SecurityHandler(Request, skillRequest);
            if (securityCode == 1) //request not validated
                return BadRequest();
            else if (securityCode == 2) //request ID doesn't match
                response.Response = new ResponseBody() { ShouldEndSession = true };
            else
                response = RequestHandler(skillRequest);
            return response;
        }
        
	
        public static SkillResponse RequestHandler(SkillRequest skillRequest)
        {
            SkillResponse response = new SkillResponse();
            switch (skillRequest.Request.Type)
            {
                case "LaunchRequest":
                    response = ResponseBuilder.Ask("RESPONSE TEXT HERE", new Reprompt("REPROMPT TEXT HERE"));
                    break;

                case "SessionEndedRequest":
                    responseBody.ShouldEndSession = true;
                    response.Response = responseBody;
                    break;

                case "IntentRequest":
                    response = IntentRequestHandler(skillRequest, mess);
                    break;
            }
            return response;
        }


        public static SkillResponse IntentRequestHandler(SkillRequest skillRequest,  MessLog mess)
        {
            SkillResponse response = new SkillResponse();
            IntentData intentData = new IntentData(skillRequest, mess);
            switch (intentData.IntentName)
            {
                case "YOURCUSTOMINTENTNAME1":
                    //YOUR CUSTOM INTENT CODE HERE
                    break;
                case "YOURCUSTOMINTENTNAME2":
                    //YOUR CUSTOM INTENT CODE HERE
                    break;							
		case "AMAZON.HelpIntent":
                    response = ResponseBuilder.Ask("RESPONSE TEXT HERE", new Reprompt("REPROMPT TEXT HERE"));
                    break;
                case "AMAZON.CancelIntent":
                    response.Response = new ResponseBody() { ShouldEndSession = true };
                    break;
                case "AMAZON.StopIntent":
                    response.Response = new ResponseBody() { ShouldEndSession = true };
                    break;
                case "AMAZON.NavigateHomeIntent":
                    response.Response = new ResponseBody() { ShouldEndSession = true };
                    break;
                case "AMAZON.FallBackIntent":
                    response = ResponseBuilder.Tell("I did not understand that. Please try again.");
                    break;	
            }
            return response;
        }
    }
	
	
    public class IntentData
   	{
        public string IntentName { get; set; }
        private SkillRequest SkillRequest { get; set; }
        private IntentRequest IntentRequest { get; set; }
	//YOUR INTENT DATA PARAMETERS HERE

        public IntentData(SkillRequest skillRequest)
        {
            SkillRequest = skillRequest;
            IntentRequest = SkillRequest.Request as IntentRequest;
            IntentName = IntentRequest.Intent.Name;
        }
    }
}
