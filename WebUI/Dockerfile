# Stage 1: Build frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY WebUI/WebUI.Frontend/package*.json ./
RUN npm install
COPY WebUI/WebUI.Frontend/ ./
RUN npm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /app
COPY *.sln ./
COPY Common/*.csproj ./Common/
COPY WebUI/WebUI.API/*.csproj ./WebUI/WebUI.API/
COPY WebUI/WebUI.Core/*.csproj ./WebUI/WebUI.Core/
COPY WebUI/WebUI.Data/*.csproj ./WebUI/WebUI.Data/
RUN dotnet restore WebUI/WebUI.API/WebUI.API.csproj

COPY . ./
RUN dotnet publish WebUI/WebUI.API/WebUI.API.csproj -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /app/frontend/dist ./wwwroot

EXPOSE 5000
EXPOSE 5001

ENV ASPNETCORE_URLS=http://+:5000;https://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebUI.API.dll"]
