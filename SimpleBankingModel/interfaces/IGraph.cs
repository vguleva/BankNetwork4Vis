using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using IronPython.Modules;
using IronPython.Runtime;
using log4net;
using Microsoft.Scripting.Hosting;
using SimpleBankingModel.classes;

namespace SimpleBankingModel.interfaces
{
    interface IGraph
    {
        /// <summary>
        /// Generate list of interbank edges
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edge> Generate();
    }

    public abstract class Graph// : IGraph
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(Graph));
        protected static ScriptRuntime Runtime;
        protected static string PathToNetworkX;
        //protected Graph(){}
        //public IList Generate(dynamic script)
        //{
        //    return new List();
        //}

        protected void InitEngine()
        {
            // prepare engine
            var info = new DirectoryInfo((Environment.CurrentDirectory));
            if (info.Parent != null)
                if (info.Parent.Parent != null)
                    PathToNetworkX = Path.Combine(info.Parent.Parent.FullName, "networkx-1.9.1");
            //const string pathToNetworkX = @"D:\Valenitna\Downloads\STATIC\";

            // host python
            ScriptEngine engine = Python.CreateEngine();
            ICollection<string> searchingPaths = engine.GetSearchPaths();
            if (!Directory.Exists(@"C:\Program Files (x86)\IronPython 2.7"))
            {
                Log.Fatal("You need IronPython 2.7 to install!");
                throw new Exception("You must have IronPython being installed");
            }
            searchingPaths.Add(@"C:\Program Files (x86)\IronPython 2.7\Lib");
            // add packages for script
            searchingPaths.Add(PathToNetworkX);
            searchingPaths.Add(Path.Combine(PathToNetworkX, "networkx"));
            searchingPaths.Add(@"d:\Valenitna\Documents\Projects VS2010\Financial Network Simulation\Financial Network Simulation\scipy-0.16.0\scipy");
            searchingPaths.Add(@"d:\Valenitna\Documents\Projects VS2010\Financial Network Simulation\Financial Network Simulation\numpy-1.9.2\numpy");
            // for further code: paths.Add(String.Concat(pathToNetworkX, @"networkx-1.9.1.tar\dist\networkx-1.9.1\networkx-1.9.1\networkx\generators"));
            engine.SetSearchPaths(searchingPaths);

            Runtime = engine.Runtime;
        }

    }

    class BarabasiAlbertGraph:Graph,IGraph
    {
        /// <summary>
        /// number of nodes in the network
        /// </summary>
        private readonly int _nodes;
        /// <summary>
        /// Number of edges to attach from a new node to existing nodes
        /// </summary>
        private readonly int _attached;

        public BarabasiAlbertGraph(int nodes, int attached)
        {
            base.InitEngine();
            _nodes = nodes;
            _attached = attached;
        }

        public IEnumerable<Edge> Generate()
        {
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\random_graphs.py")))
                Log.Error("Python script file does not exist!");
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\directed.py")))
                Log.Error("Python script file does not exist!");
            
            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, @"networkx\generators\random_graphs.py"));
            var tuples = (IList)script.barabasi_albert_graph(_nodes, _attached).edges();
            var newlist = new List<Edge>();
            return newlist;//(from PythonTuple tuple in tuples select new Edge("b" + (int) tuple[0], "b" + (int) tuple[1], 1, 3, 0))
                //.ToList();
        }
    }
    
    class RandomPowerlawTree:Graph,IGraph{
        public IEnumerable<Edge> Generate()
        {
            throw new NotImplementedException();
        }
    }
}
