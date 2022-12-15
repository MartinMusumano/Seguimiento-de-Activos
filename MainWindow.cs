using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Windows;
using uPLibrary.Networking.M2Mqtt;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Collections.ObjectModel;

namespace Seguimiento_de_Activos
{
    // -------------------------------- MQTT -------------------------------- //
    public partial class MainWindow : Window
    {
        Timer timerReconexion = new Timer(2500);

        public void ConexionMQTT()
        {
            string txtBroker = "nam1.cloud.thethings.network";
            int Port = 1883;
            string txtUsername = "unm-posicionamiento-indoor@ttn";
            string txtPassword = "NNSXS.QCLKI6L5SAEWU2TOT2LTBC4IKUGGASEAEKEG65I.WZF2HNCK5W5CP6FCUP2M5RUFBDNI7ST6YBXI2EQSPXZW25A7BKDQ";

            try
            {
                client_tts = new MqttClient(txtBroker, Port, false, MqttSslProtocols.None, null, null);
                client_tts.ProtocolVersion = MqttProtocolVersion.Version_3_1;
                byte code = client_tts.Connect("PC2", txtUsername, txtPassword, true, 3);

                if (code == 0) //Conexion correcta
                {
                    client_tts.Subscribe(new string[] { "v3/unm-posicionamiento-indoor@ttn/devices/+/up" }, new byte[] { 0 });
                    client_tts.MqttMsgPublishReceived += Client_tts_MqttMsgPublishReceived;     //Evento de recepcion
                    client_tts.ConnectionClosed += Client_tts_ConnectionClosed;

                    //Deshabilita el Timer
                    timerReconexion.Stop();
                    timerReconexion.Elapsed -= new ElapsedEventHandler(timerTick);
                    intentos = 0;
                    this.Dispatcher.Invoke(() =>
                    {
                        lblEstado.Content = "Conectado";
                        RecButton.Visibility = Visibility.Hidden;
                    });
                }
                else
                    Reconexion();   
            }
            catch (Exception)
            {
                Reconexion();
            }
        }

        //Evento de Desconexión
        public void Client_tts_ConnectionClosed(object sender, EventArgs e)
        {
            Reconexion();
        }

        void Reconexion()
        {
            if (intentos == 0)
            {   //Configura el Timer
                timerReconexion.AutoReset = false;          //Solo contará 1 vez cuando se llama a Start()
                timerReconexion.Elapsed += new ElapsedEventHandler(timerTick);
            }
            timerReconexion.Start();
        }

        short intentos = 0;
        void timerTick(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => lblEstado.Content = "Reconectando...");
            intentos++;
            if (intentos == 6)
            {   //Deshabilita el Timer
                timerReconexion.Stop();
                timerReconexion.Elapsed -= new ElapsedEventHandler(timerTick);
                MessageBox.Show("No se ha podido conectar con el Servidor.");
                this.Dispatcher.Invoke(() =>
                {
                    lblEstado.Content = "Desconectado";
                    RecButton.Visibility = Visibility.Visible;  //Muestra boton Reconectar manual
                });
            }
            ConexionMQTT();
        }

        private void Reconnect_Click(object sender, RoutedEventArgs e)
        {
            intentos = 0;
            this.Dispatcher.Invoke(() => RecButton.Visibility = Visibility.Hidden);
            ConexionMQTT();
        }


