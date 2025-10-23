# Build stage
FROM node:22.11.0-alpine AS build
WORKDIR /app

# Build arguments for React environment variables
ARG REACT_APP_TENANT_ID
ARG REACT_APP_TENANT_SUBDOMAIN
ARG REACT_APP_CLIENT_ID
ARG REACT_APP_API_CLIENT_ID
ARG REACT_APP_API_ENDPOINT
ARG REACT_APP_REDIRECT_URI

# Set environment variables for build
ENV REACT_APP_TENANT_ID=$REACT_APP_TENANT_ID
ENV REACT_APP_TENANT_SUBDOMAIN=$REACT_APP_TENANT_SUBDOMAIN
ENV REACT_APP_CLIENT_ID=$REACT_APP_CLIENT_ID
ENV REACT_APP_API_CLIENT_ID=$REACT_APP_API_CLIENT_ID
ENV REACT_APP_API_ENDPOINT=$REACT_APP_API_ENDPOINT
ENV REACT_APP_REDIRECT_URI=$REACT_APP_REDIRECT_URI

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --silent

# Copy source code
COPY . .

# Build the React app (environment variables will be embedded)
RUN npm run build

# Runtime stage with nginx to serve the static files
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built app from build stage
COPY --from=build /app/build .

# Copy nginx configuration (we'll create this)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
