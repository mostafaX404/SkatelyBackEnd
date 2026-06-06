using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RolesConfigurations : IEntityTypeConfiguration<IdentityRole>
{


    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
      builder.HasData(
        new IdentityRole {Id = "admin-id",Name="Admin",NormalizedName="ADMIN"},
        new IdentityRole {Id = "customer-id",Name="Customer",NormalizedName="CUSTOMER"}
      );
    }
}