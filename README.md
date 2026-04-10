# Device Manager

A full-stack device management system built with ASP.NET Core and Angular.

## Prerequisites
- .NET 10 SDK
- Node.js 20+
- Docker

## Running the project

### 1. Start the database
docker compose up -d

### 2. Start the backend
cd backend
dotnet run --project DeviceManager.API

### 3. Start the frontend
cd frontend/device-manager-ui
npm install
ng serve

## Running the tests
cd backend
dotnet test DeviceManager.IntegrationTests