# Use the LTS version for better stability
FROM node:22-alpine

# Set the working directory inside the container
WORKDIR /app

# Copy only the package files first to leverage Docker layer caching
# This means 'npm install' only runs if you change your dependencies
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application code
COPY . .

# Expose the Angular default port
EXPOSE 4200

# Set the environment variable to skip the analytics prompt
ENV NG_CLI_ANALYTICS=false

# Start the application
CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--poll", "2000"]
