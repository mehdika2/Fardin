using System.Text;
using System.Net.Sockets;
using System.Xml;
using System.Collections.Specialized;

namespace Fardin
{
	public class HttpRequest
	{
		public Dictionary<string, object> Items = new Dictionary<string, object>();

		public string Method { get { return Items["R_HTTP_METHOD"].ToString(); } }
		public Uri Uri { get { return Items["R_URI"] as Uri; } }
		public string HttpVersion { get { return Items["R_HTTP_VERSION"].ToString(); } }
		public HeaderCollection Headers { get { return Items["R_HEADERS"] as HeaderCollection; } }
		public CookieCollection Cookies { get { return Items["R_COOKIES"] as CookieCollection; } }
		public byte[] Content
		{
			get
			{
				if (Items.ContainsKey("R_CONTENT"))
					return Items["R_CONTENT"] as byte[];
				return null;
			}
		}
		public NameValueCollection RequestParameters
		{
			get
			{
				if (Items.ContainsKey("R_PARAMETERS"))
					return Items["R_PARAMETERS"] as NameValueCollection;
				return null;
			}
		}
		public NameValueCollection UrlParameters
		{
			get
			{
				if (Items.ContainsKey("R_URL_PARAMETERS"))
					return Items["R_URL_PARAMETERS"] as NameValueCollection;
				return null;
			}
		}
		public HttpRequestPartCollection FormsData { get { return Items["R_FORMS_DATA"] as HttpRequestPartCollection; } }
		public bool IsMultipartRequest { get { return Headers.GetValue("Content-Type")?.StartsWith("multipart/form-data") ?? false; } }

		public object this[string key]
		{
			get
			{
				if (string.IsNullOrWhiteSpace(key) || !Items.ContainsKey(key))
					throw new KeyNotFoundException($"Key '{key}' not found.");
				return Items[key];
			}
		}

		public override string ToString()
		{
			return Method + " " + Uri.AbsolutePath;
		}
	}
}
