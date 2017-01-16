using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDP_L7
{
    class Graph
    {
        public int numberOfVertices;
        public List<List<int>> adjacencyList;

        public Graph(int numberOfVertices)
        {
            this.numberOfVertices = numberOfVertices;

            adjacencyList = new List<List<int>>();
            for (int i = 0; i < numberOfVertices; i++)
            {
                adjacencyList.Add(new List<int>());
            }
        }


        public void addEdgeBetween(int vertex1, int vertex2)
        {
            List<int> neighbours1 = adjacencyList.ElementAt(vertex1);
            neighbours1.Add(vertex2);

            List<int> neighbours2 = adjacencyList.ElementAt(vertex2);
            neighbours2.Add(vertex1);
        }
    }
}
