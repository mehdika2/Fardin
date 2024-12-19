using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fardin
{
	public static class MimeTypeHelper
	{
		private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ ".txt", "text/plain" },
			{ ".pdf", "application/pdf" },
			{ ".doc", "application/msword" },
			{ ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
			{ ".xls", "application/vnd.ms-excel" },
			{ ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
			{ ".jpg", "image/jpeg" },
			{ ".jpeg", "image/jpeg" },
			{ ".png", "image/png" },
			{ ".gif", "image/gif" },
			{ ".html", "text/html" },
			{ ".css", "text/css" },
			{ ".js", "application/javascript" },
			{ ".json", "application/json" },
			{ ".xml", "application/xml" },
			{ ".zip", "application/zip" },
			{ ".rar", "application/x-rar-compressed" },
			{ ".tar", "application/x-tar" },
			{ ".gz", "application/gzip" },
			{ ".bmp", "image/bmp" },
			{ ".svg", "image/svg+xml" },
			{ ".tiff", "image/tiff" },
			{ ".mp3", "audio/mpeg" },
			{ ".wav", "audio/wav" },
			{ ".mp4", "video/mp4" },
			{ ".avi", "video/x-msvideo" },
			{ ".mov", "video/quicktime" },
			{ ".mkv", "video/x-matroska" },
			{ ".ppt", "application/vnd.ms-powerpoint" },
			{ ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
			{ ".csv", "text/csv" },
			{ ".sql", "application/sql" },
			{ ".md", "text/markdown" },
			{ ".log", "text/plain" },
			{ ".woff", "font/woff" },
			{ ".woff2", "font/woff2" },
			{ ".eot", "application/vnd.ms-fontobject" },
			{ ".otf", "font/otf" },
			{ ".ttf", "font/ttf" },
			{ ".apk", "application/vnd.android.package-archive" },
			{ ".ico", "image/x-icon" },
			{ ".exe", "application/octet-stream" },
			{ ".dll", "application/octet-stream" },
			{ ".bin", "application/octet-stream" },
			{ ".iso", "application/octet-stream" },
			{ ".dmg", "application/octet-stream" },
			{ ".psd", "application/octet-stream" },
			{ ".ai", "application/postscript" },
			{ ".eps", "application/postscript" },
		};

		public static string GetMimeType(string filename)
		{
			string extension = Path.GetExtension(filename);
			if (extension != null && MimeTypes.TryGetValue(extension, out string mimeType))
			{
				return mimeType;
			}
			return "application/octet-stream"; // Default MIME type
		}
	}
}
