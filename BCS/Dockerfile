#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["BCS/HMTSolution.BCS.csproj", "BCS/"]
COPY ["BusinessObjects/BusinessInfrastructure/HMTSolution.Infrastructure.csproj", "BusinessObjects/BusinessInfrastructure/"]
COPY ["MongoAccessor/MongoRepo/MongoRepo/HMTSolution.MongoRepo.csproj", "MongoAccessor/MongoRepo/MongoRepo/"]
COPY ["MongoAccessor/MongoAccess/HMTSolution.MongoAccess.csproj", "MongoAccessor/MongoAccess/"]
RUN dotnet restore "BCS/HMTSolution.BCS.csproj"
COPY . .
WORKDIR "/src/BCS"
RUN dotnet build "HMTSolution.BCS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HMTSolution.BCS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HMTSolution.BCS.dll"]
