using System;

namespace Seguimiento_de_Activos
{
    //Clase utilizada para decodificar el Json
    public class SensorPayload
    {

        public int beacon_num;

        public int head;

        public ScanData[] scan_data;

    }
}
