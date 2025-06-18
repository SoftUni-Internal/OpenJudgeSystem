FROM ubuntu:22.04

ENV TZ=Etc/UTC
ENV LANG=en_US.UTF-8
ENV LANGUAGE=en_US:en
ENV LC_ALL=en_US.UTF-8

# Base Setup
RUN apt-get update && apt-get install -y --no-install-recommends \
      curl \
      sudo \
      apt-transport-https \
      ca-certificates \
      gnupg \
      lsb-release \
      build-essential \
      locales \
      acl \
      unzip && \
    ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone && \
    sed -i '/en_US.UTF-8/s/^# //g' /etc/locale.gen && \
    locale-gen && \
    rm -rf /var/lib/apt/lists/*

RUN apt-get update && apt-get install -y \
      dotnet-sdk-8.0 \
      dotnet-sdk-6.0 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /judge-resources/dotnet/nuget-packages

# Set environment variables for offline mode
ENV NUGET_PACKAGES=/judge-resources/dotnet/nuget-packages
COPY ./packages-net6.csproj /tmp/packages-net6.csproj
COPY ./packages-net8.csproj /tmp/packages-net8.csproj
COPY ./NuGet.config /opt/NuGet.config

WORKDIR /tmp
RUN dotnet restore packages-net6.csproj && \
    dotnet restore packages-net8.csproj

# Create dummy console apps to download runtime dependencies
RUN dotnet new console -n DummyNet6 -f net6.0 && \
    dotnet new console -n DummyNet8 -f net8.0

RUN cd DummyNet6 && \
    dotnet restore --runtime linux-x64 && \
    dotnet publish --no-restore --runtime linux-x64 --self-contained false

RUN cd DummyNet8 && \
    dotnet restore --runtime linux-x64 && \
    dotnet publish --no-restore --runtime linux-x64 --self-contained false

RUN rm -rf /tmp/*

# Copy nuget.config (just in case create 2 versions for case sensitivity issues)
WORKDIR /root/.nuget/NuGet
RUN cp /opt/NuGet.config /root/.nuget/NuGet/NuGet.config && \
    cp /opt/NuGet.config /root/.nuget/NuGet/NuGet.Config

RUN chmod -R 755 /judge-resources/dotnet/nuget-packages

RUN ln -sf /usr/lib/dotnet/dotnet /usr/bin/dotnet && \
    chmod +x /usr/lib/dotnet/dotnet

ENV PATH="/usr/lib/dotnet:$PATH"
