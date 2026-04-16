# Device Manager

A full-stack device management system built with ASP.NET Core and Angular.
Includes an AI-powered feature that generates user-friendly device descriptions based on technical specifications using a local LLM via LM Studio.

---

## Prerequisites

Make sure you have the following installed:

* .NET 10 SDK
* Node.js 20+
* Docker & Docker Compose
* LM Studio

---

## Running the Project

### 1. Start the database

```bash
docker compose up -d
```

---

### 2. Start the backend

```bash
cd backend
dotnet run --project DeviceManager.API
```

The API will be available at:
`http://localhost:5000` (or the configured port)

---

### 3. Start LM Studio (AI Description Generator)

1. Open LM Studio
2. Go to **Local Server**
3. Load a model (e.g. `google/gemma-3-4b`)
4. Start the server (default: `http://localhost:1234`)

⚠️ The backend expects:

```json
"LmStudio": {
  "BaseUrl": "http://localhost:1234",
  "Model": "google/gemma-3-4b"
}
```

If the model is not loaded, the description generator will return **503 Service Unavailable**.

---

### 4. Start the frontend

```bash
cd frontend/device-manager-ui
npm install
ng serve
```

Frontend will be available at:
`http://localhost:4200`

---

## Running the Tests

```bash
cd backend
dotnet test DeviceManager.IntegrationTests
```

---

## Notes

* The AI description feature is optional but recommended for full functionality.
* If LM Studio is not running, the application will continue to work, but descriptions will not be generated.
* Ensure ports `1234`, `5000`, and `4200` are available.
* Ensure device-manager-db-data is created before running Docker Compose
---
