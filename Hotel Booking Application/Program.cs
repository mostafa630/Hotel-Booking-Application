
using Hotel_Booking_Application.Connection;
using Hotel_Booking_Application.Repositories;
using Serilog;

namespace Hotel_Booking_Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //register the SQlConnectionFactory so I can use it any where to Create SqlConnection.
            builder.Services.AddSingleton<SqlConnectionFactory>();
            //register the repositories so I can use it any where to access the database.
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<RoomTypeRepository>();
            builder.Services.AddScoped<RoomRepository>();

            // Initialize Serilog 
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
