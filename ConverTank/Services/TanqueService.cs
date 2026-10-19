namespace ConverTank.Services
{
    public class TanqueService
    {
        public static double CalcularVolume(double raioMetros, double comprimentoMetros, double alturaCm)
        {
            if (raioMetros <= 0 || comprimentoMetros <= 0 || alturaCm <= 0)
            {
                return 0;
            }

            double r = raioMetros * 100.0;
            double l = comprimentoMetros * 100.0;
            double h = alturaCm;
            double diametro = 2.0 * r;

            if (h >= diametro)
            {
                double volumeCheio = Math.PI * Math.Pow(r, 2) * l / 1000.0;
                return Math.Round(volumeCheio, 2);
            }

            double cosArg = (r - h) / r;
            if (cosArg > 1.0)
            {
                cosArg = 1.0;
            }
            else if (cosArg < -1.0)
            {
                cosArg = -1.0;
            }

            double termoRaiz = 2.0 * r * h - Math.Pow(h, 2);
            if (termoRaiz < 0)
            {
                termoRaiz = 0;
            }

            double areaSegmento = Math.Pow(r, 2) * Math.Acos(cosArg) - (r - h) * Math.Sqrt(termoRaiz);
            double volumeLitros = areaSegmento * l / 1000.0;

            if (double.IsNaN(volumeLitros) || double.IsInfinity(volumeLitros) || volumeLitros < 0)
            {
                return 0;
            }

            return Math.Round(volumeLitros, 2);
        }

        public static double CalcularVolumePorDiametro(double diametroMetros, double comprimentoMetros, double alturaCm)
        {
            return CalcularVolume(diametroMetros / 2.0, comprimentoMetros, alturaCm);
        }
    }
}