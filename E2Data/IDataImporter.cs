using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace E2Data
{
    public interface IDataImporter
    {
        DataTable Import(String path);
    }
}
