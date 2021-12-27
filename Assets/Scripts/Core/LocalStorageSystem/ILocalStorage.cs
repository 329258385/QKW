using System;
using System.Collections.Generic;
using System.Text;

namespace Solarmax
{
    public interface ILocalStorage
    {
        string Name();

        void Save(LocalStorageSystem manager);

        void Load(LocalStorageSystem manager);
    }
}
