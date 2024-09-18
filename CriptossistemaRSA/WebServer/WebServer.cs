using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Reflection.Metadata;

namespace CriptossistemaRSA
{
    public static class WebServer
    {
        //static Dictionary<Socket, string> clientes = new Dictionary<Socket, string>();
        static List<Cliente> clientes = new List<Cliente> ();
        public static void Server()
        {
            // Configurar o socket do servidor
            IPAddress ip = IPAddress.Parse("127.0.0.1"); // IP local
            IPEndPoint endPoint = new IPEndPoint(ip, 11000);
            Socket servidorSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                servidorSocket.Bind(endPoint);
                servidorSocket.Listen(10);
                Console.WriteLine("Servidor de chat aguardando conexões...");

                while (true)
                {
                    // Aceitar novos clientes
                    Socket clienteSocket = servidorSocket.Accept();
                    Console.WriteLine("Novo cliente conectado.");

                    // Criar uma nova thread para lidar com o cliente
                    Thread clienteThread = new Thread(HandleCliente);
                    clienteThread.Start(clienteSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // Método para lidar com cada cliente
        static void HandleCliente(object obj)
        {
            Socket clienteSocket = (Socket)obj;
            string nomeCliente = "";

            try
            {
                // Receber o nome do cliente
                byte[] nomeBytes = new byte[512];
                int bytesRecebidos = clienteSocket.Receive(nomeBytes);
                nomeCliente = Encoding.UTF8.GetString(nomeBytes, 0, bytesRecebidos).Trim();

                byte[] keyPublicBytes = new byte[512];

                int keyPublicReceive = clienteSocket.Receive(keyPublicBytes);
                string keyPublic = Encoding.UTF8.GetString(keyPublicBytes, 0, keyPublicReceive).Trim();

                // Adicionar o cliente ao dicionário com o nome
                clientes.Add(new Cliente(clienteSocket, nomeCliente, keyPublic));
                
                Console.WriteLine($"{nomeCliente} entrou no chat.");

                // Notificar todos os outros clientes que um novo cliente entrou
                EnviarMensagemParaTodos($"{nomeCliente} entrou no chat.", clienteSocket);

                while (true)
                {
                    // Receber dados do cliente
                    byte[] bytes = new byte[512];
                    int bytesRecebidosMensagem = clienteSocket.Receive(bytes);
                    string dados = Encoding.UTF8.GetString(bytes, 0, bytesRecebidosMensagem).Trim();

                    string mensagemComNome = $"{nomeCliente}: {dados}";
                    EnviarMensagemParaTodos(mensagemComNome, clienteSocket);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Erro ao lidar com cliente {nomeCliente}: " + ex.Message);
                clienteSocket.Close();
            }
        }

        static void EnviarMensagemParaTodos(string mensagem, Socket remetente)
        {

            foreach (var cliente in clientes)
            {
                byte[] mensagemBytes = SignatureDigital.CifraMessage(mensagem, cliente.PublicKey);

                if (cliente.Socket != remetente && cliente.Socket.Connected)
                {
                    try
                    {
                        cliente.Socket.Send(mensagemBytes);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Erro ao enviar mensagem para {cliente.Name}: " + ex.Message);
                    }
                }
            }
        }
    }

    public class Cliente
    {
        public Cliente(Socket socket, string name, string publicKey)
        {
            Socket = socket;
            Name = name;   
            PublicKey = publicKey;
        }

        public Socket Socket { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }
    } 
}
