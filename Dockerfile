# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY ./src/Skarabeus_Api/Skarabeus_Api.csproj ./Skarabeus_Api/Skarabeus_Api.csproj
COPY ./src/Skarabeus_Data/Skarabeus_Data.csproj ./Skarabeus_Data/Skarabeus_Data.csproj
RUN dotnet restore ./Skarabeus_Api/Skarabeus_Api.csproj

# Copy the rest of the project files
COPY ./src ./
RUN dotnet publish ./Skarabeus_Api/Skarabeus_Api.csproj -c Release -o out

# Use a smaller runtime image for the API
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose API port
EXPOSE 5000
ENTRYPOINT ["dotnet", "Skarabeus_Api.dll"]