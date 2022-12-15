using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;

namespace Seguimiento_de_Activos
{
    public partial class MainWindow : Window
    {
        List<Tarjeta> listaTarjetas = new List<Tarjeta>();

        //Diccionario cuyas claves son EUIs (nodos) y cuyos valores son colecciones de payloads
        Dictionary<string[], ObservableCollection<VisiblePayload>> sectores = new Dictionary<string[], ObservableCollection<VisiblePayload>>();

        static MqttClient client_tts;

        public MainWindow()
        {
            InitializeComponent();

            LeerTarjetas();
            LeerSectores();

            // Cada sector se enlaza a una colección del diccionario
            GridBloques1.ItemsSource = sectores.Values.ElementAt(0);   
            GridBloques2.ItemsSource = sectores.Values.ElementAt(1);
            GridAltaPresion.ItemsSource = sectores.Values.ElementAt(2);
            GridDeposito1.ItemsSource = sectores.Values.ElementAt(3);
           
            ConexionMQTT();
        }

        private void LeerTarjetas()
        {
            listaTarjetas.Clear();
            try
            {
                StreamReader archivo = new StreamReader("tarjetas.txt");
                string linea;

                while ((linea = archivo.ReadLine()) != null)    //Lee las tarjetas del archivo
                {
                    string[] fila = linea.Split(';');
                    listaTarjetas.Add(new Tarjeta(fila[0], fila[1], (Sustancia.Tipo)Convert.ToInt16(fila[2])));
                }

                archivo.Close();
            }
            catch (Exception) //En caso de error al leer el archivo
            {
                CargarTarjetas();
            }
        }

        private void CargarTarjetas()
        {
            //Carga algunas tarjetas
            listaTarjetas.Clear();
            listaTarjetas.Add(new Tarjeta("d3b662e4f683", "Tarjeta1", Sustancia.Tipo.Bloques));
            listaTarjetas.Add(new Tarjeta("c8060118acea", "Tarjeta2", Sustancia.Tipo.Inyeccion));
            listaTarjetas.Add(new Tarjeta("d7f6f5b916dc", "Tarjeta3", Sustancia.Tipo.Spray));

            //Genera el archivo
            StreamWriter archivo = new StreamWriter("tarjetas.txt");
            foreach (Tarjeta tarjeta in listaTarjetas)
                archivo.WriteLine(tarjeta.MAC + ";" + tarjeta.alias + ";" + ((int)tarjeta.tipo).ToString());
            archivo.Close();
        }

        private void LeerSectores()
        {
            try
            {
                StreamReader archivo = new StreamReader("sectores.txt");
                string linea;

                short i = 0;

                Label lblSector = new Label();

                while ((linea = archivo.ReadLine()) != null)    //Lectura del archivo
                {
                    string[] fila = linea.Split(';');
                    i++;
                    switch (i)
                    {
                        case 1:
                            lblSector = Sector1;
                            break;
                        case 2:
                            lblSector = Sector2;
                            break;
                        case 3:
                            lblSector = Sector3;
                            break;
                        case 4:
                            lblSector = Sector4;
                            break;
                        default:
                            break;
                    }
                    lblSector.Content = fila[0];                //Alias del sector
                    string[] EUIs = fila.Skip(1).ToArray();     //EUIs del sector
                    sectores.Add(EUIs, new ObservableCollection<VisiblePayload>()); // Crea una entrada del diccionario
                }
                archivo.Close();
            }
            catch (Exception) //Si no se pudo leer el archivo
            {
                CargarSectores();
            }
        }

        private void CargarSectores()
        {
            //Genera el archivo con alias y EUIs por defecto
            StreamWriter archivo = new StreamWriter("sectores.txt");
            archivo.WriteLine("Inyección Bloques 1;CA9877FFFFFCC461");
            archivo.WriteLine("Inyección Bloques 2;E93B4FFFFF0BC68D");
            archivo.WriteLine("Inyección Alta Presión;CC5DAFFFFF1A149D");
            archivo.WriteLine("Depósito;EA1D74FFFF0F25B5;FC355EFFFFCE8FCE;F54ED0FFFFF6E9C7;D8D9F5FFFF823969");
            archivo.Close();

            //Carga sectores al diccionario
            sectores.Add(new string[] { "CA9877FFFFFCC461" }, new ObservableCollection<VisiblePayload>());
            sectores.Add(new string[] { "E93B4FFFFF0BC68D" }, new ObservableCollection<VisiblePayload>());
            sectores.Add(new string[] { "CC5DAFFFFF1A149D" }, new ObservableCollection<VisiblePayload>());
            sectores.Add(new string[] { "EA1D74FFFF0F25B5", "FC355EFFFFCE8FCE" , "F54ED0FFFFF6E9C7", "D8D9F5FFFF823969" },
                                                              new ObservableCollection<VisiblePayload>());
        }

