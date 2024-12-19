using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fardin
{
    public class HeaderCollection : List<Header>
    {
        #nullable enable
        public string? this[string name]
		{
            get
            {
                if (this.Select(i => i.Name).Contains(name))
                    return this.First(i => i.Name == name).Value;
                return null;
            }
        }

        #region Dependent Properties
        #nullable enable
        public string? Accept
		{
            get
            {
                if(this.Select(i => i.Name).Contains("Accept"))
                    return this.First(i => i.Name == "Accept").Value;
                return null;
            }
        }

        #nullable enable
        public string? AcceptLanguage
        {
            get
            {
                if (this.Select(i => i.Name).Contains("Accept-Language"))
                    return this.First(i => i.Name == "Accept-Language").Value;
                return null;
            }
        }

        public string? AcceptEncoding
        {
            get
            {
                if (this.Select(i => i.Name).Contains("Accept-Encoding"))
                    return this.First(i => i.Name == "Accept-Encoding").Value;
                return null;
            }
        }
        #endregion

        public void Add(string name, string value)
        {
            Add(new Header(name, value));
        }

        public void Add(string name, string[] values)
        {
            Add(new Header(name, values));
        }

        public string? GetValue(string name, int index = 0)
        {
            return this.FirstOrDefault(header => string.Equals(header.Name, name, StringComparison.OrdinalIgnoreCase))?.Values[index];
        }

        public string[]? GetValues(string name)
        {
            return this.FirstOrDefault(header => string.Equals(header.Name, name, StringComparison.OrdinalIgnoreCase))?.Values.ToArray();
        }
    }
}
