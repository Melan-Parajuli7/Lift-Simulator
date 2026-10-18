namespace ElevatorApp
{
    public static class FloorExtensions
    {
        /// <summary>"G" or "1" - what is shown on buttons and LED displays.</summary>
        public static string Label(this Floor floor)
        {
            return floor == Floor.Ground ? "G" : "1";
        }

        /// <summary>"Ground Floor" or "First Floor" - used in log messages.</summary>
        public static string DisplayName(this Floor floor)
        {
            return floor == Floor.Ground ? "Ground Floor" : "First Floor";
        }
    }
}
