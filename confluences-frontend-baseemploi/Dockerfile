# Build the Vue app
FROM node:14.4.0-alpine as build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY ./ .
RUN npm run build
# Copy the built app in an NGINX contaier
FROM nginx:1.25.3-alpine as production-stage
RUN mkdir /app
COPY --from=build-stage /app/dist /app
COPY nginx.conf /etc/nginx/nginx.conf

# Running the script at container run
COPY ./substitute_environment_variables.sh /docker-entrypoint.d/substitute_environment_variables.sh
RUN chmod +x /docker-entrypoint.d/substitute_environment_variables.sh

EXPOSE 8080