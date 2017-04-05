namespace Blockchain.Models
{
    public class BlockchainSyncProgress
    {
        public int StartBlocks { get; }
        public int CurrentBlocks { get; private set; }
        public int TotalBlocks { get; }
        public double Percentage { get; private set; }

        public BlockchainSyncProgress(int currentBlocks, int totalBlocks)
        {
            StartBlocks = CurrentBlocks = currentBlocks;
            TotalBlocks = totalBlocks;
            Percentage = 0;
        }

        public void Update(int currentBlocks)
        {
            CurrentBlocks = currentBlocks;
            Percentage = (CurrentBlocks - StartBlocks)/(double)(TotalBlocks - StartBlocks);
        }
    }
}
