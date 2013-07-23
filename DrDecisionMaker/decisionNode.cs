using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrDecisionMaker
{
    class decisionNode
    {
        public int col;
        public String value;
        Dictionary<String, int> result = new Dictionary<string, int>();
        decisionNode tb;
        decisionNode fb;

        public decisionNode(int col = -1, String value = null, Dictionary<String, int> result = null, decisionNode tb = null, decisionNode fb = null)
        {
            this.col = col;
            this.value = value;
            this.result = result;
            this.tb = tb;
            this.fb = fb;

        }

        public Tuple<List<List<String>>, List<List<String>>> divideSet(List<List<String>> rows, int column, String value, bool number = false)
        {
            List<List<String>> set1 = new List<List<string>>();
            List<List<String>> set2 = new List<List<string>>();

            foreach (var row in rows)
            {
                if (number)
                {
                    if (float.Parse(row[column]) >= float.Parse(value))
                    {
                        set1.Add(row);
                    }
                    else
                    {
                        set2.Add(row);
                    }

                }
                else
                {
                    if (row[column] == value)
                        set1.Add(row);
                    else
                        set2.Add(row);
                }
            }

            return Tuple.Create(set1, set2);
        }

        public Dictionary<String, int> uniqueCount(List<List<String>> rows, int col = -1)
        {
            Dictionary<String, int> count = new Dictionary<string, int>();
            if (rows.Count <= 0)
                return count;
            if (col == -1)
                col = (rows[0].Count) - 1;
            foreach (var row in rows)
            {
                if (!count.ContainsKey(row[col]))
                {
                    count[row[col]] = 0;
                }
                count[row[col]]++;

            }
            return count;
        }

        public double GiniImpurity(List<List<String>> rows)
        {
            double imp = 0, p1, p2;
            var count = uniqueCount(rows);
            int total = rows.Count;
            foreach (var key in count.Keys)
            {
                p1 = (double)count[key] / (double)total;

                foreach (var ky in count.Keys)
                {
                    if (ky == key)
                        continue;
                    p2 = (double)count[ky] / (double)total;

                    imp += p1 * p2;

                }
            }

            return imp;
        }

        public double Entropy(List<List<String>> rows)
        {
            double imp = 0, p1;
            var count = uniqueCount(rows);
            int total = rows.Count;
            foreach (var key in count.Keys)
            {
                p1 = (double)count[key] / (double)total;
                imp += p1 * Math.Log(p1) / Math.Log(2);

            }

            return Math.Abs(imp);
        }

        public decisionNode buildTree(List<List<String>> rows, Func<List<List<String>>, double> scoref)
        {
            if (rows.Count == 0)
                return new decisionNode();

            double best_gain = 0.0, gain, res;
            double currentScore = scoref(rows);
            Tuple<int, String> bestCriteria = new Tuple<int, String>(-1, "default");
            Tuple<List<List<String>>, List<List<String>>> bestSets = new Tuple<List<List<string>>, List<List<string>>>(new List<List<string>>(), new List<List<string>>());
            double total = rows.Count;
            int colCount = rows[0].Count - 1;

            for (int i = 0; i < colCount; i++)
            {
                var column = uniqueCount(rows, i);

                foreach (var key in column.Keys)
                {

                    var sets = divideSet(rows, i, key, double.TryParse(key, out res));
                    var per = (double)sets.Item1.Count / total;


                    gain = currentScore - per * scoref(sets.Item1) - (1 - per) * scoref(sets.Item2);
                    if (gain > best_gain && sets.Item1.Count > 0 && sets.Item2.Count > 0)
                    {
                        best_gain = gain;
                        bestCriteria = Tuple.Create(i, key);
                        bestSets = sets;
                    }
                }
            }

            if (best_gain > 0)
            {
                var tb = buildTree(bestSets.Item1, scoref);
                var fb = buildTree(bestSets.Item2, scoref);
                return new decisionNode(bestCriteria.Item1, bestCriteria.Item2, null, tb, fb);
            }
            else
            {
                return new decisionNode(-1, null, uniqueCount(rows));
            }
        }


        private void printDict(Dictionary<String, int> dict)
        {
            Console.Write("{");
            foreach (var key in dict.Keys)
                Console.Write(key + ":" + dict[key] + ",");
            Console.WriteLine("}");
        }
        public void printTree(decisionNode node, String indent = "")
        {
            if (node.result != null)
                printDict(node.result);
            else
            {
                Console.WriteLine(node.col + ":" + node.value + "?");
                Console.Write(indent + "T-->");
                printTree(node.tb, indent + " ");
                Console.Write(indent + "F-->");
                printTree(node.fb, indent + " ");

            }
        }

        public void classify(List<String> observation, decisionNode tree)
        {
            decisionNode branch;
            if (tree.result != null)
                printDict(tree.result);
            else
            {
                var value = observation[tree.col];
                double result;
                if (double.TryParse(value, out result))
                {
                    if (result >= double.Parse(tree.value))
                        branch = tree.tb;
                    else
                        branch = tree.fb;
                }
                else
                {
                    if (value == tree.value)
                        branch = tree.tb;
                    else
                        branch = tree.fb;
                }
                classify(observation, branch);
            }

        }

        public void prunning(decisionNode tree,double minimum, Func<List<List<String>>, double> scoref)
        {
           
            if (tree.tb.result == null)
                prunning(tree.tb,minimum, scoref);
            if (tree.fb.result == null)
                prunning(tree.fb,minimum, scoref);

            if(tree.fb.result!=null && tree.tb.result!=null)
            {
                List<List<String>> tb = new List<List<string>>();
                List<List<String>> fb = new List<List<string>>();
                List<List<String>> both = new List<List<string>>();

                foreach (var key in tree.tb.result.Keys)
                {
                    for (int i = 0; i < tree.tb.result[key]; i++)
                        tb.Add(new List<String>{key});
                }
                foreach (var key in tree.fb.result.Keys)
                {
                    for (int i = 0; i < tree.fb.result[key]; i++)
                        fb.Add(new List<String> { key });
                }
                both.AddRange(new List<List<String>> (tb));
                both.AddRange(new List<List<String>>(fb));
                var delta=scoref(both)-(scoref(tb)+scoref(fb))/2;

                if (delta < minimum)
                {
                    tree.tb = null;
                    tree.fb = null;
                    tree.result = uniqueCount(both);
                }
            }
        }
    }
}
