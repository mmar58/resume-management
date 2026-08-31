namespace backend.Domain.Entities;

/// <summary>
/// Join entity representing a Project selected for inclusion in a specific CV.
/// By storing the relation to Project, if the user updates the project details
/// in their profile, it automatically reflects on the CV.
/// </summary>
public class CVProject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CVId { get; set; }
    public Guid ProjectId { get; set; }

    // Navigation
    public CV CV { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
