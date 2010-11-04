using System.Collections.Generic;

namespace Net
{
	static public class Net
	{
		static public readonly Link Link;
		static public readonly Link Thing;
		static public readonly Link IsA;
		static public readonly Link IsNotA;

		static public readonly Link Of;
		static public readonly Link And;
		static public readonly Link Has;
		static public readonly Link SumOf;
		static public readonly Link ConsistsOf;

		static public readonly Link Name;
		static public readonly Link HasName;

		static public readonly Link Sum;
		static public readonly Link String;
		static public readonly Link Char;

		static public Dictionary<long, Link> PowerOf2Links { get; private set; }
		static public Dictionary<Link, long> PowerOf2Numbers { get; private set; }

		static Net()
		{
			#region Core

			// Наивная инициализация (Не является корректным объяснением).
			Net.IsA = Link.CreateOutcomingSelflinker(null);
			Net.IsNotA = Link.CreateOutcomingSelflinker(Net.IsA);
			Net.Link = Link.CreateCycleSelflink(Net.IsA);
			Net.Thing = Link.CreateOutcomingSelflink(Net.IsNotA, Net.Link);

			Net.IsA.Target = Net.Link; // Исключение, позволяющие завершить систему

			#endregion

			Net.Of = Link.CreateOutcomingSelflink(Net.IsA, Net.Link);
			Net.And = Link.CreateOutcomingSelflink(Net.IsA, Net.Link);
			Net.Has = Link.CreateOutcomingSelflink(Net.IsA, Net.Link);
			Net.SumOf = Link.CreateOutcomingSelflink(Net.IsA, Net.Link);
			Net.ConsistsOf = Link.CreateOutcomingSelflink(Net.IsA, Net.Link);
            Net.HasName = Link.CreateOutcomingSelflink(Net.IsA, Net.Link);

			Net.Name = Link.CreateOutcomingSelflink(Net.IsA, Net.Thing);
			Net.IsA = Link.CreateOutcomingSelflink(Net.IsA, Net.Link);

			Net.Sum = Link.CreateOutcomingSelflink(Net.IsA, Net.Thing);
			Net.String = Link.CreateOutcomingSelflink(Net.IsA, Net.Thing);
			Net.Char = Link.CreateOutcomingSelflink(Net.IsA, Net.Thing);

			InitNumbers();
			SetNames();
		}

		private static void SetNames()
		{	
			Net.Thing.SetName("thing");
			Net.Link.SetName("link");
			Net.IsA.SetName("is a");
			Net.IsNotA.SetName("is not a");

			Net.And.SetName("and");
			Net.Has.SetName("has");
			Net.SumOf.SetName("sum of");
			Net.ConsistsOf.SetName("consists of");

			Net.Name.SetName("name");
			Net.HasName.SetName("has name");

			foreach (var pair in PowerOf2Links)
			{
				pair.Value.SetName(pair.Key.ToString());
			}
		}

		private static void InitNumbers()
		{
			PowerOf2Links = new Dictionary<long, Link>();
			PowerOf2Numbers = new Dictionary<Link, long>();

			long number = 1;
			for (int i = 0; i < 63; i++)
			{
				Link link = Link.CreateCycleSelflink(Net.IsA);

				PowerOf2Links.Add(number, link);
				PowerOf2Numbers.Add(link, number);

				number *= 2;
			}
		}
	}
}
