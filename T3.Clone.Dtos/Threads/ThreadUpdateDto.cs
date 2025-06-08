namespace T3.Clone.Dtos.Threads;

public class ThreadUpdateDto
{
    public List<ThreadDto> UpdatedThreads { get; set; }
    public int UpdatedVersion { get; set; }
}