using System.Collections.Generic;

namespace Fardin
{
    public class HttpRequestPartCollection : List<HttpRequestPart>
    {
        public void Add(HeaderCollection headers, byte[] content)
        {
            Add(new HttpRequestPart(headers, content));
        }
    }
}
