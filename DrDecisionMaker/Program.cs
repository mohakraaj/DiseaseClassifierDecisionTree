using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DrDecisionMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = ReadData("data.txt");
            decisionNode decision = new decisionNode();
            List<String> check = new List<string> { "(direct)","USA","yes","5"};
            var tree = decision.buildTree(data, decision.Entropy);
            decision.prunning(tree, 1, decision.Entropy);

         //   decision.classify(check, tree);
            decision.printTree(tree);
         //   Console.WriteLine(decision.Entropy(data));
            Console.Read();
        }

        static List<List<String>> ReadData(String file)
        {

            var information= File.ReadAllLines(file);
            List<List<String>> data = new List<List<string>>();

            foreach(var line in information)
            {
                data.Add(line.Split('\t').ToList());
            }

            return data;
        }

    }
}
