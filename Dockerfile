FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend

WORKDIR src

COPY ./*.sln ./
COPY ./**/*.csproj ./

RUN for f in *.csproj; do \
        filename=$(basename $f) && \
        dirname=${filename%.*} && \
        mkdir $dirname && \
        mv $filename ./$dirname/; \
    done
RUN dotnet restore VpnBot.API/VpnBot.API.csproj

COPY ./ ./
RUN dotnet publish VpnBot.API/VpnBot.API.csproj --output ./publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final 
WORKDIR app 
COPY --from=backend src/publish .

EXPOSE 80
ENV TZ=Europe/Moscow
ENV ASPNETCORE_URLS http://0.0.0.0:80
ENTRYPOINT ["dotnet", "VpnBot.API.dll"]