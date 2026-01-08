using System.Collections.Generic;

namespace Adventure.Presentation.Mushroom
{
    internal class MushroomDropedEvent
    {
        private string v;
        private Dictionary<string, int> totals;

        public MushroomDropedEvent(string v, Dictionary<string, int> totals)
        {
            this.v = v;
            this.totals = totals;
        }
    }
}