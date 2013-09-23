//
using System;

namespace domain_dll.Redux
{
	/* The Computer Language Benchmarks Game
   http://benchmarksgame.alioth.debian.org/

   contributed by Isaac Gouy, transliterated from Oleg Mazurov's Java program
*/
	using System;
	using System.Threading;
	[Serializable]
	public class Game
	{
		private static long NCHUNKS = 150;
		private static long CHUNKSZ;
		private static long NTASKS;
		private static long n;
		private static long[] Fact;
		private static long[] maxFlips;
		private static long[] chkSums;
		private static int taskId;
		long[] p, pp, count;

		void FirstPermutation(long idx)
		{
			for (var i=0; i<p.Length; ++i)
			{
				p[i] = i;
			}

			for (var i=count.Length-1; i>0; --i)
			{
				var d = idx / Fact[i];
				count[i] = d;
				idx = idx % Fact[i];

				Array.Copy(p, 0, pp, 0, i + 1);
				for (var j=0; j<=i; ++j)
				{
					p[j] = j + d <= i ? pp[j + d] : pp[j + d - i - 1];
				}
			}
		}

		bool NextPermutation()
		{
			var first = p[1];
			p[1] = p[0];
			p[0] = first;

			var i = 1;
			while (++count[i] > i)
			{
				count[i++] = 0;
				var next = p[0] = p[1];
				for (var j=1; j<i; ++j)
				{
					p[j] = p[j + 1];
				}
				p[i] = first;
				first = next;
			}
			return true;
		}

		long CountFlips()
		{
			long flips = 1;
			long first = p[0];
			if (p[first] != 0)
			{
				Array.Copy(p, 0, pp, 0, pp.Length);
				do
				{
					++flips;
					for (long lo = 1, hi = first - 1; lo < hi; ++lo, --hi)
					{
						long t = pp[lo];
						pp[lo] = pp[hi];
						pp[hi] = t;
					}
					long tp = pp[first];
					pp[first] = first;
					first = tp;
				}
				while (pp[first] != 0);
			}
			return flips;
		}

		void RunTask(long task)
		{
			long idxMin = task * CHUNKSZ;
			long idxMax = Math.Min(Fact[n], idxMin + CHUNKSZ);

			FirstPermutation(idxMin);

			long maxflips = 1;
			long chksum = 0;
			for (long i=idxMin;;)
			{

				if (p[0] != 0)
				{
					long flips = CountFlips();
					maxflips = Math.Max(maxflips, flips);
					chksum += i % 2 == 0 ? flips : -flips;
				}

				if (++i == idxMax)
				{
					break;
				}

				NextPermutation();
			}
			maxFlips[task] = maxflips;
			chkSums[task] = chksum;
		}

		public void Compute()
		{
			p = new long[n];
			pp = new long[n];
			count = new long[n];

			long task;
			while ((task = taskId++) < NTASKS)
			{ // NOT SAFE - need PFX
				RunTask(task);
			} 
		}

		public static void Main(string[] args)
		{
			n = 7;
			if (args.Length > 0)
				n = Int64.Parse(args[0]);
			new Game().Run(n);
		}

		public void Run(long param)
		{
			n = Math.Min(10, param);
			Fact = new long[n + 1];
			Fact[0] = 1;
			for (var i = 1; i < Fact.Length; ++i)
			{
				Fact[i] = Fact[i - 1] * i;
			}
			CHUNKSZ = (Fact[n] + NCHUNKS - 1) / NCHUNKS;
			NTASKS = (Fact[n] + CHUNKSZ - 1) / CHUNKSZ;
			maxFlips = new long[NTASKS];
			chkSums = new long[NTASKS];
			taskId = 0;
			int nthreads = Environment.ProcessorCount;
			Thread[] threads = new Thread[nthreads];
			for (int i = 0; i < nthreads; ++i)
			{
				threads[i] = new Thread(new ThreadStart(Compute));
				threads[i].Start();
			}
			foreach (Thread t in threads)
			{
				t.Join();
			}
			long res = 0;
			foreach (var v in maxFlips)
			{
				res = Math.Max(res, v);
			}
			long chk = 0;
			foreach (var v in chkSums)
			{
				chk += v;
			}
			Console.WriteLine("{0}\nPfannkuchen({1}) = {2}", chk, n, res);
		}
	}
}

