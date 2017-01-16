using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCLTemplate;
using Cloo;

namespace PDP_L7 {
    public class Program {

        public static void Main(string[] args)
        {
            // initialize graph
            Graph g1 = new Graph(4);
            g1.addEdgeBetween(0, 1);
            g1.addEdgeBetween(0, 2);
            g1.addEdgeBetween(0, 3);


            int[] coloringResult = new int[g1.numberOfVertices];

            // Assign the first color to first vertex
            coloringResult[0] = 0;

            // Initialize remaining numberOfVertices-1 vertices as unassigned
            for (int i = 1; i < g1.numberOfVertices; i++)
                coloringResult[i] = -1;  // no color is assigned to i


            // A temporary array to store the available colors. True
            // value of availableColors[i] would mean that the color i is
            // assigned to one of its neighbours
            // 0 - FALSE 1 - TRUE
            int[] availableColors = new int[g1.numberOfVertices];
            for (int i = 0; i < g1.numberOfVertices; i++)
            {
                availableColors[i] = 0;
            }


            int[] listsSizes = new int[g1.numberOfVertices];
            for (int i = 0; i < g1.numberOfVertices; i++)
            {
                List<int> neighbours = g1.adjacencyList.ElementAt(i);
                listsSizes[i] = neighbours.Count;
            }
            int maxSize = 0;
            for (int i = 0; i < g1.numberOfVertices; i++)
            {
                if (listsSizes[i] > maxSize)
                {
                    maxSize = listsSizes[i];
                }
            }


            // Create array with List<List<int>>
            // Complete with -1 where not enough neighbours
            List<int> neighboursToSend = new List<int>();
            for (int i = 0; i < g1.numberOfVertices; i++)
            {
                List<int> neighbours = g1.adjacencyList.ElementAt(i);
                int countElems = 0;
                for (int j = 0; j < neighbours.Count; j++) {
                    neighboursToSend.Add(neighbours.ElementAt(j));
                    countElems++;
                }
                while (countElems < maxSize)
                {
                    neighboursToSend.Add(-1);
                    countElems++;
                }
            }
            int[] neighboursToSendArr = new int[maxSize * g1.numberOfVertices];
            neighboursToSendArr = neighboursToSend.ToArray();


            string codeForOpenCl = @"
            __kernel void
            graphColoringKernel(__global       int * numberOfVerticesArr,
                              __global       int * coloringResult,
                              __global       int * availableColors,
                              __global       int * neighboursToSendArr,
                              __global       int * maxSizeArr)
            {
                // Vector element index
                int i = get_global_id(0) + 1;
                int forValue = i * maxSizeArr[0];
                for (int j = forValue; j < forValue + maxSizeArr[0]; j++)
                {
                    if (neighboursToSendArr[j] != -1)
                    {
                        if (coloringResult[neighboursToSendArr[j]] != -1)
                        {
                            availableColors[coloringResult[neighboursToSendArr[j]]] = 1;
                        }
                    }
                }

                int c;
                for (c = 0; c < numberOfVerticesArr[0]; c++)
                {
                    if (availableColors[c] == 0)
                    {
                        break;
                    }
                }

                coloringResult[i] = c;

                int resetValue = i * maxSizeArr[0];
                for (int j = resetValue; j < resetValue + maxSizeArr[0]; j++)
                {
                    if (neighboursToSendArr[j] != -1)
                    {
                        if (coloringResult[neighboursToSendArr[j]] != -1)
                        {
                            availableColors[coloringResult[neighboursToSendArr[j]]] = 0;
                        }
                    }
                }
            }";

            //Initializes OpenCL Platforms and Devices and sets everything up
            //OpenCLTemplate.CLCalc.InitCL();
            OpenCLTemplate.CLCalc.InitCL(ComputeDeviceTypes.Cpu);

            //Compiles the source codes. The source is a string array because the user may want
            //to split the source into many strings.
            OpenCLTemplate.CLCalc.Program.Compile(new string[] { codeForOpenCl });

            //Give entry point ~ "main"
            OpenCLTemplate.CLCalc.Program.Kernel graphColoringK = new OpenCLTemplate.CLCalc.Program.Kernel("graphColoringKernel");

            //Create the extra pointers we need
            int[] numberOfVerticesArr = new int[1] { g1.numberOfVertices };
            int[] maxSizeArr = new int[1] { maxSize };

            //Creates vectors v1 and v2 in the device memory
            OpenCLTemplate.CLCalc.Program.Variable varV1 = new OpenCLTemplate.CLCalc.Program.Variable(numberOfVerticesArr);
            OpenCLTemplate.CLCalc.Program.Variable varV2 = new OpenCLTemplate.CLCalc.Program.Variable(coloringResult);
            OpenCLTemplate.CLCalc.Program.Variable varV3 = new OpenCLTemplate.CLCalc.Program.Variable(availableColors);
            OpenCLTemplate.CLCalc.Program.Variable varV4 = new OpenCLTemplate.CLCalc.Program.Variable(neighboursToSendArr);
            OpenCLTemplate.CLCalc.Program.Variable varV5 = new OpenCLTemplate.CLCalc.Program.Variable(maxSizeArr);

            OpenCLTemplate.CLCalc.Program.Variable[] argss = new OpenCLTemplate.CLCalc.Program.Variable[] { varV1, varV2, varV3, varV4, varV5 };

            int numberOfWorkers = g1.numberOfVertices - 1;
            int[] workers = new int[1] { numberOfWorkers };

            //Execute the kernel
            graphColoringK.Execute(argss, workers);

            varV2.ReadFromDeviceTo(coloringResult);

            Console.WriteLine("Graph coloring result: ");
            for (int i = 0; i < coloringResult.Length; i++)
            {
                Console.WriteLine(coloringResult[i]);
            }

            Console.Read();
        }

    }
}
