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
      unzip && \
    ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone && \
    sed -i '/en_US.UTF-8/s/^# //g' /etc/locale.gen && \
    locale-gen && \
    rm -rf /var/lib/apt/lists/*

# Install Node.js 12 and 20 via nvm and TypeScript via npm
RUN curl https://raw.githubusercontent.com/creationix/nvm/master/install.sh | bash && \
    export NVM_DIR="$HOME/.nvm" && \
    [ -s "$NVM_DIR/nvm.sh" ] && . "$NVM_DIR/nvm.sh" && \
    nvm install 12.22.12 && \
    nvm install 20.12.0 && \
    nvm alias default 20.12.0 && \
    nvm use default && \
    npm install -g typescript

# Install supported languages and frameworks
RUN apt-get update && apt-get install -y \
      dotnet-sdk-8.0 \
      dotnet-sdk-6.0 \
      openjdk-11-jdk \
      openjdk-21-jdk \
      python3.9 \
      python3-pip \
      golang-go \
      libgtk-3-0 \
      libgbm1 && \
    rm -rf /var/lib/apt/lists/*

# Docker setup
RUN curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg && \
    echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null && \
    apt-get update && apt-get install -y --no-install-recommends \
      docker-ce \
      docker-ce-cli \
      containerd.io && \
    rm -rf /var/lib/apt/lists/*

# Install Maven
ENV MAVEN_VERSION 3.9.1
WORKDIR /judge-resources
RUN curl -O https://archive.apache.org/dist/maven/maven-3/$MAVEN_VERSION/binaries/apache-maven-$MAVEN_VERSION-bin.tar.gz && \
    tar -xzvf apache-maven-$MAVEN_VERSION-bin.tar.gz && \
    rm apache-maven-$MAVEN_VERSION-bin.tar.gz && \
    ln -s /judge-resources/apache-maven-$MAVEN_VERSION/bin/mvn /usr/bin/mvn

# Java setup
WORKDIR /tmp/java
COPY ./java/gradlew ./gradlew
COPY ./java/gradlew.bat ./gradlew.bat
COPY ./java/gradle ./gradle
RUN ./gradlew

COPY ./java/build.gradle ./build.gradle
RUN ./gradlew getDeps11
RUN ./gradlew getDeps21

# JS and JS SPA
WORKDIR /judge-resources/js
COPY ./js .
RUN chmod +x ./setup_node.sh ./chromium-scripts/update-and-run.sh && \
    ./setup_node.sh && \
    ./chromium-scripts/update-and-run.sh

# Python 11 with Django support
RUN apt-get update && apt-get install -y --no-install-recommends \
      make \
      libssl-dev \
      zlib1g-dev \
      libbz2-dev \
      libreadline-dev \
      libsqlite3-dev \
      llvm \
      libncurses5-dev \
      xz-utils \
      tk-dev \
      libxml2-dev \
      libxmlsec1-dev \
      libffi-dev \
      liblzma-dev \
      mecab-ipadic-utf8 \
      git \
      libpq-dev && \
    rm -rf /var/lib/apt/lists/*

ENV PYENV_ROOT /root/.pyenv
ENV PATH $PYENV_ROOT/shims:$PYENV_ROOT/bin:$PATH
RUN curl https://pyenv.run | bash && \
    pyenv update && \
    pyenv install 3.11.5

# Install Python packages from requirements.txt
COPY ./python/requirements.txt /tmp/

RUN pip3 install --no-cache-dir -r /tmp/requirements.txt

RUN $PYENV_ROOT/versions/3.11.5/bin/pip install --no-cache-dir -r /tmp/requirements.txt

WORKDIR /judge-resources/java/java-spring-and-hibernate
COPY ./java-spring-and-hibernate .
RUN mvn dependency:go-offline
# Needed 2 versions of this package, one in the strategy pom.xml, other one is istalled here
# Otherwise maven resolves only 1 dependency
# TODO: Fix this
RUN mvn dependency:get -DgroupId=org.codehaus.plexus -DartifactId=plexus-utils -Dversion=3.0.10
RUN mvn clean install

WORKDIR /judge-resources/java/java-21-spring-and-hibernate
COPY ./java-21-spring-and-hibernate .
RUN mvn dependency:go-offline
# Needed 2 versions of this package, one in the strategy pom.xml, other one is installed here
# Otherwise maven resolves only 1 dependency
RUN mvn dependency:get -DgroupId=org.codehaus.plexus -DartifactId=plexus-utils -Dversion=3.0.10
RUN mvn clean install

# Go
ENV GO111MODULE=off

# Clean up
RUN apt-get autoremove -y && apt-get clean && rm -rf /var/lib/apt/lists/*

CMD tail -f /dev/null
