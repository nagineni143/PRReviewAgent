public class ChangeBatcher
{
    public List<ReviewBatch> CreateBatches(List<CodeChange> changes)
    {
        return changes
            .GroupBy(c => c.File)
            .Select(g => new ReviewBatch
            {
                File = g.Key,
                Changes = g.Take(20).ToList()
            })
            .ToList();
    }
}