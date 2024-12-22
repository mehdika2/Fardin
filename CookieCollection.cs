using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fardin
{
	public class CookieCollection : List<HttpCookie>
	{
		public void AddCookie(HttpCookie cookie)
		{
			Add(cookie);
		}

		public void RemoveCookie(string name)
		{
			Add(new HttpCookie(name, "", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
		}

		public override string ToString()
		{
			var cookieStrings = new List<string>();
			foreach (var cookie in this)
				cookieStrings.Add(cookie.ToString());
			return string.Join(", ", cookieStrings);
		}
	}
}
