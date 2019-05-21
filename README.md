# Template for ASP.NET Web Service Endpoint for Alexa Skill
This is a template for a a custom web service endpoint for an Alexa Skill using ASP.NET.

It uses the nu-get package [Alexa.NET](https://github.com/timheuer/alexa-skills-dotnet) to handle the requests sent by Amazon Alexa.

It handles all of the required skill request types (LaunchRequest, SessionEndedRequest & IntentRequest) and the required default intent request types (AMAZON.HelpIntent, AMAZON.CancelIntent, AMAZON.StopIntent, AMAZON.NavigateHomeIntent & AMAZON.FallBackIntent).

It also handles the security requirements imposed by amazon including validating the request and checking the requestID. This functionality uses code written by Jason Shuler and posted to his [blog](https://shulerent.com/2018/03/18/validating-alexa-skill-web-requests-in-c/).
