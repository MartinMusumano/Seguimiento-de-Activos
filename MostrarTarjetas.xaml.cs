using System;
using System.Collections.Generic;
using System.Windows;


namespace Seguimiento_de_Activos
{
    /// <summary>
    /// Muestra las tarjetas y llama a ModificarTarjeta en caso de Click en un Item
    /// </summary>
    public partial class MostrarTarjetas : Window
    {
        public List<Tarjeta> listaNueva;
        bool modif = false;

        public MostrarTarjetas(List<Tarjeta> listaTarjetas)
        {
            InitializeComponent();

            listaNueva = listaTarjetas;         //Por referencia, la vuelve publica a toda la clase
            listaT.ItemsSource = listaNueva;     
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        //Indica si se modificó la lista de Tarjetas
        public bool Modificacion
        {
            get
            {
                return modif;
            }
        }

        //Al hacer click en un elemento
        private void listaT_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tarjeta item = listaT.SelectedItem as Tarjeta;
            if (item == null)
                return;

            string auxAlias = item.alias;
            string auxTipo = item.tipo.ToString();

            ModificarTarjeta mt = new ModificarTarjeta(item);
            mt.ShowDialog();

            if(item.alias != auxAlias || item.tipo.ToString() != auxTipo) //Si existió modificación
            {
                listaT.SelectedIndex = -1;
                listaT.ItemsSource = null;
                listaT.ItemsSource = listaNueva;    //Actualiza la presentacion de la lista
                modif = true;
            }
        }
    }
}
