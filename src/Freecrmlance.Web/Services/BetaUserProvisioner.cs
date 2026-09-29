using Freecrmlance.Domain.Platform;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Web.Services;

public sealed class BetaUserProvisioner(
    UserManager<IdentityUser> userManager,
    FreecrmlanceDbContext dbContext)
{
    public async Task<int> CreateAsync(string email, string workspaceName, CancellationToken cancellationToken = default)
    {
        email = email.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Error.WriteLine("L'email est obligatoire.");
            return 1;
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            Console.Error.WriteLine("Un compte existe déjà avec cet email.");
            return 1;
        }

        Console.Write("Mot de passe : ");
        var password = ReadPassword();
        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.Error.WriteLine("Le mot de passe est obligatoire.");
            return 1;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                Console.Error.WriteLine($"- {error.Description}");
            return 1;
        }

        try
        {
            var workspace = new Workspace(workspaceName);
            dbContext.Workspaces.Add(workspace);
            dbContext.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, user.Id, WorkspaceRole.Owner));
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await userManager.DeleteAsync(user);
            throw;
        }

        Console.WriteLine($"Compte bêta créé pour {email}.");
        return 0;
    }

    private static string ReadPassword()
    {
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                    password.Length--;
                continue;
            }

            if (!char.IsControl(key.KeyChar))
                password.Append(key.KeyChar);
        }

        return password.ToString();
    }
}
