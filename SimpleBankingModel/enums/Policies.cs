using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankingModel.enums
{
    class Policies
    {
        internal enum GraphType
        {
            BarabasiAlbertGraph,
            RandomPowerlawTree,
            PowerlawClusterGraph,
            ScaleFree,
            ConnectedWattsStrogatzGraph,
            ErdosRenyi
        }
    }
}