        //Metodo para recibir y decodificar mensajes
        private void Client_tts_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);       //String con todos los caracteres

            string port = stringBetween(message, "f_port\":", ",\"f_cnt");      //Identifica el puerto (evita otro tipo de mensajes)
            if (port != "2")
                return;

            string decodedPayload = stringBetween(message, "ed_payload\":", ",\"rx_meta");      //Identifica el payload
            if (decodedPayload == null)
                return;

            string devEUI = stringBetween(message, "dev_eui\":\"", "\",\"join_eui");            //Identifica el devEUI
            if (devEUI == null)
                return;
            devEUI = devEUI.ToUpper();


            ObservableCollection<VisiblePayload> selected = null;        //Servira como referencia a la coleccion a modificar
            bool isSector4 = false;

            for(int i=0; i < sectores.Keys.Count; i++)
                foreach(string key in sectores.Keys.ElementAt(i))
                    if(key == devEUI)
                    {
                        sectores.TryGetValue(sectores.Keys.ElementAt(i), out selected); //Selecciona la coleccion a modificar
                        if (i == 3)
                            isSector4 = true;
                        break;
                    }
            if (selected == null)
                return;

            SensorPayload payload;
            try
            {   //Deserializa el Payload
                payload = JsonConvert.DeserializeObject<SensorPayload>(decodedPayload);
            }
            catch (Exception) { return; }

            if (isSector4)
                UpdateSector4(payload, selected);
            else
                UpdateSectors(payload, selected);
        }

        private void UpdateSector4(SensorPayload payload, ObservableCollection<VisiblePayload> selected)
        {
            for (int i = 0; i < payload.beacon_num; i++)
            {
                string mac = payload.scan_data[i].mac.ToUpper();
                string alias = Tarjeta.AliasConMAC(listaTarjetas, mac);

                foreach (var sector in sectores.Values)     //Chequea cada colección (sector)
                {
                    for (int j = 0; j < sector.Count; j++)
                    {
                        if (sector[j].alias == alias)
                        {
                           this.Dispatcher.Invoke( () => sector.RemoveAt(j) );  //Remueve del sector original
                           goto End1;
                        }
                    }
                }
                End1:                       //Añade en Sector4 (selected)
                this.Dispatcher.Invoke(() =>
                    selected.Add(new VisiblePayload(alias, DateTime.Now, Tarjeta.TipoConMAC(listaTarjetas, mac)))
                );
                UpdateCount();
            }
            return;
        }

        private void UpdateSectors(SensorPayload payload, ObservableCollection<VisiblePayload> selected)
        {
            for (int i = 0; i < payload.beacon_num; i++)
            {
                string mac = payload.scan_data[i].mac.ToUpper();
                string alias = Tarjeta.AliasConMAC(listaTarjetas, mac);

                foreach (var sector in sectores.Values)     //Chequea cada colección (sector)
                {
                    for (int j = 0; j < sector.Count; j++)  //Chequea cada Payload
                    {
                        if (sector[j].alias == alias)
                        {
                            if (sector == selected) //Si se detectó en el mismo sector
                                this.Dispatcher.Invoke( () => sector[j].AddRSSI(payload.scan_data[i].rssi, DateTime.Now)); //Promedio
                            else
                                this.Dispatcher.Invoke( () => 
                                {                       //Remueve del sector original y añade en el seleccionado
                                    sector.RemoveAt(j);
                                    selected.Add(new VisiblePayload(alias, DateTime.Now, payload.scan_data[i].rssi, Tarjeta.TipoConMAC(listaTarjetas, mac)));
                                });
                            goto End2;
                        }
                    }
                }
                //Si no hubo coincidencia
                this.Dispatcher.Invoke(() =>
                {   
                    selected.Add(new VisiblePayload(alias, DateTime.Now, payload.scan_data[i].rssi, Tarjeta.TipoConMAC(listaTarjetas, mac)));
                });
            End2:;
            }
            return;
        }

        //Actualiza el recuento de activos en sector4
        void UpdateCount()
        {
            short[] cuenta = new short[6];

            foreach (VisiblePayload vp in sectores.Values.ElementAt(3))
                switch (vp.tipo)
                {
                    case Sustancia.Tipo.Bloques:
                        cuenta[0]++;
                        break;
                    case Sustancia.Tipo.Inyeccion:
                        cuenta[1]++;
                        break;
                    case Sustancia.Tipo.Spray:
                        cuenta[2]++;
                        break;
                    case Sustancia.Tipo.Isocianato:
                        cuenta[3]++;
                        break;
                    case Sustancia.Tipo.Adhesivos:
                        cuenta[4]++;
                        break;
                    default:
                        cuenta[5]++;
                        break;
                }
            this.Dispatcher.Invoke(() =>
            {
                lblBloques.Content = cuenta[0].ToString();
                lblInyeccion.Content = cuenta[1].ToString();
                lblSpray.Content = cuenta[2].ToString();
                lblIsocianato.Content = cuenta[3].ToString();
                lblAdhesivos.Content = cuenta[4].ToString();
                lblSinTipo.Content = cuenta[5].ToString();
            });
            
        }

        private static string stringBetween(string Source, string Start, string End)
        {
            string result = null;
            if (Source.Contains(Start) && Source.Contains(End))
            {
                int StartIndex = Source.IndexOf(Start, 0) + Start.Length;
                int EndIndex = Source.IndexOf(End, StartIndex);
                result = Source.Substring(StartIndex, EndIndex - StartIndex);
                return result;
            }

            return result;
        }
    }
}
