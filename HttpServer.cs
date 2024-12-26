using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Fardin
{
    public class HttpServer
    {
        private const int BufferSize = 1024; // Read in chunks of 4KB
        private const int MaxRequestSize = 10 * 1024 * 1024; // Limit request size to 10 MB
        private const int ReceiveTimeoutMillis = 5000; // Timeout for receiving data

        public int Backlog { get; set; } = 5;
        public bool IsTlsSecure {  get { return _certificate != null; } }
		public string BaseDirectory { get; set; }

		Socket _server;
        X509Certificate2 _certificate;
        bool _started = false;
        string _address;
        int _port;

        public HttpServer(IPAddress address, int port)
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(new IPEndPoint(address, port));
            _address = address.ToString();
            _port = port;
        }

        public HttpServer(IPAddress address, int port, string certFile, string certPassword)
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(new IPEndPoint(address, port));
            _certificate = new X509Certificate2(certFile, certPassword);
            _address = address.ToString();
            _port = port;
		}

		public void Start()
        {
            _server.Listen(Backlog);
            _started = true;
        }

        public void Stop()
        {
            _server.Shutdown(SocketShutdown.Both);
            _started = false;
        }

        public HttpContext GetContext()
        {
            while (true)
            {
                if (!_started)
                    throw new HttpServerException("Server is not started, you must call Start() before getting context.");

                Socket client = _server.Accept();
                //client.ReceiveTimeout = ReceiveTimeoutMillis;

                var context = HandleClient(client);
                if (context == null)
                    continue;
                return context;
            }
        }

        private HttpContext HandleClient(Socket client)
        {
            while (true)
            {
                var totalBytesReceived = new List<byte>();
                if (_certificate == null)
                {
                    byte[] buffer = new byte[BufferSize];
                    int count;
                    while (true)
					{
						try
						{
                            if (totalBytesReceived.Count == 0)
                            {
                                if (!client.Connected)
                                    return null;

								if (!client.Poll(1000, SelectMode.SelectRead))
                                    break;
                            }

							count = client.Receive(buffer);
						}
						catch (SocketException)
						{
                            break;
						}

						// check is https or not
						if (totalBytesReceived.Count == 0 && count >= 5)
                            // Check if it matches the SSL/TLS handshake format
                            if (buffer[0] == 0x16 && // Handshake record
                                (buffer[1] == 0x03 && // SSL/TLS major version
                                (buffer[2] == 0x01 || buffer[2] == 0x02 || buffer[2] == 0x03 || buffer[2] == 0x04))) // TLS versions
                            {
                                client.Close();
                                return null;
                            }

                        // read bytes
                        if (count > 0)
                        {
                            byte[] validBytes = new byte[count];
                            Array.Copy(buffer, 0, validBytes, 0, count);
                            totalBytesReceived.AddRange(validBytes);
                            if (count > BufferSize) continue;
                        }
                        break;
                    }

                    if (totalBytesReceived.Count == 0)
                    {
                        client.Send(Encoding.UTF8.GetBytes("Accept"));
                        client.Close();
                        continue;
                    }

                    HttpContext context = new HttpContext(BaseDirectory);
					context.Request = HttpParser.Parse(totalBytesReceived.ToArray());

					var host = context.Request.Headers["Host"];
					host = host ?? context.Request.Headers["host"];
					context.Request.Items["R_URI"] = new Uri((IsTlsSecure ? "https" : "http") + $"://{host ?? (_address + ":" + _port)}" + context.Request.Items["R_PATH"].ToString());

					IPEndPoint clientRemoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
					context.Request.Items["R_IP_ADDRESS"] = clientRemoteEndPoint.Address.ToString();
					context.Request.Items["R_IP_PORT"] = clientRemoteEndPoint.Port;

                    SetUrlParameters(context.Request);

					context.Client = client;

					return context;
                }
                else
                {
                    var stream = new NetworkStream(client, true);
                    SslStream sslStream = new SslStream(stream, false);
                    try
                    {
                        sslStream.AuthenticateAsServer(_certificate, false, System.Security.Authentication.SslProtocols.Tls12, true);

                        if (!sslStream.IsAuthenticated)
                        {
                            sslStream.Close();
                            stream.Close();
                            client.Close();
                            return null;
                        }

                        var buffer = new byte[BufferSize];

                        while (true)
                        {
                            if (!client.Poll(1000, SelectMode.SelectRead))
                                break;
                            int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                totalBytesReceived.AddRange(buffer.AsSpan(0, bytesRead).ToArray());
                                if (totalBytesReceived.Count > MaxRequestSize)
                                {
                                    Console.WriteLine("Request size exceeded. Dropping connection.");
                                    break;
                                }
                            }
                            else break;
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Client disconnected or timeout occurred: {ex.Message}");
                        sslStream.Close();
                        stream.Close();
                        client.Close();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("SSL error: " + ex.Message);
                        sslStream.Close();
                        stream.Close();
                        client.Close();
                        return null;
                    }

                    if (totalBytesReceived.Count == 0)
                    {
                        byte[] responseBytes = Encoding.ASCII.GetBytes("Accept");
                        sslStream.Write(responseBytes, 0, responseBytes.Length);
                        sslStream.Flush();
                        return null;
                    }

                    HttpContext context = new HttpContext(BaseDirectory);
                    context.Request = HttpParser.Parse(totalBytesReceived.ToArray());

                    var host = context.Request.Headers["Host"];
                    host = host ?? context.Request.Headers["host"];
					context.Request.Items["R_URI"] = new Uri(IsTlsSecure ? "https" : "http") + $"://{host ?? (_address + ":" + _port)}" + context.Request.Items["R_PATH"].ToString();
					context.Request.Items["R_ADDRESS"] = (client.RemoteEndPoint as IPEndPoint).Address.ToString();

                    SetUrlParameters(context.Request);

					context.Client = client;
                    context.Response.NetowrkStream = stream;
                    context.Response.SecureStream = sslStream;

                    return context;
                }
            }
        }

        private void SetUrlParameters(HttpRequest request)
        {
            var uri = request.Items["R_URI"] as Uri;
            string[] urlSplited = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
            NameValueCollection parameters = new NameValueCollection();
			foreach (var part in urlSplited)
            {
                string[] parameter = part.Split('=');
                parameters.Add(parameter[0], parameter[1]);
			}
			request.Items["R_URL_PARAMETERS"] = parameters;
        }
    }
}
