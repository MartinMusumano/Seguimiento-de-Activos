using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seguimiento_de_Activos
{
    public class Sensor
    {
        public Sensor(string EUI, string Alias="")
        {
            this.EUI = EUI.ToUpper();
            this.alias = Alias;
        }

        public string EUI { get; set; }

        public string alias { get; set; }


        public static string AliasConEUI(IEnumerable<Sensor> listasensores, string EUI)
        {
            IEnumerable<string> sensorAlias = from sensor in listasensores
                                               where sensor.EUI == EUI
                                               select sensor.alias;
            if (sensorAlias.Any())
                return sensorAlias.First();
            else
                return null;
        }

    }
}
