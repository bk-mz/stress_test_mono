//
using System;
using System.IO;

namespace domain_dll.RegexDna
{
	/* The Computer Language Benchmarks Game
   http://benchmarksgame.alioth.debian.org/
 *
 * contributed by Jimmy Tang
 */
	using System;
	using System.Linq;
	using System.Threading;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	[Serializable]
	public class Game
	{
		public void Run(int param)
		{
			string sequence = File.ReadAllText("regex_dna.txt");
			int initialLength = sequence.Length;
			sequence = Regex.Replace(sequence, ">.*\n|\n", "");
			int codeLength = sequence.Length;

			string[] variants = {
				"agggtaaa|tttaccct"
          , "[cgt]gggtaaa|tttaccc[acg]"
          , "a[act]ggtaaa|tttacc[agt]t"
          , "ag[act]gtaaa|tttac[agt]ct"
          , "agg[act]taaa|ttta[agt]cct"
          , "aggg[acg]aaa|ttt[cgt]ccct"
          , "agggt[cgt]aa|tt[acg]accct"
          , "agggta[cgt]a|t[acg]taccct"
          , "agggtaa[cgt]|[acg]ttaccct"
			};

			var flags = variants.Select((v, i) =>
			{
				var flag = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(x =>
				{
					variants[i] += " " + Regex.Matches(sequence, v).Count;
					flag.Set();
				});
				return flag;
			});
			WaitHandle.WaitAll(flags.ToArray());
			Console.WriteLine(string.Join("\n", variants));

			var dict = new Dictionary<string, string> {
				{ "B", "(c|g|t)" }, 
				{ "D", "(a|g|t)" },
				{ "H", "(a|c|t)" },
				{ "K", "(g|t)" },
				{"M", "(a|c)"},
				{"N", "(a|c|g|t)"},
				{"R", "(a|g)"},
				{"S", "(c|g)"},
				{"V", "(a|c|g)"},
				{"W", "(a|t)"},
				{"Y", "(c|t)"} 
			};
			sequence = new Regex("[WYKMSRBDVHN]").Replace(sequence, m => dict[m.Value]);
			Console.WriteLine("\n{0}\n{1}\n{2}", initialLength, codeLength, sequence.Length);
		}
	}
}

