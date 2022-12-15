using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Seguimiento_de_Activos
{
    public static class Sustancia
    {
        public enum Tipo
        {
            Bloques,
            Inyeccion,
            Spray,
            Isocianato,
            Adhesivos
        }

        public static SolidColorBrush ToColor(Tipo? tipo)
        {
            SolidColorBrush color;
            switch (tipo)
            {
                case Tipo.Bloques:
                    color = new SolidColorBrush(Colors.LightBlue);
                    break;
                case Tipo.Inyeccion:
                    color = new SolidColorBrush(Colors.Yellow);
                    break;
                case Tipo.Spray:
                    color = new SolidColorBrush(Colors.LightGreen);
                    break;
                case Tipo.Isocianato:
                    color = new SolidColorBrush(Colors.WhiteSmoke);
                    break;
                case Tipo.Adhesivos:
                    color = new SolidColorBrush(Colors.Pink);
                    break;
                case null:
                    color = new SolidColorBrush(Colors.LightGray);
                    break;
                default:
                    color = new SolidColorBrush(Colors.LightGray);
                    break;
            }
            return color;
        }
    }
}
