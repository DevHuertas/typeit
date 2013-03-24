using System.Collections.Generic;

namespace DialerRole.Models
{
    public class AvailableMaterials
    {
        public List<string> GetTop3MaterialsForId(int userId)
        {
            //TODO:Load this from a database
            return new List<string>(){"Verb" , "Emotions" , "Story"};
        }
    }
}