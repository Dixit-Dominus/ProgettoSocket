using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatSocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Inizializzazione componenti
        public MainWindow()
        {
            InitializeComponent();
        }

        //Invio del messaggio
        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            //Recuperato indirizzo ip destinatario e la sua porta.
            IPAddress destIpAddress = IPAddress.Parse(txtIp.Text);
            int destinationPortNumber = int.Parse(txtPorta.Text);

            //Esecuzione metodo di invio del messaggio.
            SocketSend(destIpAddress, destinationPortNumber, txtMex.Text);
        }

        //Crazione socket di ascolto
        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            //Definizione endpoint della macchina trasmittente
            IPEndPoint sourceSocket = new IPEndPoint(IPAddress.Parse("10.73.0.6"), 56000);

            //Abilitazione della finestra di invio e ricezione messaggi.
            btnInvia.IsEnabled = true;
            txtMex.IsEnabled = true;
            btnCreaSocket.IsEnabled = false;

            //Creazione del thread di ascolto del canale.
            Thread ricezione = new Thread(new ParameterizedThreadStart(SocketReceive));
        }

        //Metodo di ricezione del socket. (Gestito dal thread quindi mettendo async non si blocca l'interfaccia grafica.)
        private async void SocketReceive(object socketSource)
        {
            //Definizione dell'endPoint di ascolto
            IPEndPoint listenerEndPoint = socketSource as IPEndPoint;
            
            //Creazione socket di ascolto
            Socket socket = new Socket(listenerEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(listenerEndPoint);

            //Messaggio grande massimo 256 caratteri.
            Byte[] byteRicevuti = new byte[256];
            string messaggio = string.Empty;
            int nCaratteriRicevuti;

            //Istruzione che non va a bloccare la task della finesta (Non si blocca l'interfaccia)
            await Task.Run(() =>
            {
                //Ascolto continuo
                while (true)
                {
                    //Se c'è qualcosa di ricevuto nel socket.
                    if (socket.Available > 0)
                    {
                        messaggio = string.Empty;
                        //Otteniamo il numero di caratteri ricevuto.
                        nCaratteriRicevuti = socket.Receive(byteRicevuti, byteRicevuti.Length, 0);
                        //Trasformiamo il numero ricevuto nella codifica ascii e otteniamo quindi un messaggio di stringa.
                        messaggio += Encoding.ASCII.GetString(byteRicevuti, 0, nCaratteriRicevuti);
                        //Viene aggiornata l'interfaccia grafica in modo asincrono.
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            lblRicevi.Content = messaggio;
                        }));
                    }
                }
            });
        }

        private void SocketSend(IPAddress destinationIp, int destinationPort, string message)
        {
            //Codifica in byte del messaggio da inviare
            Byte[] byteInviati = Encoding.ASCII.GetBytes(message.ToCharArray());
            
            //Creazione socket di invio
            Socket socket = new Socket(destinationIp.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            //Creazione invio socket di ascolto
            IPEndPoint destinationEndPoint = new IPEndPoint(destinationIp, destinationPort);
            
            //Invio dei dati.
            socket.SendTo(byteInviati, destinationEndPoint);
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool confirm1 = false;
            bool confirm2 = false;
            if (!string.IsNullOrWhiteSpace(txtIp.Text))
            {
                confirm1 = true;
            }
            if (!string.IsNullOrWhiteSpace(txtPorta.Text))
            {
                confirm2 = true;
            }
            if (confirm2 && confirm1)
            {
                btnCreaSocket.IsEnabled = true;
            }
            else
            {
                btnCreaSocket.IsEnabled = false;
            }
        }
    }
}
