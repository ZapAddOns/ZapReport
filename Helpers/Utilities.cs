namespace ZapReport.Helpers
{
    public static class Utilities
    {
        public static double GetDoseForVolume(double[] volume, double[] dose, double volumeThreshold)
        {
            return GetYForX(volume, dose, volumeThreshold);
        }

        public static double GetVolumeForDose(double[] dose, double[] volume, double doseThreshold)
        {
            return GetYForX(dose, volume, doseThreshold);
        }

        private static double GetYForX(double[] X, double[] Y, double XThreshold)
        {
            double result = 0;

            int i = 0;
            double lowerX, higherX = 0;
            double lowerY, higherY = 0;

            while (i < X.Length)
            {
                if (i == 0 && X[i] < XThreshold)
                    return Y[i];

                if (X[i] < XThreshold)
                {
                    higherX = X[i - 1];
                    lowerY = Y[i - 1];
                    lowerX = X[i];
                    higherY = Y[i];

                    result = (higherX - XThreshold) * (higherY - lowerY) / (higherX - lowerX) + lowerY;

                    break;
                }

                i++;
            }

            if (i == X.Length && result == 0)
                return Y[X.Length - 1];

            return result;
        }
    }
}
