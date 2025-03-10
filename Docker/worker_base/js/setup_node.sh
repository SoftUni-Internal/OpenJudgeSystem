#!/bin/bash
# Install nvm
curl https://raw.githubusercontent.com/creationix/nvm/master/install.sh | bash && \
export NVM_DIR="$HOME/.nvm" && \
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"

# Install Node.js v12 and v20
nvm install 12.22.12
nvm install 20.12.0
nvm alias default 20.12.0

# The order in which dependencies are installed matters:
# - The 'nvm use' command sets the Node.js version for the current shell session
#   by updating the $PATH environment variable.
# - If dependencies are installed first with v20 (20.12.0) and then with v12 (12.22.12),
#   the 'node' binary in /usr/bin/node will point to v12 after the final 'ln -sf' command.
# - Conversely, if dependencies are installed first with v12 and then with v20,
#   /usr/bin/node will point to v20.

# Why this matters:
# If a test requires Node.js v20 but /usr/bin/node points to v12, runtime errors will occur.
# To debug which directories are being searched for 'node', use:
#   echo $PATH
# The shell searches these directories from LEFT to RIGHT for the 'node' binary.

# Install dependencies for Node.js v12 and v20
nvm use 12.22.12
npm install --prefix /judge-resources/js/v12
npm install --prefix /judge-resources/js/v12/js-run-spa-in-docker-and-execute-mocha-tests

nvm use default
npm install --prefix /judge-resources/js/v20
npm install --prefix /judge-resources/js/v20/js-run-spa-in-docker-and-execute-mocha-tests
npm install -g typescript

# Update /usr/bin/node to point to the currently active Node.js version:
ln -sf "$(which node)" /usr/bin/node
