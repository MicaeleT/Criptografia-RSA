
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Vonage;
using Vonage.Messaging;
using Vonage.Request;

public class SignatureDigital
{
    public static void Main(string[] args)
    {

        var (publicKeyBob, privateKeyBob) = GenerateKeys();

        var (publicKeyAlice, privateKeyAlice) = GenerateKeys();

        //SendKeyWhatsAppMessage(publicKeyBob);

        //SendKeyWhatsAppMessage(publicKeyAlice);

        string message = "Mensagem do bob de teste para assinatura digital";

        byte[] signature = Sign(message, privateKeyBob);
        Console.WriteLine("Assinatura de bob: " + Convert.ToBase64String(signature));

        bool isValid = VerifySignature(message, signature, publicKeyBob);
        Console.WriteLine("\nA assinatura de bob é válida? " + isValid);

        byte[] cifraMessage = CifraMessage(message, publicKeyBob);
        Console.WriteLine("\nMensagem Cifrada: " + Convert.ToBase64String(cifraMessage));

        string decifraMessage = DecifraMessage(cifraMessage, privateKeyBob);
        Console.WriteLine("\nMensagem Decifrada: " + decifraMessage);


    }

    public static (string publicKey, string privateKey) GenerateKeys()
    {
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

            return (publicKey, privateKey);
        }
    }

    public static void SendKeyWhatsAppMessage(string publicKey)
    {
        // Credenciais da Conta Twilio
        string accountSid = "ACaeb613b70eb965d42b9e360adabeabd9";
        string authToken = "fb2661455c5f9b148f8f041349c3aa18";      

        // Inicializar o cliente Twilio
        TwilioClient.Init(accountSid, authToken);

        var messageOptions = new CreateMessageOptions(
        new PhoneNumber("whatsapp:+558893359502"));
        messageOptions.From = new PhoneNumber("whatsapp:+14155238886");
        messageOptions.Body = $"Aqui está a chave pública RSA:\n\n{publicKey}";


        var message = MessageResource.Create(messageOptions);
        Console.WriteLine(message.Body);
    }

    public static byte[] Sign(string message, string privateKey)
    {
        byte[] messageH = Encoding.UTF8.GetBytes(message);
        byte[] privateKeyH = Convert.FromBase64String(privateKey);

        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPrivateKey(privateKeyH, out _);

            var signature = rsa.SignData(messageH, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return signature;
        }
    }

    public static bool VerifySignature(string message, byte[] signature, string publicKey)
    {
        byte[] messageH = Encoding.UTF8.GetBytes(message);
        byte[] publicKeyH = Convert.FromBase64String(publicKey);

        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPublicKey(publicKeyH, out _);

            return rsa.VerifyData(messageH, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }

    public static byte[] CifraMessage(string message, string publicKey)
    {
        byte[] messageH = Encoding.UTF8.GetBytes(message);
        byte[] publicKeyH = Convert.FromBase64String(publicKey);

        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPublicKey(publicKeyH, out _);

            var cifraMessage = rsa.Encrypt(messageH, RSAEncryptionPadding.Pkcs1);
            return cifraMessage;
        }
    }

    public static string DecifraMessage(byte[] cifraMessage, string privateKey)
    {
        byte[] privateKeyH = Convert.FromBase64String(privateKey);

        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPrivateKey(privateKeyH, out _);

            var decrifraMessage = rsa.Decrypt(cifraMessage, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decrifraMessage);
        }
    }
}
