using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Fardin
{
    internal static class HttpParser
    {
        public static HttpRequest Parse(byte[] buffer)
        {
            var requestString = Encoding.UTF8.GetString(buffer);
            var requestLines = requestString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            var request = new HttpRequest();

            // Parse request line
            var requestLineParts = requestLines[0].Split(' ');
            if (requestLineParts.Length >= 3)
            {
                request.Items["R_HTTP_METHOD"] = requestLineParts[0];
                request.Items["R_PATH"] = requestLineParts[1];
                request.Items["R_HTTP_VERSION"] = requestLineParts[2];
            }

            var headers = new HeaderCollection();

            int i = 1;
            for (; i < requestLines.Length && requestLines[i] != ""; i++)
				headers.Add(Header.Parse(requestLines[i]));

            if (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH")
            {
                var bodyStartIndex = requestString.IndexOf("\r\n\r\n") + 4;
                if (headers.GetValue("Content-Type")?.StartsWith("multipart/form-data") ?? false)
                {
                    request.Items["R_CONTENT"] = buffer.Skip(bodyStartIndex).ToArray();

                    var boundary = headers.GetValue("Content-Type", 1).Split('=')[1];
                    var boundaryBytes = Encoding.ASCII.GetBytes("--" + boundary);
                    var parts = SplitByBoundary(request.Content, boundaryBytes);

                    request.Items["R_FORMS_DATA"] = (HttpRequestPartCollection)parts;
                }
                else request.Items["R_CONTENT"] = buffer.Skip(bodyStartIndex).ToArray();
			}

            var cookies = new CookieCollection();

            foreach(var header in headers.Where(i => string.Equals("cookie", i.Name, StringComparison.OrdinalIgnoreCase)))
            {
                string cookie = header.Value;
                string[] cookiesPart = cookie.Split(";");
                foreach(var theCookie in cookiesPart)
                {
                    string[] cookiePart = theCookie.Trim(' ').Split('=');
                    cookies.AddCookie(new HttpCookie(HttpUtility.UrlDecode(cookiePart[0]), HttpUtility.UrlDecode(cookiePart[1])));
                }
            }

			request.Items["R_HEADERS"] = headers;
			request.Items["R_COOKIES"] = cookies;

			return request;
        }

        private static List<HttpRequestPart> SplitByBoundary(byte[] body, byte[] boundary)
        {
            var parts = new List<HttpRequestPart>();
            var start = 0;
            while (start < body.Length)
            {
                var end = IndexOf(body, boundary, start);
                if (end == -1)
                {
                    break;
                }

                var content = body.Skip(start).Take(end - start - 2).ToArray();
                if (content.Length > 2)
                {
                    var headerContentSeparator = Encoding.UTF8.GetBytes("\r\n\r\n");
                    int separatorIndex = IndexOf(content, headerContentSeparator, 0);
                    if (separatorIndex == -1)
                    {
                        HeaderCollection headers = new HeaderCollection();
                        string contentString = Encoding.UTF8.GetString(content);
                        string[] contentLines = contentString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        for (int i = 0; i < contentLines.Length && contentLines[i] != ""; i++)
                            headers.Add(Header.Parse(contentLines[i]));

                        var part = new HttpRequestPart
                        {
                            Content = new byte[0],
                            Headers = headers
                        };
                        parts.Add(part);
                    }
                    else
                    {
                        byte[] contentBytes = new byte[content.Length - separatorIndex - 4];
                        Array.Copy(content, separatorIndex + 4, contentBytes, 0, contentBytes.Length);

                        byte[] headersBytes = new byte[separatorIndex];
                        Array.Copy(content, headersBytes, separatorIndex);
                        HeaderCollection headers = new HeaderCollection();
                        string contentString = Encoding.UTF8.GetString(headersBytes);
                        string[] contentLines = contentString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        for (int i = 0; i < contentLines.Length && contentLines[i] != ""; i++)
                            headers.Add(Header.Parse(contentLines[i]));

                        var part = new HttpRequestPart
                        {
                            Content = contentBytes,
                            Headers = headers
                        };
                        parts.Add(part);
                    }
                }

                start = end + boundary.Length + 2; // Skip the boundary and the trailing "\r\n"
            }
            return parts;
        }

        static int IndexOf(byte[] source, byte[] pattern, int start)
        {
            for (int i = start; i < source.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
