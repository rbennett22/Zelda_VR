using UnityEngine;
using System;


namespace Immersio.Utility
{
    public static class MathHelper
    {

        // Summary:
        //      When inValue = r0Min, returns r1Min.
        //      When inValue = r0Max, returns r1Max. 
        //      When inValue = the average of r0Min and r0Max, returns the average of r1Min and r1Max. 
        //
        public static float ConvertFromRangeToRange(float r0Min, float r0Max, float r1Min, float r1Max, float inValue)
        {
            if (r0Min >= r0Max)
            {
                String errMsg = @"Error in MathHelper::ConvertFromRangeToRange.  "
                    + "Invalid Arg.  range minimum must be less than range maximum.  "
                    + "r0Min: " + r0Min + ", r0Max: " + r0Max;
                throw new Exception(errMsg);
                //return inValue;
            }
            if (r1Min >= r1Max)
            {
                String errMsg = @"Error in MathHelper::ConvertFromRangeToRange.  "
                    + "Invalid Arg.  range minimum must be less than range maximum.  "
                    + "r1Min: " + r1Min + ", r1Max: " + r1Max;
                throw new Exception(errMsg);
                //return inValue;
            }

            float ratio = (inValue - r0Min) / (r0Max - r0Min);
            return Mathf.Lerp(r1Min, r1Max, ratio);
        }
    }
}