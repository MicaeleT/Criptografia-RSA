using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ClienteChat
{
    static Socket clienteSocket;
    static string keyPrivate = "";
    static string keyPublic = "";

    static void Main()
    {
        // Configurar o endereço IP e porta do servidor
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(ip, 11000);
        clienteSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            // Conectar ao servidor
            clienteSocket.Connect(endPoint);
            Console.WriteLine("Conectado ao servidor de chat.");

            Console.WriteLine("Digite seu nome: ");
            var name = Console.ReadLine();
            byte[] nomeBytes = Encoding.UTF8.GetBytes(name);
            clienteSocket.Send(nomeBytes);

            (keyPublic, keyPrivate) = SignatureDigital.GenerateKeys();

            Console.WriteLine("Chave pública:\n" + keyPublic + "\n");
            Console.WriteLine("Chave privada:\n" + keyPrivate + "\n");

            byte[] keyPublicBytes = Encoding.UTF8.GetBytes(keyPublic);
            clienteSocket.Send(keyPublicBytes);

            // Iniciar uma nova thread para receber mensagens
            Thread receberThread = new Thread(ReceberMensagens);
            receberThread.Start();

            // Loop para enviar mensagens
            while (true)
            {
                string mensagem = Console.ReadLine();

                if (mensagem.ToLower() == "sair")
                {
                    byte[] mensagemBytes = Encoding.UTF8.GetBytes("sair");
                    clienteSocket.Send(mensagemBytes);
                    break;
                }

                byte[] mensagemEnviadaBytes = SignatureDigital.CifraMessage(mensagem, keyPublic);

                clienteSocket.Send(mensagemEnviadaBytes);
            }

            // Fechar o cliente
            clienteSocket.Shutdown(SocketShutdown.Both);
            clienteSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    // Método para receber mensagens de outros clientes via servidor
    static void ReceberMensagens()
    {
        while (true)
        {
            try
            {
                byte[] bytes = new byte[512];
                int bytesRecebidos = clienteSocket.Receive(bytes);

                byte[] mensagemCriptografada = new byte[bytesRecebidos];
                Array.Copy(bytes, mensagemCriptografada, bytesRecebidos);

                string mensagemRecebida = SignatureDigital.DecifraMessage(mensagemCriptografada, keyPrivate);

                Console.WriteLine("Mensagem recebida: " + mensagemRecebida);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao receber mensagem: " + ex.ToString());
                break;
            }
        }
    }
}
