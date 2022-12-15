using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Seguimiento_de_Activos
{
    // Clase utilizada para mostrar los Payloads y calcular Distancia
    public class VisiblePayload
    {
        public string fecha { get; set; }

        public string alias { get; set; }

        public SolidColorBrush color { get; set; }

        public Sustancia.Tipo? tipo { get; set; }

        public string distancia { get; set; }

        Queue<sbyte> RSSIs;

        static int _MAX_MEDICIONES = 3;

        public static int MAX_MEDICIONES 
        { 
            get { return _MAX_MEDICIONES; }
            set { if (value > 0) _MAX_MEDICIONES = value; } 
        }

        public static TimeSpan MAX_INTERVALO = new TimeSpan(0,0,45);


        public VisiblePayload(string Alias, DateTime Fecha, string RSSI, Sustancia.Tipo? Tipo)
        {
            fecha = Fecha.ToString("T");
            alias = Alias;
            tipo = Tipo;
            color = Sustancia.ToColor(Tipo);

            RSSIs = new Queue<sbyte>();
            AddFirstRSSI(RSSI);
        }

        // Sobrecarga para Sector 4, que no requiere RSSI
        public VisiblePayload(string Alias, DateTime Fecha, Sustancia.Tipo? Tipo)
        {
            fecha = Fecha.ToString("T");
            alias = Alias;
            tipo = Tipo;
            color = Sustancia.ToColor(Tipo);
        }

        public void AddRSSI(string RSSI, DateTime FechaActual)    // Recibe string "-xxdBm" y Fecha de medicion
        {
            if (RSSIs == null)
                return;

            var intervalo = FechaActual - Convert.ToDateTime(this.fecha);     // Verifica intervalo entre mediciones
            if (intervalo > MAX_INTERVALO)
            {
                RSSIs.Clear();          // Si se excede el intervalo, descarta datos anteriores
                AddFirstRSSI(RSSI);
            }
            else
            {
                sbyte newrssi = Convert.ToSByte(RSSI.Substring(0, 3));

                while(RSSIs.Count >= _MAX_MEDICIONES)      // Verifica el numero de mediciones en la cola
                {
                    RSSIs.Dequeue();           
                }
                
                RSSIs.Enqueue(newrssi);

                float suma = 0;
                foreach (sbyte i in RSSIs)       // Cálculo Promedio con RSSI almacenados
                    suma += i;
                suma /= RSSIs.Count;
                distancia = DistanciaConRSSI((sbyte)suma);

            }

            this.fecha = FechaActual.ToString("T");   // Actualiza fecha de último RSSI
        }

        void AddFirstRSSI(string RSSI)    // Recibe string "-xxdBm". Sin comparación de fecha
        {
            sbyte newrssi = Convert.ToSByte(RSSI.Substring(0, 3));
            RSSIs.Enqueue(newrssi);
            distancia = DistanciaConRSSI(newrssi);
        }

        public static string DistanciaConRSSI(sbyte RSSI)
        {
            if (RSSI >= -40)
                return "< 1m";
            else if (RSSI >= -60)
                return "< 3m";
            else                        //Si hay filtro en -80dBm
                return "< 10m";
        }

    }
}
