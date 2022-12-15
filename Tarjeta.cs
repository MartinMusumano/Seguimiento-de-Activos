using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Media;

namespace Seguimiento_de_Activos
{
    public class Tarjeta
    {
        public string MAC { get; set; }

        public string alias { get; set; }

        public Sustancia.Tipo tipo { get; set; }

        public SolidColorBrush color { get; set; }

        public Tarjeta(string MAC, string Alias, Sustancia.Tipo Tipo)
        {
            this.MAC = MAC.ToUpper();
            alias = Alias;
            tipo = Tipo;
            color = Sustancia.ToColor(Tipo);
        }

        public static string AliasConMAC(IEnumerable<Tarjeta> listaTarjetas, string MAC)
        {
            IEnumerable<string> tarjetaAlias = from tarjeta in listaTarjetas
                                               where tarjeta.MAC == MAC
                                               select tarjeta.alias;
            if (tarjetaAlias.Any())
                return tarjetaAlias.First();
            else
                return MAC;
        }

        public static Sustancia.Tipo? TipoConMAC(IEnumerable<Tarjeta> listaTarjetas, string MAC)
        {
            IEnumerable<Sustancia.Tipo> tarjetaTipo = from tarjeta in listaTarjetas
                                                        where tarjeta.MAC == MAC
                                                        select tarjeta.tipo;
            if (tarjetaTipo.Any())
                return tarjetaTipo.First();
            else
                return null;
        }
    }
}
