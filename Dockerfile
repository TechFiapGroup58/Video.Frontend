
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/FiapX.VideoProcessor.Ui -c Release -o /app

FROM nginx:alpine
COPY --from=build /app/wwwroot /usr/share/nginx/html
