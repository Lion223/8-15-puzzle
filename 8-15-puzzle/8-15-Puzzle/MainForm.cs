using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Puzzle
{
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {
        class GFW
        {
            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();
            public static bool IsActive(IntPtr handle)
            {
                IntPtr activeHandle = GetForegroundWindow();
                return (activeHandle == handle);
            }
        }

        private enum FormStyle
        {
            Blue,
            Red
        }

        int[] puzzle8 = new int[9];
        int[] puzzle15 = new int[16];
        #region Variables needed for algorithms work
        int[] info = new int[4];
        bool firststep = true;
        bool bystep = false;
        int step = 0;
        int astep = 0;
        int maxdepth = 0;
        Node initNode;
        Search search;
        List<Node> solution;
        Stopwatch sw = new Stopwatch();
        bool fullexec = false;
        Dictionary<string, int> infoCount = new Dictionary<string, int>();
        Dictionary<int, int> dependCount = new Dictionary<int, int>();
        Dictionary<int, int> optimalDepth = new Dictionary<int, int>();
        int algochoice;
        int puzzlemode = 8;
        int heuristic;
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token;

        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            puzzleTc.SelectedIndex = 0;
            token = cancelTokenSource.Token;
            algorithmCb.SelectedIndex = 0;
            depthCb.SelectedIndex = 30;
            heuristicCb.SelectedIndex = 0;
            GetPuzzle(puzzle8);
            GetPuzzle(puzzle15);
            IsSolvable(puzzle8);
            IsSolvable(puzzle15);
            ToolTipMain.SetToolTip(depthCb, "Max. search depth");
            ToolTipMain.SetToolTip(stepBtn, "Step-by-step execution");
            ToolTipMain.SetToolTip(optLbl, "d - tree level;" + Environment.NewLine + "s - generated state count on this level");
            ToolTipMain.SetToolTip(testTb, "Resolvability indicator");

        }

        // check for resolvability
        public void IsSolvable(int[] p)
        {
            List<int> linearpuzzle = new List<int>();
            int inversions = 0;
            for (int i = 0; i < p.Length; i++)
                if (p[i] != 0)
                    linearpuzzle.Add(p[i]);

            for (int i = 0; i < linearpuzzle.Count - 1; i++)
            {
                for (int j = i + 1; j < linearpuzzle.Count; j++)
                    if (linearpuzzle[i] > linearpuzzle[j]) inversions++;
            }
            if (p.Length == 9)
            {
                if (inversions % 2 == 0)
                    testTb.BackColor = Color.FromArgb(0, 177, 224);
                else
                    testTb.BackColor = Color.FromArgb(205, 19, 66);
            }
            if (p.Length == 16)
            {
                int zero = 4 - (Array.IndexOf(p, 0) / 4);
                if ((zero % 2 == 0 && inversions % 2 != 0) || (zero % 2 != 0 && inversions % 2 == 0))
                    testTb2.BackColor = Color.FromArgb(0, 177, 224);
                else
                    testTb2.BackColor = Color.FromArgb(205, 19, 66);
            }

        }

        public void GetPuzzle(int[] puzzle)
        {
            if (puzzle.Length == 9)
            {
                try
                {
                    puzzle[0] = Int32.Parse(a1Tb.Text);
                    puzzle[1] = Int32.Parse(a2Tb.Text);
                    puzzle[2] = Int32.Parse(a3Tb.Text);
                    puzzle[3] = Int32.Parse(a4Tb.Text);
                    puzzle[4] = Int32.Parse(a5Tb.Text);
                    puzzle[5] = Int32.Parse(a6Tb.Text);
                    puzzle[6] = Int32.Parse(a7Tb.Text);
                    puzzle[7] = Int32.Parse(a8Tb.Text);
                    puzzle[8] = Int32.Parse(a9Tb.Text);
                }
                catch (System.FormatException)
                {

                }
            }
            if (puzzle.Length == 16)
            {
                try
                {
                    puzzle[0] = Int32.Parse(b1Tb.Text);
                    puzzle[1] = Int32.Parse(b2Tb.Text);
                    puzzle[2] = Int32.Parse(b3Tb.Text);
                    puzzle[3] = Int32.Parse(b4Tb.Text);
                    puzzle[4] = Int32.Parse(b5Tb.Text);
                    puzzle[5] = Int32.Parse(b6Tb.Text);
                    puzzle[6] = Int32.Parse(b7Tb.Text);
                    puzzle[7] = Int32.Parse(b8Tb.Text);
                    puzzle[8] = Int32.Parse(b9Tb.Text);
                    puzzle[9] = Int32.Parse(b10Tb.Text);
                    puzzle[10] = Int32.Parse(b11Tb.Text);
                    puzzle[11] = Int32.Parse(b12Tb.Text);
                    puzzle[12] = Int32.Parse(b13Tb.Text);
                    puzzle[13] = Int32.Parse(b14Tb.Text);
                    puzzle[14] = Int32.Parse(b15Tb.Text);
                    puzzle[15] = Int32.Parse(b16Tb.Text);
                }
                catch (System.FormatException)
                {

                }
            }
        }

        public void Reset()
        {
            Array.Clear(puzzle8, 0, puzzle8.Length);
            Array.Clear(puzzle15, 0, puzzle15.Length);
            Array.Clear(info, 0, info.Length);
            initNode = null;
            search = null;
            solution = null;
            step = 0;
            astep = 0;
            firststep = true;
            bystep = false;
            workTb.Text = string.Empty;
            statusTb.Text = string.Empty;
            statesTb.Text = string.Empty;
            ustatesTb.Text = string.Empty;
            dstatesTb.Text = string.Empty;
            depthTb.Text = string.Empty;
            timeTb.Text = string.Empty;
            infoCount.Clear();
            countChart.Series[0].Points.Clear();
            dependCount.Clear();
            dependChart.Series[0].Points.Clear();
            dependChart.ChartAreas[0].AxisX.ScaleView.Position = 0;
            sw.Reset();
            fullexec = false;
            optdepthTb.Text = string.Empty;
            optimalDepth.Clear();
        }

        private void GetSolution()
        {
            List<int> puzzle = new List<int>();
            if (puzzlemode == 8)
                puzzle = new List<int>(puzzle8);
            if (puzzlemode == 15)
                puzzle = new List<int>(puzzle15);
            if (astep != -1)
            {
                if (step != 0 && bystep)
                {
                    if (algochoice == 0)
                        solution = search.BreadthFirstSearch(initNode, statusTb, workTb, step, bystep);
                    if (algochoice == 1)
                        solution = search.DepthFirstSearch(initNode, statusTb, workTb, step, bystep, maxdepth);
                    if (algochoice == 2)
                        solution = search.AStarSearch(initNode, statusTb, workTb, step, bystep, maxdepth, heuristic);
                }
                else if (step != 0 && !bystep)
                {
                    if (!(testTb.BackColor == Color.FromArgb(0, 177, 224)))
                    {
                        if (algochoice == 0)
                            solution = search.BreadthFirstSearch(initNode, statusTb, workTb, astep, bystep);
                        if (algochoice == 1)
                            solution = search.DepthFirstSearch(initNode, statusTb, workTb, astep, bystep, maxdepth);
                        if (algochoice == 2)
                            solution = search.AStarSearch(initNode, statusTb, workTb, step, bystep, maxdepth, heuristic);
                    }
                    else
                    {
                        initNode = new Node(puzzle.ToArray());
                        search = new Search();
                        if (algochoice == 0)
                            solution = search.BreadthFirstSearch(initNode, statusTb, workTb, token);
                        if (algochoice == 1)
                            solution = search.DepthFirstSearch(initNode, statusTb, workTb, maxdepth, token);
                        if (algochoice == 2)
                            solution = search.AStarSearch(initNode, statusTb, workTb, maxdepth, heuristic, token);
                    }
                }
                else
                {
                    initNode = new Node(puzzle.ToArray());
                    search = new Search();
                    if (algochoice == 0)
                        solution = search.BreadthFirstSearch(initNode, statusTb, workTb, astep, bystep);
                    if (algochoice == 1)
                        solution = search.DepthFirstSearch(initNode, statusTb, workTb, astep, bystep, maxdepth);
                    if (algochoice == 2)
                        solution = search.AStarSearch(initNode, statusTb, workTb, step, bystep, maxdepth, heuristic);
                }
            }
            else
            {
                initNode = new Node(puzzle.ToArray());
                search = new Search();
                if (algochoice == 0)
                    solution = search.BreadthFirstSearch(initNode, statusTb, workTb, token);
                if (algochoice == 1)
                    solution = search.DepthFirstSearch(initNode, statusTb, workTb, maxdepth, token);
                if (algochoice == 2)
                    solution = search.AStarSearch(initNode, statusTb, workTb, maxdepth, heuristic, token);
            }
        }

        private void startfullBtn_Click(object sender, EventArgs e)
        {
            if (startfullBtn.Text == "Cancel")
            {
                cancelTokenSource.Cancel();
                return;
            }
            else
            {
                cancelTokenSource = new CancellationTokenSource();
                token = cancelTokenSource.Token;
            }

            if (statusTb.Text != string.Empty && statusTb.Text != "Cancelled")
            {
                if (MessageBox.Show("Algorithm has been finished. Wish to start over?", "Start",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Reset();
                else
                    return;
            }
            else if (statusTb.Text == "Cancelled")
                Reset();
            fullexec = true;
            if (firststep)
            {
                MetroTextBox test = null;
                if (puzzlemode == 8)
                    test = testTb;
                if (puzzlemode == 15)
                    test = testTb2;
                GetPuzzle(puzzle8);
                GetPuzzle(puzzle15);
                bystep = false;
                if (test.BackColor == Color.FromArgb(0, 177, 224))
                {
                    astep = -1;
                    sw.Reset();
                    timer1.Start();
                    sw.Start();
                    if (!bw.IsBusy)
                    {
                        progressBar.Enabled = true;
                        progressBar.Visible = true;
                        startfullBtn.Text = "Cancel";
                        bw.RunWorkerAsync();
                    }
                }
                else
                {
                    if (MessageBox.Show("Puzzle is not solvable. Continue?", "Start", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        astep = 2000;
                        sw.Reset();
                        timer1.Start();
                        sw.Start();
                        if (!bw.IsBusy)
                        {
                            progressBar.Enabled = true;
                            progressBar.Visible = true;
                            startfullBtn.Text = "Cancel";
                            bw.RunWorkerAsync();
                        }
                    }
                    else
                    {
                        // user clicked no
                    }
                }
            }
            else
            {
                bystep = false;
                astep = 1000;
                sw.Reset();
                timer1.Start();
                sw.Start();
                if (!bw.IsBusy)
                {
                    progressBar.Enabled = true;
                    progressBar.Visible = true;
                    startfullBtn.Text = "Cancel";
                    bw.RunWorkerAsync();
                }
            }
        }

        private void stepBtn_Click(object sender, EventArgs e)
        {
            if (statusTb.Text != string.Empty && statusTb.Text != "Cancelled")
            {
                if (MessageBox.Show("Algorithm has been finished.Wish to start over ? ", "Start",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Reset();
                else
                    return;
            }
            else if (statusTb.Text == "Cancelled")
                Reset();
            if (fullexec)
                Reset();
            step++;
            bystep = true;
            if (firststep)
            {
                MetroTextBox test = null;
                if (puzzlemode == 8)
                    test = testTb;
                if (puzzlemode == 15)
                    test = testTb2;
                GetPuzzle(puzzle8);
                GetPuzzle(puzzle15);
                if (test.BackColor == Color.FromArgb(0, 177, 224))
                {
                    List<int> puzzle = new List<int>();
                    if (puzzleTc.SelectedIndex == 0)
                        puzzle = new List<int>(puzzle8);
                    if (puzzleTc.SelectedIndex == 1)
                        puzzle = new List<int>(puzzle15);
                    initNode = new Node(puzzle.ToArray());
                    search = new Search();
                    GetSolution();
                    firststep = false;
                }
                else
                {
                    if (MessageBox.Show("Puzzle is not solvable. Continue?", "Start",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        List<int> puzzle = new List<int>();
                        if (puzzleTc.SelectedIndex == 0)
                            puzzle = new List<int>(puzzle8);
                        if (puzzleTc.SelectedIndex == 1)
                            puzzle = new List<int>(puzzle15);
                        initNode = new Node(puzzle.ToArray());
                        search = new Search();
                        GetSolution();
                        firststep = false;
                    }
                }
            }
            else
            {
                GetSolution();
            }
            try
            {
                if (solution.Count > 0)
                {
                    solution.Reverse();
                    workTb.Clear();
                    for (int i = 0; i < solution.Count; i++)
                    {
                        solution[i].PrintPuzzle(workTb);
                    }
                    info = search.GetInfo();
                    statesTb.Text = info[0].ToString();
                    ustatesTb.Text = info[1].ToString();
                    dstatesTb.Text = info[2].ToString();
                    depthTb.Text = info[3].ToString();
                    countChart.Series[0].Points.Clear();
                    infoCount.Clear();
                    infoCount.Add("Generated states", info[0]);
                    infoCount.Add("Added states", info[1]);
                    infoCount.Add("Discarded states", info[2]);
                    countChart.Series[0].Points.DataBindXY(infoCount.Keys, infoCount.Values);
                    dependCount = search.GetDepend();
                    if (dependCount.Count > 3)
                    {
                        optimalDepth.Clear();
                        for (int i = 3; i <= dependCount.Count; i++)
                            optimalDepth.Add(i, dependCount[i] - dependCount[i - 1]);
                        var minvalue = optimalDepth.OrderBy(kvp => kvp.Value).First();
                        var optdepth = optimalDepth.OrderBy(x => x.Value == minvalue.Value).Last();
                        optdepthTb.Text = optdepth.Key.ToString() + "; " + optdepth.Value.ToString();
                    }
                    dependChart.Series[0].Points.DataBindXY(dependCount.Keys, dependCount.Values);
                    if (dependChart.Series[0].Points.Count > 5)
                    {
                        dependChart.ChartAreas[0].AxisX.ScaleView.Size = 5;
                        dependChart.ChartAreas[0].AxisX.ScaleView.Position = dependChart.Series[0].Points.Count - 5;
                    }
                    dependChart.ChartAreas[0].AxisX.Interval = 1;
                    if ((Math.Floor(Math.Log10(dependCount.Values.Max()) + 1) > 4))
                        dependChart.Series[0].Font = new Font("Consolas", 9.75f);
                }
            }
            catch (NullReferenceException)
            {
            }
        }

        private void a1Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void a2Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void a3Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void a4Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void a5Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void a6Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void a7Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void a8Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void a9Tb_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle8);
            IsSolvable(puzzle8);
        }

        private void workTb_TextChanged(object sender, EventArgs e)
        {
            workTb.SelectionStart = workTb.Text.Length;
            workTb.ScrollToCaret();
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timeTb.Text = sw.Elapsed.ToString("mm\\:ss\\:fff");
        }

        private void algorithmCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            Reset();
            if (algorithmCb.SelectedIndex == 1 || algorithmCb.SelectedIndex == 2)
                depthCb.Visible = true;
            else
                depthCb.Visible = false;

            if (algorithmCb.SelectedIndex == 2)
                heuristicCb.Visible = true;
            else
                heuristicCb.Visible = false;

            if (algorithmCb.SelectedIndex == 0)
                algochoice = 0;
            if (algorithmCb.SelectedIndex == 1)
                algochoice = 1;
            if (algorithmCb.SelectedIndex == 2)
                algochoice = 2;
        }

        private void graphBtn_Click(object sender, EventArgs e)
        {
            if (graphBtn.Text == "Graphic representation >")
            {
                this.Size = new Size(1106, 626);
                progressBar.Size = new Size(1118, 40);
                graphBtn.Text = "Graphic representation <";
            }
            else
            {
                this.Size = new Size(473, 626);
                progressBar.Size = new Size(485, 40);
                graphBtn.Text = "Graphic representation >";
            }
        }

        private void saveImgColumnBtn_Click(object sender, EventArgs e)
        {
            countChart.Size = new Size(1510, 801);
            countChart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Consolas", 16.0f);
            countChart.SaveImage(AppDomain.CurrentDomain.BaseDirectory + "states.png", ChartImageFormat.Png);
            countChart.Size = new Size(510, 517);
            countChart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Consolas", 9.75f);
            if (MessageBox.Show("Screenshot has been made. Wish to open it?",
                "Done", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "states.png");
        }

        private void saveImgLineBtn_Click(object sender, EventArgs e)
        {
            dependChart.Size = new Size(1510, 801);
            dependChart.ChartAreas[0].AxisX.ScaleView.Size = dependChart.Series[0].Points.Count;
            dependChart.ChartAreas[0].AxisX.ScaleView.Position = 0;
            dependChart.SaveImage(AppDomain.CurrentDomain.BaseDirectory + "depend.png", ChartImageFormat.Png);
            dependChart.ChartAreas[0].AxisX.ScaleView.Size = 5;
            dependChart.Size = new Size(510, 517);
            if (MessageBox.Show("Screenshot has been made. Wish to open it?",
                "Done", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "depend.png");
        }

        private void statusTb_TextChanged(object sender, EventArgs e)
        {
            statusTb.SelectionStart = 0;
            statusTb.ScrollToCaret();
        }

        private void depthCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            maxdepth = depthCb.SelectedIndex + 1;
        }

        private void notify_BalloonTipClicked(object sender, EventArgs e)
        {
            this.Activate();
        }

        // async
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            GetSolution();
            sw.Stop();
            timer1.Stop();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Enabled = false;
            progressBar.Visible = false;
            if (solution.Count > 0)
            {
                solution.Reverse();
                for (int i = 0; i < solution.Count; i++)
                {
                    solution[i].PrintPuzzle(workTb);
                }
                info = search.GetInfo();
                statesTb.Text = info[0].ToString();
                ustatesTb.Text = info[1].ToString();
                dstatesTb.Text = info[2].ToString();
                depthTb.Text = info[3].ToString();
                countChart.Series[0].Points.Clear();
                infoCount.Clear();
                infoCount.Add("Generated states", info[0]);
                infoCount.Add("Added states", info[1]);
                infoCount.Add("Discarded states", info[2]);
                countChart.Series[0].Points.DataBindXY(infoCount.Keys, infoCount.Values);
                dependCount = search.GetDepend();
                if (dependCount.Count > 3)
                {
                    optimalDepth.Clear();
                    for (int i = 3; i <= dependCount.Count; i++)
                        optimalDepth.Add(i, dependCount[i] - dependCount[i - 1]);
                    var minvalue = optimalDepth.OrderBy(kvp => kvp.Value).First();
                    var optdepth = optimalDepth.OrderBy(x => x.Value == minvalue.Value).Last();
                    optdepthTb.Text = optdepth.Key.ToString() + "; " + optdepth.Value.ToString();
                }
                dependChart.Series[0].Points.DataBindXY(dependCount.Keys, dependCount.Values);
                if (dependChart.Series[0].Points.Count > 5)
                {
                    dependChart.ChartAreas[0].AxisX.ScaleView.Size = 5;
                    dependChart.ChartAreas[0].AxisX.ScaleView.Position = dependChart.Series[0].Points.Count - 5;
                }
                dependChart.ChartAreas[0].AxisX.Interval = 1;
                if ((Math.Floor(Math.Log10(dependCount.Values.Max()) + 1) > 4))
                    dependChart.Series[0].Font = new Font("Consolas", 9.75f);
            }
            else
            {
                //if(statusTb.Text == string.Empty)
                statusTb.Text = "No solution found";
            }
            if (!GFW.IsActive(Handle))
            {
                notify.Text = "8-15-Puzzle";
                notify.Visible = true;
                notify.Icon = SystemIcons.Information;
                notify.BalloonTipTitle = "Work is done";
                notify.BalloonTipText = "Application has ended algorithm's work";
                notify.BalloonTipIcon = ToolTipIcon.Info;
                notify.ShowBalloonTip(1000);
            }
            startfullBtn.Text = "Full";
        }

        private void heuristicCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            heuristic = heuristicCb.SelectedIndex;
        }

        private void SetStyle(FormStyle style)
        {
            if (style == FormStyle.Red)
            {
                this.Style = MetroFramework.MetroColorStyle.Red;
                progressBar.Style = MetroFramework.MetroColorStyle.Red;
                algorithmCb.Style = MetroFramework.MetroColorStyle.Red;
                depthCb.Style = MetroFramework.MetroColorStyle.Red;
                heuristicCb.Style = MetroFramework.MetroColorStyle.Red;
                graphTc.Style = MetroFramework.MetroColorStyle.Red;
                countChart.Series[0].Color = Color.FromArgb(205, 19, 66);
                dependChart.Series[0].LabelBackColor = Color.FromArgb(205, 19, 66);
                dependChart.Series[0].Color = Color.FromArgb(205, 19, 66);
                graphTc.Style = MetroFramework.MetroColorStyle.Red;
                puzzleTc.Style = MetroFramework.MetroColorStyle.Red;
                this.Text = "15-Puzzle";
                this.Refresh();
            }
            if (style == FormStyle.Blue)
            {
                this.Style = MetroFramework.MetroColorStyle.Blue;
                progressBar.Style = MetroFramework.MetroColorStyle.Blue;
                algorithmCb.Style = MetroFramework.MetroColorStyle.Blue;
                depthCb.Style = MetroFramework.MetroColorStyle.Blue;
                heuristicCb.Style = MetroFramework.MetroColorStyle.Blue;
                graphTc.Style = MetroFramework.MetroColorStyle.Blue;
                countChart.Series[0].Color = Color.FromArgb(0, 174, 219);
                dependChart.Series[0].LabelBackColor = Color.FromArgb(0, 174, 219);
                dependChart.Series[0].Color = Color.FromArgb(0, 174, 219);
                graphTc.Style = MetroFramework.MetroColorStyle.Blue;
                puzzleTc.Style = MetroFramework.MetroColorStyle.Blue;
                this.Text = "8-Puzzle";
                this.Refresh();
            }

        }

        private void puzzleTc_SelectedIndexChanged(object sender, EventArgs e)
        {
            Reset();
            if (puzzleTc.SelectedIndex == 0)
            {
                SetStyle(FormStyle.Blue);
                workTb.Font = new Font("Consolas", 12f);
            }

            if (puzzleTc.SelectedIndex == 1)
            {
                SetStyle(FormStyle.Red);
                workTb.Font = new Font("Consolas", 8f);
            }


            if (puzzleTc.SelectedIndex == 0)
                puzzlemode = 8;
            if (puzzleTc.SelectedIndex == 1)
                puzzlemode = 15;
        }

        private void b1_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b2_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b3_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b4_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b5_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b6_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b7_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b8_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b9_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b10_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b11_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b12_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b13_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b14_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b15_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }

        private void b16_TextChanged(object sender, EventArgs e)
        {
            Reset();
            GetPuzzle(puzzle15);
            IsSolvable(puzzle15);
        }
    }
}
