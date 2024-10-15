FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["QuartzScheduler.Api/QuartzScheduler.Api.csproj", "QuartzScheduler.Api/"]
COPY ["QuartzScheduler.Core/QuartzScheduler.Core.csproj", "QuartzScheduler.Core/"]
COPY ["QuartzScheduler.Common/QuartzScheduler.Common.csproj", "QuartzScheduler.Common/"]
RUN dotnet restore "QuartzScheduler.Api/QuartzScheduler.Api.csproj"
COPY . .
WORKDIR "/src/QuartzScheduler.Api"
RUN dotnet build "QuartzScheduler.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuartzScheduler.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY openssl.conf /
ENV OPENSSL_CONF=/openssl.conf

RUN DEBIAN_FRONTEND=noninteractive TZ=Etc/UTC apt-get -y install tzdata

ENTRYPOINT ["dotnet", "QuartzScheduler.Api.dll"]
