FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish ./
RUN apt-get update && \
    apt-get install -y --no-install-recommends ffmpeg && \
    rm -rf /var/lib/apt/lists/*
EXPOSE 8080
ENTRYPOINT ["dotnet", "HaruyasumiRyokouki.Backend.dll"]
