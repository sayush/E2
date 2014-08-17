using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace E2.Graph
{
    public interface IGraph
    {
        Dictionary<String, Object> GetLayout();
    }
}
