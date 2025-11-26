using RentMaster.Core.File;
using RentMaster.Core.Cloudinary;
using RentMaster.Management.RealEstate.Services;
using RentMaster.partner.Storage.Interface;
using RentMaster.Management.RealEstate.Repositories;
using RentMaster.Data;

namespace RentMaster.Management.RealEstate
{
    public static class DependencyInjection
    {
        public static IServiceCollection RealEstateModule(this IServiceCollection services)
        {
            services.AddScoped<ApartmentRepository>();
            services.AddScoped<ApartmentService>();
            services.AddScoped<ApartmentRoomRepository>();
            services.AddScoped<ApartmentRoomService>((provider) => 
                new ApartmentRoomService(
                    provider.GetRequiredService<ApartmentRoomRepository>(),
                    provider.GetRequiredService<FileService>(),
                    provider.GetRequiredService<AppDbContext>()
                )
            );
            services.AddScoped<IFileStorage, CloudinaryStorage>();
            services.AddScoped<FileService>();
            return services;
        }
    }
}