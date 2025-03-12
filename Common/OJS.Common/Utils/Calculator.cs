namespace OJS.Common.Utils
{
    using System;

    public static class Calculator
    {
        public static byte? Age(DateTime? date)
        {
            if (!date.HasValue)
            {
                return null;
            }

            var birthDate = date.Value;
            var now = DateTime.Now;

            var age = now.Year - birthDate.Year;
            if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
            {
                age--;
            }

            return (byte)age;
        }

        /// <summary>
        /// Linearly interpolate a value within [sourceMin..sourceMax]
        /// to a proportionally corresponding value within [targetMin..targetMax] or beyond, depending on 'clamp'.
        /// </summary>
        /// <param name="value">The current value to map.</param>
        /// <param name="sourceMin">Lower bound of the source range.</param>
        /// <param name="sourceMax">Upper bound of the source range.</param>
        /// <param name="targetMin">Lower bound of the target range.</param>
        /// <param name="targetMax">Upper bound of the target range.</param>
        /// <param name="clamp">If true, clamp the value to [sourceMin..sourceMax].</param>
        /// <returns>A double in [targetMin..targetMax] (if you clamp)
        /// or possibly beyond if 'value' is outside [sourceMin..sourceMax].</returns>
        public static double LinearInterpolate(
            double value,
            double sourceMin,
            double sourceMax,
            double targetMin,
            double targetMax,
            bool clamp = false)
        {
            if (clamp)
            {
                if (value < sourceMin)
                {
                    value = sourceMin;
                }

                if (value > sourceMax)
                {
                    value = sourceMax;
                }
            }

            // Avoid division by zero
            if (Math.Abs(sourceMax - sourceMin) < double.Epsilon)
            {
                return targetMin;
            }

            var fraction = (value - sourceMin) / (sourceMax - sourceMin);
            return targetMin + (fraction * (targetMax - targetMin));
        }
    }
}