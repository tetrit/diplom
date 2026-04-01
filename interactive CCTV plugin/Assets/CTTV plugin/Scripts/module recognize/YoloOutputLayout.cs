namespace Surveillance.Recognition
{
    public enum YoloOutputLayout
    {
        Auto = 0,
        ChannelsFirst = 1, // [1, features, candidates]
        ChannelsLast = 2   // [1, candidates, features]
    }

    public enum YoloConfidenceMode
    {
        Auto = 0,
        ClassOnly = 1,            // cx, cy, w, h, class...
        ObjectnessTimesClass = 2  // cx, cy, w, h, obj, class...
    }
}