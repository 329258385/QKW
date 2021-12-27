using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solarmax
{
    public interface IDataProvider
    {
        string      Path();

        void        Load();

        bool        IsXML();
    }
}
