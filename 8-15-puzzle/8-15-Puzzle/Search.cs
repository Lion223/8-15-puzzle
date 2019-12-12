using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Puzzle
{
    class Search
    {
        #region Variables needed for algorithms work
        int[] node = new int[3];
        int[] info_node = new int[4];
        int step_in = 1;
        List<Node> PathToSolution;
        //List<Node> OpenList;
        //List<Node> ClosedList;
        HashSet<Node> ClosedList;
        Queue<Node> queue;
        FastPriorityQueue<Node> priorityQueue;
        Stack<Node> stack;
        Node current;
        bool first = true;
        Dictionary<int, int> depthCount = new Dictionary<int, int>();
        HeuristicAlgorithm heuristic = new HeuristicAlgorithm();
        //int countDict = 0;
        public enum HeuristicAlgorithm
        {
            Misplaced,
            Euclid,
            Manhattan,
            Chebyshev
        }
        #endregion

        public Search()
        {

        }

        // retrieve the info about pairs "level-state"
        public Dictionary<int, int> GetDepend()
        {
            return depthCount;
        }

        // BFS full auto
        public List<Node> BreadthFirstSearch(Node root, TextBox sTb, TextBox wTb, CancellationToken cancellationToken)
        {
            PathToSolution = new List<Node>();
            //ClosedList = new List<Node>();
            ClosedList = new HashSet<Node>();
            queue = new Queue<Node>();
            queue.Enqueue(root);
            //info_node[3] = 1;
            //depthCount.Add(info_node[3], info_node[0]);
            while (queue.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.Text = "Cancelled";
                    });
                    return PathToSolution;
                }

                var vertex = queue.Dequeue();
                ClosedList.Add(vertex);

                #region info
                vertex.ExpandMoveStack();
                info_node[0] += vertex.children.Count;
                info_node[3] = GetNodeDepth(vertex);
                if (!depthCount.ContainsKey(info_node[3]))
                    depthCount.Add(info_node[3], info_node[0]);
                else if (info_node[3] != 1 && info_node[3] != 2)
                    depthCount[info_node[3]] = info_node[0];
                #endregion
                if (vertex.IsFinish())
                {
                    PathTrace(PathToSolution, vertex);
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "Solution found" + Environment.NewLine);
                    });
                    return PathToSolution;
                }
                foreach (var child in vertex.children)
                {
                    if (!Contains(queue, child) && !Contains(ClosedList, child))
                    {
                        info_node[1]++;
                        queue.Enqueue(child);
                    }
                }
            }
            return PathToSolution;
        }

        // BFS step-to-auto
        public List<Node> BreadthFirstSearch(Node root, TextBox sTb, TextBox wTb, int step, bool bystep)
        {
            if (first)
            {
                first = false;
                PathToSolution = new List<Node>();
                //ClosedList = new List<Node>();
                ClosedList = new HashSet<Node>();
                queue = new Queue<Node>();
                queue.Enqueue(root);
                //info_node[3] = 1;
                //depthCount.Add(info_node[3], info_node[0]);
            }
            while (queue.Count > 0 && step_in <= step)
            {
                var vertex = queue.Dequeue();
                ClosedList.Add(vertex);

                #region info
                if (bystep)
                    vertex.PrintPuzzle(wTb);
                vertex.ExpandMoveStack();
                info_node[0] += vertex.children.Count;
                info_node[3] = GetNodeDepth(vertex);
                if (!depthCount.ContainsKey(info_node[3]))
                    depthCount.Add(info_node[3], info_node[0]);
                else if (info_node[3] != 1 && info_node[3] != 2)
                    depthCount[info_node[3]] = info_node[0];
                #endregion
                if (vertex.IsFinish())
                {
                    PathTrace(PathToSolution, vertex);
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "Solution found" + Environment.NewLine);
                    });
                    return PathToSolution;
                }
                foreach (var child in vertex.children)
                {
                    if (!Contains(queue, child) && !Contains(ClosedList, child))
                    {
                        info_node[1]++;
                        queue.Enqueue(child);
                    }
                }
                step_in++;
                if (queue.Count == 0)
                {
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "No solution found" + Environment.NewLine);
                    });
                }
            }
            return PathToSolution;
        }

        // DFS full auto
        public List<Node> DepthFirstSearch(Node root, TextBox sTb, TextBox wTb, int maxdepth, CancellationToken cancellationToken)
        {
            PathToSolution = new List<Node>();
            //ClosedList = new List<Node>();
            ClosedList = new HashSet<Node>();
            stack = new Stack<Node>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.Text = "Cancelled";
                    });
                    return PathToSolution;
                }
                var vertex = stack.Pop();
                ClosedList.Add(vertex);

                #region info
                vertex.ExpandMoveStack();
                info_node[0] += vertex.children.Count;
                info_node[3] = GetNodeDepth(vertex);
                if (!depthCount.ContainsKey(info_node[3]))
                    depthCount.Add(info_node[3], info_node[0]);
                else if (info_node[3] != 1 && info_node[3] != 2)
                    depthCount[info_node[3]] = info_node[0];
                #endregion
                if (vertex.IsFinish())
                {
                    PathTrace(PathToSolution, vertex);
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "Solution found" + Environment.NewLine);
                    });
                    return PathToSolution;
                }
                foreach (var child in vertex.children)
                {
                    if (!Contains(stack, child) && !Contains(ClosedList, child) && GetNodeDepth(child) <= maxdepth)
                    {
                        info_node[1]++;
                        stack.Push(child);
                    }
                }
            }
            return PathToSolution;
        }

        // DFS step-to-auto
        public List<Node> DepthFirstSearch(Node root, TextBox sTb, TextBox wTb, int step, bool bystep, int maxdepth)
        {
            if (first)
            {
                first = false;
                PathToSolution = new List<Node>();
                //ClosedList = new List<Node>();
                ClosedList = new HashSet<Node>();
                stack = new Stack<Node>();
                stack.Push(root);
            }
            while (stack.Count > 0 && step_in <= step)
            {
                var vertex = stack.Pop();
                ClosedList.Add(vertex);

                #region info
                if (bystep)
                    vertex.PrintPuzzle(wTb);
                vertex.ExpandMoveStack();
                info_node[0] += vertex.children.Count;
                info_node[3] = GetNodeDepth(vertex);
                if (!depthCount.ContainsKey(info_node[3]))
                    depthCount.Add(info_node[3], info_node[0]);
                else if (info_node[3] != 1 && info_node[3] != 2)
                    depthCount[info_node[3]] = info_node[0];
                #endregion
                if (vertex.IsFinish())
                {
                    PathTrace(PathToSolution, vertex);
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "Solution found" + Environment.NewLine);
                    });
                    return PathToSolution;
                }
                foreach (var child in vertex.children)
                {
                    if (!Contains(stack, child) && !Contains(ClosedList, child) && GetNodeDepth(child) <= maxdepth)
                    {
                        info_node[1]++;
                        stack.Push(child);
                    }
                }
                if (stack.Count == 0)
                {
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "No solution found" + Environment.NewLine);
                    });
                }
                step_in++;
            }
            return PathToSolution;
        }

        // A* full auto
        public List<Node> AStarSearch(Node root, TextBox sTb, TextBox wTb, int maxdepth, int heuristic_index, CancellationToken cancellationToken)
        {
            heuristic = SetHeuristic(heuristic_index);
            PathToSolution = new List<Node>();
            ClosedList = new HashSet<Node>();
            //List<Node> ClosedList = new List<Node>();
            priorityQueue = new FastPriorityQueue<Node>(100000000);
            //SimplePriorityQueue<Node> priorityQueue = new SimplePriorityQueue<Node>();
            priorityQueue.Enqueue(root, Heuristic(root, heuristic));
            //info_node[3] = 1;
            //depthCount.Add(info_node[3], info_node[0]);
            while (priorityQueue.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.Text = "Cancelled";
                    });
                    return PathToSolution;
                }
                var vertex = priorityQueue.Dequeue();
                current = vertex;
                ClosedList.Add(vertex);
                if (vertex.IsFinish())
                {
                    PathTrace(PathToSolution, vertex);
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "Solution found" + Environment.NewLine);
                    });
                    return PathToSolution;
                }

                vertex.ExpandMoveStack();
                #region info

                info_node[0] += vertex.children.Count;
                info_node[3] = GetNodeDepth(vertex);
                if (!depthCount.ContainsKey(info_node[3]))
                    depthCount.Add(info_node[3], info_node[0]);
                else if (info_node[3] != 1 && info_node[3] != 2)
                    depthCount[info_node[3]] = info_node[0];
                #endregion


                foreach (var child in vertex.children)
                {
                    if (!ClosedList.Contains(child) && GetNodeDepth(child) <= maxdepth/*&& !priorityQueue.Contains(child)*/)
                    {
                        info_node[1] = ClosedList.Count;
                        priorityQueue.Enqueue(child, Heuristic(child, heuristic));
                    }
                    else
                        info_node[0]--;
                    /*
                    if (!Contains(priorityQueue, child) && !Contains(ClosedList, child) && )
                    {
                        info_node[1]++;
                        priorityQueue.Enqueue(child, Heuristic(child, heuristic));
                    }
                    */
                }

                while (priorityQueue.Count > 0 && ClosedList.Contains(priorityQueue.First))
                {
                    priorityQueue.Dequeue();
                }
            }

            return PathToSolution;
        }

        // A* step-to-auto
        public List<Node> AStarSearch(Node root, TextBox sTb, TextBox wTb, int step, bool bystep, int maxdepth, int heuristic_index)
        {
            if (first)
            {
                first = false;
                heuristic = SetHeuristic(heuristic_index);
                PathToSolution = new List<Node>();
                //ClosedList = new List<Node>();
                ClosedList = new HashSet<Node>();
                priorityQueue = new FastPriorityQueue<Node>(500000);
                priorityQueue.Enqueue(root, Heuristic(root, heuristic));
            }
            while (priorityQueue.Count > 0 && step_in <= step)
            {
                var vertex = priorityQueue.Dequeue();
                ClosedList.Add(vertex);

                #region info
                if (bystep)
                    vertex.PrintPuzzle(wTb);
                vertex.ExpandMoveStack();
                info_node[0] += vertex.children.Count;
                info_node[3] = GetNodeDepth(vertex);
                if (!depthCount.ContainsKey(info_node[3]))
                    depthCount.Add(info_node[3], info_node[0]);
                else if (info_node[3] != 1 && info_node[3] != 2)
                    depthCount[info_node[3]] = info_node[0];
                #endregion
                if (vertex.IsFinish())
                {
                    PathTrace(PathToSolution, vertex);
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "Solution found" + Environment.NewLine);
                    });
                    return PathToSolution;
                }
                foreach (var child in vertex.children)
                {
                    if (!Contains(priorityQueue, child) && !Contains(ClosedList, child) && GetNodeDepth(child) <= maxdepth)
                    {
                        info_node[1]++;
                        priorityQueue.Enqueue(child, Heuristic(child, heuristic));
                    }
                }
                if (priorityQueue.Count == 0)
                {
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText(Environment.NewLine + "No solution found" + Environment.NewLine);
                    });
                }
                step_in++;
            }
            return PathToSolution;
        }

        // retrieve info (state)
        public int[] GetInfo()
        {
            info_node[2] = info_node[0] - info_node[1];
            return info_node;
        }

        // get full path
        public void PathTrace(List<Node> path, Node n)
        {
            Node curr = n;
            path.Add(curr);

            info_node[3] = 1;

            while (curr.parent != null)
            {
                curr = curr.parent;
                path.Add(curr);
                info_node[3]++;
            }
        }

        // get node depth
        public int GetNodeDepth(Node n)
        {
            List<Node> path = new List<Node>();
            Node curr = n;
            path.Add(curr);
            int depth = 1;

            while (curr.parent != null)
            {
                curr = curr.parent;
                path.Add(curr);
                depth++;
            }
            return depth;
        }

        // check for state duplicate
        public static bool Contains(List<Node> list, Node c)
        {
            bool contains = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsSame(c.puzzle))
                    contains = true;
            }
            return contains;
        }

        // check for state duplicate in FILO
        public static bool Contains(Queue<Node> list, Node c)
        {
            bool contains = false;
            foreach (var item in list)
            {
                if (item.IsSame(c.puzzle))
                    contains = true;
            }
            return contains;
        }

        // check for state duplicate in LIFO
        public static bool Contains(Stack<Node> list, Node c)
        {
            bool contains = false;
            foreach (var item in list)
            {
                if (item.IsSame(c.puzzle))
                    contains = true;
            }
            return contains;
        }

        // check for state duplicate in FILO with priority
        public static bool Contains(FastPriorityQueue<Node> list, Node c)
        {
            bool contains = false;
            foreach (var item in list)
            {
                if (item.IsSame(c.puzzle))
                    contains = true;
            }
            return contains;
        }

        // check for state duplicate in HashSet with priority
        public static bool Contains(HashSet<Node> list, Node c)
        {
            bool contains = false;
            foreach (var item in list)
            {
                if (item.IsSame(c.puzzle))
                    contains = true;
            }
            return contains;
        }

        public static bool Contains(SimplePriorityQueue<Node> list, Node c)
        {
            bool contains = false;
            foreach (var item in list)
            {
                if (item.IsSame(c.puzzle))
                    contains = true;
            }
            return contains;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 42;
                for (int i = 0; i < current.puzzle.Length; i++)
                {
                    hashCode = hashCode * 17 + current.puzzle[i];
                }
                hashCode = hashCode * 397 + 4;
                hashCode = hashCode * 397 + 4;
                return hashCode;
            }
        }

        public HeuristicAlgorithm SetHeuristic(int index)
        {
            if (index == 0)
                return HeuristicAlgorithm.Misplaced;
            else if (index == 1)
                return HeuristicAlgorithm.Euclid;
            else if (index == 2)
                return HeuristicAlgorithm.Manhattan;
            else if (index == 3)
                return HeuristicAlgorithm.Chebyshev;
            return 0;
        }

        public int Heuristic(Node node, HeuristicAlgorithm heuristic)
        {
            if (heuristic == HeuristicAlgorithm.Misplaced)
            {
                if (node.puzzle.Length == 9)
                {
                    int[] goal = { 1, 2, 3, 4, 5, 6, 7, 8, 0 };
                    int h1 = GetNodeDepth(node);
                    int h2 = 0;
                    int expected = 0;
                    for (int i = 0; i < node.puzzle.Length; i++)
                    {
                        expected++;
                        if (node.puzzle[i] != 0 && node.puzzle[i] != expected)
                            h2++;
                    }
                    return h1 + h2;
                }
                if (node.puzzle.Length == 16)
                {
                    int[] goal = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0 };
                    int h1 = GetNodeDepth(node);
                    int h2 = 0;
                    int expected = 0;
                    for (int i = 0; i < node.puzzle.Length; i++)
                    {
                        expected++;
                        if (node.puzzle[i] != 0 && node.puzzle[i] != expected)
                            h2++;
                    }
                    return h1 + h2;
                }
            }
            if (heuristic == HeuristicAlgorithm.Euclid)
            {
                if (node.puzzle.Length == 9)
                {
                    int[] goal = { 1, 2, 3, 4, 5, 6, 7, 8, 0 };
                    int h1 = GetNodeDepth(node);
                    int h2 = 0;
                    int expected = 0;
                    for (int i = 0; i < node.puzzle.Length; i++)
                    {
                        int val_curr = node.puzzle[i];
                        int val_goal = Array.IndexOf(goal, val_curr);
                        expected++;
                        if (val_curr != expected)
                        {
                            int row_curr = i / 3;
                            int col_curr = i % 3;
                            int row_goal = val_goal / 3;
                            int col_goal = val_goal % 3;
                            h2 += (row_curr - row_goal) * (row_curr - row_goal) + (col_curr - col_goal) * (col_curr - col_goal);
                        }
                    }
                    //return h1 + Convert.ToInt16(Math.Sqrt(Math.Pow(h2, 2.0)));
                    //return h1 + Convert.ToInt16(Math.Sqrt((h2)));
                    //return h1 + (h2 * h2);
                    return h1 + h2;
                }
                if (node.puzzle.Length == 16)
                {
                    int[] goal = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0 };
                    int h1 = GetNodeDepth(node);
                    int h2 = 0;
                    int expected = 0;
                    for (int i = 0; i < node.puzzle.Length; i++)
                    {
                        int val_curr = node.puzzle[i];
                        int val_goal = Array.IndexOf(goal, val_curr);
                        expected++;
                        if (val_curr != 0 && val_goal != expected)
                        {
                            int row_curr = i / 4;
                            int col_curr = i % 4;
                            int row_goal = val_goal / 4;
                            int col_goal = val_goal % 4;
                            h2 += Math.Abs(row_curr - row_goal) + Math.Abs(col_curr - col_goal);
                        }
                    }
                    //return h1 + Convert.ToInt16(Math.Sqrt(Math.Pow(h2, 2.0)));
                    //return h1 + Convert.ToInt16(Math.Sqrt((h2 * h2)));
                    return h1 + (h2 * h2);
                }

            }
            if (heuristic == HeuristicAlgorithm.Manhattan)
            {
                if (node.puzzle.Length == 9)
                {
                    int[] goal = { 1, 2, 3, 4, 5, 6, 7, 8, 0 };
                    int h1 = GetNodeDepth(node);
                    int h2 = 0;
                    int expected = 0;
                    for (int i = 0; i < node.puzzle.Length; i++)
                    {
                        int val_curr = node.puzzle[i];
                        int val_goal = Array.IndexOf(goal, val_curr);
                        expected++;
                        if (val_curr != 0 && val_curr != expected)
                        {
                            int row_curr = i / 3;
                            int col_curr = i % 3;
                            int row_goal = val_goal / 3;
                            int col_goal = val_goal % 3;
                            h2 += Math.Abs(row_curr - row_goal) + Math.Abs(col_curr - col_goal);
                        }
                    }
                    return h1 + h2;
                }
                if (node.puzzle.Length == 16)
                {
                    int[] goal = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0 };
                    int h1 = GetNodeDepth(node);
                    int h2 = 0;
                    int expected = 0;
                    for (int i = 0; i < node.puzzle.Length; i++)
                    {
                        int val_curr = node.puzzle[i];
                        int val_goal = Array.IndexOf(goal, val_curr);
                        expected++;
                        if (val_curr != 0 && val_goal != expected)
                        {
                            int row_curr = i / 4;
                            int col_curr = i % 4;
                            int row_goal = val_goal / 4;
                            int col_goal = val_goal % 4;
                            h2 += Math.Abs(row_curr - row_goal) + Math.Abs(col_curr - col_goal);
                        }
                    }
                    return h1 + h2;
                }
            }
            if (heuristic == HeuristicAlgorithm.Chebyshev)
            {
                if (node.puzzle.Length == 9)
                {
                    int[] goal = { 1, 2, 3, 4, 5, 6, 7, 8, 0 };
                    int h1 = GetNodeDepth(node);
                    int h2 = 0;
                    int expected = 0;
                    for (int i = 0; i < node.puzzle.Length; i++)
                    {
                        int val_curr = node.puzzle[i];
                        int val_goal = Array.IndexOf(goal, val_curr);
                        expected++;
                        if (val_curr != 0 && val_goal != expected)
                        {
                            int row_curr = i / 3;
                            int col_curr = i % 3;
                            int row_goal = val_goal / 3;
                            int col_goal = val_goal % 3;
                            //int temp = Math.Abs(Math.Abs(row_curr - row_goal) - Math.Abs(col_curr - col_goal));
                            h2 += Math.Max(Math.Abs(row_curr - row_goal), Math.Abs(col_curr - col_goal));
                        }
                    }
                    return h1 + h2;
                }
                if (node.puzzle.Length == 16)
                {
                    int[] goal = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0 };
                    int h1 = GetNodeDepth(node);
                    int h2 = 0;
                    int expected = 0;
                    for (int i = 0; i < node.puzzle.Length; i++)
                    {
                        int val_curr = node.puzzle[i];
                        int val_goal = Array.IndexOf(goal, val_curr);
                        expected++;
                        if (val_curr != 0 && val_goal != expected)
                        {
                            int row_curr = i / 4;
                            int col_curr = i % 4;
                            int row_goal = val_goal / 4;
                            int col_goal = val_goal % 4;
                            //int temp = Math.Abs(Math.Abs(row_curr - row_goal) - Math.Abs(col_curr - col_goal));
                            int temp = Math.Max(Math.Abs(row_curr - row_goal), Math.Abs(col_curr - col_goal));
                            if (h2 < temp)
                                h2 = temp;
                        }
                    }
                    return h1 + h2;
                }
            }
            return 0;
        }
    }
}
