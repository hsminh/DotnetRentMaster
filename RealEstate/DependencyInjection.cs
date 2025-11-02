using RentMaster.Core.File;
using RentMaster.Core.Cloudinary;
using RentMaster.partner.Storage.Interface;
using RentMaster.RealEstate.Services;

namespace RentMaster.RealEstate
{
    public static class DependencyInjection
    {
        public static IServiceCollection RealEstateModule(this IServiceCollection services)
        {
            // Import Apartment to dependency injection
            services.AddScoped<ApartmentRepository>();
            services.AddScoped<ApartmentService>();
            services.AddScoped<IFileStorage, CloudinaryStorage>();
            services.AddScoped<FileService>();
            return services;
        }
    }
}