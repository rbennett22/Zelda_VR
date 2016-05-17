using System;

// NOTE: Instances of this clock track time seperately from the Cubiquity
// native-code clock, and the timestamps should not be compared.
public static class Clock
{
    public static uint timestamp
    {
        get
        {
            if (mTimestamp == uint.MaxValue)
            {
                throw new OverflowException("Clock timestamper has overflowed!");
            }
            return mTimestamp++;
        }
    }
    // We initialise the clock to a reasonably sized value, so that we can initialise
    // timestamps to small values and be sure that they will immediatly be out-of-date.
    private static uint mTimestamp = 100;
}