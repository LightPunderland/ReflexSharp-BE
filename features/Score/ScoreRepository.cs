using Features.Score;
using Microsoft.EntityFrameworkCore;
using Data;

public class ScoreRepository : GenericRepository<ScoreEntity, Guid>, IScoreRepository
{
    public ScoreRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ScoreEntity>> GetTopScoresAsync(int count)
    {
        return await _dbSet
            .OrderByDescending(s => s.Score)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScoreEntity>> GetTopScoresByUserAsync(Guid userId, int count)
    {
        return await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Score)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScoreEntity>> GetScoresByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }
}
