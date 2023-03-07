## APP BUILDER
FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Default Environment 
ENV CURRENT_VERSION="__[Version]__"

# Args
ARG distFolder=QueueConsumer/bin/Release/net6
ARG appFile=QueueConsumer.dll
 
# Copy files to /app
RUN ls
COPY ${distFolder} /app

# Run application
WORKDIR /app
RUN ls
ENV appFile=$appFile
ENTRYPOINT dotnet $appFile