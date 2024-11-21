using Features.Score;

public interface IScoreRepository : IGenericRepository<ScoreEntity, Guid>
{
    Task<IEnumerable<ScoreEntity>> GetTopScoresAsync(int count);
    Task<IEnumerable<ScoreEntity>> GetTopScoresByUserAsync(Guid userId, int count);
    Task<IEnumerable<ScoreEntity>> GetScoresByUserAsync(Guid userId);
}
