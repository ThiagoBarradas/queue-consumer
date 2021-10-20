## APP BUILDER
FROM mcr.microsoft.com/dotnet/runtime:3.1

# Default Environment 
ENV CURRENT_VERSION="__[Version]__"

# Args
ARG distFolder=QueueConsumer/bin/Release/netcoreapp3.1
ARG appFile=QueueConsumer.dll

# NewRelic
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
    && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
    && wget https://download.newrelic.com/548C16BF.gpg \
    && apt-key add 548C16BF.gpg \
    && apt-get update \
    && apt-get install -y newrelic-netcore20-agent
ENV CORECLR_ENABLE_PROFILING=0 
ENV CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A}
ENV CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent
ENV CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so
ENV NEW_RELIC_DISTRIBUTED_TRACING_ENABLED=true

# Copy files to /app
RUN ls
COPY ${distFolder} /app

# Run application
WORKDIR /app
RUN ls
ENV appFile=$appFile
ENTRYPOINT dotnet $appFile