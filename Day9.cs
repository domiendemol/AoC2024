namespace AoC2024;

public class Day9
{
	private List<int> _fileSizes = new List<int>();
	
	public void Run(List<string> input)
	{
		// parse
		_fileSizes = new List<int>(input[0].Length);
		List<int> blocks = input[0].SelectMany((c, i) => GetBlocks(c, i)).ToList();
		List<int> freeSpaceIndxs = blocks.Select((b, indx) => b == -1 ? indx : -1).Where(i => i != -1).ToList();
		List<int> fileIndxs = blocks.Select((b, indx) => b != -1 ? indx : -1).Where(i => i != -1).ToList();

		Console.WriteLine($"Part 1: {Part1(new List<int>(blocks), fileIndxs)}");
		Console.WriteLine($"Part 2: {Part2(new List<int>(blocks), fileIndxs, freeSpaceIndxs)}");
	}

	private long Part1(List<int> blocks, List<int> fileIndxs)
	{
		int lastFileIndxIndx = fileIndxs.Count - 1;
		for (int i = 0; i < blocks.Count; i++)
		{
			if (blocks[i] != -1) continue;

			int lastFileIndx = fileIndxs[lastFileIndxIndx--];
			int lastFile = blocks[lastFileIndx];
			if (lastFileIndx <= i) break;
			
			blocks[lastFileIndx] = -1;
			blocks[i] = lastFile;
			// blocks.ForEach(b => Console.Write((b == -1) ? '.' : ""+b));
			// Console.WriteLine();
		}

		return blocks.Select((b, indx) => (long) (b == -1 ? 0 : indx * b)).Sum();
	}

	private long Part2(List<int> blocks, List<int> fileIndxs, List<int> freeSpaceIndxs)
	{
		for (int i = _fileSizes.Count-1; i >= 0; i--)
		{
			// find free space block
			int fileSize = _fileSizes[i];
			for (int j = 0; j < freeSpaceIndxs.Count; j++)
			{
				int free = 1; // determine amount of free space
				while (freeSpaceIndxs[j] + free < blocks.Count && blocks[freeSpaceIndxs[j] + free] == -1) free++;
				
				if (free >= fileSize) {
					if (MoveFile(blocks, i, fileSize,freeSpaceIndxs[j]))
						freeSpaceIndxs.RemoveRange(j, fileSize);
					break;
				}
			}
		}

		return blocks.Select((b, indx) => (long) (b == -1 ? 0 : indx * b)).Sum();
	}

	private bool MoveFile(List<int> blocks, int id, int size, int target)
	{
		int from = blocks.IndexOf(id);
		if (from <= target) return false;
		
		_fileSizes[id] = Int32.MaxValue;
		for (int j = 0; j < size; j++)
		{
			blocks[from+j] = -1;
			blocks[target+j] = id;
		}
		return true;
	}
	
	private List<int> GetBlocks(char c, int index)
	{
		List<int> b = new List<int>();
		if (index % 2 == 0) {
			for (int i=0; i<char.GetNumericValue(c); i++) b.Add(index/2);
			_fileSizes.Add((int) char.GetNumericValue(c));
		}
		else {
			// free space
			for (int i=0; i<char.GetNumericValue(c); i++) b.Add(-1);
		}
		return b;
	}
	
}