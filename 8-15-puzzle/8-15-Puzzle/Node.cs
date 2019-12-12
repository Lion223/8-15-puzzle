using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Puzzle
{
    class Node : FastPriorityQueueNode
    {
        #region Variables needed for algorithms work
        public List<Node> children = new List<Node>();
        public Node parent;
        public int[] puzzle;
        public int x = 0;
        public int col;
        #endregion

        public Node(int[] p)
        {
            SetPuzzle(p);
        }

        public void SetPuzzle(int[] p)
        {
            if (p.Length == 9)
            {
                puzzle = new int[9];
                for (int i = 0; i < puzzle.Length; i++)
                    this.puzzle[i] = p[i];
                col = 3;
            }
            if (p.Length == 16)
            {
                puzzle = new int[16];
                for (int i = 0; i < puzzle.Length; i++)
                    this.puzzle[i] = p[i];
                col = 4;
            }

        }

        // define rules for transition in FILO
        public void ExpandMoveQueue()
        {
            for (int i = 0; i < puzzle.Length; i++)
            {
                if (puzzle[i] == 0)
                {
                    x = i;
                    break;
                }
            }

            MoveDown(puzzle, x);
            MoveRight(puzzle, x);
            MoveLeft(puzzle, x);
            MoveUp(puzzle, x);
        }

        // define rules for transition in LIFO
        public void ExpandMoveStack()
        {
            for (int i = 0; i < puzzle.Length; i++)
            {
                if (puzzle[i] == 0)
                {
                    x = i;
                    break;
                }
            }
            MoveUp(puzzle, x);
            MoveLeft(puzzle, x);
            MoveRight(puzzle, x);
            MoveDown(puzzle, x);
        }

        public void MoveRight(int[] p, int i)
        {
            if (i % col < col - 1)
            {
                if (p.Length == 9)
                {
                    int[] c = new int[9];
                    CopyPuzzle(c, p);

                    int temp = c[i + 1];
                    c[i + 1] = c[i];
                    c[i] = temp;
                    Node child = new Node(c);
                    children.Add(child);
                    child.parent = this;
                }
                if (p.Length == 16)
                {
                    int[] c = new int[16];
                    CopyPuzzle(c, p);

                    int temp = c[i + 1];
                    c[i + 1] = c[i];
                    c[i] = temp;
                    Node child = new Node(c);
                    children.Add(child);
                    child.parent = this;
                }
            }
        }

        public void MoveLeft(int[] p, int i)
        {
            if (i % col > 0)
            {
                if (p.Length == 9)
                {
                    int[] c = new int[9];
                    CopyPuzzle(c, p);

                    int temp = c[i - 1];
                    c[i - 1] = c[i];
                    c[i] = temp;
                    Node child = new Node(c);
                    children.Add(child);
                    child.parent = this;
                }
                if (p.Length == 16)
                {
                    int[] c = new int[16];
                    CopyPuzzle(c, p);

                    int temp = c[i - 1];
                    c[i - 1] = c[i];
                    c[i] = temp;
                    Node child = new Node(c);
                    children.Add(child);
                    child.parent = this;
                }
            }
        }

        public void MoveUp(int[] p, int i)
        {
            if (i - col >= 0)
            {
                if (p.Length == 9)
                {
                    int[] c = new int[9];
                    CopyPuzzle(c, p);

                    int temp = c[i - 3];
                    c[i - 3] = c[i];
                    c[i] = temp;
                    Node child = new Node(c);
                    children.Add(child);
                    child.parent = this;
                }
                if (p.Length == 16)
                {
                    int[] c = new int[16];
                    CopyPuzzle(c, p);

                    int temp = c[i - 3];
                    c[i - 3] = c[i];
                    c[i] = temp;
                    Node child = new Node(c);
                    children.Add(child);
                    child.parent = this;
                }
            }
        }

        public void MoveDown(int[] p, int i)
        {
            if (i + col < puzzle.Length)
            {
                if (p.Length == 9)
                {
                    int[] c = new int[9];
                    CopyPuzzle(c, p);

                    int temp = c[i + 3];
                    c[i + 3] = c[i];
                    c[i] = temp;
                    Node child = new Node(c);
                    children.Add(child);
                    child.parent = this;
                }
                if (p.Length == 16)
                {
                    int[] c = new int[16];
                    CopyPuzzle(c, p);

                    int temp = c[i + 3];
                    c[i + 3] = c[i];
                    c[i] = temp;
                    Node child = new Node(c);
                    children.Add(child);
                    child.parent = this;
                }
            }
        }

        // print out the puzzle
        public void PrintPuzzle(TextBox sTb)
        {
            sTb.Invoke((MethodInvoker)delegate
            {
                sTb.AppendText(Environment.NewLine);
            });
            int m = 0;
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sTb.Invoke((MethodInvoker)delegate
                    {
                        sTb.AppendText("[" + puzzle[m] + "]" + '\t');
                    });
                    m++;
                }
                sTb.Invoke((MethodInvoker)delegate
                {
                    sTb.AppendText(Environment.NewLine);
                });
            }
        }

        // check for identity between states of puzzles
        public bool IsSame(int[] p)
        {
            bool same = true;
            for (int i = 0; i < p.Length; i++)
            {
                if (puzzle[i] != p[i])
                {
                    same = false;
                    break;
                }
            }
            return same;
        }

        // copy state
        public void CopyPuzzle(int[] a, int[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                a[i] = b[i];
            }
        }

        // check state for finish state
        public bool IsFinish()
        {
            bool isFinished = true;
            int m = puzzle[0];
            int zero = 0;
            if (puzzle.Length == 9)
                zero = puzzle[8];
            if (puzzle.Length == 16)
                zero = puzzle[15];
            if (zero != 0)
            {
                isFinished = false;
                return isFinished;
            }


            for (int i = 1; i < puzzle.Length - 1; i++)
            {
                if (m > puzzle[i])
                    isFinished = false;
                m = puzzle[i];
            }
            return isFinished;
        }
    }
}
