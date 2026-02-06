namespace CorporateKnowledgeBase.Application.Features.Departments;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

// DTOs
public record DepartmentDto(int Id, string Name, string? Description);

// Queries
public record GetAllDepartmentsQuery : IRequest<List<DepartmentDto>>;

public class GetAllDepartmentsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDto>>
{
    public async Task<List<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        return await context.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name, d.Description))
            .ToListAsync(cancellationToken);
    }
}

// Commands
public record CreateDepartmentCommand(string Name, string? Description) : IRequest<int>;

public class CreateDepartmentHandler(IApplicationDbContext context)
    : IRequestHandler<CreateDepartmentCommand, int>
{
    public async Task<int> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dept = new Department
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };
        context.Departments.Add(dept);
        await context.SaveChangesAsync(cancellationToken);
        return dept.Id;
    }
}

public record UpdateDepartmentCommand(int Id, string Name, string? Description) : IRequest<bool>;

public class UpdateDepartmentHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateDepartmentCommand, bool>
{
    public async Task<bool> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dept = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (dept is null) return false;

        dept.Name = request.Name;
        dept.Description = request.Description;
        dept.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteDepartmentCommand(int Id) : IRequest<bool>;

public class DeleteDepartmentHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteDepartmentCommand, bool>
{
    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dept = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (dept is null) return false;

        context.Departments.Remove(dept);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
