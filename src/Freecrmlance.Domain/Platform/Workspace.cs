namespace Freecrmlance.Domain.Platform;

public sealed class Workspace
{
    private Workspace() { }

    public Workspace(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workspace name is required.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
}
