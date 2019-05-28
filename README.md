# Template for ASP.NET Web Service Endpoint for Alexa Skill
This is a template for a a custom web service endpoint for an Alexa Skill using ASP.NET.

It uses the nu-get package [Alexa.NET](https://github.com/timheuer/alexa-skills-dotnet) to handle the requests sent by Amazon Alexa.

It handles all of the required skill request types (LaunchRequest, SessionEndedRequest & IntentRequest) and the required default intent request types (AMAZON.HelpIntent, AMAZON.CancelIntent, AMAZON.StopIntent, AMAZON.NavigateHomeIntent & AMAZON.FallBackIntent).

It also handles the security requirements imposed by amazon including validating the request and checking the requestID. This functionality uses code written by Jason Shuler and posted to his [blog](https://shulerent.com/2018/03/18/validating-alexa-skill-web-requests-in-c/).

I've also included [AlexaAPIHelper.cs](/AlexaAPIHelper.cs) which includes methods that can be used to make calls on various alexa API including timezone, distance unit, temperature unit, address, full name, given name, email address and phone number. Keep in mind that some of these API's require special permission from the user. I've included a method that returns a card that asks for permissions; use it when a user tries to call one of your intents that requires a permission but that permission hasn't been set yet.

## Setup
1. Add [Alexa.NET](https://github.com/timheuer/alexa-skills-dotnet) to your project.
2. Add [ExampleController.cs](/ExampleController.cs) to your project under 'Controllers'; you can rename it to whatever you please.
3. Add [SecurityHelper.cs](/SecurityHelper.cs) to your project. Remember to change the skill ID to your skill's ID found in the amazon developer portal.
