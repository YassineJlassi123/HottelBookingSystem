# Use the official .NET SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["HotelPricingEngine.csproj", "./"]
RUN dotnet restore "HotelPricingEngine.csproj"

# Copy the rest of the application code
COPY . .
COPY wwwroot ./wwwroot

# Copy the rest of the application code and CSV file
COPY . .
COPY historical.csv /app/historical.csv

# Build the application
RUN dotnet build "HotelPricingEngine.csproj" -c Release -o /app/build

# Use the official .NET ASP.NET runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Set the ASPNETCORE_URLS environment variable
ENV ASPNETCORE_URLS=http://+:8080

# Copy the build output to the runtime image
COPY --from=build /app/build .
COPY --from=build /app/historical.csv /app/historical.csv
# Set the entry point to run the application
ENTRYPOINT ["dotnet", "HotelPricingEngine.dll"]
