using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Fardin
{
	public class HttpCookie
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public DateTime? Expires { get; set; }
		public string Path { get; set; }
        public bool HttpOnly { get; set; }
		public bool Secure { get; set; }
		public SameSiteMode SameSite { get; set; }

		public HttpCookie(string name, string value, DateTime? expires = null)
		{
			Name = name;
			Value = value;
			Expires = expires;
			Path = "/";
			HttpOnly = true;
			Secure = false;
		}

		public HttpCookie(string name, string value, string path, SameSiteMode sameSite, bool httpOnly, bool secure, DateTime? expires = null)
		{
			Name = name;
			Value = value;
			Expires = expires;
			Path = path;
			HttpOnly = httpOnly;
			Secure = secure;
			SameSite = sameSite;
		}

		public override string ToString()
		{
			return $"{HttpUtility.UrlEncode(Name)}={HttpUtility.UrlEncode(Value)};{(Expires == null ? " " : " Expires=" + Expires?.ToString("R") + "; ")}" +
				$"Path={Path};{(HttpOnly ? " HttpOnly;" : "")}{(Secure ? " Secure;" : "")}" +
				$"{(SameSite.Equals(default(SameSiteMode)) ? "" : " SameSite=" + SameSite)}".TrimEnd(';');
		}
	}

	public enum SameSiteMode
	{
		Empty,
		None,
		Lax,
		Strict
	}
}
