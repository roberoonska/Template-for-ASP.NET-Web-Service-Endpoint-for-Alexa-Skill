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
	    	return new SkillResponse() { Response =  new ResponseBody() { ShouldEndSession = true }};
            else
                return RequestHandler(skillRequest);
        }
        
	
        public static SkillResponse RequestHandler(SkillRequest skillRequest)
        {
            SkillResponse response = new SkillResponse();
            switch (skillRequest.Request.Type)
            {
                case "LaunchRequest":
                    response = ResponseBuilder.Ask("YOUR RESPONSE TEXT HERE", new Reprompt("YOUR REPROMPT TEXT HERE"));
                    break;

                case "SessionEndedRequest":
                    response.Response = new ResponseBody() { ShouldEndSession = true };
                    break;

                case "IntentRequest":
                    response = IntentRequestHandler(skillRequest);
                    break;
            }
            return response;
        }


        public static SkillResponse IntentRequestHandler(SkillRequest skillRequest)
        {
            SkillResponse response = new SkillResponse();
            IntentData intentData = new IntentData(skillRequest);
            switch (intentData.IntentName)
            {
                case "YOUR CUSTOM INTENT NAME":
                    //YOUR CUSTOM INTENT CODE HERE
                    break;
		    
                case "ANOTHER CUSTOM INTENT NAME":
                    //YOUR CUSTOM INTENT CODE HERE
                    break;	
		    
		case "AMAZON.HelpIntent":
                    response = ResponseBuilder.Ask("YOUR RESPONSE TEXT HERE", new Reprompt("YOUR REPROMPT TEXT HERE"));
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
	    private SkillRequest SkillRequest { get; set; }
	    private IntentRequest IntentRequest { get; set; }
            public string IntentName { get; set; }
	    
	    private string Host { get; set; }
            private string AccessToken { get; set; }
            private string DeviceID { get; set; }
	    
	    //YOUR INTENT DATA PARAMETERS HERE

        public IntentData(SkillRequest skillRequest)
        {
	    SkillRequest = skillRequest;
            IntentRequest = skillRequest.Request as IntentRequest;
            IntentName = IntentRequest.Intent.Name;
        }
	
	public void SetAPIAccessInfo()
        {
            Host = SkillRequest.Context.System.ApiEndpoint;
            AccessToken = SkillRequest.Context.System.ApiAccessToken;
            DeviceID = SkillRequest.Context.System.Device.DeviceID;
        }
	    
	public void YOURMETHOD()
	{
	    //SET YOUR INTENT DATA PARAMETERS HERE
	}
	    
    }
}
