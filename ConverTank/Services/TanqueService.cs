namespace ConverTank.Services
{
    public class TanqueService
    {
        public static double CalcularVolume(double raio, double comprimento, double altura)
        {
            if (altura <= 0)
                return 0;

            if (altura >= 2 * raio)
            {
                double volumeTotal = Math.PI * Math.Pow(raio, 2) * comprimento;
                return volumeTotal * 1000;
            }

            double areaSegmento =
                Math.Pow(raio, 2) *
                Math.Acos((raio - altura) / raio)
                - (raio - altura) *
                Math.Sqrt(2 * raio * altura - Math.Pow(altura, 2));

            double volume = areaSegmento * comprimento;

            return volume * 1000;
        }
    }
}