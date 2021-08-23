#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 8042
EXPOSE 4243

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Haproxy.AgentCheck.csproj", "."]
RUN dotnet restore "./Haproxy.AgentCheck.csproj"
COPY . .
WORKDIR "/src/."
RUN sed -i '/<OutputType>/d' Haproxy.AgentCheck.csproj
RUN sed -i '/<RuntimeIdentifier>/d' Haproxy.AgentCheck.csproj
RUN sed -i '/<PublishTrimmed>/d' Haproxy.AgentCheck.csproj
RUN sed -i '/<TrimMode>/d' Haproxy.AgentCheck.csproj
RUN dotnet build "Haproxy.AgentCheck.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Haproxy.AgentCheck.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Haproxy.AgentCheck.dll"]