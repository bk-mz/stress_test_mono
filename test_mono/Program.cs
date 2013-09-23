//
using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace test_mono
{
	class MainClass
	{
		static readonly Random Rand = new Random();

		const int TASK_COOLDOWN = 700;
		const int TASK_COOLDOWN_DELTA = 200;
		
		enum GameType {
			Redux = 0,
			SpectralGames = 1,
			NormalForms = 2,
			KNucleotide = 3,
			RegexDna = 4,
		}
		
		static readonly Dictionary<GameType, string> ClassPath = new Dictionary<GameType, string>{
			{GameType.Redux, "domain_dll.Redux.Game"},
			{GameType.SpectralGames, "domain_dll.SpectralNorms.Game"},
			{GameType.NormalForms, "domain_dll.NGame.Game"},
			{GameType.KNucleotide, "domain_dll.nukleotide.Game"},
			{GameType.RegexDna, "domain_dll.RegexDna.Game"},
		};
		
		public static void Main(string[] args)
		{
			var cooldown = TASK_COOLDOWN;
			if (args.Length > 0) {
				cooldown = int.Parse(args[0]);
				Console.WriteLine("Cooldown is {0} ms", cooldown);
			}
			Console.WriteLine("Starting...");
			while (true)
			{ // catch ctrl+c
				GameType type;
				GetRandomTask(out type);
				ThreadPool.QueueUserWorkItem(new WaitCallback((object state) =>
				{
					try 
					{
						RunThreadPoolTask(type);
					}
					catch (Exception ex) 
					{
						Console.WriteLine("Okay, exception! {0}", ex.GetType());
					}
				}));
				Thread.Sleep(cooldown - (int)(GetRandDouble() * TASK_COOLDOWN_DELTA));
			}
		}

		static double GetRandDouble()
		{
			return Rand.NextDouble();
		}

		static void GetRandomTask(out GameType type)
		{
			var randVal = Rand.Next(0, 5);
			type = (GameType)randVal;
		}

		static object GetRandomInt(int min, int max)
		{
			return Rand.Next(min, max);
		}

		static void RunThreadPoolTask(GameType type)
		{
			Console.WriteLine("Starting game `{0}`...", type);
			var className = ClassPath[type];
			Stopwatch sw = Stopwatch.StartNew();;
			AppDomain domain = AppDomain.CreateDomain("Domain #" + DateTime.Now.Ticks);
			try 
			{
				var instance = domain.CreateInstanceAndUnwrap("domain_dll", className);
				var method = instance.GetType().GetMethod("Run");
				method.Invoke(instance, new [] {GetRandomInt(10, 20)});
			}
			finally {
				AppDomain.Unload(domain);
				Console.WriteLine("Done game {0}. Elapsed = {1} s.", type, sw.Elapsed.TotalSeconds);
				sw.Stop();
			}
		}
	}
}
