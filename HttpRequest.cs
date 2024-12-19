using System.Text;
using System.Net.Sockets;

namespace Fardin
{
    public class HttpRequest
    {
        public string Method { get; internal set; }
        public string Url { get; internal set; }
        public string HttpVersion { get; internal set; }
        public HeaderCollection Headers { get; internal set; } = new HeaderCollection();
        public byte[] Content { get; internal set; }
        public HttpRequestPartCollection FormDatas { get; internal set; }

        #region Dependent Properties
        public bool IsMultipartRequest
        {
            get
            {
                return Headers.GetValue("Content-Type")?.StartsWith("multipart/form-data") ?? false;
            }
        }
        #endregion

        public override string ToString()
        {
            return Method + " " + Url;
        }
    }
}
