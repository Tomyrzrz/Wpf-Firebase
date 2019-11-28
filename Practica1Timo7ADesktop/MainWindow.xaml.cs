using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Firebase.Auth;
using Firebase.Storage;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.Win32;
using FirebaseConfig = FireSharp.Config.FirebaseConfig;

namespace Practica1Timo7ADesktop
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        //Crear lista de usuarios de la consulta
        List<Usuario> usuarios = new List<Usuario>();

        IFirebaseClient cliente;

        IFirebaseConfig configuracion = new FirebaseConfig
        {
            AuthSecret = "8fLXpsuRL4okAyLC4DILzvVKzNwuAQqFQhPWyLVN",
            BasePath = "https://timo7apractica1.firebaseio.com/"
        };
        
        //Imagen
        FileStream fs;
        Stream stream;
        byte[] a;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MyWindow_Loaded;
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            cliente = new FireSharp.FirebaseClient(configuracion);
            if(cliente != null)
            {
                MessageBox.Show("Conexion correcta");
            }
            consultar();

        }

        private async void Btn_insertar_Click(object sender, RoutedEventArgs e)
        {
            //Primero crear la ruta en la firebase
            FirebaseResponse response = await cliente.GetTaskAsync("Counter/node");
            Contador contar = response.ResultAs<Contador>();
            
            if (a != null) //Checa que hayas seleccionado una IMAGEN
            {
                string output = Convert.ToBase64String(a);   
                var datos = new Usuario
                {
                    Id = (Convert.ToInt32(contar.contador) + 1).ToString(),
                    Name = txt_nombre.Text,
                    Apellido_Paterno = txt_apellidop.Text,
                    Apellido_Materno = txt_apellidom.Text,
                    Img = output,                       //Imagen
                    Telefono = txt_telefono.Text
                };
                //Insertar el nuevo dato
                SetResponse respuesta = await cliente.SetTaskAsync("UsersDesktop/" + datos.Id, datos);
                Usuario result = respuesta.ResultAs<Usuario>();
                //Actualizar el ultimo Id
                var conta = new Contador
                {
                    contador = datos.Id
                };
                SetResponse resp = await cliente.SetTaskAsync("Counter/node", conta);
                MessageBox.Show("Insertado correctamente con id: " + result.Id);
                consultar();
                limpiar();
                imagen1.Source = null;
                a = null;
            }//final del if
            else
            {
                MessageBox.Show("Debes seleccionar una imagen");
            }
        }//final del metodo

        private async void consultar()
        {
            int i = 0;
            FirebaseResponse response = await cliente.GetTaskAsync("Counter/node");
            Contador contador_obj = response.ResultAs<Contador>();
            int counter = Convert.ToInt32(contador_obj.contador);
            usuarios.Clear();
            while (true)
            {
                if( i == counter)
                {
                    break;
                }
                i++;
                try
                {
                    FirebaseResponse consulta = await cliente.GetTaskAsync("UsersDesktop/" + i );
                    Usuario user = consulta.ResultAs<Usuario>();
                    usuarios.Add(new Usuario { Id = i.ToString(), 
                        Name = user.Name, 
                        Apellido_Materno = user.Apellido_Materno, 
                        Apellido_Paterno = user.Apellido_Paterno, 
                        Img = user.Img,             //Imagen
                        Telefono = user.Telefono});
                }
                catch
                {
                    //MessageBox.Show("No Tenemos Usuarios Registrados");
                }
            }
            dtg_datos.ItemsSource = null;
            dtg_datos.RowHeaderWidth = 20;
            dtg_datos.ColumnWidth = 110;
            dtg_datos.ItemsSource = usuarios;

        }

        private void limpiar()
        {
            txt_apellidom.Text = "";
            txt_apellidop.Text = "";
            txt_nombre.Text = "";
            txt_telefono.Text = "";
            txt_id.Text = "";
        }

        private void btn_consultar_Click(object sender, RoutedEventArgs e)
        {
            consultar();
        }

        private async void btn_actualizar_Click(object sender, RoutedEventArgs e)
        {

            if (a != null)
            {
                string output = Convert.ToBase64String(a);

                var datos = new Usuario
                {
                    Id = txt_id.Text,
                    Name = txt_nombre.Text,
                    Apellido_Paterno = txt_apellidop.Text,
                    Apellido_Materno = txt_apellidom.Text,
                    Telefono = txt_telefono.Text,
                    Img = output
                };

                FirebaseResponse actualiza = await cliente.UpdateTaskAsync("UsersDesktop/" + datos.Id, datos);
                Usuario dato = actualiza.ResultAs<Usuario>();
                MessageBox.Show("Usuario actualizado: " + dato.Id);
                limpiar();
                consultar();
                imagen2.Source = null;
                a = null;
            }
        }

        private async void btn_eliminar_Click(object sender, RoutedEventArgs e)
        {
            //Actualizar el campo ID
            string id = txt_id.Text;
            FirebaseResponse respuesta = await cliente.DeleteTaskAsync("UsersDesktop/" + id);
            MessageBox.Show("Usuario eliminado");
            consultar();
            limpiar();
            imagen2.Source = null;
        }

        //byte[] b;

        private void dtg_datos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txt_id.Text = ((Usuario)dtg_datos.SelectedItem).Id;
            txt_nombre.Text = ((Usuario)dtg_datos.SelectedItem).Name;
            txt_apellidop.Text = ((Usuario)dtg_datos.SelectedItem).Apellido_Paterno;
            txt_apellidom.Text = ((Usuario)dtg_datos.SelectedItem).Apellido_Materno;
            txt_telefono.Text = ((Usuario)dtg_datos.SelectedItem).Telefono;
            
            a = Convert.FromBase64String(((Usuario)dtg_datos.SelectedItem).Img);
            MemoryStream ms = new MemoryStream();
            ms.Write(a, 0, Convert.ToInt32(a.Length));
            //Bitmap bit = new Bitmap(ms, false);
            BitmapDecoder bitCoder = BitmapDecoder.Create(ms, 
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            imagen2.Source = bitCoder.Frames[0];

        }

        private void btn_cargarImagen_Click(object sender, RoutedEventArgs e)
        {
            //Objeto que sirve para abrir una ventana de dialogo
            OpenFileDialog OD = new OpenFileDialog();
            //Se le agrega un titulo a la ventana de dialogo
            OD.Title = "Select Image";
            //Se especifica el tipo de archivos que podemos seleccionar
            OD.Filter = "Image Files (*.*)|*.*";
            //Se comprueba que en la ventana se obtuvo un archivo 
            if (OD.ShowDialog() == true)
            {//Asignamos lo que seleccionamos en la ventana de dialogo al STREAM (la memoria)
                using (stream = OD.OpenFile())
                {//BitDecoder es un objeto que convierte el archivo a IMAGEN en PIXELFORMAT
                    BitmapDecoder bitCoder = BitmapDecoder.Create(stream, 
                        BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    //Se convierte la imagen en un BITMAP
                    System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    //Se convierte la imagen en BYTES
                    MemoryStream ms = new MemoryStream();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);
                    a = ms.GetBuffer();
                    //Se coloca la imagen en el componente imagen1
                    if(imagen2.Source != null)
                        imagen2.Source = bitCoder.Frames[0];
                    else 
                        imagen1.Source = bitCoder.Frames[0];
                }
                //Conversion de la imagen a BYTES
                fs = new FileStream(OD.FileName, FileMode.Open);
                byte[] foto = new byte[Convert.ToInt32(fs.Length.ToString())];
                fs.Read(foto, 0, foto.Length);
            }

        }
    }
}
