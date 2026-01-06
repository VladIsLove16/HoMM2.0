using System.Collections.Generic;

namespace Adventure.Presentation.Mushroom
{
    internal class MushroomThrownCustomEvent
    {
        private string v;
        private Dictionary<string, int> totals;

        public MushroomThrownCustomEvent(string v, Dictionary<string, int> totals)
        {
            this.v = v;
            this.totals = totals;
        }
    }
}