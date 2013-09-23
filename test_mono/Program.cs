//
using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace test_mono
{
	struct GameAction
	{
		public GameType GameType;
		public Action<GameType> Delegate;
		public IAsyncResult Result;
	}

	enum GameType
	{
		Redux = 0,
		SpectralGames = 1,
		NormalForms = 2,
		KNucleotide = 3,
		RegexDna = 4,
	}

	class MainClass
	{
		static readonly Random Rand = new Random ();
		const int TASK_COOLDOWN = 700;
		const int TASK_COOLDOWN_DELTA = 200;
		static readonly Dictionary<GameType, string> ClassPath = new Dictionary<GameType, string> {
 { GameType.Redux, "domain_dll.Redux.Game" },
 { GameType.SpectralGames, "domain_dll.SpectralNormSingle.Game" },
 { GameType.NormalForms, "domain_dll.NGame.Game" },
 { GameType.KNucleotide, "domain_dll.nukleotide.Game" },
 { GameType.RegexDna, "domain_dll.RegexDna.Game" },
		};
		static readonly Queue<GameAction> Games = new Queue<GameAction> (Environment.ProcessorCount);

		public static void Main (string[] args)
		{
			var cooldown = TASK_COOLDOWN;
			if (args.Length > 0) {
				cooldown = int.Parse (args [0]);
				Console.WriteLine ("Cooldown is {0} ms", cooldown);
			}
			while (true) { // catch ctrl+c
				if (Games.Count < Environment.ProcessorCount) {
					GameType type;
					GetRandomTask (out type);
					Console.WriteLine ("Enqueing type " + type);
					var action = new Action<GameType> (RunGame);
					Games.Enqueue (new GameAction () { 
						GameType = type,
						Delegate = action, 
						Result = action.BeginInvoke (type, null, null) // run game here
					});
				} else {
					var game = Games.Dequeue ();
					Console.WriteLine ("Waiting for {0} to complete", game.GameType);
					try {
						game.Delegate.EndInvoke (game.Result); // sync here
					} catch (Exception ex) {
						Console.WriteLine ("Got exception here, okay i guess... " + ex.GetType ());
						Console.WriteLine (ex);
					}
				}
			}
		}

		static void RunGame (GameType type)
		{
			try {
				RunThreadPoolTask (type);
			} catch (Exception ex) {
				Console.WriteLine ("Exception! Too bad");
			}
		}

		static double GetRandDouble ()
		{
			return Rand.NextDouble ();
		}

		static void GetRandomTask (out GameType type)
		{
			var randVal = Rand.Next (1, 5); // no redux
			type = (GameType)randVal;
		}

		static object GetRandomInt (int min, int max)
		{
			return Rand.Next (min, max);
		}

		static void RunThreadPoolTask (GameType type)
		{
			var className = ClassPath [type];
			var sw = Stopwatch.StartNew ();
			var domain = AppDomain.CreateDomain ("Domain #" + DateTime.Now.Ticks);
			try {
				var instance = domain.CreateInstanceAndUnwrap ("domain_dll", className);
				var method = instance.GetType ().GetMethod ("Run");
				method.Invoke (instance, new [] { GetRandomInt (10, 20) });
			} finally {
				AppDomain.Unload (domain);
				Console.WriteLine ("Done game {0}. Elapsed = {1} s.", type, sw.Elapsed.TotalSeconds);
				sw.Stop ();
			}
		}
	}
}
