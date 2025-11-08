namespace AoC2024;

public class Day22
{
	struct Buyer
	{
		public int secret;
		public int[] prices = new int[2000];
		public int[] priceChanges = new int[2000];

		public Buyer() {}
	}
	
	public (string, string) Run(List<string> input)
	{
		List<Buyer> buyers = input.Select(line => new Buyer(){secret = Int32.Parse(line)}).ToList(); 
		long part1 = buyers.Sum(b => CalcSecret(b, 2000));
		
		// Part 2
		// get all possible sequences for all our buyers
		Dictionary<string, int> sequenceCache = new Dictionary<string, int>();
		buyers.ForEach(b => AddSequencesToCache(b, buyers, sequenceCache));
		
		// order sequences by count, calculate prices for the top 5 (should be enough)
		var top5 = sequenceCache.OrderByDescending(x => x.Value).Take(5);
		int max = top5.Max(item => buyers.Sum(b => GetPriceForSequence(b, item.Key.Split(',').Select(c => Int32.Parse(c)).ToArray())));
		
		return (part1.ToString(), max.ToString());
	}

	private void AddSequencesToCache(Buyer buyer, List<Buyer> buyers, Dictionary<string, int> sequenceCache)
	{
		// generate all 4 nr sequences from this buyer's price changes
		for (int i = 0; i < buyer.priceChanges.Length-3; i++)
		{
			if (buyer.priceChanges[i + 3] < 0) continue; // ignore sequences with negative last nr
			if (buyer.priceChanges[i] + buyer.priceChanges[i+1] + buyer.priceChanges[i+2] + buyer.priceChanges[i+3] <= 0) continue; // ignore sequences with negative sum
			int[] seq = new int[]{buyer.priceChanges[i], buyer.priceChanges[i+1], buyer.priceChanges[i+2], buyer.priceChanges[i+3]};
			if (!sequenceCache.ContainsKey(string.Join(',', seq)))
			{
				sequenceCache.Add(string.Join(',', seq), 1);
			}
			else sequenceCache[string.Join(',', seq)] += 1;
		}
		// Console.WriteLine($"Checking {sequences.Count} {sequenceCache.Count} sequences for buyer {buyers.IndexOf(buyer)}");
	}

	private int GetPriceForSequence(Buyer buyer, int[] sequence)
	{
		int max = 0;
		for (int i = 0; i < buyer.prices.Length-3; i++)
		{
			if (buyer.priceChanges[i] == sequence[0] && buyer.priceChanges[i + 1] == sequence[1] &&
			    buyer.priceChanges[i + 2] == sequence[2] && buyer.priceChanges[i + 3] == sequence[3])
			{
				return buyer.prices[i+3];
			}
		}
		return 0;
	}
	
	private long CalcSecret(Buyer buyer, int iterations)
	{
		long secret = buyer.secret;
		Enumerable.Range(0, iterations).ToList().ForEach(i =>
		{
			secret = CalcNextSecret(secret);
			buyer.prices[i] = (int) (secret % 10);
			if (i!=0) buyer.priceChanges[i] = buyer.prices[i] - buyer.prices[i-1];
		});
		
		return secret;
	}

	long CalcNextSecret(long secret)
	{
		// Calculate the result of multiplying the secret number by 64. Then, mix this result into the secret number. Finally, prune the secret number.
		secret = Prune(Mix(secret, secret * 64)); // 8^2, 2^6
		// Calculate the result of dividing the secret number by 32. Round the result down to the nearest integer. Then, mix this result into the secret number. Finally, prune the secret number.
		secret = Prune(Mix(secret, secret / 32)); // 8*4, 2^5
		// Calculate the result of multiplying the secret number by 2048. Then, mix this result into the secret number. Finally, prune the secret number.
		secret = Prune(Mix(secret, secret * 2048)); // 2^11
		return secret;
		// 
	}

	long Prune(long value) => value % 16777216; // 8^8, 2^24 - retain 24 bits

	long Mix(long secret, long tomix) => secret ^ tomix; 
}