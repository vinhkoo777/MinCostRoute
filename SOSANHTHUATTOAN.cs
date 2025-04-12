using System.Diagnostics;

namespace BFS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Tạo đồ thị ngẫu nhiên lớn (100 đỉnh, 5000 cạnh)
            Graph g = new Graph();

            int numVertices = 500;
            int numEdges = 20000;
            Random rand = new Random();

            Console.WriteLine($"Tao do thi ngau nhien voi {numVertices} dinh va {numEdges} canh...");

            // Thêm đỉnh
            for (int i = 0; i < numVertices; i++)
                g.AddVertex(i.ToString());

            // Thêm cạnh ngẫu nhiên, trọng số từ 1–20
            HashSet<(int, int)> usedEdges = new HashSet<(int, int)>();
            int added = 0;
            while (added < numEdges)
            {
                int u = rand.Next(0, numVertices);
                int v = rand.Next(0, numVertices);
                int w = rand.Next(1, 21);
                if (u != v && !usedEdges.Contains((u, v)) && !usedEdges.Contains((v, u)))
                {
                    g.AddEdge(u, v, w);
                    usedEdges.Add((u, v));
                    added++;
                }
            }

            // Thêm heuristic cho A*
            for (int i = 0; i < numVertices; i++)
                g.SetHeuristic(i, rand.Next(0, 100));

            Stopwatch sw = new Stopwatch();

            Console.WriteLine("\n--- Do thoi gian thuc thi ---");

            sw.Start(); g.BFS(0); sw.Stop();
            Console.WriteLine($"BFS:            {sw.ElapsedMilliseconds} ms"); sw.Reset();

            sw.Start(); g.Dijkstra(0); sw.Stop();
            Console.WriteLine($"Dijkstra:       {sw.ElapsedMilliseconds} ms"); sw.Reset();

            sw.Start(); g.BellmanFord(0); sw.Stop();
            Console.WriteLine($"Bellman-Ford:   {sw.ElapsedMilliseconds} ms"); sw.Reset();

            sw.Start(); g.AStar(0, numVertices - 1); sw.Stop();
            Console.WriteLine($"A*:             {sw.ElapsedMilliseconds} ms"); sw.Reset();

            sw.Start(); g.FloydWarshall(); sw.Stop();
            Console.WriteLine($"Floyd-Warshall: {sw.ElapsedMilliseconds} ms");

            Console.WriteLine("\nHoan tat.");
        }

    }
    public class Vertex
    {
        public string Label;
        public bool WasVisited;
        public int Distance;
        public int Heuristic; // Dùng cho A*

        public Vertex(string label)
        {
            Label = label;
            WasVisited = false;
            Distance = int.MaxValue;
            Heuristic = 0;
        }
    }
    public class Graph
    {
        private const int MAX_VERTICES = 1000;
        private Vertex[] vertices;
        private int[,] adjMatrix;
        private int numVerts;

        public Graph()
        {
            vertices = new Vertex[MAX_VERTICES];
            adjMatrix = new int[MAX_VERTICES, MAX_VERTICES];
            numVerts = 0;

            for (int i = 0; i < MAX_VERTICES; i++)
                for (int j = 0; j < MAX_VERTICES; j++)
                    adjMatrix[i, j] = 0;
        }

        public void AddVertex(string label)
        {
            vertices[numVerts++] = new Vertex(label);
        }

        public void AddEdge(int start, int end, int weight)
        {
            adjMatrix[start, end] = weight;
            adjMatrix[end, start] = weight; // Bỏ dòng này nếu là đồ thị có hướng
        }

        public void SetHeuristic(int vertexIndex, int value)
        {
            vertices[vertexIndex].Heuristic = value;
        }

        private void ResetVisited()
        {
            for (int i = 0; i < numVerts; i++)
            {
                vertices[i].WasVisited = false;
                vertices[i].Distance = int.MaxValue;
            }
        }

        private int GetAdjUnvisitedVertex(int v)
        {
            for (int j = 0; j < numVerts; j++)
                if (adjMatrix[v, j] > 0 && !vertices[j].WasVisited)
                    return j;
            return -1;
        }

        public void BFS(int start)
        {
            ResetVisited();
            Queue<int> queue = new Queue<int>();
            vertices[start].WasVisited = true;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                int v;
                while ((v = GetAdjUnvisitedVertex(current)) != -1)
                {
                    vertices[v].WasVisited = true;
                    queue.Enqueue(v);
                }
            }
        }

        public void BellmanFord(int start)
        {
            ResetVisited();
            vertices[start].Distance = 0;

            for (int i = 0; i < numVerts - 1; i++)
            {
                for (int u = 0; u < numVerts; u++)
                {
                    for (int v = 0; v < numVerts; v++)
                    {
                        int weight = adjMatrix[u, v];
                        if (weight > 0 && vertices[u].Distance != int.MaxValue &&
                            vertices[u].Distance + weight < vertices[v].Distance)
                        {
                            vertices[v].Distance = vertices[u].Distance + weight;
                        }
                    }
                }
            }
        }

        public void Dijkstra(int start)
        {
            ResetVisited();
            vertices[start].Distance = 0;
            var pq = new SortedSet<(int dist, int vertex)>();
            pq.Add((0, start));

            while (pq.Count > 0)
            {
                var current = pq.Min;
                pq.Remove(current);
                int u = current.vertex;
                if (vertices[u].WasVisited) continue;

                vertices[u].WasVisited = true;

                for (int v = 0; v < numVerts; v++)
                {
                    int weight = adjMatrix[u, v];
                    if (weight > 0 && !vertices[v].WasVisited)
                    {
                        int newDist = vertices[u].Distance + weight;
                        if (newDist < vertices[v].Distance)
                        {
                            vertices[v].Distance = newDist;
                            pq.Add((newDist, v));
                        }
                    }
                }
            }
        }

        public void AStar(int start, int goal)
        {
            ResetVisited();
            int[] gScore = new int[numVerts];
            for (int i = 0; i < numVerts; i++) gScore[i] = int.MaxValue;
            gScore[start] = 0;

            var openSet = new SortedSet<(int fScore, int vertex)>();
            openSet.Add((vertices[start].Heuristic, start));

            while (openSet.Count > 0)
            {
                var current = openSet.Min;
                openSet.Remove(current);
                int u = current.vertex;

                if (u == goal) break;
                vertices[u].WasVisited = true;

                for (int v = 0; v < numVerts; v++)
                {
                    int weight = adjMatrix[u, v];
                    if (weight > 0 && !vertices[v].WasVisited)
                    {
                        int tentativeG = gScore[u] + weight;
                        if (tentativeG < gScore[v])
                        {
                            gScore[v] = tentativeG;
                            int fScore = tentativeG + vertices[v].Heuristic;
                            openSet.Add((fScore, v));
                        }
                    }
                }
            }

        }
        public void FloydWarshall()
        {
            int[,] dist = new int[numVerts, numVerts];

            // Khởi tạo khoảng cách ban đầu
            for (int i = 0; i < numVerts; i++)
            {
                for (int j = 0; j < numVerts; j++)
                {
                    if (i == j)
                        dist[i, j] = 0;
                    else if (adjMatrix[i, j] != 0)
                        dist[i, j] = adjMatrix[i, j];
                    else
                        dist[i, j] = int.MaxValue / 2; // Tránh overflow khi cộng
                }
            }

            // Floyd-Warshall
            for (int k = 0; k < numVerts; k++)
            {
                for (int i = 0; i < numVerts; i++)
                {
                    for (int j = 0; j < numVerts; j++)
                    {
                        if (dist[i, k] + dist[k, j] < dist[i, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                        }
                    }
                }
            }

            // Không cần in ra, vì bạn đang đo thời gian, nhưng có thể lưu dist lại nếu cần
        }
    }


}
    
        

   


