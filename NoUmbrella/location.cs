using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using ForecastIO;

namespace NoUmbrella
{
    public static class location
    {
        [FunctionName("location")]

        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "location/{location}")]HttpRequestMessage req, string location, TraceWriter log)
        {
            log.Info("location function received request for location: " + location);
            var helptext = "Please provide a location in the format of latitude,longitude e.g. for Melbourne Australia /api/location/-37.8136,-144.9631";
            var errortext = "Internal error please contact admin";

            var darkskysecret = Environment.GetEnvironmentVariable("DarkSkyApiSecret", EnvironmentVariableTarget.Process);

            if (string.IsNullOrWhiteSpace(darkskysecret)) {
                log.Error("Couldn't read darksky api key");
                return req.CreateResponse(HttpStatusCode.InternalServerError, errortext);
            }

            if (location.Equals("{location}") || string.IsNullOrWhiteSpace(location)){
                log.Verbose("Location parameter was missing");
                return req.CreateResponse(HttpStatusCode.OK, helptext);
            }

            var request = new ForecastIORequest(darkskysecret, Parselatitude(location), Parselongitude(location), Unit.si);
            var forecastIoResponse = request.Get();
            string[] noumbrellaicons;
            string icon;
            var response = MakeUmbrellaResponse(forecastIoResponse, out noumbrellaicons, out icon);

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        private static string MakeUmbrellaResponse(ForecastIOResponse forecastIoResponse, out string[] noumbrellaicons, out string icon)
        {
            noumbrellaicons = new string[]
                {"clear-day", "clear-night", "fog", "cloudy", "partly-cloudy-day", "partly-cloudy-night"};
            icon = forecastIoResponse.currently.icon;
            var response = "unknown";
            if (icon.Equals("rain"))
            {
                response = "umbrella";
            }
            else if (icon.Equals("wind"))
            {
                response = "marypoppins";
            }
            else
            {
                string s = icon;
                if (Array.Exists(noumbrellaicons, x => x.Equals(s)))
                {
                    response = "noumbrella";
                }
            }
            return response;
        }

        public static float Parselatitude(string locationstring) {

            return float.Parse(locationstring.Split(',')[0]);
        }

        public static float Parselongitude(string locationstring)
        {
            return float.Parse(locationstring.Split(',')[1]);
        }
    }
}