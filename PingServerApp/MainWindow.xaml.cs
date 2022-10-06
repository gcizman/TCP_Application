using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
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


namespace PingServerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

           
        }

        // declaring some local variables and method for clicking buttons.

        bool eco = false;
        async void OnClick1(object sender, RoutedEventArgs e)
        {
            // add to datagrid
            dataGrid.Items.Clear();
            await TcpRequest();
        }

       

        void OnClick2(object sender, RoutedEventArgs e)
        {
            eco = false;
        }
        class Body { public Guid Id { get; set; } }

        // class for model

        public class TCPConnectionModel
        {
            public TimeOnly Started { get; set; }
            public TimeOnly Processed { get; set; }
            public TimeSpan Elapsed { get { return Processed - Started; } set { } }
        }

        // TCPRequest for body and serialize and deserialize json data

        async Task TcpRequest()
        {
            try
            {

                // initiate tcp
                IPHostEntry IpHostInfo = Dns.GetHostEntry("127.0.0.1");
                IPAddress IpAddress = IpHostInfo.AddressList[1];

                // initiate endPoint
                var endPoint = new IPEndPoint(IpAddress, 12345);

                // starting tcp client
                using TcpClient Client = new TcpClient();
                await Client.ConnectAsync(endPoint);
                await using NetworkStream stream = Client.GetStream();
                eco = true;
                while (eco)
                {

                    // start timer
                    TCPConnectionModel displayData = new() { Started = TimeOnly.FromDateTime(DateTime.Now) };

                    // initiate send data
                    Body requestBody = new() { Id = Guid.NewGuid() };

                    //initiate recieve data
                    var buffer = new byte[1_024];

                    // Json Serialize
                    string jsonString = JsonSerializer.Serialize(requestBody);

                    // tcp send
                    await stream.WriteAsync(Encoding.ASCII.GetBytes(jsonString));

                    // tcp recieve and decode
                    int received = await stream.ReadAsync(buffer);
                    var message = Encoding.UTF8.GetString(buffer, 0, received);

                    // json Deserialize
                    Body? recievedBody = JsonSerializer.Deserialize<Body>(message);

                    // stop timer
                    displayData.Processed = TimeOnly.FromDateTime(DateTime.Now);

                    // delay for 500ms
                    Thread.Sleep(500);

                    // add to datagrid
                    dataGrid.Items.Add(displayData);

                    // display result in status bar
                    resultveiw.Text = message;
                }
            }
            catch (Exception ex)
            {
                resultveiw.Text = ex.Message;
            }
        }
    }
}
