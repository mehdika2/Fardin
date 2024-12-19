using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fardin
{
    class HttpServerException : Exception
    {
        public HttpServerException(string message)
        : base(message)
        {
        }

        public HttpServerException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
