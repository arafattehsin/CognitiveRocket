using AdaptiveCards;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S2S.Helpers
{
    public class Common
    {
       
        public static string GenerateReferenceID(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Helper Method to modify the Adaptive Card template. 
        public static AdaptiveCard GetAdaptiveCard(Prospect userProfile)
        {
            // Found flight number, just return the flight details now.
            string json = System.IO.File.ReadAllText(@"Data\\Assets\\Card.json");
            var card = AdaptiveCards.AdaptiveCard.FromJson(json).Card;
            var body = card.Body;

            // Fact 
            AdaptiveFactSet adaptiveFactSet = (AdaptiveCards.AdaptiveFactSet)body[2];

            // Name
            AdaptiveFact name = (AdaptiveFact)adaptiveFactSet.Facts[0];
            name.Value = userProfile.Name;

            // Phone Number
            AdaptiveFact phoneNumber = (AdaptiveFact)adaptiveFactSet.Facts[1];
            phoneNumber.Value = userProfile.PhoneNumber;

            // Email Address
            AdaptiveFact emailAddr = (AdaptiveFact)adaptiveFactSet.Facts[2];
            emailAddr.Value = userProfile.Email;

            return card;
        }
    }
}
