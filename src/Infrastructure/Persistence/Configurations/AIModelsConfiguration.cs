using Domain.AIModel;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AIModelsConfiguration : IEntityTypeConfiguration<AIModel>
{
    public void Configure(EntityTypeBuilder<AIModel> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new AIModelId(x));

        builder.Property(x => x.Name)
            .HasColumnType("varchar(200)")
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasColumnType("varchar(32)")
            .IsRequired();

        builder.Property(x => x.ParametersBillions)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.ReleaseDate)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
    }
}