        //Boton Modificar Activo
        private void Modificar_Click(object sender, RoutedEventArgs e)
        {
            MostrarTarjetas mt = new MostrarTarjetas(listaTarjetas);
            mt.ShowDialog();
            if(mt.Modificacion == true)
            {
                StreamWriter archivo = new StreamWriter("tarjetas.txt");
                foreach (Tarjeta tarjeta in listaTarjetas)
                    archivo.WriteLine(tarjeta.MAC + ";" + tarjeta.alias + ";" + ((int)tarjeta.tipo).ToString());
                
                archivo.Close();
            }
        }

        private void Edit_Sector1(object sender, RoutedEventArgs e)
        {
            ModificarSector ms = new ModificarSector(false, Sector1.Content.ToString(), sectores.Keys.ElementAt(0));

            if(ms.ShowDialog() == true)
            {
                string[] rta = ms.Answer;
                string rtaS = string.Join(";", rta);

                string[] lineas = File.ReadAllLines("sectores.txt");
                lineas[0] = rtaS;
                File.WriteAllLines("sectores.txt", lineas);

                Sector1.Content = rta[0];
                ChangeKey(sectores,0, rta.Skip(1).ToArray());
            }
        }

        private void Edit_Sector2(object sender, RoutedEventArgs e)
        {
            ModificarSector ms = new ModificarSector(false, Sector2.Content.ToString(), sectores.Keys.ElementAt(1));
            if (ms.ShowDialog() == true)
            {
                string[] rta = ms.Answer;
                string rtaS = string.Join(";", rta);

                string[] lineas = File.ReadAllLines("sectores.txt");
                lineas[1] = rtaS;
                File.WriteAllLines("sectores.txt", lineas);

                Sector2.Content = rta[0];
                ChangeKey(sectores, 1, rta.Skip(1).ToArray());
            }
        }

        private void Edit_Sector3(object sender, RoutedEventArgs e)
        {
            ModificarSector ms = new ModificarSector(false, Sector3.Content.ToString(), sectores.Keys.ElementAt(2));
            if (ms.ShowDialog() == true)
            {
                string[] rta = ms.Answer;
                string rtaS = string.Join(";", rta);

                string[] lineas = File.ReadAllLines("sectores.txt");
                lineas[2] = rtaS;
                File.WriteAllLines("sectores.txt", lineas);

                Sector3.Content = rta[0];
                ChangeKey(sectores, 2, rta.Skip(1).ToArray());
            }
        }

        private void Edit_Sector4(object sender, RoutedEventArgs e)
        {
            ModificarSector ms = new ModificarSector(true, Sector4.Content.ToString(), sectores.Keys.ElementAt(3));
            if (ms.ShowDialog() == true)
            {
                string[] rta = ms.Answer;
                string rtaS = string.Join(";", rta);

                string[] lineas = File.ReadAllLines("sectores.txt");
                lineas[3] = rtaS;
                File.WriteAllLines("sectores.txt", lineas);

                Sector4.Content = rta[0];
                ChangeKey(sectores, 3, rta.Skip(1).ToArray());
            }
        }

        //Modifica la clave del diccionario en el índice especificado
        void ChangeKey(IDictionary<string[], ObservableCollection<VisiblePayload>> dic, int index, string[] newKey)
        {
            var keys = dic.Keys.ToArray(); //Coleccion con las claves originales
            dic.Clear();
           
            for(int i = 0; i < keys.Count(); i++)
            {
                if(i==index)
                    dic.Add(newKey, new ObservableCollection<VisiblePayload>());
                else
                    dic.Add(keys[i], new ObservableCollection<VisiblePayload>());
            }   

            //Vuelve a establecer el enlace con los DataGrid (ya se han borrado los Payloads iniciales)
            GridBloques1.ItemsSource = dic.Values.ElementAt(0);
            GridBloques2.ItemsSource = dic.Values.ElementAt(1);
            GridAltaPresion.ItemsSource = dic.Values.ElementAt(2);
            GridDeposito1.ItemsSource = dic.Values.ElementAt(3);
        }

        // Botón Configuracion
        private void Configurar_Click(object sender, RoutedEventArgs e)
        {
            Configuración c = new Configuración();
            c.ShowDialog();
        }

        // Al cerrar
        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode); // Prevent memory leak
            System.Windows.Application.Current.Shutdown();
        }
    }
}
