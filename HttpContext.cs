﻿using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Fardin
{
	public class HttpContext : IDisposable
	{
		const string version = "1.1";
		private string baseDirectory;

		public HttpContext(string baseDirecotry)
		{
			baseDirectory = baseDirecotry;
		}

		public HttpRequest Request { get; internal set; }
		public HttpResponse Response { get; set; } = new HttpResponse();
		public Socket Client { get; internal set; }

		public void Close()
		{
			byte[] oldBytes = CompressResponse(Response.ResponseStream.ToArray());

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"{Response.HttpVersion} {Response.StatusCode}{(Response.StatusText != string.Empty ? " " + Response.StatusText : "")}");

			// add response headers
			foreach (var header in Response.Headers)
				sb.AppendLine($"{header.Name}: {string.Join("; ", header.Values)}");

			// set content type
			if (Response.Headers.Any(i => i.Name.ToLower() == "content-type") && Response.Headers["content-type"] == "text/css")
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(baseDirectory, Request.Uri.AbsolutePath.Trim('/')));
				sb.AppendLine($"etag: {fileInfo.Length}-{fileInfo.LastWriteTimeUtc.Ticks}");
			}
			else if(!Regex.Match(sb.ToString(), "^content-type", RegexOptions.IgnoreCase | RegexOptions.Multiline).Success)
				sb.AppendLine("content-type: text/html; charset=utf-8");

			// add cokies
			foreach (var cookie in Response.Cookies)
				sb.AppendLine("set-cookie: " + cookie);

			// server version header
			sb.AppendLine($"server: Fardin/{version}");

			// content length
			sb.AppendLine("content-Length: " + oldBytes.Length);

			sb.AppendLine();

			byte[] newBytes = Encoding.UTF8.GetBytes(sb.ToString());
			byte[] mergedArray = new byte[newBytes.Length + oldBytes.Length];
			Array.Copy(newBytes, 0, mergedArray, 0, newBytes.Length);
			Array.Copy(oldBytes, 0, mergedArray, newBytes.Length, oldBytes.Length);

			if (Response.SecureStream == null)
				Client.Send(mergedArray);
			else
			{
				Response.SecureStream.Write(mergedArray);
				Response.SecureStream.Close();
				Response.NetowrkStream.Close();
			}

			Response.ResponseStream.Close();
			Client.Close();
		}

		private byte[] CompressResponse(byte[] bytes)
		{
			if (Request.Headers.AcceptEncoding == null)
				return bytes;

			string[] acceptEncodings = Request.Headers.AcceptEncoding.Split(',');
			foreach (string acceptEncoding in acceptEncodings)
				switch (acceptEncoding.Trim())
				{
					case "gzip":
						Response.Headers.Add("vary", "Accept-Encoding");
						Response.Headers.Add("content-encoding", acceptEncoding);
						return Compression.Gzip.CompressGzip(bytes);
					case "deflate":
						Response.Headers.Add("vary", "Accept-Encoding");
						Response.Headers.Add("content-encoding", acceptEncoding);
						return Compression.Deflate.CompressDeflate(bytes);
					case "br":
						Response.Headers.Add("vary", "Accept-Encoding");
						Response.Headers.Add("content-encoding", acceptEncoding);
						return Compression.Brotli.CompressBrotli(bytes);
				}
			return bytes;
		}

		private byte[] ReadAllBytes(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			// Ensure the stream is at the beginning
			if (stream.CanSeek)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}

			using (MemoryStream memoryStream = new MemoryStream())
			{
				// Buffer for reading
				byte[] buffer = new byte[4096]; // 4 KB buffer
				int bytesRead;

				// Read from the stream until no more bytes are available
				while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					memoryStream.Write(buffer, 0, bytesRead);
				}

				// Return the byte array
				return memoryStream.ToArray();
			}
		}

		public void Dispose()
		{
			Close();
		}
	}
}
